using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace Integracao.ControlID.PoC.ViewModels.System
{
    public class SystemNetworkViewModel
    {
        [Required(ErrorMessage = "Informe o endereço IP.")]
        [Display(Name = "Endereço IP")]
        public string IpAddress { get; set; } = string.Empty;

        [Required(ErrorMessage = "Informe a máscara de rede.")]
        [Display(Name = "Máscara de Rede")]
        public string Netmask { get; set; } = string.Empty;

        [Required(ErrorMessage = "Informe o gateway.")]
        [Display(Name = "Gateway")]
        public string Gateway { get; set; } = string.Empty;

        [Display(Name = "Hostname customizado ativo")]
        public bool CustomHostnameEnabled { get; set; }

        [Display(Name = "Hostname do dispositivo")]
        public string DeviceHostname { get; set; } = string.Empty;

        [Range(1, 65535, ErrorMessage = "A porta web deve estar entre 1 e 65535.")]
        [Display(Name = "Porta Web")]
        public int WebServerPort { get; set; } = 80;

        [Display(Name = "SSL habilitado")]
        public bool SslEnabled { get; set; }

        [Display(Name = "Certificado autoassinado")]
        public bool SelfSignedCertificate { get; set; } = true;

        [Display(Name = "DNS primário")]
        public string DnsPrimary { get; set; } = "8.8.8.8";

        [Display(Name = "DNS secundário")]
        public string DnsSecondary { get; set; } = "8.8.4.4";

        [Display(Name = "Certificado PEM")]
        public IFormFile? CertificateFile { get; set; }

        public string ResultMessage { get; set; } = string.Empty;
        public string ResultStatusType { get; set; } = string.Empty;
        public string LastResponseJson { get; set; } = string.Empty;
        public string ErrorMessage { get; set; } = string.Empty;

        [Display(Name = "Confirmacao de alteracao de rede")]
        public string ConfirmationPhrase { get; set; } = string.Empty;
    }
}
