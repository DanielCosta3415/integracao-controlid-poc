using System;

namespace Integracao.ControlID.PoC.ViewModels.ChangeLogs
{
    /// <summary>
    /// ViewModel simplificado para exibição/detalhe de change log.
    /// </summary>
    public class ChangeLogViewModel
    {
        public long Id { get; set; }
        public string OperationType { get; set; } = string.Empty;
        public string TableName { get; set; } = string.Empty;
        public long? TableId { get; set; }
        public DateTime? Timestamp { get; set; }
        public string PerformedBy { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime? CreatedAt { get; set; }
    }
}
