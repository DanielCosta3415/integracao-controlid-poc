using System;
using System.Net;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Threading.Tasks;
using Integracao.ControlID.PoC.Services.Observability;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;

namespace Integracao.ControlID.PoC.Middlewares
{
    public class ExceptionHandlingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlingMiddleware> _logger;

        public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                var correlationId = ObservabilityConstants.GetCorrelationId(context);
                _logger.LogError(
                    OperationalEventIds.UnhandledException,
                    ex,
                    "Excecao nao tratada durante o processamento da requisicao. Correlation {CorrelationId}.",
                    correlationId);

                if (context.Response.HasStarted)
                    throw;

                context.Response.Clear();
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                context.Response.Headers[HeaderNames.CacheControl] = "no-store, no-cache, max-age=0";
                context.Response.Headers[HeaderNames.Pragma] = "no-cache";
                context.Response.Headers[HeaderNames.Expires] = "0";
                var traceId = context.TraceIdentifier;

                if (!ExpectsJson(context.Request))
                {
                    context.Response.ContentType = "text/html; charset=utf-8";
                    var encodedTraceId = HtmlEncoder.Default.Encode(traceId);
                    var encodedCorrelationId = HtmlEncoder.Default.Encode(correlationId);
                    await context.Response.WriteAsync(
                        $"<!doctype html><html lang=\"pt-BR\"><head><meta charset=\"utf-8\"><title>Erro interno</title></head>" +
                        $"<body><main><h1>Nao foi possivel concluir a operacao</h1><p>Tente novamente. Se o problema persistir, informe o identificador {encodedTraceId}.</p>" +
                        $"<p>Correlacao: {encodedCorrelationId}</p></main></body></html>");
                    return;
                }

                context.Response.ContentType = "application/json; charset=utf-8";

                var errorResponse = new
                {
                    Success = false,
                    Message = "Ocorreu um erro interno no servidor.",
                    // SECURITY: detalhes internos ficam apenas no log para evitar
                    // vazamento de infraestrutura, paths locais e stack trace ao client-side.
                    TraceId = traceId,
                    CorrelationId = correlationId
                };

                var json = JsonSerializer.Serialize(errorResponse, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    WriteIndented = true
                });

                await context.Response.WriteAsync(json);
            }
        }

        private static bool ExpectsJson(HttpRequest request)
        {
            var path = request.Path.Value ?? string.Empty;
            if (path.StartsWith("/api/", StringComparison.OrdinalIgnoreCase) ||
                path.Equals("/push", StringComparison.OrdinalIgnoreCase) ||
                path.Equals("/result", StringComparison.OrdinalIgnoreCase) ||
                path.EndsWith(".fcgi", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            var acceptedMediaTypes = request.GetTypedHeaders().Accept;
            if (acceptedMediaTypes is null || acceptedMediaTypes.Count == 0)
                return true;

            return !acceptedMediaTypes.Any(mediaType =>
                mediaType.MediaType.Value?.Equals("text/html", StringComparison.OrdinalIgnoreCase) == true ||
                mediaType.MediaType.Value?.Equals("application/xhtml+xml", StringComparison.OrdinalIgnoreCase) == true);
        }
    }
}
