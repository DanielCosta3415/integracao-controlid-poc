using Integracao.ControlID.PoC.Services.Observability;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;

namespace Integracao.ControlID.PoC.Tests.Services.Observability;

public class OperationalMetricsTests
{
    [Fact]
    public void RecordHttpRequest_CapturesPrometheusSafePathWithoutRawIdentifiers()
    {
        OperationalMetrics.ResetForTests();

        OperationalMetrics.RecordHttpRequest(
            "GET",
            "/Users/Details/123?session=secret",
            StatusCodes.Status500InternalServerError,
            12.34);

        var snapshot = OperationalMetrics.CaptureSnapshot();
        var counter = Assert.Single(snapshot.Counters, metric => metric.Name == "controlid.http.requests");
        var histogram = Assert.Single(snapshot.Histograms, metric => metric.Name == "controlid.http.request.duration");

        Assert.Equal("/users/details/{id}", counter.Tags["path"]);
        Assert.Equal("5xx", counter.Tags["status_group"]);
        Assert.Equal(1, counter.Value);
        Assert.Equal(1, histogram.Count);
        Assert.DoesNotContain("123", PrometheusMetricsWriter.Format(snapshot), StringComparison.Ordinal);
        Assert.DoesNotContain("secret", PrometheusMetricsWriter.Format(snapshot), StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void PrometheusFormat_RendersCountersAndDurationSummary()
    {
        OperationalMetrics.ResetForTests();

        OperationalMetrics.RecordPushOperation("poll", "empty");
        OperationalMetrics.RecordOfficialApiInvocation("system-information", "POST", "success", StatusCodes.Status200OK, 8.5);

        var prometheus = PrometheusMetricsWriter.Format(OperationalMetrics.CaptureSnapshot());

        Assert.Contains("controlid_push_operations_total{operation=\"poll\",outcome=\"empty\"} 1", prometheus);
        Assert.Contains("controlid_official_api_invocations_total", prometheus);
        Assert.Contains("controlid_official_api_duration_milliseconds_count", prometheus);
        Assert.Contains("controlid_observability_snapshot_unix_time_seconds", prometheus);
    }

    [Fact]
    public void RecordProductFlow_CapturesPrivacySafeProductLabels()
    {
        OperationalMetrics.ResetForTests();

        OperationalMetrics.RecordProductFlow(
            "official_api",
            "official_endpoint_invoked",
            "submit",
            StatusCodes.Status200OK,
            42.5);

        var prometheus = PrometheusMetricsWriter.Format(OperationalMetrics.CaptureSnapshot());

        Assert.Contains("controlid_product_flow_events_total", prometheus);
        Assert.Contains("event=\"official_endpoint_invoked\"", prometheus);
        Assert.Contains("flow=\"official_api\"", prometheus);
        Assert.Contains("outcome=\"success\"", prometheus);
        Assert.Contains("controlid_product_flow_duration_milliseconds_count", prometheus);
        Assert.DoesNotContain("user_id", prometheus, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("session", prometheus, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("password", prometheus, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void RuntimeCapacityMetrics_CaptureSafeStorageAndProcessGaugesWithoutPaths()
    {
        OperationalMetrics.ResetForTests();

        var root = Directory.CreateTempSubdirectory("controlid-capacity-test-").FullName;
        try
        {
            var databasePath = Path.Combine(root, "integracao_controlid.db");
            File.WriteAllBytes(databasePath, new byte[] { 1, 2, 3 });
            var logsDirectory = Directory.CreateDirectory(Path.Combine(root, "Logs")).FullName;
            File.WriteAllText(Path.Combine(logsDirectory, "app_log.txt"), "safe log");

            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["ConnectionStrings:DefaultConnection"] = $"Data Source={databasePath}",
                    ["Logging:File:Path"] = logsDirectory
                })
                .Build();

            RuntimeCapacityMetricsProvider.RecordSnapshot(new CapacityTestServiceProvider(
                configuration,
                new CapacityTestHostEnvironment(root)));

            var prometheus = PrometheusMetricsWriter.Format(OperationalMetrics.CaptureSnapshot());

            Assert.Contains("controlid_runtime_process_memory_bytes", prometheus);
            Assert.Contains("controlid_runtime_managed_heap_bytes", prometheus);
            Assert.Contains("controlid_runtime_storage_local_bytes{scope=\"sqlite\"}", prometheus);
            Assert.Contains("controlid_runtime_storage_local_bytes{scope=\"logs\"}", prometheus);
            Assert.Contains("controlid_runtime_disk_free_percent", prometheus);
            Assert.DoesNotContain(root, prometheus, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("integracao_controlid.db", prometheus, StringComparison.OrdinalIgnoreCase);
            Assert.DoesNotContain("app_log.txt", prometheus, StringComparison.OrdinalIgnoreCase);
        }
        finally
        {
            Directory.Delete(root, recursive: true);
        }
    }

    private sealed class CapacityTestServiceProvider : IServiceProvider
    {
        private readonly IConfiguration _configuration;
        private readonly IHostEnvironment _environment;

        public CapacityTestServiceProvider(IConfiguration configuration, IHostEnvironment environment)
        {
            _configuration = configuration;
            _environment = environment;
        }

        public object? GetService(Type serviceType)
        {
            if (serviceType == typeof(IConfiguration))
                return _configuration;

            if (serviceType == typeof(IHostEnvironment))
                return _environment;

            return null;
        }
    }

    private sealed class CapacityTestHostEnvironment : IHostEnvironment
    {
        public CapacityTestHostEnvironment(string contentRootPath)
        {
            ContentRootPath = contentRootPath;
        }

        public string EnvironmentName { get; set; } = Environments.Development;
        public string ApplicationName { get; set; } = "Integracao.ControlID.PoC.Tests";
        public string ContentRootPath { get; set; }
        public IFileProvider ContentRootFileProvider { get; set; } = new NullFileProvider();
    }
}
