using System.ComponentModel.DataAnnotations;

namespace Integracao.ControlID.PoC.ViewModels.AdvancedOfficial
{
    public class NetworkInterlockViewModel
    {
        [Display(Name = "Intertravamento habilitado")]
        public bool InterlockEnabled { get; set; } = true;

        [Display(Name = "Permitir bypass por API")]
        public bool ApiBypassEnabled { get; set; }

        [Display(Name = "Permitir bypass por REX")]
        public bool RexBypassEnabled { get; set; }

        public string ErrorMessage { get; set; } = string.Empty;
        public string ResultMessage { get; set; } = string.Empty;
        public string ResultStatusType { get; set; } = string.Empty;
        public string ResponseJson { get; set; } = string.Empty;
    }
}
