using System.Text.Json;
using Integracao.ControlID.PoC.Models.ControlIDApi;

namespace Integracao.ControlID.PoC.Services.ControlIDApi
{
    public sealed class OfficialApiResultPresentationService
    {
        private static readonly JsonSerializerOptions IndentedJsonOptions = new()
        {
            WriteIndented = true
        };

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

        public string FormatJson(string rawJson, OfficialApiJsonPayload? document)
        {
            return FormatJsonPayload(rawJson, document);
        }

        public static string FormatJsonPayload(string rawJson, OfficialApiJsonPayload? document)
        {
            if (document == null)
            {
                return rawJson;
            }

            return JsonSerializer.Serialize(document.RootElement, IndentedJsonOptions);
        }

        public string FormatResponseBody(OfficialApiInvocationResult result)
        {
            return string.IsNullOrWhiteSpace(result.ResponseBody)
                ? "Operacao concluida sem corpo de resposta."
                : result.ResponseBody;
        }
    }
}
