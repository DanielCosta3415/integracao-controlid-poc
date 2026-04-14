using System.Collections.Generic;

namespace Integracao.ControlID.PoC.ViewModels.Monitor
{
    /// <summary>
    /// ViewModel para exibir a lista de eventos recebidos via Push.
    /// </summary>
    public class MonitorPushListViewModel
    {
        public List<PushEventViewModel> Events { get; set; } = new List<PushEventViewModel>();
        public string ErrorMessage { get; set; } = string.Empty;
    }
}
