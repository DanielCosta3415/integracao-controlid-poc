using System;

namespace Integracao.ControlID.PoC.ViewModels.Catra
{
    /// <summary>
    /// ViewModel simplificado para exibição/detalhe de evento de catraca.
    /// </summary>
    public class CatraEventViewModel
    {
        public long Id { get; set; }
        public int Direction { get; set; }
        public DateTime? Time { get; set; }
        public string Info { get; set; } = string.Empty;
        public long? UserId { get; set; }
        public long? DeviceId { get; set; }
        public DateTime? CreatedAt { get; set; }
    }
}
