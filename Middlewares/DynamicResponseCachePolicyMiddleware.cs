using Microsoft.Net.Http.Headers;

namespace Integracao.ControlID.PoC.Middlewares;

public sealed class DynamicResponseCachePolicyMiddleware
{
    private readonly RequestDelegate _next;

    public DynamicResponseCachePolicyMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        context.Response.Headers[HeaderNames.CacheControl] = "no-store, no-cache, max-age=0";
        context.Response.Headers[HeaderNames.Pragma] = "no-cache";
        context.Response.Headers[HeaderNames.Expires] = "0";

        await _next(context);
    }
}
