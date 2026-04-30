using System.Collections.Generic;

namespace Integracao.ControlID.PoC.ViewModels.Config
{
    /// <summary>
    /// ViewModel para exibir a lista de configurações.
    /// </summary>
    public class ConfigListViewModel
    {
        public List<ConfigViewModel> Configs { get; set; } = new List<ConfigViewModel>();
        public string ErrorMessage { get; set; } = string.Empty;
    }
}
