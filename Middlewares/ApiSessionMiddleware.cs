using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Integracao.ControlID.PoC.Middlewares
{
    public class ApiSessionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ApiSessionMiddleware> _logger;

        public ApiSessionMiddleware(RequestDelegate next, ILogger<ApiSessionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            // Garante que a sessão está carregada (caso esteja configurada no pipeline)
            if (context.Session != null)
            {
                if (!context.Session.IsAvailable)
                {
                    _logger.LogWarning("Sessão não estava disponível, tentando carregar...");
                    await context.Session.LoadAsync();
                }
            }
            else
            {
                _logger.LogWarning("HttpContext.Session é nulo! Middleware pode estar fora de ordem.");
            }

            // Aqui podem ser feitas validações customizadas de autenticação/token por sessão, ex:
            // var isAuthenticated = context.Session?.GetString("IsAuthenticated") == "true";
            // if (!isAuthenticated && context.Request.Path.StartsWithSegments("/api"))
            // {
            //     context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            //     await context.Response.WriteAsync("Sessão não autenticada.");
            //     return;
            // }

            await _next(context);
        }
    }
}
