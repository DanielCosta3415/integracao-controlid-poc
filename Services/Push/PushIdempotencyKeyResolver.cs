using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Http;

namespace Integracao.ControlID.PoC.Services.Push;

public sealed class PushIdempotencyKeyResolver
{
    private const int MaxKeyLength = 128;

    public Guid? Resolve(HttpRequest request)
    {
        var key = ResolveRawKey(request);
        if (string.IsNullOrWhiteSpace(key) || key.Length > MaxKeyLength)
            return null;

        key = key.Trim();
        return Guid.TryParse(key, out var explicitGuid)
            ? explicitGuid
            : CreateDeterministicGuid(key);
    }

    private static string? ResolveRawKey(HttpRequest request)
    {
        if (request.Query.TryGetValue("idempotency_key", out var queryKey) &&
            queryKey.Count > 0 &&
            !string.IsNullOrWhiteSpace(queryKey.ToString()))
        {
            return queryKey.ToString();
        }

        if (request.Headers.TryGetValue("Idempotency-Key", out var headerKey) &&
            headerKey.Count > 0 &&
            !string.IsNullOrWhiteSpace(headerKey.ToString()))
        {
            return headerKey.ToString();
        }

        return null;
    }

    private static Guid CreateDeterministicGuid(string key)
    {
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(key));
        var guidBytes = hash[..16];

        guidBytes[7] = (byte)((guidBytes[7] & 0x0F) | 0x50);
        guidBytes[8] = (byte)((guidBytes[8] & 0x3F) | 0x80);

        return new Guid(guidBytes);
    }
}
