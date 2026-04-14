using System.ComponentModel.DataAnnotations;

namespace Integracao.ControlID.PoC.ViewModels.Hardware
{
    public class DoorStateViewModel
    {
        [Display(Name = "Porta específica")]
        [Range(1, 64, ErrorMessage = "Informe uma porta entre 1 e 64.")]
        public int? DoorNumber { get; set; }

        public string Summary { get; set; } = string.Empty;
        public string ResponseJson { get; set; } = string.Empty;
        public string ErrorMessage { get; set; } = string.Empty;
    }
}
