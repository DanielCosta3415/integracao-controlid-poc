using System;

namespace Integracao.ControlID.PoC.ViewModels.AccessLogs
{
    /// <summary>
    /// ViewModel para filtro de pesquisa de logs de acesso.
    /// </summary>
    public class AccessLogFilterViewModel
    {
        public long? UserId { get; set; }
        public long? DeviceId { get; set; }
        public int? Event { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }
}
