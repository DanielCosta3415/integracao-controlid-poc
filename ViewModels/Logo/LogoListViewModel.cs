using System.Collections.Generic;

namespace Integracao.ControlID.PoC.ViewModels.Logo
{
    /// <summary>
    /// ViewModel para exibir a lista de logos.
    /// </summary>
    public class LogoListViewModel
    {
        public List<LogoViewModel> Logos { get; set; } = [];
        public string ErrorMessage { get; set; } = string.Empty;
    }
}
