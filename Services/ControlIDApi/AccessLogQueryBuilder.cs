namespace Integracao.ControlID.PoC.Services.ControlIDApi;

public static class AccessLogQueryBuilder
{
    public static object Build(
        long? id = null,
        long? userId = null,
        long? deviceId = null,
        int? eventCode = null,
        DateTime? startDate = null,
        DateTime? endDate = null)
    {
        var filters = new Dictionary<string, object>();
        Add(filters, "id", id);
        Add(filters, "user_id", userId);
        Add(filters, "device_id", deviceId);
        Add(filters, "event", eventCode);

        var timeRange = new Dictionary<string, long>();
        if (startDate.HasValue)
        {
            timeRange[">="] = new DateTimeOffset(startDate.Value).ToUnixTimeSeconds();
        }

        if (endDate.HasValue)
        {
            timeRange["<"] = new DateTimeOffset(endDate.Value.Date.AddDays(1)).ToUnixTimeSeconds();
        }

        if (timeRange.Count > 0)
            filters["time"] = timeRange;

        return new
        {
            @object = "access_logs",
            fields = new[] { "id", "time", "event", "device_id", "user_id", "portal_id" },
            where = new Dictionary<string, object> { ["access_logs"] = filters },
            order = new[] { "time", "descending" }
        };
    }

    private static void Add<T>(Dictionary<string, object> filters, string name, T? value)
        where T : struct
    {
        if (value.HasValue)
            filters[name] = value.Value;
    }
}
