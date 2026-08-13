using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Integracao.ControlID.PoC.Services.Observability;

public sealed class RuntimeCapacityMetricsBackgroundService : BackgroundService
{
    private readonly IServiceProvider _services;
    private readonly ILogger<RuntimeCapacityMetricsBackgroundService> _logger;
    private readonly TimeSpan _interval;

    public RuntimeCapacityMetricsBackgroundService(
        IServiceProvider services,
        IConfiguration configuration,
        ILogger<RuntimeCapacityMetricsBackgroundService> logger)
    {
        _services = services;
        _logger = logger;
        var configuredSeconds = configuration.GetValue<int?>("Observability:CapacitySnapshotIntervalSeconds") ?? 300;
        _interval = TimeSpan.FromSeconds(Math.Clamp(configuredSeconds, 60, 3600));
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        RecordSnapshot();
        using var timer = new PeriodicTimer(_interval);

        while (await timer.WaitForNextTickAsync(stoppingToken))
            RecordSnapshot();
    }

    private void RecordSnapshot()
    {
        try
        {
            RuntimeCapacityMetricsProvider.RecordSnapshot(_services);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Não foi possível atualizar as métricas locais de capacidade.");
        }
    }
}
