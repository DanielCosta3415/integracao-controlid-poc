using System.Collections.Concurrent;
using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Globalization;

namespace Integracao.ControlID.PoC.Services.Observability;

public static class OperationalMetrics
{
    public const string MeterName = "Integracao.ControlID.PoC.Operations";

    private const string HttpRequestsMetricName = "controlid.http.requests";
    private const string HttpRequestDurationMetricName = "controlid.http.request.duration";
    private const string LocalAuthAttemptsMetricName = "controlid.local_auth.attempts";
    private const string OfficialApiInvocationsMetricName = "controlid.official_api.invocations";
    private const string OfficialApiDurationMetricName = "controlid.official_api.duration";
    private const string CallbackIngressMetricName = "controlid.callback.ingress";
    private const string PushOperationsMetricName = "controlid.push.operations";
    private const string ProductFlowEventsMetricName = "controlid.product.flow.events";
    private const string ProductFlowDurationMetricName = "controlid.product.flow.duration";

    private static readonly Meter Meter = new(MeterName, "1.0.0");
    private static readonly ConcurrentDictionary<string, CounterAccumulator> CounterSnapshots = new(StringComparer.Ordinal);
    private static readonly ConcurrentDictionary<string, HistogramAccumulator> HistogramSnapshots = new(StringComparer.Ordinal);
    private static readonly ConcurrentDictionary<string, GaugeAccumulator> GaugeSnapshots = new(StringComparer.Ordinal);

    private static readonly Counter<long> HttpRequests = Meter.CreateCounter<long>(
        HttpRequestsMetricName,
        unit: "{request}",
        description: "Completed HTTP requests by method, status group and endpoint path.");

    private static readonly Histogram<double> HttpRequestDurationMs = Meter.CreateHistogram<double>(
        HttpRequestDurationMetricName,
        unit: "ms",
        description: "HTTP request duration in milliseconds.");

    private static readonly Counter<long> LocalAuthAttempts = Meter.CreateCounter<long>(
        LocalAuthAttemptsMetricName,
        unit: "{attempt}",
        description: "Local authentication attempts by outcome and role.");

    private static readonly Counter<long> OfficialApiInvocations = Meter.CreateCounter<long>(
        OfficialApiInvocationsMetricName,
        unit: "{invocation}",
        description: "Outbound official Control iD API invocations by endpoint and outcome.");

    private static readonly Histogram<double> OfficialApiDurationMs = Meter.CreateHistogram<double>(
        OfficialApiDurationMetricName,
        unit: "ms",
        description: "Outbound official Control iD API duration in milliseconds.");

    private static readonly Counter<long> CallbackIngress = Meter.CreateCounter<long>(
        CallbackIngressMetricName,
        unit: "{event}",
        description: "Callback and monitor ingress events by family, path and outcome.");

    private static readonly Counter<long> PushOperations = Meter.CreateCounter<long>(
        PushOperationsMetricName,
        unit: "{operation}",
        description: "Push queue operations by operation and outcome.");

    private static readonly Counter<long> ProductFlowEvents = Meter.CreateCounter<long>(
        ProductFlowEventsMetricName,
        unit: "{event}",
        description: "Privacy-safe product flow events by flow, event, action and outcome.");

    private static readonly Histogram<double> ProductFlowDurationMs = Meter.CreateHistogram<double>(
        ProductFlowDurationMetricName,
        unit: "ms",
        description: "Privacy-safe product flow request duration in milliseconds.");

    public static void RecordHttpRequest(string method, string path, int statusCode, double elapsedMilliseconds)
    {
        var tags = CreateTags(
            ("method", NormalizeLabel(method)),
            ("path", NormalizePath(path)),
            ("status_group", BuildStatusGroup(statusCode)));

        HttpRequests.Add(1, tags);
        HttpRequestDurationMs.Record(elapsedMilliseconds, tags);
        IncrementCounter(HttpRequestsMetricName, tags, 1);
        RecordHistogram(HttpRequestDurationMetricName, tags, elapsedMilliseconds);
    }

    public static void RecordLocalAuth(string outcome, string role = "none")
    {
        var tags = CreateTags(
            ("outcome", NormalizeLabel(outcome)),
            ("role", NormalizeLabel(role)));

        LocalAuthAttempts.Add(1, tags);
        IncrementCounter(LocalAuthAttemptsMetricName, tags, 1);
    }

    public static void RecordOfficialApiInvocation(
        string endpointId,
        string method,
        string outcome,
        int? statusCode,
        double elapsedMilliseconds)
    {
        var tags = CreateTags(
            ("endpoint_id", NormalizeLabel(endpointId)),
            ("method", NormalizeLabel(method)),
            ("outcome", NormalizeLabel(outcome)),
            ("status_group", statusCode.HasValue ? BuildStatusGroup(statusCode.Value) : "none"));

        OfficialApiInvocations.Add(1, tags);
        OfficialApiDurationMs.Record(elapsedMilliseconds, tags);
        IncrementCounter(OfficialApiInvocationsMetricName, tags, 1);
        RecordHistogram(OfficialApiDurationMetricName, tags, elapsedMilliseconds);
    }

