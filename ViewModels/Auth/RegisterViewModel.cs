using System.ComponentModel.DataAnnotations;

namespace Integracao.ControlID.PoC.ViewModels.Auth
{
    /// <summary>
    /// ViewModel para registro de novo usuário local.
    /// </summary>
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Nome completo obrigatório.")]
        [Display(Name = "Nome completo")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Usuário obrigatório.")]
        [Display(Name = "Usuário ou Matrícula")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "E-mail obrigatório.")]
        [EmailAddress(ErrorMessage = "E-mail inválido.")]
        [Display(Name = "E-mail")]
        public string Email { get; set; } = string.Empty;

        [Phone(ErrorMessage = "Telefone inválido.")]
        [Display(Name = "Telefone")]
        public string Phone { get; set; } = string.Empty;

        [Required(ErrorMessage = "Senha obrigatória.")]
        [DataType(DataType.Password)]
        [Display(Name = "Senha")]
        public string Password { get; set; } = string.Empty;

        [DataType(DataType.Password)]
        [Display(Name = "Confirmação de Senha")]
        [Compare("Password", ErrorMessage = "As senhas não conferem.")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
