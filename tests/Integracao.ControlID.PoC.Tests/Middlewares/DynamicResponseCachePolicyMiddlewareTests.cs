using Integracao.ControlID.PoC.Middlewares;
using Microsoft.AspNetCore.Http;

namespace Integracao.ControlID.PoC.Tests.Middlewares;

public sealed class DynamicResponseCachePolicyMiddlewareTests
{
    [Fact]
    public async Task InvokeAsync_DisablesBrowserCachingForDynamicResponses()
    {
        var context = new DefaultHttpContext();
        var middleware = new DynamicResponseCachePolicyMiddleware(_ => Task.CompletedTask);

        await middleware.InvokeAsync(context);

        Assert.Equal("no-store, no-cache, max-age=0", context.Response.Headers.CacheControl);
        Assert.Equal("no-cache", context.Response.Headers.Pragma);
        Assert.Equal("0", context.Response.Headers.Expires);
    }
}
