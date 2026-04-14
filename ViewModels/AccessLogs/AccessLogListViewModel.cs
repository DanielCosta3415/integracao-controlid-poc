using System.Collections.Generic;

namespace Integracao.ControlID.PoC.ViewModels.AccessLogs
{
    /// <summary>
    /// ViewModel para exibir a lista de logs de acesso.
    /// </summary>
    public class AccessLogListViewModel
    {
        public List<AccessLogViewModel> AccessLogs { get; set; } = new List<AccessLogViewModel>();
        public string ErrorMessage { get; set; } = string.Empty;
    }
}
