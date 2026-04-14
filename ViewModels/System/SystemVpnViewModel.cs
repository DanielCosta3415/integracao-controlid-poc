using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace Integracao.ControlID.PoC.ViewModels.System
{
    public class SystemVpnViewModel
    {
        [Display(Name = "VPN habilitada")]
        public bool Enabled { get; set; }

        [Display(Name = "Login manual habilitado")]
        public bool LoginEnabled { get; set; }

        [Display(Name = "Usuário VPN")]
        public string Login { get; set; } = string.Empty;

        [Display(Name = "Senha VPN")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [Display(Name = "Arquivo VPN (.conf ou .zip)")]
        public IFormFile? VpnFile { get; set; }

        public bool? HasFile { get; set; }
        public int? StatusCode { get; set; }
        public string StatusDescription { get; set; } = string.Empty;
        public string InformationJson { get; set; } = string.Empty;
        public string StatusJson { get; set; } = string.Empty;
        public string FileStatusJson { get; set; } = string.Empty;
        public string ResultMessage { get; set; } = string.Empty;
        public string ResultStatusType { get; set; } = string.Empty;
        public string LastResponseJson { get; set; } = string.Empty;
        public string ErrorMessage { get; set; } = string.Empty;
    }
}
