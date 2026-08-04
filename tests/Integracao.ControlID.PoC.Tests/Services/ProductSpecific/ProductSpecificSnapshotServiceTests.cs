using System.Collections.Concurrent;
using System.Text.Json;
using Integracao.ControlID.PoC.Models.ControlIDApi;
using Integracao.ControlID.PoC.Services.ControlIDApi;
using Integracao.ControlID.PoC.Services.ProductSpecific;
using Integracao.ControlID.PoC.ViewModels.ProductSpecific;

namespace Integracao.ControlID.PoC.Tests.Services.ProductSpecific;

public class ProductSpecificSnapshotServiceTests
{
    [Fact]
    public async Task PopulateAllAsync_UsesOneConfigurationRequestAndThreeStatusRequests()
    {
        var api = new RecordingOfficialApiService();
        var service = new ProductSpecificSnapshotService(api, new ProductSpecificJsonReader());

        await service.PopulateAllAsync(new ProductSpecificViewModel());

        Assert.Equal(4, api.EndpointIds.Count);
        Assert.Equal(1, api.EndpointIds.Count(static id => id == "get-configuration"));
        Assert.Contains("get-sip-status", api.EndpointIds);
        Assert.Contains("has-pjsip-audio-message", api.EndpointIds);
        Assert.Contains("has-audio-access-messages", api.EndpointIds);
    }

    private sealed class RecordingOfficialApiService : IOfficialControlIdApiService
    {
        public ConcurrentBag<string> EndpointIds { get; } = [];

        public bool TryGetConnection(out string deviceAddress, out string sessionString)
        {
            deviceAddress = "http://device.local";
            sessionString = "session";
            return true;
        }

        public string GetDeviceAddress() => "http://device.local";
        public string GetSessionString() => "session";

        public Task<OfficialApiInvocationResult> InvokeAsync(string endpointId, object? payload = null, string additionalQuery = "", CancellationToken cancellationToken = default)
            => Task.FromResult(new OfficialApiInvocationResult { Success = true });

        public Task<OfficialApiInvocationResult> InvokeBinaryAsync(string endpointId, ReadOnlyMemory<byte> payload, string additionalQuery = "", CancellationToken cancellationToken = default)
            => Task.FromResult(new OfficialApiInvocationResult { Success = true });

        public Task<OfficialApiInvocationResult> InvokeDirectAsync(string endpointId, string deviceAddress, string sessionString = "", object? payload = null, string additionalQuery = "", CancellationToken cancellationToken = default)
            => InvokeAsync(endpointId, payload, additionalQuery, cancellationToken);

        public Task<(OfficialApiInvocationResult Result, OfficialApiJsonPayload? Document)> InvokeJsonAsync(string endpointId, object? payload = null, string additionalQuery = "", CancellationToken cancellationToken = default)
        {
            EndpointIds.Add(endpointId);
            using var document = JsonDocument.Parse("{}");
            return Task.FromResult<(OfficialApiInvocationResult, OfficialApiJsonPayload?)>((
                new OfficialApiInvocationResult { Success = true, ResponseBody = "{}" },
                new OfficialApiJsonPayload(document.RootElement.Clone())));
        }

        public Task<(OfficialApiInvocationResult Result, OfficialApiJsonPayload? Document)> InvokeJsonDirectAsync(string endpointId, string deviceAddress, string sessionString = "", object? payload = null, string additionalQuery = "", CancellationToken cancellationToken = default)
            => InvokeJsonAsync(endpointId, payload, additionalQuery, cancellationToken);
    }
}
