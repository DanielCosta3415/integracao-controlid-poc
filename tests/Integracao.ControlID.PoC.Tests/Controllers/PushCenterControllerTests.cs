using System.Net;
using System.Text;
using Integracao.ControlID.PoC.Controllers;
using Integracao.ControlID.PoC.Helpers;
using Integracao.ControlID.PoC.Models.Database;
using Integracao.ControlID.PoC.Options;
using Integracao.ControlID.PoC.Services.Callbacks;
using Integracao.ControlID.PoC.Services.Database;
using Integracao.ControlID.PoC.Services.Push;
using Integracao.ControlID.PoC.Tests.TestSupport;
using Integracao.ControlID.PoC.ViewModels.Push;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace Integracao.ControlID.PoC.Tests.Controllers;

public class PushCenterControllerTests
{
    [Fact]
    public async Task Queue_RejectsInvalidJsonPayload()
    {
        using var database = new SqliteTestDatabase();
        var controller = CreateController(database);

        var result = await controller.Queue(new PushQueueCommandViewModel
        {
            DeviceId = "device-1",
            CommandType = "custom",
            Payload = "{invalid-json"
        });

        var view = Assert.IsType<ViewResult>(result);
        Assert.Equal("Index", view.ViewName);
        Assert.Empty(await CreateRepository(database).GetAllPushCommandsAsync());
    }

    [Fact]
    public async Task Queue_PersistsValidCommandAsPending()
    {
        using var database = new SqliteTestDatabase();
        var controller = CreateController(database);

        var result = await controller.Queue(new PushQueueCommandViewModel
        {
            DeviceId = "device-1",
            CommandType = "custom",
            Payload = "{\"actions\":[]}"
        });

        Assert.IsType<RedirectToActionResult>(result);
        var command = Assert.Single(await CreateRepository(database).GetAllPushCommandsAsync());
        Assert.Equal("pending", command.Status);
        Assert.Equal("device-1", command.DeviceId);
        Assert.Equal("{\"actions\":[]}", command.Payload);
    }

    [Fact]
    public async Task Poll_ReturnsOldestPayloadAndMarksCommandAsDelivered()
    {
        using var database = new SqliteTestDatabase();
        var repository = CreateRepository(database);
        var commandId = Guid.NewGuid();
        await repository.AddPushCommandAsync(new PushCommandLocal
        {
            CommandId = commandId,
            CommandType = "custom",
            DeviceId = "device-1",
            Payload = "{\"open\":true}",
            RawJson = "{\"open\":true}",
            Status = "pending",
            CreatedAt = DateTime.UtcNow.AddMinutes(-1)
        });
        var controller = CreateController(database);

        var result = await controller.Poll("device-1", null);

        var content = Assert.IsType<ContentResult>(result);
        Assert.Equal("application/json; charset=utf-8", content.ContentType);
        Assert.Equal("{\"open\":true}", content.Content);

        var updated = await repository.GetPushCommandByIdAsync(commandId);
        Assert.NotNull(updated);
        Assert.Equal("delivered", updated.Status);
    }

    [Fact]
    public async Task Result_CreatesCompletedRecordWhenCommandIdIsMissing()
    {
        using var database = new SqliteTestDatabase();
        var controller = CreateController(database);
        controller.ControllerContext.HttpContext.Request.QueryString = new QueryString("?device_id=device-1&user_id=101");
        controller.ControllerContext.HttpContext.Request.Body = new MemoryStream(Encoding.UTF8.GetBytes("{\"ok\":true}"));

        var result = await controller.Result(null);

        Assert.IsType<OkResult>(result);
        var command = Assert.Single(await CreateRepository(database).GetAllPushCommandsAsync());
        Assert.Equal("result", command.CommandType);
        Assert.Equal("completed", command.Status);
        Assert.Equal("device-1", command.DeviceId);
        Assert.Equal("101", command.UserId);
        Assert.Equal("{\"ok\":true}", command.Payload);
    }

    [Fact]
    public async Task Result_UsesIdempotencyKeyToUpdateExistingStandaloneResult()
    {
        using var database = new SqliteTestDatabase();
        var controller = CreateController(database);
        controller.ControllerContext.HttpContext.Request.Headers["Idempotency-Key"] = "result-key-1";
        controller.ControllerContext.HttpContext.Request.Body = new MemoryStream(Encoding.UTF8.GetBytes("{\"ok\":true}"));

        Assert.IsType<OkResult>(await controller.Result(null));

        controller.ControllerContext.HttpContext.Request.Body = new MemoryStream(Encoding.UTF8.GetBytes("{\"ok\":false}"));
        Assert.IsType<OkResult>(await controller.Result(null));

        var command = Assert.Single(await CreateRepository(database).GetAllPushCommandsAsync());
        Assert.Equal("result", command.CommandType);
        Assert.Equal("{\"ok\":false}", command.Payload);
        Assert.NotNull(command.UpdatedAt);
    }

