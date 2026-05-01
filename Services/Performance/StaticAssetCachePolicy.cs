using Microsoft.AspNetCore.Http;
using Microsoft.Net.Http.Headers;

namespace Integracao.ControlID.PoC.Services.Performance;

public static class StaticAssetCachePolicy
{
    public static readonly TimeSpan VersionedAssetMaxAge = TimeSpan.FromDays(365);

    public static void ApplyVersionedAssetCacheHeaders(HttpContext context)
    {
        if (!context.Request.Query.ContainsKey("v"))
            return;

        var cacheControl = new CacheControlHeaderValue
        {
            Public = true,
            MaxAge = VersionedAssetMaxAge
        };
        cacheControl.Extensions.Add(new NameValueHeaderValue("immutable"));

        context.Response.GetTypedHeaders().CacheControl = cacheControl;
    }
}
