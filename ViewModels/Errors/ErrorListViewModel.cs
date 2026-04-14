using System.Collections.Generic;

namespace Integracao.ControlID.PoC.ViewModels.Errors
{
    /// <summary>
    /// ViewModel para exibir uma lista de erros ocorridos.
    /// </summary>
    public class ErrorListViewModel
    {
        public List<ErrorViewModel> Errors { get; set; } = new List<ErrorViewModel>();
        public string ErrorMessage { get; set; } = string.Empty;
    }
}
