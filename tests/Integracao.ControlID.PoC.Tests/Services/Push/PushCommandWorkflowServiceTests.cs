using Integracao.ControlID.PoC.Services.Database;
using Integracao.ControlID.PoC.Services.Push;
using Integracao.ControlID.PoC.Tests.TestSupport;
using Integracao.ControlID.PoC.ViewModels.Push;
using Microsoft.Extensions.Logging.Abstractions;

namespace Integracao.ControlID.PoC.Tests.Services.Push;

public class PushCommandWorkflowServiceTests
{
    [Fact]
    public async Task QueueAsync_RejectsInvalidJsonWithoutPersisting()
    {
        using var database = new SqliteTestDatabase();
        var service = CreateService(database);

        var result = await service.QueueAsync(new PushQueueCommandViewModel
        {
            DeviceId = "device-1",
            CommandType = "custom",
            Payload = "{invalid"
        });

        Assert.False(result.IsQueued);
        Assert.Null(result.Command);
        Assert.Empty(await CreateRepository(database).GetAllPushCommandsAsync());
    }

    [Fact]
    public async Task StoreLegacyEventAsync_ExtractsKnownJsonFields()
    {
        using var database = new SqliteTestDatabase();
        var service = CreateService(database);

        var command = await service.StoreLegacyEventAsync(
            """
            {
              "event": "door_open",
              "status": "received",
              "device_id": "device-1",
              "user_id": "42",
              "payload": { "door": 1 }
            }
            """);

        Assert.Equal("door_open", command.CommandType);
        Assert.Equal("received", command.Status);
        Assert.Equal("device-1", command.DeviceId);
        Assert.Equal("42", command.UserId);
        Assert.Equal("{ \"door\": 1 }", command.Payload);
    }

    [Fact]
    public async Task StoreResultAsync_UpdatesExistingCommand()
    {
        using var database = new SqliteTestDatabase();
        var service = CreateService(database);
        var queued = await service.QueueAsync(new PushQueueCommandViewModel
        {
            DeviceId = "device-1",
            CommandType = "custom",
            Payload = "{\"open\":true}"
        });

        var result = await service.StoreResultAsync(
            queued.Command!.CommandId,
            "{\"ok\":true}",
            "device-1",
            "42",
            null);

        Assert.Equal(queued.Command.CommandId, result.CommandId);
        Assert.Equal(PushCommandStatuses.Completed, result.Status);
        Assert.Equal("{\"ok\":true}", result.Payload);
        Assert.NotNull(result.UpdatedAt);
    }

    private static PushCommandWorkflowService CreateService(SqliteTestDatabase database)
    {
        return new PushCommandWorkflowService(
            CreateRepository(database),
            NullLogger<PushCommandWorkflowService>.Instance);
    }

    private static PushCommandRepository CreateRepository(SqliteTestDatabase database)
    {
        return new PushCommandRepository(database.Context, NullLogger<PushCommandRepository>.Instance);
    }
}
