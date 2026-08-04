using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;

namespace Integracao.ControlID.PoC.Tests.Frontend;

public sealed class RenderedApplicationContractTests : IClassFixture<PocWebApplicationFactory>
{
    private readonly PocWebApplicationFactory _factory;

    public RenderedApplicationContractTests(PocWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task Register_RendersPasswordPolicyAndSecurityHeadersThroughRealPipeline()
    {
        using var client = _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });

        using var response = await client.GetAsync("/Auth/Register", TestContext.Current.CancellationToken);
        var html = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);

        response.EnsureSuccessStatusCode();
        Assert.Contains("Content-Security-Policy", response.Headers.Select(header => header.Key));
        Assert.Contains("no-store", response.Headers.CacheControl?.ToString(), StringComparison.Ordinal);
        Assert.Contains("data-val-length-min=\"12\"", html, StringComparison.Ordinal);
        Assert.Contains("data-val-length-max=\"128\"", html, StringComparison.Ordinal);
        Assert.Contains("id=\"mainContent\"", html, StringComparison.Ordinal);
        Assert.DoesNotContain(" onsubmit=", html, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task Readiness_UsesMigratedIsolatedDatabase()
    {
        using var client = _factory.CreateClient();

        using var response = await client.GetAsync("/health/ready", TestContext.Current.CancellationToken);

        response.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task OpenApi_RendersVersionedDocumentInIsolatedDevelopmentPipeline()
    {
        using var client = _factory.CreateClient();

        using var response = await client.GetAsync("/swagger/v1/swagger.json", TestContext.Current.CancellationToken);
        var document = await response.Content.ReadAsStringAsync(TestContext.Current.CancellationToken);

        response.EnsureSuccessStatusCode();
        Assert.Equal("application/json", response.Content.Headers.ContentType?.MediaType);
        Assert.Contains("\"title\": \"Integracao Control iD PoC\"", document, StringComparison.Ordinal);
        Assert.Contains("\"version\": \"v1\"", document, StringComparison.Ordinal);
        Assert.Contains("\"paths\"", document, StringComparison.Ordinal);
    }
}

public sealed class PocWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly string _directoryPath = Path.Combine(
        Path.GetTempPath(),
        "controlid-poc-web-tests-" + Guid.NewGuid().ToString("N"));

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        Directory.CreateDirectory(_directoryPath);
        var databasePath = Path.Combine(_directoryPath, "web-test.db");

        builder.UseEnvironment("Development");
        builder.ConfigureAppConfiguration((_, configuration) =>
        {
            configuration.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = $"Data Source={databasePath}",
                ["Session:CookieSecure"] = "SameAsRequest",
                ["CallbackSecurity:RequireSharedKey"] = "false",
                ["CallbackSecurity:RequireSignedRequests"] = "false",
                ["ControlIDApi:RequireAllowedDeviceHosts"] = "false",
                ["OpenApi:Enabled"] = "true"
            });
        });
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (!disposing || !Directory.Exists(_directoryPath))
            return;

        try
        {
            Directory.Delete(_directoryPath, recursive: true);
        }
        catch (IOException)
        {
        }
        catch (UnauthorizedAccessException)
        {
        }
    }
}
