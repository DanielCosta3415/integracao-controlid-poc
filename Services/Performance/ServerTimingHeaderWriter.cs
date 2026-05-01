using System.Globalization;
using Microsoft.AspNetCore.Http;

namespace Integracao.ControlID.PoC.Services.Performance;

public static class ServerTimingHeaderWriter
{
    public static void AppendMetric(HttpResponse response, string name, TimeSpan duration)
    {
        if (response.HasStarted || string.IsNullOrWhiteSpace(name))
            return;

        response.Headers.Append("Server-Timing", FormatMetric(name, duration));
    }

    public static string FormatMetric(string name, TimeSpan duration)
    {
        return $"{name};dur={duration.TotalMilliseconds.ToString("0.###", CultureInfo.InvariantCulture)}";
    }
}
