using System.ComponentModel.DataAnnotations;

namespace Integracao.ControlID.PoC.ViewModels.Config
{
    public class ConfigDiagnosticsViewModel
    {
        [Required(ErrorMessage = "Informe o host para o teste de conexão.")]
        [Display(Name = "Host TCP")]
        public string ConnectionHost { get; set; } = "8.8.8.8";

        [Required(ErrorMessage = "Informe a porta para o teste de conexão.")]
        [Range(1, 65535, ErrorMessage = "A porta deve estar entre 1 e 65535.")]
        [Display(Name = "Porta TCP")]
        public int ConnectionPort { get; set; } = 53;

        [Required(ErrorMessage = "Informe o host para o ping.")]
        [Display(Name = "Host de Ping")]
        public string PingHost { get; set; } = "8.8.8.8";

        [Required(ErrorMessage = "Informe o host para o NSLookup.")]
        [Display(Name = "Host de NSLookup")]
        public string NslookupHost { get; set; } = "www.controlid.com.br";

        public string ConnectionResultJson { get; set; } = string.Empty;
        public string PingResultJson { get; set; } = string.Empty;
        public string NslookupResultJson { get; set; } = string.Empty;
        public string ErrorMessage { get; set; } = string.Empty;
    }
}
