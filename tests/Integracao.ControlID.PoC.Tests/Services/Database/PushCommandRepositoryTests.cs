using Integracao.ControlID.PoC.Models.Database;
using Integracao.ControlID.PoC.Services.Database;
using Integracao.ControlID.PoC.Tests.TestSupport;
using Microsoft.Extensions.Logging.Abstractions;

namespace Integracao.ControlID.PoC.Tests.Services.Database;

public class PushCommandRepositoryTests
{
    [Fact]
    public async Task GetNextPendingCommandAsync_ReturnsOldestPendingCommand_ForTargetDeviceOrGlobalQueue()
    {
        using var database = new SqliteTestDatabase();
        var repository = CreateRepository(database);
        var olderGlobal = CreateCommand("global", string.Empty, DateTime.UtcNow.AddMinutes(-10), "pending");
        var newerTarget = CreateCommand("target", "device-1", DateTime.UtcNow.AddMinutes(-5), "pending");
        var otherDevice = CreateCommand("other", "device-2", DateTime.UtcNow.AddMinutes(-20), "pending");

        await repository.AddPushCommandAsync(otherDevice);
        await repository.AddPushCommandAsync(newerTarget);
        await repository.AddPushCommandAsync(olderGlobal);

        var result = await repository.GetNextPendingCommandAsync("device-1");

        Assert.NotNull(result);
        Assert.Equal(olderGlobal.CommandId, result.CommandId);
    }

    [Fact]
    public async Task GetNextPendingCommandAsync_IgnoresAlreadyDeliveredCommands()
    {
        using var database = new SqliteTestDatabase();
        var repository = CreateRepository(database);
        var delivered = CreateCommand("delivered", "device-1", DateTime.UtcNow.AddMinutes(-10), "delivered");
        var pending = CreateCommand("pending", "device-1", DateTime.UtcNow.AddMinutes(-5), "pending");

        await repository.AddPushCommandAsync(delivered);
        await repository.AddPushCommandAsync(pending);

        var result = await repository.GetNextPendingCommandAsync("device-1");

        Assert.NotNull(result);
        Assert.Equal(pending.CommandId, result.CommandId);
    }

    [Fact]
    public async Task SearchPushCommandsAsync_FiltersByStatusAndPeriod()
    {
        using var database = new SqliteTestDatabase();
        var repository = CreateRepository(database);
        var now = DateTime.UtcNow;
        var pending = CreateCommand("pending", "device-1", now.AddMinutes(-5), "pending");
        var completed = CreateCommand("completed", "device-1", now.AddMinutes(-1), "completed");

        await repository.AddPushCommandAsync(pending);
        await repository.AddPushCommandAsync(completed);

        var result = await repository.SearchPushCommandsAsync(
            status: "completed",
            startDate: now.AddMinutes(-2),
            endDate: now.AddMinutes(2));

        var item = Assert.Single(result);
        Assert.Equal(completed.CommandId, item.CommandId);
    }

    private static PushCommandRepository CreateRepository(SqliteTestDatabase database)
    {
        return new PushCommandRepository(database.Context, NullLogger<PushCommandRepository>.Instance);
    }

    private static PushCommandLocal CreateCommand(string type, string deviceId, DateTime createdAt, string status)
    {
        return new PushCommandLocal
        {
            CommandId = Guid.NewGuid(),
            CommandType = type,
            DeviceId = deviceId,
            UserId = string.Empty,
            Payload = "{\"type\":\"" + type + "\"}",
            RawJson = "{\"type\":\"" + type + "\"}",
            Status = status,
            CreatedAt = createdAt
        };
    }
}
