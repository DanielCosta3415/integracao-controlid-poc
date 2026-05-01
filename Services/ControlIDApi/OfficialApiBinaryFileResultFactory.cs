using System.Text;
using Integracao.ControlID.PoC.Models.ControlIDApi;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;

namespace Integracao.ControlID.PoC.Services.ControlIDApi
{
    public sealed class OfficialApiBinaryFileResultFactory
    {
        public FileContentResult Create(OfficialApiInvocationResult result, string fileName, string fallbackContentType)
        {
            var contentType = NormalizeContentType(result.ResponseContentType, fallbackContentType);

            var payload = result.ResponseBodyIsBase64 && !string.IsNullOrWhiteSpace(result.ResponseBody)
                ? Convert.FromBase64String(result.ResponseBody)
                : Encoding.UTF8.GetBytes(result.ResponseBody ?? string.Empty);

            return new FileContentResult(payload, contentType)
            {
                FileDownloadName = fileName
            };
        }

        private static string NormalizeContentType(string? contentType, string fallbackContentType)
        {
            if (MediaTypeHeaderValue.TryParse(contentType, out var parsedContentType) &&
                !string.IsNullOrWhiteSpace(parsedContentType.MediaType))
            {
                return parsedContentType.MediaType;
            }

            return MediaTypeHeaderValue.TryParse(fallbackContentType, out var parsedFallback) &&
                   !string.IsNullOrWhiteSpace(parsedFallback.MediaType)
                ? parsedFallback.MediaType
                : "application/octet-stream";
        }
    }
}
