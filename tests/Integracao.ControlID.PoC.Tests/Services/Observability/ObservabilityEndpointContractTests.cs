namespace Integracao.ControlID.PoC.Tests.Services.Observability;

public class ObservabilityEndpointContractTests
{
    [Fact]
    public void Program_MapsMetricsEndpointBehindAdministratorAuthorization()
    {
        var program = ReadRepoFile("Program.cs");

        Assert.Contains("app.MapGet(\"/metrics\", PrometheusMetricsWriter.WriteAsync)", program);
        Assert.Contains("metricsEndpoint.RequireAuthorization(\"AdministratorOnly\")", program);
        Assert.Contains("Observability:Metrics:AllowAnonymous must be false", program);
    }

    [Fact]
    public void ObservabilityArtifacts_DefineExecutableAlertsAndDashboards()
    {
        var alerts = ReadRepoFile("docs", "observability", "alert-rules.json");
        var dashboard = ReadRepoFile("docs", "observability", "dashboard.json");
        var script = ReadRepoFile("tools", "observability-check.ps1");

        Assert.Contains("\"OBS-001\"", alerts);
        Assert.Contains("\"controlid_http_requests_total\"", alerts);
        Assert.Contains("\"hardware-contract\"", alerts);
        Assert.Contains("\"process-health\"", dashboard);
        Assert.Contains("\"controlid_official_api_invocations_total\"", dashboard);
        Assert.Contains("OfflineValidateOnly", script);
        Assert.Contains("RequireHardwareContract", script);
    }

    private static string ReadRepoFile(params string[] segments)
    {
        return File.ReadAllText(Path.Combine(FindRepositoryRoot(), Path.Combine(segments)));
    }

    private static string FindRepositoryRoot()
    {
        var current = new DirectoryInfo(AppContext.BaseDirectory);
        while (current is not null)
        {
            if (File.Exists(Path.Combine(current.FullName, "Integracao.ControlID.PoC.sln")))
            {
                return current.FullName;
            }

            current = current.Parent;
        }

        throw new DirectoryNotFoundException("Repository root was not found from the test output directory.");
    }
}
