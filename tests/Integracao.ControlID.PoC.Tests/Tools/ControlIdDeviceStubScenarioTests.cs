extern alias ControlIdDeviceStub;

using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using StubRequestStatistics = ControlIdDeviceStub::StubRequestStatistics;
using StubRuntimeState = ControlIdDeviceStub::StubRuntimeState;
using StubScenario = ControlIdDeviceStub::StubScenario;
using StubScenarioResponder = ControlIdDeviceStub::StubScenarioResponder;

namespace Integracao.ControlID.PoC.Tests.Tools;

public sealed class ControlIdDeviceStubScenarioTests
{
    [Theory]
    [InlineData("slow")]
    [InlineData("timeout")]
    [InlineData("rate-limited")]
    [InlineData("invalid-json")]
    [InlineData("oversized-response")]
    [InlineData("network-drop")]
    public void Create_accepts_documented_scenarios(string name)
    {
        var scenario = StubScenario.Create(name, 25, "/load_objects.fcgi", 2_000_000);

        Assert.Equal(name, scenario.Name);
        Assert.Equal(25, scenario.DelayMs);
        Assert.Equal("/load_objects.fcgi", scenario.Endpoint);
        Assert.True(scenario.AppliesTo("/load_objects.fcgi"));
        Assert.False(scenario.AppliesTo("/login.fcgi"));
    }

    [Fact]
    public void Create_rejects_unknown_scenario()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => StubScenario.Create("unknown", null, null, null));
    }

    [Fact]
    public void Reset_builds_deterministic_dataset_and_profile()
    {
        var runtime = new StubRuntimeState();
        runtime.Reset(100, "idflex");

        var payload = new JsonObject
        {
            ["object"] = "users",
            ["limit"] = 101,
            ["offset"] = 0
        };
        var response = runtime.Device.LoadObjects(payload);
        using var document = JsonDocument.Parse(JsonSerializer.Serialize(response));

        Assert.Equal(100, document.RootElement.GetProperty("users").GetArrayLength());
        Assert.Equal("idflex", runtime.Profile.Name);
        Assert.Equal(100, runtime.DatasetSize);
    }

    [Fact]
    public void Statistics_are_safe_under_concurrent_updates()
    {
        var statistics = new StubRequestStatistics();

        Parallel.For(0, 1_000, index => statistics.Record(TimeSpan.FromMilliseconds(index % 10)));
        using var document = JsonDocument.Parse(JsonSerializer.Serialize(statistics.Snapshot()));

        Assert.Equal(1_000, document.RootElement.GetProperty("count").GetInt64());
        Assert.Equal(9, document.RootElement.GetProperty("max_ms").GetDouble());
    }

    [Fact]
    public async Task Session_expired_scenario_returns_invalid_session_contract()
    {
        var runtime = new StubRuntimeState();
        runtime.ConfigureScenario("session-expired");
        var context = new DefaultHttpContext
        {
            Response = { Body = new MemoryStream() }
        };
        context.RequestServices = new ServiceCollection()
            .AddLogging()
            .ConfigureHttpJsonOptions(static _ => { })
            .BuildServiceProvider();

        var result = await StubScenarioResponder.TryCreateAsync(context, runtime, "/session_is_valid.fcgi");

        Assert.NotNull(result);
        await result.ExecuteAsync(context);
        context.Response.Body.Position = 0;
        using var document = await JsonDocument.ParseAsync(
            context.Response.Body,
            cancellationToken: TestContext.Current.CancellationToken);
        Assert.False(document.RootElement.GetProperty("session_is_valid").GetBoolean());
    }
}
