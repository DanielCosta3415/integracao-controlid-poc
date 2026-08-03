using Integracao.ControlID.PoC.Data;
using Integracao.ControlID.PoC.Services.Observability;
using Integracao.ControlID.PoC.Tests.TestSupport;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Integracao.ControlID.PoC.Tests.Services.Observability;

public class SqliteReadinessHealthCheckTests
{
    [Fact]
    public async Task CheckHealthAsync_ReturnsHealthy_WhenSqliteCanConnect()
    {
        using var database = new SqliteTestDatabase();
        database.Context.Database.EnsureDeleted();
        database.Context.Database.Migrate();
        var services = new ServiceCollection();
        services.AddDbContext<IntegracaoControlIDContext>(options => options.UseSqlite(database.Connection));

        await using var provider = services.BuildServiceProvider();
        var check = new SqliteReadinessHealthCheck(provider.GetRequiredService<IServiceScopeFactory>());

        var result = await check.CheckHealthAsync(new HealthCheckContext(), TestContext.Current.CancellationToken);

        Assert.Equal(HealthStatus.Healthy, result.Status);
    }

    [Fact]
    public async Task CheckHealthAsync_ReturnsUnhealthy_WhenSchemaHasPendingMigrations()
    {
        using var database = new SqliteTestDatabase();
        var services = new ServiceCollection();
        services.AddDbContext<IntegracaoControlIDContext>(options => options.UseSqlite(database.Connection));

        await using var provider = services.BuildServiceProvider();
        var check = new SqliteReadinessHealthCheck(provider.GetRequiredService<IServiceScopeFactory>());

        var result = await check.CheckHealthAsync(new HealthCheckContext(), TestContext.Current.CancellationToken);

        Assert.Equal(HealthStatus.Unhealthy, result.Status);
        Assert.Contains("pending migration", result.Description, StringComparison.OrdinalIgnoreCase);
    }
}
