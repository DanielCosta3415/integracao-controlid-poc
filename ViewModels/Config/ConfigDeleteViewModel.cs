namespace Integracao.ControlID.PoC.ViewModels.Config
{
    /// <summary>
    /// ViewModel para confirmação de exclusão de parâmetro de configuração.
    /// </summary>
    public class ConfigDeleteViewModel
    {
        public long Id { get; set; }
        public string Group { get; set; } = string.Empty;
        public string Key { get; set; } = string.Empty;
    }
}
