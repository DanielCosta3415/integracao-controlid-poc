using System.Text.Json;
using Integracao.ControlID.PoC.Models.ControlIDApi;

namespace Integracao.ControlID.PoC.Services.ControlIDApi;

public interface IOfficialControlIdApiService
{
    bool TryGetConnection(out string deviceAddress, out string sessionString);
    string GetDeviceAddress();
    string GetSessionString();
    Task<OfficialApiInvocationResult> InvokeAsync(string endpointId, object? payload = null, string additionalQuery = "");
    Task<OfficialApiInvocationResult> InvokeDirectAsync(
        string endpointId,
        string deviceAddress,
        string sessionString = "",
        object? payload = null,
        string additionalQuery = "");
    Task<(OfficialApiInvocationResult Result, JsonDocument? Document)> InvokeJsonAsync(
        string endpointId,
        object? payload = null,
        string additionalQuery = "");
    Task<(OfficialApiInvocationResult Result, JsonDocument? Document)> InvokeJsonDirectAsync(
        string endpointId,
        string deviceAddress,
        string sessionString = "",
        object? payload = null,
        string additionalQuery = "");
}
