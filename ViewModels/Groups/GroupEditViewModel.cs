using System;
using System.ComponentModel.DataAnnotations;

namespace Integracao.ControlID.PoC.ViewModels.Groups
{
    /// <summary>
    /// ViewModel para criação ou edição de grupos.
    /// </summary>
    public class GroupEditViewModel
    {
        public long? Id { get; set; }

        [Required(ErrorMessage = "Nome do grupo obrigatório.")]
        [Display(Name = "Nome do Grupo")]
        public string Name { get; set; } = string.Empty;
    }
}