    [Fact]
    public async Task Result_RejectsOversizedBodyWhenContentLengthIsMissing()
    {
        using var database = new SqliteTestDatabase();
        var controller = CreateController(database, options => options.MaxBodyBytes = 8);
        controller.ControllerContext.HttpContext.Request.Body = new MemoryStream(Encoding.UTF8.GetBytes("0123456789"));

        var result = await controller.Result(null);

        var status = Assert.IsType<ObjectResult>(result);
        Assert.Equal(StatusCodes.Status413PayloadTooLarge, status.StatusCode);
        Assert.Empty(await CreateRepository(database).GetAllPushCommandsAsync());
    }

    [Fact]
    public async Task Clear_KeepsCommandsWhenConfirmationIsInvalid()
    {
        using var database = new SqliteTestDatabase();
        var repository = CreateRepository(database);
        await repository.AddPushCommandAsync(new PushCommandLocal
        {
            CommandId = Guid.NewGuid(),
            CommandType = "custom",
            DeviceId = "device-1",
            Payload = "{\"open\":true}",
            RawJson = "{\"open\":true}",
            Status = "pending"
        });
        var controller = CreateController(database);

        var result = await controller.Clear("wrong");

        Assert.IsType<RedirectToActionResult>(result);
        Assert.Single(await repository.GetAllPushCommandsAsync());
    }

    [Fact]
    public async Task Clear_DeletesCommandsWhenConfirmationIsValid()
    {
        using var database = new SqliteTestDatabase();
        var repository = CreateRepository(database);
        await repository.AddPushCommandAsync(new PushCommandLocal
        {
            CommandId = Guid.NewGuid(),
            CommandType = "custom",
            DeviceId = "device-1",
            Payload = "{\"open\":true}",
            RawJson = "{\"open\":true}",
            Status = "pending"
        });
        var controller = CreateController(database);

        var result = await controller.Clear(HighImpactOperationGuard.ConfirmClearPushQueue);

        Assert.IsType<RedirectToActionResult>(result);
        Assert.Empty(await repository.GetAllPushCommandsAsync());
    }

    [Fact]
    public async Task Purge_KeepsCommandsWhenConfirmationIsInvalid()
    {
        using var database = new SqliteTestDatabase();
        var repository = CreateRepository(database);
        var command = await repository.AddPushCommandAsync(CreateCommand("custom", "device-1", "pending"));
        command.ReceivedAt = DateTime.UtcNow.AddDays(-40);
        await repository.UpdatePushCommandAsync(command);
        var controller = CreateController(database);

        var result = await controller.Purge(30, "wrong");

        Assert.IsType<RedirectToActionResult>(result);
        Assert.Single(await repository.GetAllPushCommandsAsync());
    }

    [Fact]
    public async Task Purge_DeletesOnlyCommandsOlderThanRetentionWindow()
    {
        using var database = new SqliteTestDatabase();
        var repository = CreateRepository(database);
        var oldCommand = await repository.AddPushCommandAsync(CreateCommand("old", "device-1", "completed"));
        oldCommand.ReceivedAt = DateTime.UtcNow.AddDays(-40);
        await repository.UpdatePushCommandAsync(oldCommand);
        await repository.AddPushCommandAsync(CreateCommand("new", "device-1", "completed"));
        var controller = CreateController(database);

        var result = await controller.Purge(30, HighImpactOperationGuard.ConfirmPurgePushQueue);

        Assert.IsType<RedirectToActionResult>(result);
        var remaining = await repository.GetAllPushCommandsAsync();
        var item = Assert.Single(remaining);
        Assert.NotEqual(oldCommand.CommandId, item.CommandId);
    }

    private static PushCenterController CreateController(
        SqliteTestDatabase database,
        Action<CallbackSecurityOptions>? configure = null)
    {
        var options = new CallbackSecurityOptions();
        configure?.Invoke(options);

        var httpContext = new DefaultHttpContext();
        httpContext.Connection.RemoteIpAddress = IPAddress.Loopback;

        return new PushCenterController(
            CreateWorkflowService(database),
            new CallbackSecurityEvaluator(Microsoft.Extensions.Options.Options.Create(options)),
            new CallbackSignatureValidator(Microsoft.Extensions.Options.Options.Create(options), NullLogger<CallbackSignatureValidator>.Instance),
            new CallbackRequestBodyReader(Microsoft.Extensions.Options.Options.Create(options)),
            new PushIdempotencyKeyResolver(),
            NullLogger<PushCenterController>.Instance)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            },
            TempData = new TempDataDictionary(httpContext, new DictionaryTempDataProvider())
        };
    }

    private static PushCommandRepository CreateRepository(SqliteTestDatabase database)
    {
        return new PushCommandRepository(database.Context, NullLogger<PushCommandRepository>.Instance);
    }

    private static PushCommandWorkflowService CreateWorkflowService(SqliteTestDatabase database)
    {
        return new PushCommandWorkflowService(
            CreateRepository(database),
            NullLogger<PushCommandWorkflowService>.Instance);
    }

    private static PushCommandLocal CreateCommand(string type, string deviceId, string status)
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
            CreatedAt = DateTime.UtcNow
        };
    }
}
