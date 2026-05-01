using Integracao.ControlID.PoC.Models.Database;
using Integracao.ControlID.PoC.Services.Database;
using Integracao.ControlID.PoC.Tests.TestSupport;
using Microsoft.Extensions.Logging.Abstractions;

namespace Integracao.ControlID.PoC.Tests.Services.Database;

public class MonitorEventRepositoryTests
{
    [Fact]
    public async Task GetAllMonitorEventsAsync_AppliesDefaultListLimit()
    {
        using var database = new SqliteTestDatabase();
        var repository = CreateRepository(database);

        for (var i = 0; i < LocalDataQueryLimits.DefaultListLimit + 5; i++)
        {
            await repository.AddMonitorEventAsync(CreateEvent("access"));
        }

        var result = await repository.GetAllMonitorEventsAsync();

        Assert.Equal(LocalDataQueryLimits.DefaultListLimit, result.Count);
    }

    [Fact]
    public async Task GetRecentMonitorEventsAsync_AppliesExplicitLimit()
    {
        using var database = new SqliteTestDatabase();
        var repository = CreateRepository(database);

        for (var i = 0; i < 6; i++)
        {
            await repository.AddMonitorEventAsync(CreateEvent("access-" + i));
        }

        var result = await repository.GetRecentMonitorEventsAsync(4);

        Assert.Equal(4, result.Count);
    }

    [Fact]
    public async Task DeleteMonitorEventsOlderThanAsync_RemovesOnlyEventsBeforeCutoff()
    {
        using var database = new SqliteTestDatabase();
        var repository = CreateRepository(database);
        var oldEvent = await repository.AddMonitorEventAsync(CreateEvent("old"));
        oldEvent.ReceivedAt = DateTime.UtcNow.AddDays(-10);
        await repository.UpdateMonitorEventAsync(oldEvent);
        var recentEvent = await repository.AddMonitorEventAsync(CreateEvent("recent"));

        var removedCount = await repository.DeleteMonitorEventsOlderThanAsync(DateTime.UtcNow.AddDays(-5));

        Assert.Equal(1, removedCount);
        Assert.Null(await repository.GetMonitorEventByIdAsync(oldEvent.EventId));
        Assert.NotNull(await repository.GetMonitorEventByIdAsync(recentEvent.EventId));
    }

    private static MonitorEventRepository CreateRepository(SqliteTestDatabase database)
    {
        return new MonitorEventRepository(database.Context, NullLogger<MonitorEventRepository>.Instance);
    }

    private static MonitorEventLocal CreateEvent(string eventType)
    {
        return new MonitorEventLocal
        {
            EventId = Guid.NewGuid(),
            EventType = eventType,
            RawJson = "{\"event\":\"" + eventType + "\"}",
            Payload = "{\"event\":\"" + eventType + "\"}",
            Status = "received"
        };
    }
}
