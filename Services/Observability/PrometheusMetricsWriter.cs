using System.Globalization;
using System.Text;
using Microsoft.AspNetCore.Http;

namespace Integracao.ControlID.PoC.Services.Observability;

public static class PrometheusMetricsWriter
{
    /// <summary>
    /// Renders the current in-process metrics snapshot and refreshes local
    /// capacity gauges at scrape time to avoid background jobs in the PoC.
    /// </summary>
    public static async Task WriteAsync(HttpContext context)
    {
        context.Response.ContentType = "text/plain; version=0.0.4; charset=utf-8";
        RuntimeCapacityMetricsProvider.RecordSnapshot(context.RequestServices);
        var payload = Format(OperationalMetrics.CaptureSnapshot());
        await context.Response.WriteAsync(payload, context.RequestAborted);
    }

    /// <summary>
    /// Converts the sanitized in-memory metric snapshot to Prometheus text format.
    /// Labels must remain low-cardinality and free of personal data or secrets.
    /// </summary>
    public static string Format(OperationalMetricsSnapshot snapshot)
    {
        var builder = new StringBuilder();
        builder.AppendLine("# HELP controlid_observability_snapshot_unix_time_seconds Unix timestamp when the local metrics snapshot was rendered.");
        builder.AppendLine("# TYPE controlid_observability_snapshot_unix_time_seconds gauge");
        builder
            .Append("controlid_observability_snapshot_unix_time_seconds ")
            .AppendLine(snapshot.CollectedAtUtc.ToUnixTimeSeconds().ToString(CultureInfo.InvariantCulture));

        foreach (var group in snapshot.Counters.GroupBy(static metric => metric.Name, StringComparer.Ordinal))
        {
            var prometheusName = ToPrometheusName(group.Key) + "_total";
            builder.Append("# HELP ").Append(prometheusName).Append(" In-process counter for ").Append(group.Key).AppendLine(".");
            builder.Append("# TYPE ").Append(prometheusName).AppendLine(" counter");

            foreach (var metric in group)
            {
                builder
                    .Append(prometheusName)
                    .Append(FormatLabels(metric.Tags))
                    .Append(' ')
                    .AppendLine(metric.Value.ToString(CultureInfo.InvariantCulture));
            }
        }

        foreach (var group in snapshot.Histograms.GroupBy(static metric => metric.Name, StringComparer.Ordinal))
        {
            var prometheusName = ToPrometheusName(group.Key) + "_milliseconds";
            builder.Append("# HELP ").Append(prometheusName).Append(" In-process duration summary for ").Append(group.Key).AppendLine(".");
            builder.Append("# TYPE ").Append(prometheusName).AppendLine(" summary");

            foreach (var metric in group)
            {
                var labels = FormatLabels(metric.Tags);
                builder.Append(prometheusName).Append("_count").Append(labels).Append(' ')
                    .AppendLine(metric.Count.ToString(CultureInfo.InvariantCulture));
                builder.Append(prometheusName).Append("_sum").Append(labels).Append(' ')
                    .AppendLine(metric.Sum.ToString("0.###", CultureInfo.InvariantCulture));
                builder.Append(prometheusName).Append("_max").Append(labels).Append(' ')
                    .AppendLine(metric.Max.ToString("0.###", CultureInfo.InvariantCulture));
            }
        }

        foreach (var group in snapshot.Gauges.GroupBy(static metric => metric.Name, StringComparer.Ordinal))
        {
            var prometheusName = ToPrometheusName(group.Key);
            builder.Append("# HELP ").Append(prometheusName).Append(" In-process gauge for ").Append(group.Key).AppendLine(".");
            builder.Append("# TYPE ").Append(prometheusName).AppendLine(" gauge");

            foreach (var metric in group)
            {
                builder
                    .Append(prometheusName)
                    .Append(FormatLabels(metric.Tags))
                    .Append(' ')
                    .AppendLine(metric.Value.ToString("0.###", CultureInfo.InvariantCulture));
            }
        }

        return builder.ToString();
    }

    private static string ToPrometheusName(string name)
    {
        var builder = new StringBuilder(name.Length);
        foreach (var character in name)
        {
            builder.Append(char.IsAsciiLetterOrDigit(character) ? character : '_');
        }

        return builder.ToString();
    }

    private static string FormatLabels(IReadOnlyDictionary<string, string> tags)
    {
        if (tags.Count == 0)
            return string.Empty;

        var values = tags
            .OrderBy(static tag => tag.Key, StringComparer.Ordinal)
            .Select(static tag => $"{tag.Key}=\"{EscapeLabelValue(tag.Value)}\"");

        return "{" + string.Join(",", values) + "}";
    }

    private static string EscapeLabelValue(string value)
    {
        return value
            .Replace("\\", "\\\\", StringComparison.Ordinal)
            .Replace("\"", "\\\"", StringComparison.Ordinal)
            .Replace("\r", "\\n", StringComparison.Ordinal)
            .Replace("\n", "\\n", StringComparison.Ordinal);
    }
}
