using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Globalization;

namespace Integracao.ControlID.PoC.Helpers;

public static class PrivacyLogHelper
{
    private const int TokenLength = 12;

    public static string PseudonymizeUser(string? value)
    {
        return Pseudonymize(value, "anonymous");
    }

    public static string PseudonymizeIp(IPAddress? address)
    {
        return address == null
            ? "ip:unknown"
            : $"ip:{Hash(address.ToString())}";
    }

    public static string PseudonymizeEndpoint(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return "endpoint:unknown";

        if (Uri.TryCreate(value, UriKind.Absolute, out var uri))
            return $"endpoint:{uri.Scheme}:{Hash(uri.Authority)}";

        return $"endpoint:{Hash(value)}";
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

        return $"ref:{Hash(value.Trim())}";
    }

    private static string Hash(string value)
    {
        var normalized = value.Trim().ToUpperInvariant();
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(normalized));
        return Convert.ToHexString(bytes)[..TokenLength].ToLowerInvariant();
    }
}
