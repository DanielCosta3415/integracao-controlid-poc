using System.Text.Json;
using Integracao.ControlID.PoC.Middlewares;
using Integracao.ControlID.PoC.Services.Observability;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging.Abstractions;

namespace Integracao.ControlID.PoC.Tests.Middlewares;

public class CorrelationIdMiddlewareTests
{
    [Fact]
    public async Task InvokeAsync_ReusesSafeIncomingCorrelationId()
    {
        var context = new DefaultHttpContext();
        context.Request.Headers[ObservabilityConstants.CorrelationIdHeaderName] = "client-request-123";
        context.Response.Body = new MemoryStream();

        var middleware = new CorrelationIdMiddleware(
            async httpContext => await httpContext.Response.WriteAsync("ok", TestContext.Current.CancellationToken),
            NullLogger<CorrelationIdMiddleware>.Instance);

        await middleware.InvokeAsync(context);

        Assert.Equal("client-request-123", context.Items[ObservabilityConstants.CorrelationIdItemName]);
        Assert.Equal("client-request-123", context.Response.Headers[ObservabilityConstants.CorrelationIdHeaderName]);
    }

    [Fact]
    public async Task InvokeAsync_ReplacesUnsafeIncomingCorrelationId()
    {
        var context = new DefaultHttpContext();
        context.Request.Headers[ObservabilityConstants.CorrelationIdHeaderName] = "bad value with spaces";
        context.Response.Body = new MemoryStream();

        var middleware = new CorrelationIdMiddleware(
            async httpContext => await httpContext.Response.WriteAsync("ok", TestContext.Current.CancellationToken),
            NullLogger<CorrelationIdMiddleware>.Instance);

        await middleware.InvokeAsync(context);

        var correlationId = Assert.IsType<string>(context.Items[ObservabilityConstants.CorrelationIdItemName]);
        Assert.NotEqual("bad value with spaces", correlationId);
        Assert.True(CorrelationIdMiddleware.TryNormalizeCorrelationId(correlationId, out _));
        Assert.Equal(correlationId, context.Response.Headers[ObservabilityConstants.CorrelationIdHeaderName]);
    }

    [Fact]
    public async Task ExceptionPipeline_ReturnsSafeCorrelationIdWithoutExceptionDetails()
    {
        var context = new DefaultHttpContext();
        context.Request.Headers[ObservabilityConstants.CorrelationIdHeaderName] = "incident-abc-123";
        context.Response.Body = new MemoryStream();

        var exceptionMiddleware = new ExceptionHandlingMiddleware(
            _ => throw new InvalidOperationException("database file path should stay in logs"),
            NullLogger<ExceptionHandlingMiddleware>.Instance);
        var correlationMiddleware = new CorrelationIdMiddleware(
            exceptionMiddleware.InvokeAsync,
            NullLogger<CorrelationIdMiddleware>.Instance);

        await correlationMiddleware.InvokeAsync(context);

        context.Response.Body.Position = 0;
        using var document = await JsonDocument.ParseAsync(
            context.Response.Body,
            cancellationToken: TestContext.Current.CancellationToken);
        var root = document.RootElement;

        Assert.Equal(StatusCodes.Status500InternalServerError, context.Response.StatusCode);
        Assert.Equal("incident-abc-123", root.GetProperty("correlationId").GetString());
        Assert.DoesNotContain("database file path", root.GetRawText(), StringComparison.OrdinalIgnoreCase);
    }
}
