namespace Integracao.ControlID.PoC.ViewModels.AccessLogs
{
    /// <summary>
    /// ViewModel para confirmação de exclusão de log de acesso.
    /// </summary>
    public class AccessLogDeleteViewModel
    {
        public long Id { get; set; }
        public string Info { get; set; } = string.Empty;
    }
}
