using Integracao.ControlID.PoC.Models.ControlIDApi;

namespace Integracao.ControlID.PoC.Services.ControlIDApi;

public interface IControlIdSystemClient
{
    Task<(OfficialApiInvocationResult Result, OfficialApiJsonPayload? Document)> GetInformationAsync(
        CancellationToken cancellationToken = default);

    Task<(OfficialApiInvocationResult Result, OfficialApiJsonPayload? Document)> GetInformationDirectAsync(
        string deviceAddress,
        CancellationToken cancellationToken = default);
}

public sealed class ControlIdSystemClient(OfficialControlIdApiService officialApi) : IControlIdSystemClient
{
    public Task<(OfficialApiInvocationResult Result, OfficialApiJsonPayload? Document)> GetInformationAsync(
        CancellationToken cancellationToken = default)
    {
        return officialApi.InvokeJsonAsync("system-information", cancellationToken: cancellationToken);
    }

    public Task<(OfficialApiInvocationResult Result, OfficialApiJsonPayload? Document)> GetInformationDirectAsync(
        string deviceAddress,
        CancellationToken cancellationToken = default)
    {
        return officialApi.InvokeJsonDirectAsync(
            "system-information",
            deviceAddress,
            cancellationToken: cancellationToken);
    }
}
