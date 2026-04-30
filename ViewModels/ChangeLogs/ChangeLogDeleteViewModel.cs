namespace Integracao.ControlID.PoC.ViewModels.ChangeLogs
{
    /// <summary>
    /// ViewModel para confirmação de exclusão de change log.
    /// </summary>
    public class ChangeLogDeleteViewModel
    {
        public long Id { get; set; }
        public string OperationType { get; set; } = string.Empty;
        public string TableName { get; set; } = string.Empty;
    }
}
