using System;
using System.Net.Http;
using System.Text;
using Integracao.ControlID.PoC.Models.ControlIDApi;

namespace Integracao.ControlID.PoC.Helpers
{
    public static class SecurityTextHelper
    {
        private const int MaxPublicMessageLength = 240;

        private static readonly (string Source, string Target)[] CommonEncodingArtifacts =
        [
            ("\u00C3\u00A1", "á"),
            ("\u00C3\u00A2", "â"),
            ("\u00C3\u00A3", "ã"),
            ("\u00C3\u00A0", "à"),
            ("\u00C3\u00A9", "é"),
            ("\u00C3\u00AA", "ê"),
            ("\u00C3\u00AD", "í"),
            ("\u00C3\u00B3", "ó"),
            ("\u00C3\u00B4", "ô"),
            ("\u00C3\u00B5", "õ"),
            ("\u00C3\u00BA", "ú"),
            ("\u00C3\u00A7", "ç"),
            ("\u00C3\u0081", "Á"),
            ("\u00C3\u0089", "É"),
            ("\u00C3\u0093", "Ó"),
            ("\u00C3\u009A", "Ú"),
            ("\u00C3\u0087", "Ç"),
            ("\u00C3\u0095", "Õ"),
            ("\u00C3\u0082", "Â"),
            ("\u00C3\u008A", "Ê"),
            ("\u00E2\u20AC\u0153", "\""),
            ("\u00E2\u20AC\u009D", "\""),
            ("\u00E2\u20AC\u02DC", "'"),
            ("\u00E2\u20AC\u2122", "'"),
            ("\u00E2\u20AC\u201C", "–"),
            ("\u00E2\u20AC\u201D", "—"),
            ("\u00C2\u00BA", "º"),
            ("\u00C2\u00AA", "ª"),
            ("\u00C2", string.Empty),
            ("\uFFFD", string.Empty)
        ];

        public static string NormalizeForDisplay(string? value, string fallback = "Informação indisponível.")
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return fallback;
            }

            var sanitizedInput = RepairCommonEncodingArtifacts(value);
            var builder = new StringBuilder(sanitizedInput.Length);
            foreach (var character in sanitizedInput)
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

        private static string RepairCommonEncodingArtifacts(string value)
        {
            // DOCUMENTAÇÃO: algumas telas ainda podem receber texto salvo com
            // encoding legado. Centralizar a correção evita regressões visuais
            // enquanto o restante do acervo é saneado gradualmente.
            var normalized = value;
            foreach (var (source, target) in CommonEncodingArtifacts)
            {
                normalized = normalized.Replace(source, target, StringComparison.Ordinal);
            }

            return normalized;
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
