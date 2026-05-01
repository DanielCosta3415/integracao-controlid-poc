using Integracao.ControlID.PoC.Services.Database;
using Integracao.ControlID.PoC.Models.Database;
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

    [Fact]
    public async Task DeliverNextAsync_ClaimsSinglePendingCommandOnceAcrossConcurrentPolls()
    {
        using var database = new FileSqliteTestDatabase();
        var commandId = Guid.NewGuid();
        await using (var setupContext = database.CreateContext())
        {
            var repository = new PushCommandRepository(setupContext, NullLogger<PushCommandRepository>.Instance);
            await repository.AddPushCommandAsync(new PushCommandLocal
            {
                CommandId = commandId,
                CommandType = "custom",
                DeviceId = "device-1",
                UserId = string.Empty,
                Payload = "{\"open\":true}",
                RawJson = "{\"open\":true}",
                Status = PushCommandStatuses.Pending,
                CreatedAt = DateTime.UtcNow.AddMinutes(-1)
            });
        }

        var pollTasks = Enumerable.Range(0, 8)
            .Select(_ => Task.Run(async () =>
            {
                await using var context = database.CreateContext();
                var service = new PushCommandWorkflowService(
                    new PushCommandRepository(context, NullLogger<PushCommandRepository>.Instance),
                    NullLogger<PushCommandWorkflowService>.Instance);

                return await service.DeliverNextAsync("device-1");
            }))
            .ToArray();

        var results = await Task.WhenAll(pollTasks);

        var delivered = Assert.Single(results, result => result != null);
        Assert.Equal(commandId, delivered!.CommandId);
        Assert.Equal(PushCommandStatuses.Delivered, delivered.Status);

        await using var verificationContext = database.CreateContext();
        var verificationRepository = new PushCommandRepository(verificationContext, NullLogger<PushCommandRepository>.Instance);
        var persisted = await verificationRepository.GetPushCommandByIdAsync(commandId);
        Assert.NotNull(persisted);
        Assert.Equal(PushCommandStatuses.Delivered, persisted.Status);
        Assert.Equal(0, await verificationRepository.CountPendingPushCommandsAsync());
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
