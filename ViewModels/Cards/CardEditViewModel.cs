using System;
using System.ComponentModel.DataAnnotations;

namespace Integracao.ControlID.PoC.ViewModels.Cards
{
    /// <summary>
    /// ViewModel para criação ou edição de cartões.
    /// </summary>
    public class CardEditViewModel
    {
        public long? Id { get; set; }

        [Required(ErrorMessage = "Usuário obrigatório.")]
        [Display(Name = "Usuário")]
        public long UserId { get; set; }

        [Required(ErrorMessage = "Valor do cartão obrigatório.")]
        [Display(Name = "Valor do Cartão (número/hexadecimal/QRcode)")]
        public string Value { get; set; } = string.Empty;

        [Required(ErrorMessage = "Tipo obrigatório.")]
        [Display(Name = "Tipo do Cartão (RFID, QRCode, etc)")]
        public string Type { get; set; } = string.Empty;

        [Display(Name = "Início da Validade")]
        public DateTime? BeginTime { get; set; }

        [Display(Name = "Fim da Validade")]
        public DateTime? EndTime { get; set; }

        [Display(Name = "Status")]
        public string Status { get; set; } = string.Empty;
    }
}
