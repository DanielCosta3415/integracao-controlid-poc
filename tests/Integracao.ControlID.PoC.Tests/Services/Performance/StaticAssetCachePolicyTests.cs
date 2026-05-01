using Integracao.ControlID.PoC.Services.Performance;
using Microsoft.AspNetCore.Http;

namespace Integracao.ControlID.PoC.Tests.Services.Performance;

public class StaticAssetCachePolicyTests
{
    [Fact]
    public void ApplyVersionedAssetCacheHeaders_AddsLongImmutableCache_ForVersionedAsset()
    {
        var context = new DefaultHttpContext();
        context.Request.QueryString = new QueryString("?v=abc123");

        StaticAssetCachePolicy.ApplyVersionedAssetCacheHeaders(context);

        var cacheControl = context.Response.GetTypedHeaders().CacheControl;
        Assert.NotNull(cacheControl);
        Assert.True(cacheControl.Public);
        Assert.Equal(StaticAssetCachePolicy.VersionedAssetMaxAge, cacheControl.MaxAge);
        Assert.Contains(cacheControl.Extensions, extension => extension.Name == "immutable");
    }

    [Fact]
    public void ApplyVersionedAssetCacheHeaders_LeavesUnversionedAssetUnchanged()
    {
        var context = new DefaultHttpContext();

        StaticAssetCachePolicy.ApplyVersionedAssetCacheHeaders(context);

        Assert.Null(context.Response.GetTypedHeaders().CacheControl);
    }
}
