using System.Text.Json;
using System.Text.Json.Nodes;
using Integracao.ControlID.PoC.Models.ControlIDApi;
using Microsoft.AspNetCore.Http;

namespace Integracao.ControlID.PoC.Services.ControlIDApi;

public static class OfficialObjectPaging
{
    public const int PageSize = 100;
    public const string CurrentPageItemKey = "OfficialObjects.CurrentPage";
    public const string HasNextPageItemKey = "OfficialObjects.HasNextPage";

    public static int NormalizePage(int page) => Math.Clamp(page, 1, 10_000);

    public static string ApplyRequest(string requestBody, int page)
    {
        var root = JsonNode.Parse(string.IsNullOrWhiteSpace(requestBody) ? "{}" : requestBody) as JsonObject
            ?? throw new InvalidOperationException("O payload de consulta de objetos deve ser um objeto JSON.");

        var normalizedPage = NormalizePage(page);
        root["limit"] ??= PageSize + 1;
        root["offset"] ??= (normalizedPage - 1) * PageSize;
        return root.ToJsonString();
    }

    public static OfficialApiJsonPayload ApplyResponse(
        OfficialApiJsonPayload payload,
        int page,
        HttpContext httpContext)
    {
        var root = JsonSerializer.SerializeToNode(payload.RootElement) as JsonObject;
        var firstArray = root?.FirstOrDefault(static property => property.Value is JsonArray).Value as JsonArray;
        var hasNextPage = firstArray?.Count > PageSize;

        while (firstArray?.Count > PageSize)
            firstArray.RemoveAt(firstArray.Count - 1);

        httpContext.Items[CurrentPageItemKey] = NormalizePage(page);
        httpContext.Items[HasNextPageItemKey] = hasNextPage;

        if (root == null)
            return payload;

        return new OfficialApiJsonPayload(JsonSerializer.SerializeToElement(root));
    }
}
