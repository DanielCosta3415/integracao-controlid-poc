namespace Integracao.ControlID.PoC.ViewModels.Logo
{
    /// <summary>
    /// ViewModel para confirmação de exclusão de logo.
    /// </summary>
    public class LogoDeleteViewModel
    {
        public long Id { get; set; }
        public string FileName { get; set; } = string.Empty;
    }
}
