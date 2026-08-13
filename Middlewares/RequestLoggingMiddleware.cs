using System;
using System.Diagnostics;
using System.Security.Claims;
using System.Threading.Tasks;
using Integracao.ControlID.PoC.Helpers;
using Integracao.ControlID.PoC.Services.Analytics;
using Integracao.ControlID.PoC.Services.Observability;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;

namespace Integracao.ControlID.PoC.Middlewares
{
    public class RequestLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<RequestLoggingMiddleware> _logger;

        public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var sw = Stopwatch.StartNew();

            try
            {
                await _next(context);
                sw.Stop();

                var request = context.Request;
                var response = context.Response;

                var correlationId = ObservabilityConstants.GetCorrelationId(context);
                OperationalMetrics.RecordHttpRequest(
                    request.Method,
                    ResolveMetricPath(context),
                    response.StatusCode,
                    sw.Elapsed.TotalMilliseconds);
                RecordProductAnalytics(request, response.StatusCode, sw.Elapsed.TotalMilliseconds);

                _logger.Log(
                    response.StatusCode >= StatusCodes.Status500InternalServerError ? LogLevel.Warning : LogLevel.Information,
                    OperationalEventIds.RequestCompleted,
                    "[{Timestamp}] {Method} {Path} => {StatusCode} ({Elapsed} ms) IP:{IPRef} User:{UserRef} Correlation:{CorrelationId} Trace:{TraceId}",
                    DateTime.UtcNow,
                    PrivacyLogHelper.SanitizeForLog(request.Method),
                    PrivacyLogHelper.SanitizeForLog(request.Path.Value),
                    response.StatusCode,
                    sw.ElapsedMilliseconds,
                    PrivacyLogHelper.PseudonymizeIp(context.Connection.RemoteIpAddress),
                    context.User.Identity?.IsAuthenticated == true
                        ? PrivacyLogHelper.PseudonymizeUser(context.User.FindFirstValue(ClaimTypes.NameIdentifier) ?? context.User.Identity.Name)
                        : "anonymous",
                    PrivacyLogHelper.SanitizeForLog(correlationId),
                    PrivacyLogHelper.SanitizeForLog(context.TraceIdentifier)
                );
            }
            catch
            {
                sw.Stop();
                OperationalMetrics.RecordHttpRequest(
                    context.Request.Method,
                    ResolveMetricPath(context),
                    StatusCodes.Status500InternalServerError,
                    sw.Elapsed.TotalMilliseconds);
                RecordProductAnalytics(context.Request, StatusCodes.Status500InternalServerError, sw.Elapsed.TotalMilliseconds);

                throw;
            }
        }

        private static string ResolveMetricPath(HttpContext context)
        {
            return context.GetEndpoint() is RouteEndpoint routeEndpoint
                ? routeEndpoint.RoutePattern.RawText ?? "matched"
                : "unmatched";
        }

        private static void RecordProductAnalytics(HttpRequest request, int statusCode, double elapsedMilliseconds)
        {
            if (!ProductAnalyticsEventClassifier.TryClassify(request.Method, request.Path.Value, out var productEvent))
                return;

            OperationalMetrics.RecordProductFlow(
                productEvent.Flow,
                productEvent.Name,
                productEvent.Action,
                statusCode,
                elapsedMilliseconds);
        }
    }
}
