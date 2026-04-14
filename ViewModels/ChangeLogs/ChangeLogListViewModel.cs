using System.Collections.Generic;

namespace Integracao.ControlID.PoC.ViewModels.ChangeLogs
{
    /// <summary>
    /// ViewModel para exibir a lista de change logs.
    /// </summary>
    public class ChangeLogListViewModel
    {
        public List<ChangeLogViewModel> ChangeLogs { get; set; } = new List<ChangeLogViewModel>();
        public string ErrorMessage { get; set; } = string.Empty;
    }
}
