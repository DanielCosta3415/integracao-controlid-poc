using System;
using System.Diagnostics;
using System.Security.Claims;
using System.Threading.Tasks;
using Integracao.ControlID.PoC.Helpers;
using Integracao.ControlID.PoC.Services.Observability;
using Microsoft.AspNetCore.Http;
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
                    request.Path.Value ?? string.Empty,
                    response.StatusCode,
                    sw.Elapsed.TotalMilliseconds);

                _logger.Log(
                    response.StatusCode >= StatusCodes.Status500InternalServerError ? LogLevel.Warning : LogLevel.Information,
                    OperationalEventIds.RequestCompleted,
                    "[{Timestamp}] {Method} {Path} => {StatusCode} ({Elapsed} ms) IP:{IPRef} User:{UserRef} Correlation:{CorrelationId} Trace:{TraceId}",
                    DateTime.UtcNow,
                    request.Method,
                    request.Path,
                    response.StatusCode,
                    sw.ElapsedMilliseconds,
                    PrivacyLogHelper.PseudonymizeIp(context.Connection.RemoteIpAddress),
                    context.User.Identity?.IsAuthenticated == true
                        ? PrivacyLogHelper.PseudonymizeUser(context.User.FindFirstValue(ClaimTypes.NameIdentifier) ?? context.User.Identity.Name)
                        : "anonymous",
                    correlationId,
                    context.TraceIdentifier
                );
            }
            catch (Exception ex)
            {
                sw.Stop();
                OperationalMetrics.RecordHttpRequest(
                    context.Request.Method,
                    context.Request.Path.Value ?? string.Empty,
                    StatusCodes.Status500InternalServerError,
                    sw.Elapsed.TotalMilliseconds);

                _logger.LogError(OperationalEventIds.RequestFailed, ex, "Erro durante o processamento da requisicao [{Method}] {Path}. Tempo decorrido: {Elapsed} ms. Correlation {CorrelationId}.",
                    context.Request.Method,
                    context.Request.Path,
                    sw.ElapsedMilliseconds,
                    ObservabilityConstants.GetCorrelationId(context)
                );
                throw;
            }
        }
    }
}
