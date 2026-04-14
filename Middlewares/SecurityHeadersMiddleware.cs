using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace Integracao.ControlID.PoC.Middlewares
{
    public class SecurityHeadersMiddleware
    {
        private readonly RequestDelegate _next;

        public SecurityHeadersMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            context.Response.OnStarting(() =>
            {
                var headers = context.Response.Headers;
                headers["X-Content-Type-Options"] = "nosniff";
                headers["X-Frame-Options"] = "SAMEORIGIN";
                headers["Referrer-Policy"] = "strict-origin-when-cross-origin";
                headers["Cross-Origin-Opener-Policy"] = "same-origin";
                return Task.CompletedTask;
            });

            await _next(context);
        }
    }
}
