using System.Collections.Generic;

namespace Integracao.ControlID.PoC.ViewModels.RemoteActions
{
    /// <summary>
    /// ViewModel para exibir a lista de ações remotas disponíveis.
    /// </summary>
    public class RemoteActionListViewModel
    {
        public List<RemoteActionViewModel> Actions { get; set; } = new List<RemoteActionViewModel>();
        public string ErrorMessage { get; set; } = string.Empty;
    }
}
