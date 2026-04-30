using System;

namespace Integracao.ControlID.PoC.ViewModels.Config
{
    /// <summary>
    /// ViewModel simplificado para exibição/detalhe de configuração.
    /// </summary>
    public class ConfigViewModel
    {
        public long Id { get; set; }
        public string Group { get; set; } = string.Empty;
        public string Key { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
    }
}
