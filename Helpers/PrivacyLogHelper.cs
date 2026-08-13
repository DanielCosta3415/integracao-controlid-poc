using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Globalization;

namespace Integracao.ControlID.PoC.Helpers;

public static class PrivacyLogHelper
{
    private const int TokenLength = 12;
    private const int MaxSafeLogValueLength = 256;

    public static string SanitizeForLog(object? value, string emptyValue = "none")
    {
        var text = value?.ToString();
        if (string.IsNullOrWhiteSpace(text))
            return emptyValue;

        var normalized = text
            .Replace('\r', ' ')
            .Replace('\n', ' ')
            .Replace('\t', ' ')
            .Trim();

        if (normalized.Length == 0)
            return emptyValue;

        return normalized.Length <= MaxSafeLogValueLength
            ? normalized
            : normalized[..MaxSafeLogValueLength];
    }

    public static string PseudonymizeUser(string? value)
    {
        return SanitizeForLog(Pseudonymize(value, "anonymous"), "anonymous");
    }

    public static string PseudonymizeIp(IPAddress? address)
    {
        return address == null
            ? "ip:unknown"
            : SanitizeForLog($"ip:{Hash(address.ToString())}", "ip:unknown");
    }

    public static string PseudonymizeEndpoint(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return "endpoint:unknown";

        if (Uri.TryCreate(value, UriKind.Absolute, out var uri))
            return SanitizeForLog($"endpoint:{uri.Scheme}:{Hash(uri.Authority)}", "endpoint:unknown");

        return SanitizeForLog($"endpoint:{Hash(value)}", "endpoint:unknown");
    }

    public static string PseudonymizeIdentifier(object? value, string emptyValue = "ref:unknown")
    {
        return value switch
        {
            null => emptyValue,
            string text => Pseudonymize(text, emptyValue),
            IFormattable formattable => Pseudonymize(formattable.ToString(null, CultureInfo.InvariantCulture), emptyValue),
            _ => Pseudonymize(value.ToString(), emptyValue)
        };
    }

    public static string Pseudonymize(string? value, string emptyValue = "none")
    {
        if (string.IsNullOrWhiteSpace(value))
            return emptyValue;

        return SanitizeForLog($"ref:{Hash(value.Trim())}", emptyValue);
    }

    private static string Hash(string value)
    {
        var normalized = value.Trim().ToUpperInvariant();
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(normalized));
        return Convert.ToHexString(bytes)[..TokenLength].ToLowerInvariant();
    }
}
