using System.ComponentModel.DataAnnotations;

namespace Integracao.ControlID.PoC.ViewModels.Config
{
    public class ConfigOfficialViewModel
    {
        [Required(ErrorMessage = "Informe o payload JSON para leitura.")]
        [Display(Name = "Payload para get_configuration")]
        public string GetPayload { get; set; } = "{\n  \"general\": [\"beep_enabled\", \"relay1_timeout\"]\n}";

        [Required(ErrorMessage = "Informe o payload JSON para alteração.")]
        [Display(Name = "Payload para set_configuration")]
        public string SetPayload { get; set; } = "{\n  \"general\": {\n    \"beep_enabled\": \"1\"\n  }\n}";

        public string GetResponseJson { get; set; } = string.Empty;
        public string SetResponseJson { get; set; } = string.Empty;
        public string ErrorMessage { get; set; } = string.Empty;
    }
}
