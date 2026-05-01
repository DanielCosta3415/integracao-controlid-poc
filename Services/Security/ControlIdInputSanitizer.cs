using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Integracao.ControlID.PoC.Models.ControlIDApi;
using Integracao.ControlID.PoC.Options;
using Microsoft.Extensions.Options;

namespace Integracao.ControlID.PoC.Services.Security
{
    public sealed class ControlIdInputSanitizer
    {
        private const int MaxAddressLength = 2048;
        private const int MaxSessionLength = 2048;
        private const int MaxQueryLength = 4096;
        private const int MaxFormValueLength = 4096;
        private const int MaxBinaryPayloadBytes = 25 * 1024 * 1024;
        private readonly ControlIdEgressOptions _egressOptions;

        public ControlIdInputSanitizer()
            : this(Microsoft.Extensions.Options.Options.Create(new ControlIdEgressOptions()))
        {
        }

        public ControlIdInputSanitizer(IOptions<ControlIdEgressOptions> egressOptions)
        {
            _egressOptions = egressOptions.Value;
        }

        public bool TryNormalizeBaseAddress(string? hostOrUrl, string? scheme, int? port, out string baseAddress, out string errorMessage)
        {
            baseAddress = string.Empty;
            errorMessage = string.Empty;

            if (string.IsNullOrWhiteSpace(hostOrUrl))
            {
                errorMessage = "Informe o IP ou domínio do equipamento Control iD.";
                return false;
            }

            var candidate = hostOrUrl.Trim();
            if (candidate.Length > MaxAddressLength || HasControlCharacters(candidate))
            {
                errorMessage = "O endereço informado contém caracteres inválidos.";
                return false;
            }

            var effectiveScheme = string.Equals(scheme, "https", StringComparison.OrdinalIgnoreCase) ? "https" : "http";
            var uriCandidate = candidate.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
                               candidate.StartsWith("https://", StringComparison.OrdinalIgnoreCase)
                ? candidate
                : $"{effectiveScheme}://{candidate}";

            if (!Uri.TryCreate(uriCandidate, UriKind.Absolute, out var parsedUri))
            {
                errorMessage = "Informe um IP, domínio ou URL válida para o equipamento.";
                return false;
            }

            if (!IsSupportedScheme(parsedUri.Scheme))
            {
                errorMessage = "Use apenas os protocolos http ou https para conectar o equipamento.";
                return false;
            }

            if (!string.IsNullOrWhiteSpace(parsedUri.UserInfo) || !string.IsNullOrWhiteSpace(parsedUri.Fragment))
            {
                errorMessage = "A URL do equipamento não pode conter credenciais embutidas ou fragmentos.";
                return false;
            }

            if (!IsAllowedDeviceHost(parsedUri.Host))
            {
                errorMessage = "O host informado não está na allowlist de equipamentos Control iD.";
                return false;
            }

            if (port.HasValue)
            {
                if (port is < 1 or > 65535)
                {
                    errorMessage = "Informe uma porta válida entre 1 e 65535.";
                    return false;
                }

                parsedUri = new UriBuilder(parsedUri) { Port = port.Value }.Uri;
            }

            baseAddress = parsedUri.GetLeftPart(UriPartial.Authority).TrimEnd('/');
            return true;
        }

        public string NormalizeDeviceAddress(string? deviceAddress)
        {
            // SECURITY: centraliza a validação do host antes de qualquer chamada remota
            // para evitar SSRF acidental, userinfo embutido e URLs malformadas.
            if (!TryNormalizeBaseAddress(deviceAddress, "http", null, out var normalizedAddress, out var errorMessage))
            {
                throw new InvalidOperationException(errorMessage);
            }

            return normalizedAddress;
        }

        public string NormalizeSessionString(string? sessionString)
        {
            if (string.IsNullOrWhiteSpace(sessionString))
            {
                return string.Empty;
            }

            var normalized = sessionString.Trim();
            if (normalized.Length > MaxSessionLength || HasControlCharacters(normalized))
            {
                throw new InvalidOperationException("A sessão informada contém caracteres inválidos.");
            }

            return normalized;
        }