    public static void RecordCallbackIngress(string eventFamily, string path, string outcome, int statusCode)
    {
        var tags = CreateTags(
            ("event_family", NormalizeLabel(eventFamily)),
            ("path", NormalizePath(path)),
            ("outcome", NormalizeLabel(outcome)),
            ("status_group", BuildStatusGroup(statusCode)));

        CallbackIngress.Add(1, tags);
        IncrementCounter(CallbackIngressMetricName, tags, 1);
    }

    public static void RecordPushOperation(string operation, string outcome)
    {
        var tags = CreateTags(
            ("operation", NormalizeLabel(operation)),
            ("outcome", NormalizeLabel(outcome)));

        PushOperations.Add(1, tags);
        IncrementCounter(PushOperationsMetricName, tags, 1);
    }

    public static void RecordProductFlow(
        string flow,
        string eventName,
        string action,
        int statusCode,
        double elapsedMilliseconds)
    {
        var tags = CreateTags(
            ("flow", NormalizeLabel(flow)),
            ("event", NormalizeLabel(eventName)),
            ("action", NormalizeLabel(action)),
            ("outcome", BuildProductOutcome(statusCode)),
            ("status_group", BuildStatusGroup(statusCode)));

        ProductFlowEvents.Add(1, tags);
        ProductFlowDurationMs.Record(elapsedMilliseconds, tags);
        IncrementCounter(ProductFlowEventsMetricName, tags, 1);
        RecordHistogram(ProductFlowDurationMetricName, tags, elapsedMilliseconds);
    }

    public static OperationalMetricsSnapshot CaptureSnapshot()
    {
        return new OperationalMetricsSnapshot(
            DateTimeOffset.UtcNow,
            CounterSnapshots.Values
                .Select(static accumulator => accumulator.Capture())
                .OrderBy(static metric => metric.Name, StringComparer.Ordinal)
                .ThenBy(static metric => BuildTagSignature(metric.Tags), StringComparer.Ordinal)
                .ToArray(),
            HistogramSnapshots.Values
                .Select(static accumulator => accumulator.Capture())
                .OrderBy(static metric => metric.Name, StringComparer.Ordinal)
                .ThenBy(static metric => BuildTagSignature(metric.Tags), StringComparer.Ordinal)
                .ToArray(),
            GaugeSnapshots.Values
                .Select(static accumulator => accumulator.Capture())
                .OrderBy(static metric => metric.Name, StringComparer.Ordinal)
                .ThenBy(static metric => BuildTagSignature(metric.Tags), StringComparer.Ordinal)
                .ToArray());
    }

    public static void RecordGauge(string name, double value, params (string Name, string Value)[] tags)
    {
        var normalizedTags = ToTagDictionary(CreateTags(
            tags.Select(static tag => (tag.Name, NormalizeLabel(tag.Value))).ToArray()));
        var key = BuildSnapshotKey(name, normalizedTags);
        var accumulator = GaugeSnapshots.GetOrAdd(key, _ => new GaugeAccumulator(name, normalizedTags));
        accumulator.Record(value);
    }

    public static void ResetForTests()
    {
        CounterSnapshots.Clear();
        HistogramSnapshots.Clear();
        GaugeSnapshots.Clear();
    }

    private static void IncrementCounter(string name, TagList tags, long amount)
    {
        var normalizedTags = ToTagDictionary(tags);
        var key = BuildSnapshotKey(name, normalizedTags);
        var accumulator = CounterSnapshots.GetOrAdd(key, _ => new CounterAccumulator(name, normalizedTags));
        accumulator.Add(amount);
    }

    private static void RecordHistogram(string name, TagList tags, double value)
    {
        var normalizedTags = ToTagDictionary(tags);
        var key = BuildSnapshotKey(name, normalizedTags);
        var accumulator = HistogramSnapshots.GetOrAdd(key, _ => new HistogramAccumulator(name, normalizedTags));
        accumulator.Record(value);
    }

    private static TagList CreateTags(params (string Name, string Value)[] values)
    {
        var tags = new TagList();
        foreach (var (name, value) in values)
            tags.Add(name, value);

        return tags;
    }

    private static IReadOnlyDictionary<string, string> ToTagDictionary(TagList tags)
    {
        return tags
            .ToDictionary(
                static tag => tag.Key,
                static tag => Convert.ToString(tag.Value, CultureInfo.InvariantCulture) ?? "unknown",
                StringComparer.Ordinal);
    }

    private static string BuildSnapshotKey(string name, IReadOnlyDictionary<string, string> tags)
    {
        return $"{name}|{BuildTagSignature(tags)}";
    }

