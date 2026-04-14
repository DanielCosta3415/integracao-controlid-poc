using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Integracao.ControlID.PoC.Models.ControlIDApi;
using Microsoft.Extensions.Logging;

namespace Integracao.ControlID.PoC.Services.ControlIDApi
{
    public class OfficialApiInvokerService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<OfficialApiInvokerService> _logger;

        public OfficialApiInvokerService(IHttpClientFactory httpClientFactory, ILogger<OfficialApiInvokerService> logger)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
        }

        public async Task<OfficialApiInvocationResult> InvokeAsync(
            OfficialApiEndpointDefinition endpoint,
            string deviceAddress,
            string sessionString,
            string additionalQuery,
            string requestBody)
        {
            var result = new OfficialApiInvocationResult();

            if (string.IsNullOrWhiteSpace(deviceAddress))
            {
                result.ErrorMessage = "Informe o endereço do equipamento.";
                return result;
            }

            if (endpoint.RequiresSession && string.IsNullOrWhiteSpace(sessionString))
            {
                result.ErrorMessage = "Esta operação exige uma sessão ativa.";
                return result;
            }

            var client = _httpClientFactory.CreateClient();
            client.Timeout = TimeSpan.FromSeconds(60);

            try
            {
                var requestUrl = BuildUrl(deviceAddress, endpoint.Path, endpoint.RequiresSession ? sessionString : string.Empty, additionalQuery);
                using var request = new HttpRequestMessage(new HttpMethod(endpoint.Method), requestUrl)
                {
                    Content = BuildContent(endpoint, requestBody)
                };

                result.RequestUrl = requestUrl;

                using var response = await client.SendAsync(request);
                var responseBytes = await response.Content.ReadAsByteArrayAsync();
                var responseContentType = response.Content.Headers.ContentType?.MediaType ?? string.Empty;

                result.Success = response.IsSuccessStatusCode;
                result.StatusCode = (int)response.StatusCode;
                result.ResponseContentType = responseContentType;

                if (LooksLikeBinary(responseContentType))
                {
                    result.ResponseBody = Convert.ToBase64String(responseBytes);
                    result.ResponseBodyIsBase64 = true;
                }
                else
                {
                    result.ResponseBody = Encoding.UTF8.GetString(responseBytes);
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao invocar o endpoint oficial {EndpointId}.", endpoint.Id);
                result.ErrorMessage = ex.Message;
                return result;
            }
        }

        private static string BuildUrl(string deviceAddress, string path, string sessionString, string additionalQuery)
        {
            var queryItems = new List<string>();

            if (!string.IsNullOrWhiteSpace(sessionString))
                queryItems.Add($"session={Uri.EscapeDataString(sessionString)}");

            if (!string.IsNullOrWhiteSpace(additionalQuery))
                queryItems.Add(additionalQuery.TrimStart('?'));

            var queryString = queryItems.Count == 0 ? string.Empty : $"?{string.Join("&", queryItems)}";
            return $"{deviceAddress.TrimEnd('/')}{path}{queryString}";
        }

        private static HttpContent? BuildContent(OfficialApiEndpointDefinition endpoint, string requestBody)
        {
            return endpoint.BodyKind switch
            {
                "none" => null,
                "json" => new StringContent(string.IsNullOrWhiteSpace(requestBody) ? "{}" : requestBody, Encoding.UTF8, "application/json"),
                "form" => BuildFormContent(requestBody),
                "binary" => BuildBinaryContent(requestBody),
                "multipart" => BuildMultipartContent(requestBody),
                _ => new StringContent(requestBody ?? string.Empty, Encoding.UTF8, endpoint.ContentType)
            };
        }

        private static HttpContent BuildFormContent(string requestBody)
        {
            if (string.IsNullOrWhiteSpace(requestBody))
                return new FormUrlEncodedContent(new Dictionary<string, string>());

            if (requestBody.TrimStart().StartsWith("{", StringComparison.Ordinal))
            {
                var values = JsonSerializer.Deserialize<Dictionary<string, object?>>(requestBody) ?? new Dictionary<string, object?>();
                return new FormUrlEncodedContent(values.ToDictionary(pair => pair.Key, pair => pair.Value?.ToString() ?? string.Empty));
            }

            var pairs = requestBody
                .Split('&', StringSplitOptions.RemoveEmptyEntries)
                .Select(item => item.Split('=', 2))
                .ToDictionary(parts => parts[0], parts => parts.Length > 1 ? parts[1] : string.Empty);

            return new FormUrlEncodedContent(pairs);
        }

        private static HttpContent BuildBinaryContent(string requestBody)
        {
            var content = new ByteArrayContent(Convert.FromBase64String(requestBody.Trim()));
            content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/octet-stream");
            return content;
        }

        private static HttpContent BuildMultipartContent(string requestBody)
        {
            var payload = JsonSerializer.Deserialize<MultipartInvokePayload>(requestBody, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            }) ?? new MultipartInvokePayload();

            var content = new MultipartFormDataContent();

            foreach (var field in payload.Fields)
                content.Add(new StringContent(field.Value), field.Key);

            foreach (var file in payload.Files)
            {
                var fileContent = new ByteArrayContent(Convert.FromBase64String(file.Base64Content));
                fileContent.Headers.ContentType = MediaTypeHeaderValue.Parse(file.ContentType);
                content.Add(fileContent, file.Name, file.FileName);
            }

            return content;
        }

        private static bool LooksLikeBinary(string contentType)
        {
            if (string.IsNullOrWhiteSpace(contentType))
                return false;

            return !contentType.Contains("json", StringComparison.OrdinalIgnoreCase) &&
                   !contentType.Contains("text", StringComparison.OrdinalIgnoreCase) &&
                   !contentType.Contains("xml", StringComparison.OrdinalIgnoreCase);
        }

        private sealed class MultipartInvokePayload
        {
            public Dictionary<string, string> Fields { get; set; } = new();
            public List<MultipartInvokeFile> Files { get; set; } = new();
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
