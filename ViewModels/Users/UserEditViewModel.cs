using System;
using System.ComponentModel.DataAnnotations;

namespace Integracao.ControlID.PoC.ViewModels.Users
{
    /// <summary>
    /// ViewModel para criação ou edição de usuários.
    /// </summary>
    public class UserEditViewModel
    {
        public long? Id { get; set; }

        [Required(ErrorMessage = "Nome obrigatório.")]
        [Display(Name = "Nome completo")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Matrícula obrigatória.")]
        [Display(Name = "Matrícula/Registro")]
        public string Registration { get; set; } = string.Empty;

        [EmailAddress(ErrorMessage = "E-mail inválido.")]
        [Display(Name = "E-mail")]
        public string Email { get; set; } = string.Empty;

        [Phone(ErrorMessage = "Telefone inválido.")]
        [Display(Name = "Telefone")]
        public string Phone { get; set; } = string.Empty;

        [DataType(DataType.Password)]
        [Display(Name = "Senha")]
        public string Password { get; set; } = string.Empty;

        [Display(Name = "Tipo de Usuário")]
        public int UserTypeId { get; set; }

        [Display(Name = "Status")]
        public string Status { get; set; } = string.Empty;

        [Display(Name = "Início da Validade")]
        public DateTime? BeginTime { get; set; }

        [Display(Name = "Fim da Validade")]
        public DateTime? EndTime { get; set; }
    }
}
