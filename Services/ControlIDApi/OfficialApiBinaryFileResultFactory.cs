using System.Text;
using Integracao.ControlID.PoC.Models.ControlIDApi;
using Microsoft.AspNetCore.Mvc;

namespace Integracao.ControlID.PoC.Services.ControlIDApi
{
    public sealed class OfficialApiBinaryFileResultFactory
    {
        public FileContentResult Create(OfficialApiInvocationResult result, string fileName, string fallbackContentType)
        {
            var contentType = string.IsNullOrWhiteSpace(result.ResponseContentType)
                ? fallbackContentType
                : result.ResponseContentType;

            var payload = result.ResponseBodyIsBase64 && !string.IsNullOrWhiteSpace(result.ResponseBody)
                ? Convert.FromBase64String(result.ResponseBody)
                : Encoding.UTF8.GetBytes(result.ResponseBody ?? string.Empty);

            return new FileContentResult(payload, contentType)
            {
                FileDownloadName = fileName
            };
        }
    }
}
