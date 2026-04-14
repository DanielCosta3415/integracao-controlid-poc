using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace Integracao.ControlID.PoC.ViewModels.Hardware
{
    public class BiometryValidationViewModel
    {
        [Display(Name = "Arquivo de biometria")]
        public IFormFile? BiometryFile { get; set; }

        public string ResultMessage { get; set; } = string.Empty;
        public string ResultStatusType { get; set; } = string.Empty;
        public string ResponseJson { get; set; } = string.Empty;
        public string ErrorMessage { get; set; } = string.Empty;
    }
}
