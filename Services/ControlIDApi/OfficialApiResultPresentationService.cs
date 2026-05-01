using System.Text.Json;
using Integracao.ControlID.PoC.Models.ControlIDApi;

namespace Integracao.ControlID.PoC.Services.ControlIDApi
{
    public sealed class OfficialApiResultPresentationService
    {
        public void EnsureSuccess(OfficialApiInvocationResult result, string message)
        {
            if (result.Success)
            {
                return;
            }

            if (!string.IsNullOrWhiteSpace(result.ErrorMessage))
            {
                throw new InvalidOperationException($"{message}: {result.ErrorMessage}");
            }

            if (!string.IsNullOrWhiteSpace(result.ResponseBody) && !result.ResponseBodyIsBase64)
            {
                throw new InvalidOperationException($"{message} (status HTTP {result.StatusCode}; corpo de resposta omitido por segurança).");
            }

            throw new InvalidOperationException($"{message} (status HTTP {result.StatusCode}).");
        }

        public string FormatJson(string rawJson, JsonDocument? document)
        {
            if (document == null)
            {
                return rawJson;
            }

            return JsonSerializer.Serialize(document.RootElement, new JsonSerializerOptions
            {
                WriteIndented = true
            });
        }

        public string FormatResponseBody(OfficialApiInvocationResult result)
        {
            return string.IsNullOrWhiteSpace(result.ResponseBody)
                ? "Operacao concluida sem corpo de resposta."
                : result.ResponseBody;
        }
    }
}
