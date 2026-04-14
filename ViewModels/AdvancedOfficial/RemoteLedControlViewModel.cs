using System.ComponentModel.DataAnnotations;

namespace Integracao.ControlID.PoC.ViewModels.AdvancedOfficial
{
    public class RemoteLedControlViewModel
    {
        [Required]
        [Display(Name = "Cor ARGB")]
        public string Color { get; set; } = "4278255360";

        [Range(0, 4, ErrorMessage = "O evento deve ficar entre 0 e 4.")]
        [Display(Name = "Evento")]
        public int Event { get; set; } = 2;

        public string ErrorMessage { get; set; } = string.Empty;
        public string ResultMessage { get; set; } = string.Empty;
        public string ResultStatusType { get; set; } = string.Empty;
        public string ResponseJson { get; set; } = string.Empty;
    }
}
