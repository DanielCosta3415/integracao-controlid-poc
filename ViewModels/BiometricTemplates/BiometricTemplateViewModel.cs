using System;

namespace Integracao.ControlID.PoC.ViewModels.BiometricTemplates
{
    /// <summary>
    /// ViewModel simplificado para exibição/detalhe de template biométrico.
    /// </summary>
    public class BiometricTemplateViewModel
    {
        public long Id { get; set; }
        public long UserId { get; set; }
        public string Template { get; set; } = string.Empty;
        public int Type { get; set; }
        public int FingerPosition { get; set; }
        public int FingerType { get; set; }
        public DateTime? BeginTime { get; set; }
        public DateTime? EndTime { get; set; }
        public DateTime? CreatedAt { get; set; }
    }
}
