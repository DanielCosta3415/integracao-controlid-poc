using System;

namespace Integracao.ControlID.PoC.ViewModels.AccessRules
{
    /// <summary>
    /// ViewModel simplificado para exibição/detalhe de regra de acesso.
    /// </summary>
    public class AccessRuleViewModel
    {
        public long Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int Type { get; set; }
        public int Priority { get; set; }
    }
}

