using System.Collections.Generic;

namespace Integracao.ControlID.PoC.ViewModels.Groups
{
    /// <summary>
    /// ViewModel para exibir a lista de grupos.
    /// </summary>
    public class GroupListViewModel
    {
        public List<GroupViewModel> Groups { get; set; } = new List<GroupViewModel>();
        public string ErrorMessage { get; set; } = string.Empty;
    }
}
