namespace Integracao.ControlID.PoC.ViewModels.AccessRules
{
    /// <summary>
    /// ViewModel para confirmação de exclusão de regra de acesso.
    /// </summary>
    public class AccessRuleDeleteViewModel
    {
        public long Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}
