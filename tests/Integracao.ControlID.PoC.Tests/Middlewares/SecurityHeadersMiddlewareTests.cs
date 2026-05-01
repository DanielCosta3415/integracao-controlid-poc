using Integracao.ControlID.PoC.Middlewares;
using Microsoft.AspNetCore.Http;

namespace Integracao.ControlID.PoC.Tests.Middlewares;

public class SecurityHeadersMiddlewareTests
{
    [Fact]
    public async Task InvokeAsync_Adds_DefenseInDepth_Headers()
    {
        var context = new DefaultHttpContext();
        var middleware = new SecurityHeadersMiddleware(async httpContext =>
        {
            await httpContext.Response.StartAsync();
            await httpContext.Response.WriteAsync("ok");
        });

        await middleware.InvokeAsync(context);

        Assert.Equal("nosniff", context.Response.Headers["X-Content-Type-Options"]);
        Assert.Equal("none", context.Response.Headers["X-Permitted-Cross-Domain-Policies"]);
        Assert.Equal("no-referrer", context.Response.Headers["Referrer-Policy"]);
        Assert.Equal("same-origin", context.Response.Headers["Cross-Origin-Resource-Policy"]);
        Assert.Contains("object-src 'none'", context.Response.Headers["Content-Security-Policy"].ToString());
        Assert.DoesNotContain("unsafe-inline", context.Response.Headers["Content-Security-Policy"].ToString());
    }
}
