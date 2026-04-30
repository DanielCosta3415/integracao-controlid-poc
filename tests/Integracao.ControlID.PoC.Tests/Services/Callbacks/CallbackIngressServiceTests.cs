using System.Net;
using System.Text;
using Integracao.ControlID.PoC.Options;
using Integracao.ControlID.PoC.Services.Callbacks;
using Integracao.ControlID.PoC.Services.Database;
using Integracao.ControlID.PoC.Tests.TestSupport;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;

namespace Integracao.ControlID.PoC.Tests.Services.Callbacks;

public class CallbackIngressServiceTests
{
    [Fact]
    public async Task PersistAsync_PersistsAcceptedCallbackWithFamilyPathAndQueryMetadata()
    {
        using var database = new SqliteTestDatabase();
        var service = CreateService(database);
        var context = CreateHttpContext(
            "/new_user_identified.fcgi",
            "{\"event\":14}",
            "?device_id=device-1&user_id=101");

        var result = await service.PersistAsync(context, "identification");

        Assert.True(result.Accepted);
        Assert.Equal(StatusCodes.Status200OK, result.StatusCode);

        var repository = CreateRepository(database);
        var events = await repository.GetAllMonitorEventsAsync();
        var item = Assert.Single(events);
        Assert.Equal(result.EventId, item.EventId);
        Assert.Equal("identification:/new_user_identified.fcgi", item.EventType);
        Assert.Equal("device-1", item.DeviceId);
        Assert.Equal("101", item.UserId);
        Assert.Equal("received", item.Status);
        Assert.Equal("{\"event\":14}", item.Payload);
    }

    [Fact]
    public async Task PersistAsync_RejectsUnauthorizedCallbackWithoutPersisting()
    {
        using var database = new SqliteTestDatabase();
        var service = CreateService(database, options =>
        {
            options.RequireSharedKey = true;
            options.SharedKey = "expected";
        });
        var context = CreateHttpContext("/new_card.fcgi", "{\"card\":1}");

        var result = await service.PersistAsync(context, "identification");

        Assert.False(result.Accepted);
        Assert.Equal(StatusCodes.Status401Unauthorized, result.StatusCode);

        var repository = CreateRepository(database);
        Assert.Empty(await repository.GetAllMonitorEventsAsync());
    }

    [Fact]
    public async Task PersistAsync_RejectsOversizedBodyWithoutPersisting()
    {
        using var database = new SqliteTestDatabase();
        var service = CreateService(database, options => options.MaxBodyBytes = 4);
        var context = CreateHttpContext("/new_card.fcgi", "payload-too-large");
        context.Request.ContentLength = null;

        var result = await service.PersistAsync(context, "identification");

        Assert.False(result.Accepted);
        Assert.Equal(StatusCodes.Status413PayloadTooLarge, result.StatusCode);

        var repository = CreateRepository(database);
        Assert.Empty(await repository.GetAllMonitorEventsAsync());
    }

    private static CallbackIngressService CreateService(
        SqliteTestDatabase database,
        Action<CallbackSecurityOptions>? configure = null)
    {
        var options = new CallbackSecurityOptions();
        configure?.Invoke(options);
        var optionsMonitor = Microsoft.Extensions.Options.Options.Create(options);

        return new CallbackIngressService(
            new CallbackSecurityEvaluator(optionsMonitor),
            new CallbackRequestBodyReader(optionsMonitor),
            CreateRepository(database),
            NullLogger<CallbackIngressService>.Instance);
    }

    private static MonitorEventRepository CreateRepository(SqliteTestDatabase database)
    {
        return new MonitorEventRepository(database.Context, NullLogger<MonitorEventRepository>.Instance);
    }

    private static DefaultHttpContext CreateHttpContext(string path, string body, string query = "")
    {
        var context = new DefaultHttpContext();
        context.Connection.RemoteIpAddress = IPAddress.Parse("127.0.0.1");
        context.Request.Path = path;
        context.Request.QueryString = new QueryString(query);
        context.Request.ContentType = "application/json";
        var bytes = Encoding.UTF8.GetBytes(body);
        context.Request.ContentLength = bytes.Length;
        context.Request.Body = new MemoryStream(bytes);
        return context;
    }
}
