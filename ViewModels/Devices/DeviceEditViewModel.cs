using System;
using System.ComponentModel.DataAnnotations;

namespace Integracao.ControlID.PoC.ViewModels.Devices
{
    /// <summary>
    /// ViewModel para criação ou edição de dispositivos.
    /// </summary>
    public class DeviceEditViewModel
    {
        public long? Id { get; set; }

        [Required(ErrorMessage = "Nome do dispositivo obrigatório.")]
        [Display(Name = "Nome do Dispositivo")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Endereço IP obrigatório.")]
        [Display(Name = "Endereço IP")]
        public string Ip { get; set; } = string.Empty;

        [Display(Name = "Número de Série")]
        public string SerialNumber { get; set; } = string.Empty;

        [Display(Name = "Versão do Firmware")]
        public string Firmware { get; set; } = string.Empty;

        [Display(Name = "Status")]
        public string Status { get; set; } = string.Empty;

        [Display(Name = "Descrição")]
        public string Description { get; set; } = string.Empty;
    }
}
