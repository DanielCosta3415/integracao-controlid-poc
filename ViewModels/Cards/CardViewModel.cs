using System;

namespace Integracao.ControlID.PoC.ViewModels.Cards
{
    /// <summary>
    /// ViewModel simplificado para exibição/detalhe de cartão.
    /// </summary>
    public class CardViewModel
    {
        public long Id { get; set; }
        public long UserId { get; set; }
        public string Value { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public DateTime? BeginTime { get; set; }
        public DateTime? EndTime { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime? CreatedAt { get; set; }
    }
}
