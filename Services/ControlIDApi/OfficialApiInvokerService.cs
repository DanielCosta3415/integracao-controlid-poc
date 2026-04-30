using System.Diagnostics;
using System.Text;
using Integracao.ControlID.PoC.Helpers;
using Integracao.ControlID.PoC.Models.ControlIDApi;
using Integracao.ControlID.PoC.Services.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Integracao.ControlID.PoC.Services.ControlIDApi
{
    public class OfficialApiInvokerService
    {
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<OfficialApiInvokerService> _logger;
        private readonly ControlIdInputSanitizer _inputSanitizer;
        private readonly TimeSpan _requestTimeout;

        public OfficialApiInvokerService(
            IHttpClientFactory httpClientFactory,
            ILogger<OfficialApiInvokerService> logger,
            ControlIdInputSanitizer inputSanitizer,
            IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            _logger = logger;
            _inputSanitizer = inputSanitizer;

            var configuredTimeout = configuration.GetValue<int?>("ControlIDApi:ConnectionTimeoutSeconds") ?? 60;
            _requestTimeout = TimeSpan.FromSeconds(Math.Clamp(configuredTimeout, 5, 300));
        }

        /// <summary>
        /// Invoca um endpoint oficial da Access API usando o endereco e o contexto de sessao informados.
        /// </summary>
        /// <param name="endpoint">Metadados do endpoint a ser chamado.</param>
        /// <param name="deviceAddress">Endereco base do equipamento, incluindo esquema e porta quando aplicavel.</param>
        /// <param name="sessionString">Token de sessao oficial exigido por endpoints autenticados.</param>
        /// <param name="additionalQuery">Query string adicional montada pela PoC para filtros ou ids.</param>
        /// <param name="requestBody">Payload bruto ja serializado para o corpo da requisicao.</param>
        /// <returns>Resultado padronizado da chamada, incluindo status, payload e mensagem de erro segura.</returns>
        public async Task<OfficialApiInvocationResult> InvokeAsync(
            OfficialApiEndpointDefinition endpoint,
            string deviceAddress,
            string sessionString,
            string additionalQuery,
            string requestBody)
        {
            var result = new OfficialApiInvocationResult();
            var stopwatch = Stopwatch.StartNew();
            var requestUrl = string.Empty;
            var deviceTarget = "unknown-device";

            if (string.IsNullOrWhiteSpace(deviceAddress))
            {
                _logger.LogWarning(
                    "Official API invocation blocked for {EndpointId} because the device address is empty.",
                    endpoint.Id);

                result.ErrorMessage = "Informe o endereco do equipamento.";
                return result;
            }

            if (endpoint.RequiresSession && string.IsNullOrWhiteSpace(sessionString))
            {
                _logger.LogWarning(
                    "Official API invocation blocked for {EndpointId} because no active session was provided.",
                    endpoint.Id);

                result.ErrorMessage = "Esta operacao exige uma sessao ativa.";
                return result;
            }

            var client = _httpClientFactory.CreateClient();
            client.Timeout = _requestTimeout;

            try
            {
                var normalizedDeviceAddress = _inputSanitizer.NormalizeDeviceAddress(deviceAddress);
                var normalizedSessionString = endpoint.RequiresSession
                    ? _inputSanitizer.NormalizeSessionString(sessionString)
                    : string.Empty;
                var normalizedQuery = _inputSanitizer.NormalizeAdditionalQuery(additionalQuery);
                requestUrl = BuildUrl(normalizedDeviceAddress, endpoint.Path, normalizedSessionString, normalizedQuery);
                deviceTarget = BuildMonitoringTarget(normalizedDeviceAddress);

                _logger.LogInformation(
                    "Invoking official endpoint {EndpointId} {Method} {Path} against {DeviceTarget}.",
                    endpoint.Id,
                    endpoint.Method,
                    endpoint.Path,
                    deviceTarget);

                using var request = new HttpRequestMessage(new HttpMethod(endpoint.Method), requestUrl)
                {
                    Content = _inputSanitizer.BuildSanitizedContent(endpoint, requestBody)
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

                stopwatch.Stop();
                _logger.Log(
                    result.Success ? LogLevel.Information : LogLevel.Warning,
                    "Official endpoint {EndpointId} completed with status {StatusCode} in {ElapsedMs} ms. Target {DeviceTarget}. ContentType {ContentType}.",
                    endpoint.Id,
                    result.StatusCode,
                    stopwatch.ElapsedMilliseconds,
                    deviceTarget,
                    responseContentType);

                return result;
            }
            catch (InvalidOperationException ex)
            {
                stopwatch.Stop();
                _logger.LogWarning(
                    ex,
                    "Validation failure while invoking official endpoint {EndpointId} after {ElapsedMs} ms. Target {DeviceTarget}.",
                    endpoint.Id,
                    stopwatch.ElapsedMilliseconds,
                    deviceTarget);

                result.ErrorMessage = SecurityTextHelper.NormalizeForDisplay(
                    ex.Message,
                    "Nao foi possivel validar os dados enviados ao endpoint.");
                return result;
            }
            catch (TaskCanceledException ex)
            {
                stopwatch.Stop();
                _logger.LogError(
                    ex,
                    "Timeout while invoking official endpoint {EndpointId} after {ElapsedMs} ms. Target {DeviceTarget}.",
                    endpoint.Id,
                    stopwatch.ElapsedMilliseconds,
                    deviceTarget);

                result.ErrorMessage = "Tempo limite excedido ao comunicar com o equipamento.";
                return result;
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                _logger.LogError(
                    ex,
                    "Unexpected failure while invoking official endpoint {EndpointId} after {ElapsedMs} ms. Target {DeviceTarget}. Url {RequestUrl}.",
                    endpoint.Id,
                    stopwatch.ElapsedMilliseconds,
                    deviceTarget,
                    requestUrl);

                result.ErrorMessage = SecurityTextHelper.BuildSafeUserMessage("Falha ao invocar o endpoint oficial", ex);
                return result;
            }
        }

        /// <summary>
        /// Monta a URL final da chamada oficial incluindo sessao e filtros adicionais.
        /// </summary>
        /// <param name="deviceAddress">Endereco base ja normalizado do equipamento.</param>
        /// <param name="path">Path oficial do endpoint catalogado.</param>
        /// <param name="sessionString">Sessao oficial adicionada como query quando necessaria.</param>
        /// <param name="additionalQuery">Query adicional normalizada pela camada de seguranca.</param>
        /// <returns>URL final usada pelo HttpClient.</returns>
        private static string BuildUrl(string deviceAddress, string path, string sessionString, string additionalQuery)
        {
            var queryItems = new List<string>();

            if (!string.IsNullOrWhiteSpace(sessionString))
            {
                queryItems.Add($"session={Uri.EscapeDataString(sessionString)}");
            }

            if (!string.IsNullOrWhiteSpace(additionalQuery))
            {
                queryItems.Add(additionalQuery.TrimStart('?'));
            }

            var queryString = queryItems.Count == 0 ? string.Empty : $"?{string.Join("&", queryItems)}";
            return $"{deviceAddress.TrimEnd('/')}{path}{queryString}";
        }

        /// <summary>
        /// Heuristica simples para decidir se a resposta deve ser mantida em Base64 no resultado.
        /// </summary>
        /// <param name="contentType">Content-Type retornado pelo equipamento.</param>
        /// <returns>True quando o conteudo parece binario e deve ser preservado em Base64.</returns>
        private static bool LooksLikeBinary(string contentType)
        {
            if (string.IsNullOrWhiteSpace(contentType))
            {
                return false;
            }

            return !contentType.Contains("json", StringComparison.OrdinalIgnoreCase) &&
                   !contentType.Contains("text", StringComparison.OrdinalIgnoreCase) &&
                   !contentType.Contains("xml", StringComparison.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Reduz o endereco do equipamento para um alvo seguro de log, sem caminho, sessao ou query string.
        /// </summary>
        /// <param name="deviceAddress">Endereco base normalizado.</param>
        /// <returns>Alvo usado nos logs de observabilidade.</returns>
        private static string BuildMonitoringTarget(string deviceAddress)
        {
            return Uri.TryCreate(deviceAddress, UriKind.Absolute, out var uri)
                ? $"{uri.Scheme}://{uri.Authority}"
                : "invalid-device-address";
        }
    }
}
