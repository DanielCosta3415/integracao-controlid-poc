using System.Collections.Generic;

namespace Integracao.ControlID.PoC.ViewModels.BiometricTemplates
{
    /// <summary>
    /// ViewModel para exibir a lista de templates biométricos.
    /// </summary>
    public class BiometricTemplateListViewModel
    {
        public List<BiometricTemplateViewModel> Templates { get; set; } = new List<BiometricTemplateViewModel>();
        public string ErrorMessage { get; set; } = string.Empty;
    }
}