    private static string BuildTagSignature(IReadOnlyDictionary<string, string> tags)
    {
        return string.Join(
            "|",
            tags
                .OrderBy(static tag => tag.Key, StringComparer.Ordinal)
                .Select(static tag => $"{tag.Key}={tag.Value}"));
    }

    private static string BuildStatusGroup(int statusCode)
    {
        if (statusCode < 100 || statusCode > 599)
            return "unknown";

        return $"{statusCode / 100}xx";
    }

    private static string BuildProductOutcome(int statusCode)
    {
        if (statusCode is >= 200 and <= 299)
            return "success";

        if (statusCode is >= 300 and <= 399)
            return "redirect";

        if (statusCode is >= 400 and <= 499)
            return "blocked_or_invalid";

        if (statusCode is >= 500 and <= 599)
            return "server_error";

        return "unknown";
    }

    private static string NormalizePath(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return "unknown";

        var pathOnly = value.Trim();
        var queryStart = pathOnly.IndexOfAny(['?', '#']);
        if (queryStart >= 0)
            pathOnly = pathOnly[..queryStart];

        if (pathOnly == "/")
            return "/";

        var segments = pathOnly
            .Split('/', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(NormalizePathSegment)
            .Where(static segment => !string.IsNullOrWhiteSpace(segment))
            .ToArray();

        return segments.Length == 0 ? "/" : "/" + string.Join("/", segments);
    }

    private static string NormalizePathSegment(string value)
    {
        if (long.TryParse(value, NumberStyles.None, CultureInfo.InvariantCulture, out _) ||
            Guid.TryParse(value, out _))
        {
            return "{id}";
        }

        return NormalizeLabel(value, maxLength: 48);
    }

    private static string NormalizeLabel(string? value, int maxLength = 64)
    {
        if (string.IsNullOrWhiteSpace(value))
            return "unknown";

        var normalized = value.Trim().ToLowerInvariant();
        var buffer = new char[Math.Min(normalized.Length, maxLength)];

        for (var index = 0; index < buffer.Length; index++)
        {
            var character = normalized[index];
            buffer[index] = char.IsAsciiLetterOrDigit(character) || character is '-' or '_' or '.'
                ? character
                : '_';
        }

        var label = new string(buffer).Trim('_');
        return string.IsNullOrWhiteSpace(label) ? "unknown" : label;
    }

    private sealed class CounterAccumulator
    {
        private long _value;

        public CounterAccumulator(string name, IReadOnlyDictionary<string, string> tags)
        {
            Name = name;
            Tags = tags;
        }

        private string Name { get; }
        private IReadOnlyDictionary<string, string> Tags { get; }

        public void Add(long amount)
        {
            Interlocked.Add(ref _value, amount);
        }

        public CounterMetricSnapshot Capture()
        {
            return new CounterMetricSnapshot(Name, Tags, Interlocked.Read(ref _value));
        }
    }

    private sealed class HistogramAccumulator
    {
        private readonly object _sync = new();
        private long _count;
        private double _sum;
        private double _max;

        public HistogramAccumulator(string name, IReadOnlyDictionary<string, string> tags)
        {
            Name = name;
            Tags = tags;
        }

        private string Name { get; }
        private IReadOnlyDictionary<string, string> Tags { get; }

        public void Record(double value)
        {
            lock (_sync)
            {
                _count++;
                _sum += value;
                _max = Math.Max(_max, value);
            }
        }

        public HistogramMetricSnapshot Capture()
        {
            lock (_sync)
            {
                return new HistogramMetricSnapshot(Name, Tags, _count, _sum, _max);
            }
        }
    }

    private sealed class GaugeAccumulator
    {
        private readonly object _sync = new();
        private double _value;

        public GaugeAccumulator(string name, IReadOnlyDictionary<string, string> tags)
        {
            Name = name;
            Tags = tags;
        }

        private string Name { get; }
        private IReadOnlyDictionary<string, string> Tags { get; }

        public void Record(double value)
        {
            lock (_sync)
            {
                _value = value;
            }
        }

        public GaugeMetricSnapshot Capture()
        {
            lock (_sync)
            {
                return new GaugeMetricSnapshot(Name, Tags, _value);
            }
        }
    }
}

public sealed record OperationalMetricsSnapshot(
    DateTimeOffset CollectedAtUtc,
    IReadOnlyList<CounterMetricSnapshot> Counters,
    IReadOnlyList<HistogramMetricSnapshot> Histograms,
    IReadOnlyList<GaugeMetricSnapshot> Gauges);

public sealed record CounterMetricSnapshot(
    string Name,
    IReadOnlyDictionary<string, string> Tags,
    long Value);

public sealed record HistogramMetricSnapshot(
    string Name,
    IReadOnlyDictionary<string, string> Tags,
    long Count,
    double Sum,
    double Max);

public sealed record GaugeMetricSnapshot(
    string Name,
    IReadOnlyDictionary<string, string> Tags,
    double Value);
