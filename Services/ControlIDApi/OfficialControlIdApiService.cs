using System.Text.Json;
using Integracao.ControlID.PoC.Models.ControlIDApi;
using Microsoft.AspNetCore.Http;

namespace Integracao.ControlID.PoC.Services.ControlIDApi
{
    public class OfficialControlIdApiService
    {
        private const string SessionDeviceAddressKey = "ControlID_DeviceAddress";
        private const string SessionSessionStringKey = "ControlID_SessionString";

        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly OfficialApiCatalogService _catalogService;
        private readonly OfficialApiInvokerService _invokerService;

        public OfficialControlIdApiService(
            IHttpContextAccessor httpContextAccessor,
            OfficialApiCatalogService catalogService,
            OfficialApiInvokerService invokerService)
        {
            _httpContextAccessor = httpContextAccessor;
            _catalogService = catalogService;
            _invokerService = invokerService;
        }

        public bool TryGetConnection(out string deviceAddress, out string sessionString)
        {
            var httpContext = _httpContextAccessor.HttpContext;

            deviceAddress = httpContext?.Session.GetString(SessionDeviceAddressKey) ?? string.Empty;
            sessionString = httpContext?.Session.GetString(SessionSessionStringKey) ?? string.Empty;

            return !string.IsNullOrWhiteSpace(deviceAddress) && !string.IsNullOrWhiteSpace(sessionString);
        }

        public string GetDeviceAddress()
        {
            return _httpContextAccessor.HttpContext?.Session.GetString(SessionDeviceAddressKey) ?? string.Empty;
        }

        public string GetSessionString()
        {
            return _httpContextAccessor.HttpContext?.Session.GetString(SessionSessionStringKey) ?? string.Empty;
        }

        public async Task<OfficialApiInvocationResult> InvokeAsync(string endpointId, object? payload = null, string additionalQuery = "")
        {
            return await InvokeDirectAsync(endpointId, GetDeviceAddress(), GetSessionString(), payload, additionalQuery);
        }

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
                return new OfficialApiInvocationResult
                {
                    ErrorMessage = $"Endpoint oficial '{endpointId}' não encontrado."
                };
            }

            return await _invokerService.InvokeAsync(
                endpoint,
                deviceAddress,
                sessionString,
                additionalQuery,
                SerializePayload(payload));
        }

        public async Task<(OfficialApiInvocationResult Result, JsonDocument? Document)> InvokeJsonAsync(
            string endpointId,
            object? payload = null,
            string additionalQuery = "")
        {
            return await InvokeJsonDirectAsync(endpointId, GetDeviceAddress(), GetSessionString(), payload, additionalQuery);
        }

        public async Task<(OfficialApiInvocationResult Result, JsonDocument? Document)> InvokeJsonDirectAsync(
            string endpointId,
            string deviceAddress,
            string sessionString = "",
            object? payload = null,
            string additionalQuery = "")
        {
            var result = await InvokeDirectAsync(endpointId, deviceAddress, sessionString, payload, additionalQuery);

            if (!result.Success || result.ResponseBodyIsBase64 || string.IsNullOrWhiteSpace(result.ResponseBody))
                return (result, null);

            try
            {
                return (result, JsonDocument.Parse(result.ResponseBody));
            }
            catch (JsonException)
            {
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
