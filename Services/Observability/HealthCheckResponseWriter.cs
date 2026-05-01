using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Integracao.ControlID.PoC.Services.Observability;

public static class HealthCheckResponseWriter
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web)
    {
        WriteIndented = false
    };

    public static async Task WriteAsync(HttpContext context, HealthReport report)
    {
        context.Response.ContentType = "application/json; charset=utf-8";

        var payload = new
        {
            status = report.Status.ToString(),
            totalDurationMs = Math.Round(report.TotalDuration.TotalMilliseconds, 2),
            checks = report.Entries
                .OrderBy(entry => entry.Key, StringComparer.Ordinal)
                .ToDictionary(
                    entry => entry.Key,
                    entry => new
                    {
                        status = entry.Value.Status.ToString(),
                        durationMs = Math.Round(entry.Value.Duration.TotalMilliseconds, 2),
                        tags = entry.Value.Tags.OrderBy(tag => tag, StringComparer.Ordinal).ToArray()
                    },
                    StringComparer.Ordinal)
        };

        await JsonSerializer.SerializeAsync(
            context.Response.Body,
            payload,
            SerializerOptions,
            context.RequestAborted);
    }
}
