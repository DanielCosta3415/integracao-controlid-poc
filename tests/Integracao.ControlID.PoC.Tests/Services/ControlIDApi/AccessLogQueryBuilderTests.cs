using System.Text.Json;
using Integracao.ControlID.PoC.Services.ControlIDApi;

namespace Integracao.ControlID.PoC.Tests.Services.ControlIDApi;

public sealed class AccessLogQueryBuilderTests
{
    [Fact]
    public void Build_PushesExactAndDateFiltersToOfficialAccessLogsQuery()
    {
        var start = new DateTime(2026, 8, 1, 0, 0, 0, DateTimeKind.Local);
        var end = new DateTime(2026, 8, 2, 0, 0, 0, DateTimeKind.Local);

        var payload = JsonSerializer.SerializeToElement(AccessLogQueryBuilder.Build(
            userId: 10,
            deviceId: 20,
            eventCode: 7,
            startDate: start,
            endDate: end));

        Assert.Equal("access_logs", payload.GetProperty("object").GetString());
        var filters = payload.GetProperty("where").GetProperty("access_logs");
        Assert.Equal(10, filters.GetProperty("user_id").GetInt64());
        Assert.Equal(20, filters.GetProperty("device_id").GetInt64());
        Assert.Equal(7, filters.GetProperty("event").GetInt32());
        Assert.Equal(new DateTimeOffset(start).ToUnixTimeSeconds(), filters.GetProperty("time").GetProperty(">=").GetInt64());
        Assert.Equal(new DateTimeOffset(end.AddDays(1)).ToUnixTimeSeconds(), filters.GetProperty("time").GetProperty("<").GetInt64());
        Assert.Equal("descending", payload.GetProperty("order")[1].GetString());
    }
}
