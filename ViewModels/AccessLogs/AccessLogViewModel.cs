using System;

namespace Integracao.ControlID.PoC.ViewModels.AccessLogs
{
    /// <summary>
    /// ViewModel simplificado para exibição/detalhe de log de acesso.
    /// </summary>
    public class AccessLogViewModel
    {
        public long Id { get; set; }
        public DateTime? Time { get; set; }
        public int Event { get; set; }
        public long? DeviceId { get; set; }
        public long? UserId { get; set; }
        public int? PortalId { get; set; }
        public string Info { get; set; } = string.Empty;
        public DateTime? CreatedAt { get; set; }
    }
}
