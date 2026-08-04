using System.Text.Json;
using Integracao.ControlID.PoC.Models.ControlIDApi;
using Integracao.ControlID.PoC.Services.ControlIDApi;
using Microsoft.AspNetCore.Http;

namespace Integracao.ControlID.PoC.Tests.Services.ControlIDApi;

public class OfficialObjectPagingTests
{
    [Fact]
    public void ApplyRequest_AddsBoundedLimitAndOffsetWithoutChangingFilters()
    {
        var request = OfficialObjectPaging.ApplyRequest(
            """{"object":"users","where":{"users":{"name":"Ada"}}}""",
            3);

        using var document = JsonDocument.Parse(request);
        Assert.Equal(OfficialObjectPaging.PageSize + 1, document.RootElement.GetProperty("limit").GetInt32());
        Assert.Equal(200, document.RootElement.GetProperty("offset").GetInt32());
        Assert.Equal("Ada", document.RootElement.GetProperty("where").GetProperty("users").GetProperty("name").GetString());
    }

    [Fact]
    public void ApplyResponse_TrimsLookaheadItemAndRecordsNavigationState()
    {
        var values = Enumerable.Range(1, OfficialObjectPaging.PageSize + 1)
            .Select(static id => new { id })
            .ToArray();
        using var source = JsonDocument.Parse(JsonSerializer.Serialize(new { users = values }));
        var context = new DefaultHttpContext();

        var result = OfficialObjectPaging.ApplyResponse(
            new OfficialApiJsonPayload(source.RootElement.Clone()),
            2,
            context);

        Assert.Equal(OfficialObjectPaging.PageSize, result.RootElement.GetProperty("users").GetArrayLength());
        Assert.Equal(2, context.Items[OfficialObjectPaging.CurrentPageItemKey]);
        Assert.Equal(true, context.Items[OfficialObjectPaging.HasNextPageItemKey]);
    }
}
