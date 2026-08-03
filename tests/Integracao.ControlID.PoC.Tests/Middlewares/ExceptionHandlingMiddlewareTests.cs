using Integracao.ControlID.PoC.Middlewares;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;

namespace Integracao.ControlID.PoC.Tests.Middlewares;

public sealed class ExceptionHandlingMiddlewareTests
{
    [Fact]
    public async Task InvokeAsync_ReturnsHtmlForBrowserRequestWithoutInternalDetails()
    {
        var context = CreateContext("/Home/Index", "text/html");
        var middleware = CreateMiddleware();

        await middleware.InvokeAsync(context);

        var body = await ReadResponseAsync(context);
        Assert.Equal(StatusCodes.Status500InternalServerError, context.Response.StatusCode);
        Assert.StartsWith("text/html", context.Response.ContentType, StringComparison.OrdinalIgnoreCase);
        Assert.Equal("no-store, no-cache, max-age=0", context.Response.Headers.CacheControl);
        Assert.Contains(context.TraceIdentifier, body, StringComparison.Ordinal);
        Assert.DoesNotContain("sensitive-internal-detail", body, StringComparison.Ordinal);
    }

    [Fact]
    public async Task InvokeAsync_ReturnsJsonForApiRequestWithoutInternalDetails()
    {
        var context = CreateContext("/api/notifications/card", null);
        var middleware = CreateMiddleware();

        await middleware.InvokeAsync(context);

        var body = await ReadResponseAsync(context);
        Assert.Equal(StatusCodes.Status500InternalServerError, context.Response.StatusCode);
        Assert.StartsWith("application/json", context.Response.ContentType, StringComparison.OrdinalIgnoreCase);
        Assert.Equal("no-store, no-cache, max-age=0", context.Response.Headers.CacheControl);
        Assert.Contains("\"success\": false", body, StringComparison.Ordinal);
        Assert.DoesNotContain("sensitive-internal-detail", body, StringComparison.Ordinal);
    }

    [Fact]
    public async Task InvokeAsync_ReturnsJsonForWildcardAcceptToPreserveMachineClientContract()
    {
        var context = CreateContext("/Home/Index", "*/*");
        var middleware = CreateMiddleware();

        await middleware.InvokeAsync(context);

        var body = await ReadResponseAsync(context);
        Assert.Equal(StatusCodes.Status500InternalServerError, context.Response.StatusCode);
        Assert.StartsWith("application/json", context.Response.ContentType, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("\"success\": false", body, StringComparison.Ordinal);
        Assert.DoesNotContain("sensitive-internal-detail", body, StringComparison.Ordinal);
    }

    private static ExceptionHandlingMiddleware CreateMiddleware()
    {
        return new ExceptionHandlingMiddleware(
            _ => throw new InvalidOperationException("sensitive-internal-detail"),
            NullLogger<ExceptionHandlingMiddleware>.Instance);
    }

    private static DefaultHttpContext CreateContext(string path, string? accept)
    {
        var context = new DefaultHttpContext();
        context.Request.Path = path;
        context.Response.Body = new MemoryStream();
        if (accept != null)
            context.Request.Headers.Accept = accept;

        return context;
    }

    private static async Task<string> ReadResponseAsync(HttpContext context)
    {
        context.Response.Body.Position = 0;
        using var reader = new StreamReader(context.Response.Body);
        return await reader.ReadToEndAsync(TestContext.Current.CancellationToken);
    }
}
