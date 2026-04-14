using System;
using System.ComponentModel.DataAnnotations;

namespace Integracao.ControlID.PoC.ViewModels.QRCodes
{
    /// <summary>
    /// ViewModel para criação ou edição de QR Codes.
    /// </summary>
    public class QRCodeEditViewModel
    {
        public long? Id { get; set; }

        [Required(ErrorMessage = "Usuário obrigatório.")]
        [Display(Name = "Usuário")]
        public long UserId { get; set; }

        [Required(ErrorMessage = "Valor do QR Code obrigatório.")]
        [Display(Name = "Valor do QR Code")]
        public string Value { get; set; } = string.Empty;
    }
}

