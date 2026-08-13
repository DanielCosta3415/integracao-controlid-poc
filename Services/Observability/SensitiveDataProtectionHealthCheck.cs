using Integracao.ControlID.PoC.Options;
using Integracao.ControlID.PoC.Services.Database;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;

namespace Integracao.ControlID.PoC.Services.Observability;

public sealed class SensitiveDataProtectionHealthCheck : IHealthCheck
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly SensitiveDataProtectionOptions _options;

    public SensitiveDataProtectionHealthCheck(
        IServiceScopeFactory scopeFactory,
        IOptions<SensitiveDataProtectionOptions> options)
    {
        _scopeFactory = scopeFactory;
        _options = options.Value;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        if (!_options.RequireProtectedSensitiveColumns)
            return HealthCheckResult.Healthy("Sensitive-column protection is optional in this environment.");

        try
        {
            using var scope = _scopeFactory.CreateScope();
            var store = scope.ServiceProvider.GetRequiredService<SensitiveDataProtectionStore>();
            var unprotectedCount = await store.CountUnprotectedValuesAsync(cancellationToken);

            return unprotectedCount == 0
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
