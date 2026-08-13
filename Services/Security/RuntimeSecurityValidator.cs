using Integracao.ControlID.PoC.Options;
using Microsoft.Extensions.Options;

namespace Integracao.ControlID.PoC.Services.Security;

internal static class RuntimeSecurityValidator
{
    internal static void Validate(WebApplication app)
    {
        if (app.Environment.IsDevelopment())
            return;

        var allowedHosts = app.Configuration["AllowedHosts"];
        var configuredHosts = (allowedHosts ?? string.Empty)
            .Split([';', ','], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

        if (configuredHosts.Length == 0 || configuredHosts.Any(static host => host == "*"))
        {
            throw new InvalidOperationException(
                "AllowedHosts must be explicitly configured for non-Development environments.");
        }

        if (configuredHosts.Any(IsPlaceholderValue))
        {
            throw new InvalidOperationException(
                "AllowedHosts must not contain placeholder values for non-Development environments.");
        }

        var dataProtectionKeyPath = app.Configuration["DataProtection:KeyPath"];
        if (string.IsNullOrWhiteSpace(dataProtectionKeyPath) || IsPlaceholderValue(dataProtectionKeyPath))
        {
            throw new InvalidOperationException(
                "DataProtection:KeyPath must point to persistent storage for non-Development environments.");
        }

        ValidateDataProtectionCertificate(app.Configuration);

        if (!app.Configuration.GetValue<bool>("Security:RequireHttps"))
        {
            throw new InvalidOperationException(
                "Security:RequireHttps must be true for non-Development environments.");
        }

        ValidateSensitiveDataProtection(app);
        ValidateCallbackSecurity(app);

        if (app.Configuration.GetValue<bool>("OpenApi:Enabled"))
        {
            throw new InvalidOperationException(
                "OpenApi:Enabled must be false for non-Development environments.");
        }

        if (app.Configuration.GetValue<bool>("Observability:Metrics:AllowAnonymous"))
        {
            throw new InvalidOperationException(
                "Observability:Metrics:AllowAnonymous must be false for non-Development environments.");
        }

        ValidateForwardedHeaders(app.Configuration);
        ValidateDeviceEgress(app);
    }

    internal static string? ReadSecretFile(string? path, string contentRootPath)
    {
        if (string.IsNullOrWhiteSpace(path))
            return null;

        var resolvedPath = Path.GetFullPath(path, contentRootPath);
        return File.ReadAllText(resolvedPath).TrimEnd('\r', '\n');
    }

    private static void ValidateDataProtectionCertificate(IConfiguration configuration)
    {
        var certificatePath = configuration["DataProtection:CertificatePath"];
        var certificatePassphrase = configuration["DataProtection:CertificatePassword"];
        var certificatePasswordFile = configuration["DataProtection:CertificatePasswordFile"];
        if (string.IsNullOrWhiteSpace(certificatePath) || IsPlaceholderValue(certificatePath))
        {
            throw new InvalidOperationException(
                "DataProtection:CertificatePath must point to a PKCS#12 certificate for non-Development environments.");
        }

        if (string.IsNullOrWhiteSpace(certificatePassphrase) && string.IsNullOrWhiteSpace(certificatePasswordFile))
        {
            throw new InvalidOperationException(
                "DataProtection certificate password must be supplied directly or through CertificatePasswordFile for non-Development environments.");
        }
    }

    private static void ValidateSensitiveDataProtection(WebApplication app)
    {
        var options = app.Services.GetRequiredService<IOptions<SensitiveDataProtectionOptions>>().Value;
        if (!options.RequireProtectedSensitiveColumns)
        {
            throw new InvalidOperationException(
                "Database:Encryption:RequireProtectedSensitiveColumns must be true for non-Development environments.");
        }

        if (!options.RequireEncryptedVolume || !options.EncryptedVolumeAttested)
        {
            throw new InvalidOperationException(
                "Database encrypted-volume protection must be required and explicitly attested for non-Development environments.");
        }
    }

    private static void ValidateCallbackSecurity(WebApplication app)
    {
        var options = app.Services.GetRequiredService<IOptions<CallbackSecurityOptions>>().Value;
        if (!options.RequireSharedKey)
        {
            throw new InvalidOperationException(
                "CallbackSecurity:RequireSharedKey must be true for non-Development environments.");
        }

        if (string.IsNullOrWhiteSpace(options.SharedKey))
        {
            throw new InvalidOperationException(
                "CallbackSecurity:SharedKey must be configured for non-Development environments.");
        }

        if (options.SharedKey.Trim().Length < 32 || IsPlaceholderValue(options.SharedKey))
        {
            throw new InvalidOperationException(
                "CallbackSecurity:SharedKey must be a non-placeholder value with at least 32 characters for non-Development environments.");
        }

        if (!options.RequireSignedRequests)
        {
            throw new InvalidOperationException(
                "CallbackSecurity:RequireSignedRequests must be true for non-Development environments.");
        }
    }

    private static void ValidateForwardedHeaders(IConfiguration configuration)
    {
        if (!configuration.GetValue<bool>("ForwardedHeaders:Enabled"))
            return;

        var knownProxies = configuration
            .GetSection("ForwardedHeaders:KnownProxies")
            .Get<string[]>() ?? [];

        if (knownProxies.Length == 0 || knownProxies.Any(IsPlaceholderValue))
        {
            throw new InvalidOperationException(
                "ForwardedHeaders:KnownProxies must list trusted reverse proxy IPs when ForwardedHeaders:Enabled is true outside Development.");
        }
    }

    private static void ValidateDeviceEgress(WebApplication app)
    {
        var options = app.Services.GetRequiredService<IOptions<ControlIdEgressOptions>>().Value;
        var allowedDeviceHosts = options.AllowedDeviceHosts
            .Where(static host => !string.IsNullOrWhiteSpace(host))
            .Select(static host => host.Trim())
            .ToArray();

        if (!options.RequireAllowedDeviceHosts ||
            allowedDeviceHosts.Length == 0 ||
            allowedDeviceHosts.Any(static host => host == "*"))
        {
            throw new InvalidOperationException(
                "ControlIDApi:RequireAllowedDeviceHosts must be true and ControlIDApi:AllowedDeviceHosts must list allowed device hosts for non-Development environments.");
        }

        if (allowedDeviceHosts.Any(IsPlaceholderValue))
        {
            throw new InvalidOperationException(
                "ControlIDApi:AllowedDeviceHosts must not contain placeholder values for non-Development environments.");
        }

        if (!options.RequireHttpsDeviceUrls)
        {
            throw new InvalidOperationException(
                "ControlIDApi:RequireHttpsDeviceUrls must be true for non-Development environments.");
        }
    }

    private static bool IsPlaceholderValue(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return true;

        var normalized = value.Trim();
        return normalized.Contains('<', StringComparison.Ordinal) ||
               normalized.Contains('>', StringComparison.Ordinal) ||
               normalized.Contains("placeholder", StringComparison.OrdinalIgnoreCase) ||
               normalized.Contains("changeme", StringComparison.OrdinalIgnoreCase) ||
               normalized.Contains("replace", StringComparison.OrdinalIgnoreCase) ||
               normalized.Contains("example", StringComparison.OrdinalIgnoreCase) ||
               normalized.Contains("localhost", StringComparison.OrdinalIgnoreCase);
    }
}
