using System;
using System.Diagnostics;
using System.Threading.Tasks;
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

                _logger.LogInformation(
                    "[{Timestamp}] {Method} {Path} => {StatusCode} ({Elapsed} ms) IP:{IP}",
                    DateTime.UtcNow,
                    request.Method,
                    request.Path,
                    response.StatusCode,
                    sw.ElapsedMilliseconds,
                    context.Connection.RemoteIpAddress?.ToString()
                );
            }
            catch (Exception ex)
            {
                sw.Stop();
                _logger.LogError(ex, "Erro durante o processamento da requisição [{Method}] {Path}. Tempo decorrido: {Elapsed} ms",
                    context.Request.Method,
                    context.Request.Path,
                    sw.ElapsedMilliseconds
                );
                throw;
            }
        }
    }
}
