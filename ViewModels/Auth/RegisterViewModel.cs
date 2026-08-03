using System.ComponentModel.DataAnnotations;
using Integracao.ControlID.PoC.Models.Security;

namespace Integracao.ControlID.PoC.ViewModels.Auth
{
    /// <summary>
    /// ViewModel para registro de novo usuário local.
    /// </summary>
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Nome completo obrigatório.")]
        [StringLength(LocalIdentityPolicy.NameMaxLength, MinimumLength = 2, ErrorMessage = "Informe um nome entre 2 e 160 caracteres.")]
        [Display(Name = "Nome completo")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Usuário obrigatório.")]
        [StringLength(LocalIdentityPolicy.UsernameMaxLength, MinimumLength = LocalIdentityPolicy.UsernameMinLength, ErrorMessage = "O usuário deve ter entre 3 e 128 caracteres.")]
        [RegularExpression(LocalIdentityPolicy.UsernameAllowedPattern, ErrorMessage = "Use apenas letras, números, ponto, hífen, sublinhado ou @ no usuário.")]
        [Display(Name = "Usuário ou Matrícula")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "E-mail obrigatório.")]
        [EmailAddress(ErrorMessage = "E-mail inválido.")]
        [StringLength(LocalIdentityPolicy.EmailMaxLength, ErrorMessage = "O e-mail deve ter no máximo 254 caracteres.")]
        [Display(Name = "E-mail")]
        public string Email { get; set; } = string.Empty;

        [Phone(ErrorMessage = "Telefone inválido.")]
        [StringLength(LocalIdentityPolicy.PhoneMaxLength, ErrorMessage = "O telefone deve ter no máximo 32 caracteres.")]
        [Display(Name = "Telefone")]
        public string Phone { get; set; } = string.Empty;

        [Required(ErrorMessage = "Senha obrigatória.")]
        [StringLength(LocalIdentityPolicy.PasswordMaxLength, MinimumLength = LocalIdentityPolicy.PasswordMinLength, ErrorMessage = "A senha deve ter entre 12 e 128 caracteres.")]
        [DataType(DataType.Password)]
        [Display(Name = "Senha")]
        public string Password { get; set; } = string.Empty;

        [DataType(DataType.Password)]
        [StringLength(LocalIdentityPolicy.PasswordMaxLength, ErrorMessage = "A confirmação deve ter no máximo 128 caracteres.")]
        [Display(Name = "Confirmação de Senha")]
        [Compare("Password", ErrorMessage = "As senhas não conferem.")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
