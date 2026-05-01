using Integracao.ControlID.PoC.Services.Performance;
using Microsoft.AspNetCore.Http;

namespace Integracao.ControlID.PoC.Tests.Services.Performance;

public class ServerTimingHeaderWriterTests
{
    [Fact]
    public void AppendMetric_AddsServerTimingMetricWithInvariantMilliseconds()
    {
        var context = new DefaultHttpContext();

        ServerTimingHeaderWriter.AppendMetric(
            context.Response,
            "dashboard-local-metrics",
            TimeSpan.FromMilliseconds(12.3456));

        Assert.Equal("dashboard-local-metrics;dur=12.346", context.Response.Headers["Server-Timing"].ToString());
    }

    [Fact]
    public void FormatMetric_UsesInvariantDecimalSeparator()
    {
        var result = ServerTimingHeaderWriter.FormatMetric("metric", TimeSpan.FromMilliseconds(1.5));

        Assert.Equal("metric;dur=1.5", result);
    }
}
