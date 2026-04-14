using System;
using System.Net.Http;
using System.Text;
using Integracao.ControlID.PoC.Models.ControlIDApi;

namespace Integracao.ControlID.PoC.Helpers
{
    public static class SecurityTextHelper
    {
        private const int MaxPublicMessageLength = 240;

        public static string NormalizeForDisplay(string? value, string fallback = "Informação indisponível.")
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return fallback;
            }

            var builder = new StringBuilder(value.Length);
            foreach (var character in value)
            {
                if (!char.IsControl(character) || character == ' ')
                {
                    builder.Append(character);
                }
            }

            var normalized = builder.ToString().Trim();
            if (normalized.Length == 0)
            {
                return fallback;
            }

            return normalized.Length > MaxPublicMessageLength
                ? normalized[..(MaxPublicMessageLength - 3)] + "..."
                : normalized;
        }

        public static string BuildSafeUserMessage(string context, Exception? exception)
        {
            return exception switch
            {
                TaskCanceledException => $"{context}: tempo de resposta excedido.",
                HttpRequestException => $"{context}: falha de comunicação com o equipamento.",
                InvalidOperationException invalidOperationException => $"{context}: {NormalizeForDisplay(invalidOperationException.Message)}",
                _ => $"{context}: ocorreu uma falha inesperada. Consulte os logs da PoC."
            };
        }

        public static string BuildApiFailureMessage(OfficialApiInvocationResult result, string errorPrefix)
        {
            if (!string.IsNullOrWhiteSpace(result.ErrorMessage))
            {
                return $"{errorPrefix}: {NormalizeForDisplay(result.ErrorMessage)}";
            }

            if (!string.IsNullOrWhiteSpace(result.ResponseBody))
            {
                return $"{errorPrefix}: {NormalizeForDisplay(result.ResponseBody)}";
            }

            return $"{errorPrefix} (status HTTP {result.StatusCode}).";
        }

        public static string MaskSensitiveValue(string? value, int prefix = 4, int suffix = 4)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return "-";
            }

            var normalized = NormalizeForDisplay(value, "-");
            if (normalized == "-" || normalized.Length <= prefix + suffix)
            {
                return new string('*', Math.Max(4, normalized.Length));
            }

            return $"{normalized[..prefix]}...{normalized[^suffix..]}";
        }
    }
}
