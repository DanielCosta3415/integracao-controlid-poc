namespace Integracao.ControlID.PoC.ViewModels.RemoteActions
{
    /// <summary>
    /// ViewModel simplificado para exibição/detalhe de ação remota.
    /// </summary>
    public class RemoteActionViewModel
    {
        public string Action { get; set; } = string.Empty;
        public string DisplayName { get; set; } = string.Empty;
        public string Parameters { get; set; } = string.Empty;
        public string Result { get; set; } = string.Empty;
    }
}
