using System.Collections.Generic;

namespace Integracao.ControlID.PoC.ViewModels.AccessRules
{
    /// <summary>
    /// ViewModel para exibir a lista de regras de acesso.
    /// </summary>
    public class AccessRuleListViewModel
    {
        public List<AccessRuleViewModel> AccessRules { get; set; } = new List<AccessRuleViewModel>();
        public string ErrorMessage { get; set; } = string.Empty;
    }
}
