using System.Collections.Concurrent;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using Integracao.ControlID.PoC.Options;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Integracao.ControlID.PoC.Services.Callbacks
{
    public sealed class CallbackSignatureValidator
    {
        private readonly CallbackSecurityOptions _options;
        private readonly ILogger<CallbackSignatureValidator> _logger;
        private readonly ConcurrentDictionary<string, DateTimeOffset> _seenNonces = new(StringComparer.Ordinal);

        public CallbackSignatureValidator(
            IOptions<CallbackSecurityOptions> options,
            ILogger<CallbackSignatureValidator> logger)
        {
            _options = options.Value;
            _logger = logger;
        }

        public CallbackSignatureValidationResult Validate(HttpRequest request, string body)
        {
            if (!_options.RequireSignedRequests)
                return CallbackSignatureValidationResult.Allow();

            if (string.IsNullOrWhiteSpace(_options.SharedKey))
                return CallbackSignatureValidationResult.Reject(StatusCodes.Status500InternalServerError, "Callback signature security is misconfigured.");

            var signatureHeaderName = GetHeaderName(_options.SignatureHeaderName, "X-ControlID-Signature");
            var timestampHeaderName = GetHeaderName(_options.TimestampHeaderName, "X-ControlID-Timestamp");
            var nonceHeaderName = GetHeaderName(_options.NonceHeaderName, "X-ControlID-Nonce");

            var signature = request.Headers[signatureHeaderName].ToString();
            var timestamp = request.Headers[timestampHeaderName].ToString();
            var nonce = request.Headers[nonceHeaderName].ToString();

            if (string.IsNullOrWhiteSpace(signature) ||
                string.IsNullOrWhiteSpace(timestamp) ||
                string.IsNullOrWhiteSpace(nonce))
            {
                return CallbackSignatureValidationResult.Reject(StatusCodes.Status401Unauthorized, "Callback signature headers are missing.");
            }

            if (!TryParseTimestamp(timestamp, out var requestTimestamp))
                return CallbackSignatureValidationResult.Reject(StatusCodes.Status401Unauthorized, "Callback timestamp is invalid.");

            var maxSkew = TimeSpan.FromSeconds(Math.Clamp(_options.MaxClockSkewSeconds, 30, 3600));
            var now = DateTimeOffset.UtcNow;
            if (requestTimestamp < now.Subtract(maxSkew) || requestTimestamp > now.Add(maxSkew))
                return CallbackSignatureValidationResult.Reject(StatusCodes.Status401Unauthorized, "Callback timestamp is outside the accepted window.");

            if (nonce.Length > 128 || nonce.Any(char.IsControl))
                return CallbackSignatureValidationResult.Reject(StatusCodes.Status401Unauthorized, "Callback nonce is invalid.");

            var expectedSignature = ComputeSignature(request, body, timestamp, nonce);
            if (!FixedTimeEquals(NormalizeSignature(signature), expectedSignature))
                return CallbackSignatureValidationResult.Reject(StatusCodes.Status401Unauthorized, "Callback signature is invalid.");

            RemoveExpiredNonces(now);
            var replayKey = $"{request.Path}|{nonce}";
            if (!_seenNonces.TryAdd(replayKey, now.AddSeconds(Math.Clamp(_options.NonceTtlSeconds, 60, 3600))))
            {
                _logger.LogWarning("Blocked replayed callback nonce for {Path}.", request.Path);
                return CallbackSignatureValidationResult.Reject(StatusCodes.Status409Conflict, "Callback nonce was already used.");
            }

            return CallbackSignatureValidationResult.Allow();
        }

        public string ComputeSignature(HttpRequest request, string body, string timestamp, string nonce)
        {
            var bodyHash = SHA256.HashData(Encoding.UTF8.GetBytes(body ?? string.Empty));
            var canonical = string.Join(
                "\n",
                request.Method.ToUpperInvariant(),
                request.Path.Value ?? string.Empty,
                request.QueryString.HasValue ? request.QueryString.Value : string.Empty,
                timestamp,
                nonce,
                Convert.ToBase64String(bodyHash));

            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(_options.SharedKey));
            return Convert.ToBase64String(hmac.ComputeHash(Encoding.UTF8.GetBytes(canonical)));
        }

        private static string GetHeaderName(string configuredHeaderName, string fallback)
        {
            return string.IsNullOrWhiteSpace(configuredHeaderName) ? fallback : configuredHeaderName.Trim();
        }

        private static string NormalizeSignature(string signature)
        {
            var normalized = signature.Trim();
            return normalized.StartsWith("sha256=", StringComparison.OrdinalIgnoreCase)
                ? normalized["sha256=".Length..].Trim()
                : normalized;
        }

        private static bool TryParseTimestamp(string timestamp, out DateTimeOffset parsed)
        {
            if (long.TryParse(timestamp, NumberStyles.Integer, CultureInfo.InvariantCulture, out var unixSeconds))
            {
                try
                {
                    parsed = DateTimeOffset.FromUnixTimeSeconds(unixSeconds);
                    return true;
                }
                catch (ArgumentOutOfRangeException)
                {
                    parsed = default;
                    return false;
                }
            }

            return DateTimeOffset.TryParse(timestamp, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out parsed);
        }

        private static bool FixedTimeEquals(string providedSignature, string expectedSignature)
        {
            try
            {
                var providedBytes = Convert.FromBase64String(providedSignature);
                var expectedBytes = Convert.FromBase64String(expectedSignature);

                return providedBytes.Length == expectedBytes.Length &&
                       CryptographicOperations.FixedTimeEquals(providedBytes, expectedBytes);
            }
            catch (FormatException)
            {
                return false;
            }
        }

        private void RemoveExpiredNonces(DateTimeOffset now)
        {
            foreach (var item in _seenNonces)
            {
                if (item.Value <= now)
                    _seenNonces.TryRemove(item.Key, out _);
            }
        }
    }

    public sealed class CallbackSignatureValidationResult
    {
        private CallbackSignatureValidationResult(bool isAllowed, int statusCode, string message)
        {
            IsAllowed = isAllowed;
            StatusCode = statusCode;
            Message = message;
        }

        public bool IsAllowed { get; }
        public int StatusCode { get; }
        public string Message { get; }

        public static CallbackSignatureValidationResult Allow()
        {
            return new CallbackSignatureValidationResult(true, StatusCodes.Status200OK, string.Empty);
        }

        public static CallbackSignatureValidationResult Reject(int statusCode, string message)
        {
            return new CallbackSignatureValidationResult(false, statusCode, message);
        }
    }
}
