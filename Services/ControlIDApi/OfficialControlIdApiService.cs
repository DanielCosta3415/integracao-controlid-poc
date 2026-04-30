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
        public async Task<OfficialApiInvocationResult> InvokeAsync(string endpointId, object? payload = null, string additionalQuery = "")
        {
            return await InvokeDirectAsync(endpointId, GetDeviceAddress(), GetSessionString(), payload, additionalQuery);
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
            string additionalQuery = "")
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

            return await _invokerService.InvokeAsync(
                endpoint,
                deviceAddress,
                sessionString,
                additionalQuery,
                SerializePayload(payload));
        }

        /// <summary>
        /// Invoca um endpoint oficial usando o contexto atual da PoC e tenta parsear o retorno como JSON.
        /// </summary>
        /// <param name="endpointId">Identificador do endpoint oficial registrado no catalogo.</param>
        /// <param name="payload">Payload opcional que sera serializado para JSON quando necessario.</param>
        /// <param name="additionalQuery">Query string extra aplicada a chamada oficial.</param>
        /// <returns>Tupla com o resultado bruto e o documento JSON quando o parse for possivel.</returns>
        public async Task<(OfficialApiInvocationResult Result, JsonDocument? Document)> InvokeJsonAsync(
            string endpointId,
            object? payload = null,
            string additionalQuery = "")
        {
            return await InvokeJsonDirectAsync(endpointId, GetDeviceAddress(), GetSessionString(), payload, additionalQuery);
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
        public async Task<(OfficialApiInvocationResult Result, JsonDocument? Document)> InvokeJsonDirectAsync(
            string endpointId,
            string deviceAddress,
            string sessionString = "",
            object? payload = null,
            string additionalQuery = "")
        {
            var result = await InvokeDirectAsync(endpointId, deviceAddress, sessionString, payload, additionalQuery);

            if (!result.Success || result.ResponseBodyIsBase64 || string.IsNullOrWhiteSpace(result.ResponseBody))
            {
                return (result, null);
            }

            try
            {
                return (result, JsonDocument.Parse(result.ResponseBody));
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
    }
}
