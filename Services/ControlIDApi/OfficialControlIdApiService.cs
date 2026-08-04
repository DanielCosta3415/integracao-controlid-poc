using System.Text.Json;
using Integracao.ControlID.PoC.Models.ControlIDApi;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Integracao.ControlID.PoC.Services.ControlIDApi
{
    public class OfficialControlIdApiService : IOfficialControlIdApiService
    {
        private const string SessionDeviceAddressKey = "ControlID_DeviceAddress";
        private const string SessionSessionStringKey = "ControlID_SessionString";

        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly OfficialApiCatalogService _catalogService;
        private readonly OfficialApiInvokerService _invokerService;
        private readonly ILogger<OfficialControlIdApiService> _logger;

        public OfficialControlIdApiService(
            IHttpContextAccessor httpContextAccessor,
            OfficialApiCatalogService catalogService,
            OfficialApiInvokerService invokerService,
            ILogger<OfficialControlIdApiService> logger)
        {
            _httpContextAccessor = httpContextAccessor;
            _catalogService = catalogService;
            _invokerService = invokerService;
            _logger = logger;
        }

        /// <summary>
        /// Tenta recuperar do contexto HTTP o equipamento e a sessao atualmente em uso pela PoC.
        /// </summary>
        /// <param name="deviceAddress">Saida com o endereco persistido do equipamento.</param>
        /// <param name="sessionString">Saida com a sessao oficial persistida.</param>
        /// <returns>True quando endereco e sessao estao presentes ao mesmo tempo.</returns>
        public bool TryGetConnection(out string deviceAddress, out string sessionString)
        {
            var httpContext = _httpContextAccessor.HttpContext;

            deviceAddress = httpContext?.Session.GetString(SessionDeviceAddressKey) ?? string.Empty;
            sessionString = httpContext?.Session.GetString(SessionSessionStringKey) ?? string.Empty;

            return !string.IsNullOrWhiteSpace(deviceAddress) && !string.IsNullOrWhiteSpace(sessionString);
        }

        /// <summary>
        /// Recupera apenas o endereco base do equipamento atualmente salvo na sessao da PoC.
        /// </summary>
        /// <returns>Endereco do equipamento ou string vazia quando nao houver contexto ativo.</returns>
        public string GetDeviceAddress()
        {
            return _httpContextAccessor.HttpContext?.Session.GetString(SessionDeviceAddressKey) ?? string.Empty;
        }

        /// <summary>
        /// Recupera apenas a sessao oficial atualmente salva na PoC.
        /// </summary>
        /// <returns>Token de sessao oficial ou string vazia quando nao houver autenticacao ativa.</returns>
        public string GetSessionString()
        {
            return _httpContextAccessor.HttpContext?.Session.GetString(SessionSessionStringKey) ?? string.Empty;
        }

        /// <summary>
        /// Invoca um endpoint oficial usando o contexto atual armazenado na sessao da PoC.
        /// </summary>
        /// <param name="endpointId">Identificador do endpoint oficial registrado no catalogo.</param>
        /// <param name="payload">Payload opcional que sera serializado para JSON quando necessario.</param>
        /// <param name="additionalQuery">Query string extra aplicada a chamada oficial.</param>
        /// <returns>Resultado normalizado da invocacao.</returns>
        public async Task<OfficialApiInvocationResult> InvokeAsync(
            string endpointId,
            object? payload = null,
            string additionalQuery = "",
            CancellationToken cancellationToken = default)
        {
            return await InvokeDirectAsync(endpointId, GetDeviceAddress(), GetSessionString(), payload, additionalQuery, cancellationToken);
        }

        public async Task<OfficialApiInvocationResult> InvokeBinaryAsync(
            string endpointId,
            ReadOnlyMemory<byte> payload,
            string additionalQuery = "",
            CancellationToken cancellationToken = default)
        {
            var endpoint = _catalogService.GetById(endpointId);
            if (endpoint == null)
            {
                return new OfficialApiInvocationResult
                {
                    ErrorMessage = $"Endpoint oficial '{endpointId}' não encontrado."
                };
            }

            return await _invokerService.InvokeBinaryAsync(
                endpoint,
                GetDeviceAddress(),
                GetSessionString(),
                additionalQuery,
                payload,
                cancellationToken);
        }

        public async Task<OfficialApiInvocationResult> InvokeToStreamAsync(
            string endpointId,
            Stream destination,
            Func<OfficialApiStreamMetadata, CancellationToken, ValueTask> onResponseHeaders,
            object? payload = null,
            string additionalQuery = "",
            CancellationToken cancellationToken = default)
        {
            var endpoint = _catalogService.GetById(endpointId);
            if (endpoint == null)
            {
                return new OfficialApiInvocationResult
                {
                    ErrorMessage = $"Endpoint oficial '{endpointId}' não encontrado."
                };
            }

            return await _invokerService.InvokeToStreamAsync(
                endpoint,
                GetDeviceAddress(),
                GetSessionString(),
                additionalQuery,
                SerializePayload(payload),
                destination,
                onResponseHeaders,
                cancellationToken);
        }

        /// <summary>
        /// Invoca um endpoint oficial com endereco e sessao informados manualmente.
        /// </summary>
        /// <param name="endpointId">Identificador do endpoint oficial registrado no catalogo.</param>
        /// <param name="deviceAddress">Endereco do equipamento alvo.</param>
        /// <param name="sessionString">Sessao oficial usada na chamada, quando necessaria.</param>
        /// <param name="payload">Payload opcional que sera serializado para JSON quando necessario.</param>
        /// <param name="additionalQuery">Query string extra aplicada a chamada oficial.</param>
        /// <returns>Resultado normalizado da invocacao.</returns>
        public async Task<OfficialApiInvocationResult> InvokeDirectAsync(
            string endpointId,
            string deviceAddress,
            string sessionString = "",
            object? payload = null,
            string additionalQuery = "",
            CancellationToken cancellationToken = default)
        {
            var endpoint = _catalogService.GetById(endpointId);
            if (endpoint == null)
            {
                _logger.LogWarning(
                    "Official API orchestration failed because endpoint {EndpointId} is not registered in the catalog.",
                    endpointId);

                return new OfficialApiInvocationResult
                {
                    ErrorMessage = $"Endpoint oficial '{endpointId}' nao encontrado."
                };
            }

            var serializedPayload = SerializePayload(payload);
            if (ShouldApplyObjectPaging(endpointId, out var page))
                serializedPayload = OfficialObjectPaging.ApplyRequest(serializedPayload, page);

            return await _invokerService.InvokeAsync(
                endpoint,
                deviceAddress,
                sessionString,
                additionalQuery,
                serializedPayload,
                cancellationToken);
        }

        /// <summary>
        /// Invoca um endpoint oficial usando o contexto atual da PoC e tenta parsear o retorno como JSON.
        /// </summary>
        /// <param name="endpointId">Identificador do endpoint oficial registrado no catalogo.</param>
        /// <param name="payload">Payload opcional que sera serializado para JSON quando necessario.</param>
        /// <param name="additionalQuery">Query string extra aplicada a chamada oficial.</param>
        /// <returns>Tupla com o resultado bruto e o documento JSON quando o parse for possivel.</returns>
        public async Task<(OfficialApiInvocationResult Result, OfficialApiJsonPayload? Document)> InvokeJsonAsync(
            string endpointId,
            object? payload = null,
            string additionalQuery = "",
            CancellationToken cancellationToken = default)
        {
            return await InvokeJsonDirectAsync(endpointId, GetDeviceAddress(), GetSessionString(), payload, additionalQuery, cancellationToken);
        }

        /// <summary>
        /// Invoca um endpoint oficial e tenta materializar o retorno em JSON para fluxos que dependem de parse estruturado.
        /// </summary>
        /// <param name="endpointId">Identificador do endpoint oficial registrado no catalogo.</param>
        /// <param name="deviceAddress">Endereco do equipamento alvo.</param>
        /// <param name="sessionString">Sessao oficial usada na chamada, quando necessaria.</param>
        /// <param name="payload">Payload opcional que sera serializado para JSON quando necessario.</param>
        /// <param name="additionalQuery">Query string extra aplicada a chamada oficial.</param>
        /// <returns>Tupla contendo o resultado original e o documento JSON parseado quando possivel.</returns>
        public async Task<(OfficialApiInvocationResult Result, OfficialApiJsonPayload? Document)> InvokeJsonDirectAsync(
            string endpointId,
            string deviceAddress,
            string sessionString = "",
            object? payload = null,
            string additionalQuery = "",
            CancellationToken cancellationToken = default)
        {
            var result = await InvokeDirectAsync(endpointId, deviceAddress, sessionString, payload, additionalQuery, cancellationToken);

            if (!result.Success || result.ResponseBodyIsBase64 || string.IsNullOrWhiteSpace(result.ResponseBody))
            {
                return (result, null);
            }

            try
            {
                using var document = JsonDocument.Parse(result.ResponseBody);
                var payloadDocument = new OfficialApiJsonPayload(document.RootElement.Clone());
                if (ShouldApplyObjectPaging(endpointId, out var page) && _httpContextAccessor.HttpContext is { } httpContext)
                    payloadDocument = OfficialObjectPaging.ApplyResponse(payloadDocument, page, httpContext);

                return (result, payloadDocument);
            }
            catch (JsonException ex)
            {
                _logger.LogWarning(
                    ex,
                    "Official endpoint {EndpointId} returned a non-JSON payload while JSON parsing was expected. Status {StatusCode}.",
                    endpointId,
                    result.StatusCode);
                return (result, null);
            }
        }

        private static string SerializePayload(object? payload)
        {
            return payload switch
            {
                null => string.Empty,
                string stringPayload => stringPayload,
                _ => JsonSerializer.Serialize(payload)
            };
        }

        private bool ShouldApplyObjectPaging(string endpointId, out int page)
        {
            var httpContext = _httpContextAccessor.HttpContext;
            var isPagedRequest = string.Equals(endpointId, "load-objects", StringComparison.Ordinal) &&
                                 httpContext != null &&
                                 HttpMethods.IsGet(httpContext.Request.Method);
            page = isPagedRequest && int.TryParse(httpContext?.Request.Query["page"], out var requestedPage)
                ? OfficialObjectPaging.NormalizePage(requestedPage)
                : 1;
            return isPagedRequest;
        }
    }
}
