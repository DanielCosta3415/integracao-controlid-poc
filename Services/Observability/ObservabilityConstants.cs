using System.Diagnostics;
using Microsoft.AspNetCore.Http;

namespace Integracao.ControlID.PoC.Services.Observability;

public static class ObservabilityConstants
{
    public const string CorrelationIdHeaderName = "X-Correlation-ID";
    public const string CorrelationIdItemName = "__IntegracaoControlIdCorrelationId";
    public const string CorrelationIdScopeProperty = "CorrelationId";
    public const string TraceIdScopeProperty = "TraceId";

    public static string GetCorrelationId(HttpContext context)
    {
        if (context.Items.TryGetValue(CorrelationIdItemName, out var value) &&
            value is string correlationId &&
            !string.IsNullOrWhiteSpace(correlationId))
        {
            return correlationId;
        }

        var activityTraceId = Activity.Current?.TraceId.ToString();
        if (!string.IsNullOrWhiteSpace(activityTraceId))
            return activityTraceId;

        return context.TraceIdentifier;
    }
}
