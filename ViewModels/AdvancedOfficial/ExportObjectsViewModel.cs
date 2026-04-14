using System.ComponentModel.DataAnnotations;

namespace Integracao.ControlID.PoC.ViewModels.AdvancedOfficial
{
    public class ExportObjectsViewModel
    {
        [Required(ErrorMessage = "Informe o tipo de objeto a exportar.")]
        [Display(Name = "Objeto")]
        public string ObjectName { get; set; } = "users";

        public string ErrorMessage { get; set; } = string.Empty;
        public string ResultMessage { get; set; } = string.Empty;
        public string ResultStatusType { get; set; } = string.Empty;
        public string ResponseJson { get; set; } = string.Empty;
    }
}
