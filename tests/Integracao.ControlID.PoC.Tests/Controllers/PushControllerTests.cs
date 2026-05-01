using System.Net;
using System.Text;
using Integracao.ControlID.PoC.Controllers;
using Integracao.ControlID.PoC.Options;
using Integracao.ControlID.PoC.Services.Callbacks;
using Integracao.ControlID.PoC.Services.Database;
using Integracao.ControlID.PoC.Services.Push;
using Integracao.ControlID.PoC.Tests.TestSupport;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace Integracao.ControlID.PoC.Tests.Controllers;

public class PushControllerTests
{
    [Fact]
    public async Task Receive_PersistsInvalidJsonAsLegacyReceivedEvent()
    {
        using var database = new SqliteTestDatabase();
        var controller = CreateController(database);
        controller.ControllerContext.HttpContext.Request.Body = new MemoryStream(Encoding.UTF8.GetBytes("not-json"));

        var result = await controller.Receive();

        Assert.IsType<OkObjectResult>(result);
        var command = Assert.Single(await CreateRepository(database).GetAllPushCommandsAsync());
        Assert.Equal("legacy_push_event", command.CommandType);
        Assert.Equal("received", command.Status);
        Assert.Equal("not-json", command.RawJson);
    }

    [Fact]
    public async Task Receive_UsesIdempotencyKeyToUpdateExistingLegacyEvent()
    {
        using var database = new SqliteTestDatabase();
        var controller = CreateController(database);
        controller.ControllerContext.HttpContext.Request.Headers["Idempotency-Key"] = "legacy-event-key";
        controller.ControllerContext.HttpContext.Request.Body = new MemoryStream(Encoding.UTF8.GetBytes("{\"event\":\"first\"}"));

        Assert.IsType<OkObjectResult>(await controller.Receive());

        controller.ControllerContext.HttpContext.Request.Body = new MemoryStream(Encoding.UTF8.GetBytes("{\"event\":\"second\"}"));
        Assert.IsType<OkObjectResult>(await controller.Receive());

        var command = Assert.Single(await CreateRepository(database).GetAllPushCommandsAsync());
        Assert.Equal("second", command.CommandType);
        Assert.Equal("{\"event\":\"second\"}", command.RawJson);
        Assert.NotNull(command.UpdatedAt);
    }

    [Fact]
    public async Task Receive_RejectsRequestWhenSharedKeyIsMissing()
    {
        using var database = new SqliteTestDatabase();
        var controller = CreateController(database, options =>
        {
            options.RequireSharedKey = true;
            options.SharedKey = "expected";
        });
        controller.ControllerContext.HttpContext.Request.Body = new MemoryStream(Encoding.UTF8.GetBytes("{}"));

        var result = await controller.Receive();

        var status = Assert.IsType<ObjectResult>(result);
        Assert.Equal(StatusCodes.Status401Unauthorized, status.StatusCode);
        Assert.Empty(await CreateRepository(database).GetAllPushCommandsAsync());
    }

    [Fact]
    public async Task Receive_RejectsOversizedBodyWhenContentLengthIsMissing()
    {
        using var database = new SqliteTestDatabase();
        var controller = CreateController(database, options => options.MaxBodyBytes = 8);
        controller.ControllerContext.HttpContext.Request.Body = new MemoryStream(Encoding.UTF8.GetBytes("0123456789"));

        var result = await controller.Receive();

        var status = Assert.IsType<ObjectResult>(result);
        Assert.Equal(StatusCodes.Status413PayloadTooLarge, status.StatusCode);
        Assert.Empty(await CreateRepository(database).GetAllPushCommandsAsync());
    }

    private static PushController CreateController(
        SqliteTestDatabase database,
        Action<CallbackSecurityOptions>? configure = null)
    {
        var options = new CallbackSecurityOptions();
        configure?.Invoke(options);

        var httpContext = new DefaultHttpContext();
        httpContext.Connection.RemoteIpAddress = IPAddress.Loopback;

        return new PushController(
            NullLogger<PushController>.Instance,
            new CallbackSecurityEvaluator(Microsoft.Extensions.Options.Options.Create(options)),
            new CallbackSignatureValidator(Microsoft.Extensions.Options.Options.Create(options), NullLogger<CallbackSignatureValidator>.Instance),
            CreateWorkflowService(database),
            new CallbackRequestBodyReader(Microsoft.Extensions.Options.Options.Create(options)),
            new PushIdempotencyKeyResolver())
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
}
