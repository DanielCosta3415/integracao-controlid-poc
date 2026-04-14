using System.ComponentModel.DataAnnotations;
using Integracao.ControlID.PoC.Models.ControlIDApi;

namespace Integracao.ControlID.PoC.ViewModels.OfficialApi
{
    public class OfficialApiInvokeViewModel
    {
        public string EndpointId { get; set; } = string.Empty;
        [StringLength(2048, ErrorMessage = "Informe um endereço do equipamento com até 2048 caracteres.")]
        public string DeviceAddress { get; set; } = string.Empty;
        [StringLength(2048, ErrorMessage = "A sessão informada excede o limite aceito pela PoC.")]
        public string SessionString { get; set; } = string.Empty;
        [StringLength(4096, ErrorMessage = "A query adicional excede o limite aceito pela PoC.")]
        public string AdditionalQuery { get; set; } = string.Empty;
        public string RequestBody { get; set; } = string.Empty;
        public string ErrorMessage { get; set; } = string.Empty;
        public OfficialApiEndpointDefinition? Endpoint { get; set; }
        public OfficialApiContractViewModel? Contract { get; set; }
        public OfficialApiInvocationResult? Result { get; set; }
    }
}
