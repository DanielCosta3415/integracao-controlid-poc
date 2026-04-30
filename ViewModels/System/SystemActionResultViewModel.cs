namespace Integracao.ControlID.PoC.ViewModels.System
{
    /// <summary>
    /// ViewModel para exibir resultado de ações administrativas.
    /// </summary>
    public class SystemActionResultViewModel
    {
        public string Message { get; set; } = string.Empty;
        public string StatusType { get; set; } = string.Empty; // success, danger, warning, etc.
    }
}
