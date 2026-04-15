using System.Net;
using System.Security.Cryptography;
using System.Text;
using Integracao.ControlID.PoC.Options;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace Integracao.ControlID.PoC.Services.Callbacks
{
    public class CallbackSecurityEvaluator
    {
        private readonly CallbackSecurityOptions _options;

        public CallbackSecurityEvaluator(IOptions<CallbackSecurityOptions> options)
        {
            _options = options.Value;
        }

        /// <summary>
        /// Avalia se um callback recebido pode entrar na PoC com base em tamanho, IP remoto e chave compartilhada.
        /// </summary>
        /// <param name="httpContext">Contexto HTTP bruto da requisicao recebida.</param>
        /// <returns>Resultado de autorizacao com status HTTP recomendado quando houver rejeicao.</returns>
        public CallbackSecurityEvaluationResult Evaluate(HttpContext httpContext)
        {
            var maxBodyBytes = NormalizeMaxBodyBytes();
            if (httpContext.Request.ContentLength.HasValue &&
                httpContext.Request.ContentLength.Value > maxBodyBytes)
            {
                return CallbackSecurityEvaluationResult.Reject(
                    StatusCodes.Status413PayloadTooLarge,
                    $"Payload exceeds the configured limit of {maxBodyBytes} bytes.");
            }

            if (!IsRemoteIpAllowed(httpContext.Connection.RemoteIpAddress))
            {
                return CallbackSecurityEvaluationResult.Reject(
                    StatusCodes.Status403Forbidden,
                    "Remote address is not allowed for callback ingress.");
            }

            if (!_options.RequireSharedKey)
                return CallbackSecurityEvaluationResult.Allow();

            if (string.IsNullOrWhiteSpace(_options.SharedKey))
            {
                return CallbackSecurityEvaluationResult.Reject(
                    StatusCodes.Status500InternalServerError,
                    "Callback security is misconfigured.");
            }

            var headerName = GetSharedKeyHeaderName();
            if (!httpContext.Request.Headers.TryGetValue(headerName, out var providedValue) ||
                string.IsNullOrWhiteSpace(providedValue))
            {
                return CallbackSecurityEvaluationResult.Reject(
                    StatusCodes.Status401Unauthorized,
                    "Callback shared key is missing.");
            }

            if (!HasSharedKeyMatch(providedValue.ToString(), _options.SharedKey))
            {
                return CallbackSecurityEvaluationResult.Reject(
                    StatusCodes.Status401Unauthorized,
                    "Callback shared key is invalid.");
            }

            return CallbackSecurityEvaluationResult.Allow();
        }

        /// <summary>
        /// Verifica se o endereco remoto esta dentro da lista permitida, considerando loopback quando configurado.
        /// </summary>
        /// <param name="remoteIp">IP remoto capturado pelo ASP.NET Core.</param>
        /// <returns>True quando o IP e aceito pela politica atual de callbacks.</returns>
        public bool IsRemoteIpAllowed(IPAddress? remoteIp)
        {
            var allowedIps = _options.AllowedRemoteIps
                .Where(static value => !string.IsNullOrWhiteSpace(value))
                .Select(static value => value.Trim())
                .ToList();

            if (allowedIps.Count == 0)
                return true;

            if (remoteIp == null)
                return false;

            if (_options.AllowLoopback && IPAddress.IsLoopback(remoteIp))
                return true;

            foreach (var allowedIp in allowedIps)
            {
                if (!IPAddress.TryParse(allowedIp, out var parsedAllowedIp))
                    continue;

                if (parsedAllowedIp.Equals(remoteIp))
                    return true;

                if (parsedAllowedIp.AddressFamily != remoteIp.AddressFamily)
                {
                    if (parsedAllowedIp.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork &&
                        remoteIp.IsIPv4MappedToIPv6 &&
                        parsedAllowedIp.Equals(remoteIp.MapToIPv4()))
                    {
                        return true;
                    }

                    if (parsedAllowedIp.AddressFamily == System.Net.Sockets.AddressFamily.InterNetworkV6 &&
                        remoteIp.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork &&
                        parsedAllowedIp.IsIPv4MappedToIPv6 &&
                        parsedAllowedIp.MapToIPv4().Equals(remoteIp))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        private int NormalizeMaxBodyBytes()
        {
            return _options.MaxBodyBytes > 0 ? _options.MaxBodyBytes : 1024 * 1024;
        }

        private string GetSharedKeyHeaderName()
        {
            return string.IsNullOrWhiteSpace(_options.SharedKeyHeaderName)
                ? "X-ControlID-Callback-Key"
                : _options.SharedKeyHeaderName;
        }

        private static bool HasSharedKeyMatch(string providedValue, string expectedValue)
        {
            var providedBytes = Encoding.UTF8.GetBytes(providedValue);
            var expectedBytes = Encoding.UTF8.GetBytes(expectedValue);

            return providedBytes.Length == expectedBytes.Length &&
                   CryptographicOperations.FixedTimeEquals(providedBytes, expectedBytes);
        }
    }

    public sealed class CallbackSecurityEvaluationResult
    {
        private CallbackSecurityEvaluationResult(bool isAllowed, int statusCode, string message)
        {
            IsAllowed = isAllowed;
            StatusCode = statusCode;
            Message = message;
        }

        public bool IsAllowed { get; }
        public int StatusCode { get; }
        public string Message { get; }

        public static CallbackSecurityEvaluationResult Allow()
        {
            return new CallbackSecurityEvaluationResult(true, StatusCodes.Status200OK, string.Empty);
        }

        public static CallbackSecurityEvaluationResult Reject(int statusCode, string message)
        {
            return new CallbackSecurityEvaluationResult(false, statusCode, message);
        }
    }
}
