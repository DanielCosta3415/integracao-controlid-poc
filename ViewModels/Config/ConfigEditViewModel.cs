using System;
using System.ComponentModel.DataAnnotations;

namespace Integracao.ControlID.PoC.ViewModels.Config
{
    /// <summary>
    /// ViewModel para criação ou edição de parâmetros de configuração.
    /// </summary>
    public class ConfigEditViewModel
    {
        public long? Id { get; set; }

        [Required(ErrorMessage = "Grupo obrigatório.")]
        [Display(Name = "Grupo")]
        public string Group { get; set; } = string.Empty;

        [Required(ErrorMessage = "Chave obrigatória.")]
        [Display(Name = "Chave")]
        public string Key { get; set; } = string.Empty;

        [Required(ErrorMessage = "Valor obrigatório.")]
        [Display(Name = "Valor")]
        public string Value { get; set; } = string.Empty;
    }
}
