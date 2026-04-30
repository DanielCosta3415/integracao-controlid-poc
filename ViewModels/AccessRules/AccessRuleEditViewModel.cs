using System;
using System.ComponentModel.DataAnnotations;

namespace Integracao.ControlID.PoC.ViewModels.AccessRules
{
    /// <summary>
    /// ViewModel para criação ou edição de regras de acesso.
    /// </summary>
    public class AccessRuleEditViewModel
    {
        public long? Id { get; set; }

        [Required(ErrorMessage = "Nome obrigatório.")]
        [Display(Name = "Nome da Regra")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Tipo obrigatório.")]
        [Display(Name = "Tipo")]
        public int Type { get; set; }

        [Required(ErrorMessage = "Prioridade obrigatória.")]
        [Display(Name = "Prioridade")]
        public int Priority { get; set; }

    }
}

