using Integracao.ControlID.PoC.Options;
using Integracao.ControlID.PoC.Services.Database;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;

namespace Integracao.ControlID.PoC.Services.Observability;

public sealed class SensitiveDataProtectionHealthCheck : IHealthCheck
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly SensitiveDataProtectionOptions _options;
    private readonly SensitiveDataProtectionVerificationState _verificationState;

    public SensitiveDataProtectionHealthCheck(
        IServiceScopeFactory scopeFactory,
        SensitiveDataProtectionVerificationState verificationState,
        IOptions<SensitiveDataProtectionOptions> options)
    {
        _scopeFactory = scopeFactory;
        _verificationState = verificationState;
        _options = options.Value;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        if (!_options.RequireProtectedSensitiveColumns)
            return HealthCheckResult.Healthy("Sensitive-column protection is optional in this environment.");

        var cacheDuration = TimeSpan.FromSeconds(Math.Clamp(_options.VerificationCacheSeconds, 30, 3600));
        if (_verificationState.IsFresh(cacheDuration))
            return HealthCheckResult.Healthy("Sensitive columns are protected (cached verification).");

        try
        {
            using var scope = _scopeFactory.CreateScope();
            var store = scope.ServiceProvider.GetRequiredService<SensitiveDataProtectionStore>();
            var hasUnprotectedValues = await store.HasUnprotectedValuesAsync(cancellationToken);

            if (!hasUnprotectedValues)
                _verificationState.MarkVerified();
            else
                _verificationState.Invalidate();

            return !hasUnprotectedValues
                ? HealthCheckResult.Healthy("Sensitive columns are protected.")
                : HealthCheckResult.Unhealthy("Legacy plaintext values remain in sensitive columns.");
        }
        catch (Exception exception)
        {
            return HealthCheckResult.Unhealthy(
                "Sensitive-column protection could not be verified.",
                exception);
        }
    }
}
