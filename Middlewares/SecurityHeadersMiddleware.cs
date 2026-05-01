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
                headers["Permissions-Policy"] = "camera=(), microphone=(), geolocation=(), payment=(), usb=()";
                headers["Content-Security-Policy"] =
                    "default-src 'self'; " +
                    "base-uri 'self'; " +
                    "form-action 'self'; " +
                    "frame-ancestors 'self'; " +
                    "img-src 'self' data:; " +
                    "font-src 'self' https://fonts.gstatic.com; " +
                    "style-src 'self' 'unsafe-inline' https://fonts.googleapis.com; " +
                    "script-src 'self' 'unsafe-inline'; " +
                    "connect-src 'self'";
                return Task.CompletedTask;
            });

            await _next(context);
        }
    }
}
