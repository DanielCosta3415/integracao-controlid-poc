using System.Diagnostics;
using Integracao.ControlID.PoC.Services.Observability;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Integracao.ControlID.PoC.Middlewares;

public sealed class CorrelationIdMiddleware
{
    private const int MaxCorrelationIdLength = 128;
    private readonly RequestDelegate _next;
    private readonly ILogger<CorrelationIdMiddleware> _logger;

    public CorrelationIdMiddleware(RequestDelegate next, ILogger<CorrelationIdMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = ResolveCorrelationId(context);
        context.Items[ObservabilityConstants.CorrelationIdItemName] = correlationId;
        context.Response.Headers[ObservabilityConstants.CorrelationIdHeaderName] = correlationId;

        context.Response.OnStarting(() =>
        {
            context.Response.Headers[ObservabilityConstants.CorrelationIdHeaderName] = correlationId;
            return Task.CompletedTask;
        });

        using var scope = _logger.BeginScope(new Dictionary<string, object>
        {
            [ObservabilityConstants.CorrelationIdScopeProperty] = correlationId,
            [ObservabilityConstants.TraceIdScopeProperty] = context.TraceIdentifier
        });

        await _next(context);
    }

    public static string ResolveCorrelationId(HttpContext context)
    {
        var incoming = context.Request.Headers[ObservabilityConstants.CorrelationIdHeaderName].ToString();
        if (TryNormalizeCorrelationId(incoming, out var normalized))
            return normalized;

        var activityTraceId = Activity.Current?.TraceId.ToString();
        if (TryNormalizeCorrelationId(activityTraceId, out normalized))
            return normalized;

        return Guid.NewGuid().ToString("N");
    }

    public static bool TryNormalizeCorrelationId(string? value, out string correlationId)
    {
        correlationId = string.Empty;

        if (string.IsNullOrWhiteSpace(value))
            return false;

        var candidate = value.Trim();
        if (candidate.Length > MaxCorrelationIdLength)
            return false;

        if (candidate.Any(static character => !IsAllowedCorrelationIdCharacter(character)))
            return false;

        correlationId = candidate;
        return true;
    }

    private static bool IsAllowedCorrelationIdCharacter(char character)
    {
        return char.IsAsciiLetterOrDigit(character) ||
               character is '-' or '_' or '.' or ':' or '/';
    }
}
