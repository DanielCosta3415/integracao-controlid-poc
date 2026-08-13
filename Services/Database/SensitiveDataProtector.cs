using Microsoft.AspNetCore.DataProtection;

namespace Integracao.ControlID.PoC.Services.Database;

public sealed class SensitiveDataProtector
{
    public const string ProtectedValuePrefix = "dp:v1:";

    private readonly IDataProtectionProvider _provider;

    public SensitiveDataProtector(IDataProtectionProvider provider)
    {
        _provider = provider;
    }

    public string Protect(string? value, string purpose)
    {
        if (string.IsNullOrEmpty(value) || IsProtected(value))
            return value ?? string.Empty;

        var protectedValue = _provider.CreateProtector(purpose).Protect(value);
        return ProtectedValuePrefix + protectedValue;
    }

    public string Unprotect(string? value, string purpose)
    {
        if (string.IsNullOrEmpty(value) || !IsProtected(value))
            return value ?? string.Empty;

        return _provider
            .CreateProtector(purpose)
            .Unprotect(value[ProtectedValuePrefix.Length..]);
    }

    public static bool IsProtected(string value)
    {
        return value.StartsWith(ProtectedValuePrefix, StringComparison.Ordinal);
    }
}
