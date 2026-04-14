using Integracao.ControlID.PoC.Models.ControlIDApi;

namespace Integracao.ControlID.PoC.ViewModels.OfficialApi
{
    public class OfficialApiInvokeViewModel
    {
        public string EndpointId { get; set; } = string.Empty;
        public string DeviceAddress { get; set; } = string.Empty;
        public string SessionString { get; set; } = string.Empty;
        public string AdditionalQuery { get; set; } = string.Empty;
        public string RequestBody { get; set; } = string.Empty;
        public string ErrorMessage { get; set; } = string.Empty;
        public OfficialApiEndpointDefinition? Endpoint { get; set; }
        public OfficialApiInvocationResult? Result { get; set; }
    }
}