        public string NormalizeAdditionalQuery(string? additionalQuery)
        {
            if (string.IsNullOrWhiteSpace(additionalQuery))
            {
                return string.Empty;
            }

            var query = additionalQuery.Trim().TrimStart('?');
            if (query.Length > MaxQueryLength)
            {
                throw new InvalidOperationException("A query adicional excede o limite aceito pela PoC.");
            }

            // SECURITY: reconstroi a query parâmetro a parâmetro para neutralizar
            // caracteres de controle, duplicação malformada e injeção manual de delimitadores.
            var normalizedSegments = new List<string>();
            foreach (var segment in query.Split('&', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            {
                var parts = segment.Split('=', 2);
                var key = Uri.UnescapeDataString(parts[0]).Trim();
                var value = parts.Length > 1 ? Uri.UnescapeDataString(parts[1]).Trim() : string.Empty;

                if (string.IsNullOrWhiteSpace(key) || HasControlCharacters(key) || HasControlCharacters(value))
                {
                    throw new InvalidOperationException("A query adicional contém parâmetros inválidos.");
                }

                normalizedSegments.Add($"{Uri.EscapeDataString(key)}={Uri.EscapeDataString(value)}");
            }

            return string.Join("&", normalizedSegments);
        }

        public HttpContent? BuildSanitizedContent(OfficialApiEndpointDefinition endpoint, string? requestBody)
        {
            return endpoint.BodyKind switch
            {
                "none" => null,
                "json" => BuildJsonContent(requestBody),
                "form" => BuildFormContent(requestBody),
                "binary" => BuildBinaryContent(requestBody),
                "multipart" => BuildMultipartContent(requestBody),
                _ => new StringContent(requestBody ?? string.Empty, Encoding.UTF8, NormalizeContentType(endpoint.ContentType))
            };
        }

        private static HttpContent BuildJsonContent(string? requestBody)
        {
            var rawJson = string.IsNullOrWhiteSpace(requestBody) ? "{}" : requestBody.Trim();
            using var document = JsonDocument.Parse(rawJson);
            var normalizedJson = JsonSerializer.Serialize(document.RootElement);
            return new StringContent(normalizedJson, Encoding.UTF8, "application/json");
        }

        private HttpContent BuildFormContent(string? requestBody)
        {
            if (string.IsNullOrWhiteSpace(requestBody))
            {
                return new FormUrlEncodedContent(Array.Empty<KeyValuePair<string, string>>());
            }

            var normalizedPairs = requestBody.TrimStart().StartsWith("{", StringComparison.Ordinal)
                ? NormalizeJsonFormPairs(requestBody)
                : NormalizeKeyValuePairs(requestBody);

            return new FormUrlEncodedContent(normalizedPairs);
        }

        private HttpContent BuildBinaryContent(string? requestBody)
        {
            var payloadBytes = DecodeBase64Payload(requestBody, "O conteúdo binário informado não está em base64 válido.");
            var content = new ByteArrayContent(payloadBytes);
            content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/octet-stream");
            return content;
        }

        private HttpContent BuildMultipartContent(string? requestBody)
        {
            if (string.IsNullOrWhiteSpace(requestBody))
            {
                throw new InvalidOperationException("O payload multipart precisa ser informado em JSON.");
            }

            var payload = JsonSerializer.Deserialize<MultipartInvokePayload>(requestBody, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }) ?? new MultipartInvokePayload();

            var content = new MultipartFormDataContent();

            foreach (var field in payload.Fields ?? new Dictionary<string, string>())
            {
                var fieldName = NormalizeToken(field.Key, "campo multipart");
                var fieldValue = NormalizeTextField(field.Value);
                content.Add(new StringContent(fieldValue), fieldName);
            }

            foreach (var file in payload.Files ?? new List<MultipartInvokeFile>())
            {
                var fileName = NormalizeFileName(file.FileName);
                var partName = NormalizeToken(file.Name, "arquivo multipart");
                var fileBytes = DecodeBase64Payload(file.Base64Content, "O arquivo multipart não está em base64 válido.");

                var fileContent = new ByteArrayContent(fileBytes);
                fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse(NormalizeContentType(file.ContentType));
                content.Add(fileContent, partName, fileName);
            }

            return content;
        }

        private IEnumerable<KeyValuePair<string, string>> NormalizeJsonFormPairs(string requestBody)
        {
            using var document = JsonDocument.Parse(requestBody);
            if (document.RootElement.ValueKind != JsonValueKind.Object)
            {
                throw new InvalidOperationException("O corpo de formulário em JSON deve ser um objeto simples.");
            }

            return document.RootElement.EnumerateObject()
                .Select(property => new KeyValuePair<string, string>(
                    NormalizeToken(property.Name, "campo de formulário"),
                    NormalizeTextField(ConvertElementToString(property.Value))))
                .ToArray();
        }

        private IEnumerable<KeyValuePair<string, string>> NormalizeKeyValuePairs(string requestBody)
        {
            var normalizedPairs = new List<KeyValuePair<string, string>>();
            foreach (var segment in requestBody.Split('&', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            {
                var parts = segment.Split('=', 2);
                var key = Uri.UnescapeDataString(parts[0]).Trim();
                var value = parts.Length > 1 ? Uri.UnescapeDataString(parts[1]).Trim() : string.Empty;

                normalizedPairs.Add(new KeyValuePair<string, string>(
                    NormalizeToken(key, "campo de formulário"),
                    NormalizeTextField(value)));
            }

            return normalizedPairs;
        }

        private byte[] DecodeBase64Payload(string? requestBody, string errorMessage)
        {
            if (string.IsNullOrWhiteSpace(requestBody))
            {
                throw new InvalidOperationException(errorMessage);
            }

            try
            {
                var bytes = Convert.FromBase64String(requestBody.Trim());
                if (bytes.Length > MaxBinaryPayloadBytes)
                {
                    throw new InvalidOperationException("O payload enviado excede o limite de segurança configurado pela PoC.");
                }

                return bytes;
            }
            catch (FormatException)
            {
                throw new InvalidOperationException(errorMessage);
            }
        }

        private static string NormalizeContentType(string? contentType)
        {
            return MediaTypeHeaderValue.TryParse(contentType, out var parsedContentType)
                ? parsedContentType.MediaType ?? "application/octet-stream"
                : "application/octet-stream";
        }

        private string NormalizeToken(string? value, string label)
        {
            var normalized = value?.Trim() ?? string.Empty;
            if (string.IsNullOrWhiteSpace(normalized) || HasControlCharacters(normalized) || normalized.Length > 128)
            {
                throw new InvalidOperationException($"O {label} contém caracteres inválidos.");
            }

            return normalized;
        }

        private string NormalizeTextField(string? value)
        {
            var normalized = value?.Trim() ?? string.Empty;
            if (HasControlCharacters(normalized) || normalized.Length > MaxFormValueLength)
            {
                throw new InvalidOperationException("Um dos valores enviados contém caracteres inválidos.");
            }

            return normalized;
        }

        private static string NormalizeFileName(string? fileName)
        {
            var safeFileName = Path.GetFileName(fileName ?? string.Empty).Trim();
            if (string.IsNullOrWhiteSpace(safeFileName) || safeFileName.Length > 200)
            {
                return "payload.bin";
            }

            return safeFileName;
        }

        private static bool HasControlCharacters(string value)
        {
            return value.Any(character => char.IsControl(character));
        }

        private static bool IsSupportedScheme(string scheme)
        {
            return string.Equals(scheme, Uri.UriSchemeHttp, StringComparison.OrdinalIgnoreCase) ||
                   string.Equals(scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase);
        }

        private bool IsAllowedDeviceHost(string host)
        {
            var allowedHosts = _egressOptions.AllowedDeviceHosts
                .Where(static value => !string.IsNullOrWhiteSpace(value))
                .Select(static value => value.Trim())
                .ToList();

            if (!_egressOptions.RequireAllowedDeviceHosts && allowedHosts.Count == 0)
                return true;

            if (string.IsNullOrWhiteSpace(host) || allowedHosts.Count == 0)
                return false;

            foreach (var allowedHost in allowedHosts)
            {
                if (allowedHost == "*")
                    continue;

                if (string.Equals(host, allowedHost, StringComparison.OrdinalIgnoreCase))
                    return true;

                if (allowedHost.StartsWith("*.", StringComparison.Ordinal) &&
                    host.EndsWith(allowedHost[1..], StringComparison.OrdinalIgnoreCase) &&
                    host.Length > allowedHost.Length - 1)
                {
                    return true;
                }
            }

            return false;
        }

        private static string ConvertElementToString(JsonElement element)
        {
            return element.ValueKind switch
            {
                JsonValueKind.String => element.GetString() ?? string.Empty,
                JsonValueKind.Number => element.GetRawText(),
                JsonValueKind.True => "true",
                JsonValueKind.False => "false",
                JsonValueKind.Null => string.Empty,
                _ => JsonSerializer.Serialize(element)
            };
        }

        private sealed class MultipartInvokePayload
        {
            public Dictionary<string, string>? Fields { get; set; } = new();
            public List<MultipartInvokeFile>? Files { get; set; } = new();
        }

        private sealed class MultipartInvokeFile
        {
            public string Name { get; set; } = "file";
            public string FileName { get; set; } = "payload.bin";
            public string ContentType { get; set; } = "application/octet-stream";
            public string Base64Content { get; set; } = string.Empty;
        }
    }
}
