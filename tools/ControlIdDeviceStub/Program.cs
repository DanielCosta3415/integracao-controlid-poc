var builder = WebApplication.CreateBuilder(args);
builder.WebHost.UseUrls(
    Environment.GetEnvironmentVariable("CONTROLID_STUB_URL") ??
    builder.Configuration["Stub:Url"] ??
    "http://127.0.0.1:6600");

var app = builder.Build();
var runtime = new StubRuntimeState();

StubManagementEndpoints.Map(app, runtime);

app.MapMethods("/{**path}", ["GET", "POST"], async (HttpContext context) =>
{
    var request = context.Request;
    var path = request.Path.Value?.ToLowerInvariant() ?? "/";
    var startedAt = TimeProvider.System.GetTimestamp();

    try
    {
        var scenarioResult = await StubScenarioResponder.TryCreateAsync(context, runtime, path);
        if (scenarioResult != null)
            return scenarioResult;

        var bodyJson = await StubRequestBodyReader.ReadJsonAsync(request, context.RequestAborted);
        lock (runtime.Device)
            return StubEndpointRouter.Route(request, path, bodyJson, runtime);
    }
    finally
    {
        runtime.RecordRequest(path, TimeProvider.System.GetElapsedTime(startedAt));
        Console.WriteLine($"[{DateTime.Now:HH:mm:ss}] {request.Method} {path}{request.QueryString} scenario={runtime.Scenario.Name}");
    }
});

app.Run();
