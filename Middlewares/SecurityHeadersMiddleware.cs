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
            var headers = context.Response.Headers;
            headers["X-Content-Type-Options"] = "nosniff";
            headers["X-Frame-Options"] = "SAMEORIGIN";
            headers["X-Permitted-Cross-Domain-Policies"] = "none";
            headers["Referrer-Policy"] = "no-referrer";
            headers["Cross-Origin-Opener-Policy"] = "same-origin";
            headers["Cross-Origin-Resource-Policy"] = "same-origin";
            headers["Permissions-Policy"] = "camera=(), microphone=(), geolocation=(), payment=(), usb=()";
            headers["Content-Security-Policy"] =
                "default-src 'self'; " +
                "base-uri 'self'; " +
                "object-src 'none'; " +
                "form-action 'self'; " +
                "frame-ancestors 'self'; " +
                "img-src 'self' data:; " +
                "media-src 'self' data:; " +
                "font-src 'self' https://fonts.gstatic.com; " +
                "style-src 'self' https://fonts.googleapis.com; " +
                "script-src 'self'; " +
                "connect-src 'self'";

            await _next(context);
        }
    }
}
