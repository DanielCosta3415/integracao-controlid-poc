using Integracao.ControlID.PoC.Services.Observability;
using Microsoft.AspNetCore.Http;

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
}
