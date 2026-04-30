using System;

namespace Integracao.ControlID.PoC.ViewModels.Groups
{
    /// <summary>
    /// ViewModel simplificado para exibição/detalhe de grupo.
    /// </summary>
    public class GroupViewModel
    {
        public long Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}

