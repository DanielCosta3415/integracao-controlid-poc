internal static class StubManagementEndpoints
{
    public static void Map(WebApplication app, StubRuntimeState runtime)
    {
        app.MapGet("/__stub/status", () => Results.Json(runtime.CreateStatus()));
        app.MapGet("/__stub/catalog", () => Results.Json(new
        {
            scenarios = StubScenario.Names,
            profiles = StubDeviceProfile.Names,
            dataset_sizes = StubDatasetFactory.SupportedSizes
        }));

        app.MapPost("/__stub/scenario", (StubScenarioRequest request) =>
        {
            try
            {
                runtime.ConfigureScenario(request.Name, request.DelayMs, request.Endpoint, request.ResponseBytes);
                return Results.Json(runtime.CreateStatus());
            }
            catch (ArgumentOutOfRangeException ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
        });

        app.MapPost("/__stub/reset", (StubResetRequest request) =>
        {
            try
            {
                runtime.Reset(request.DatasetSize, request.Profile);
                return Results.Json(runtime.CreateStatus());
            }
            catch (ArgumentOutOfRangeException ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
        });
    }
}

internal sealed record StubScenarioRequest(string? Name, int? DelayMs, string? Endpoint, int? ResponseBytes);
internal sealed record StubResetRequest(int DatasetSize = 1, string? Profile = "idface");
