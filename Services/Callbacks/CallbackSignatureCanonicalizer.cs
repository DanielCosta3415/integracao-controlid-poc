using System.Security.Cryptography;
using System.Text;

namespace Integracao.ControlID.PoC.Services.Callbacks;

public static class CallbackSignatureCanonicalizer
{
    public static string ComputeSignature(
        string sharedKey,
        string method,
        string path,
        string queryString,
        string timestamp,
        string nonce,
        ReadOnlySpan<byte> body)
    {
        var bodyHash = SHA256.HashData(body);
        var canonical = string.Join(
            "\n",
            method.ToUpperInvariant(),
            path,
            queryString,
            timestamp,
            nonce,
            Convert.ToBase64String(bodyHash));

        using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(sharedKey));
        return Convert.ToBase64String(hmac.ComputeHash(Encoding.UTF8.GetBytes(canonical)));
    }
}
