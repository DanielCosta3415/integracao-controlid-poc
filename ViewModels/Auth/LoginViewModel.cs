using System.ComponentModel.DataAnnotations;
using Integracao.ControlID.PoC.Models.Security;

namespace Integracao.ControlID.PoC.ViewModels.Auth
{
    /// <summary>
    /// ViewModel para autenticação/login do usuário.
    /// </summary>
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Informe o nome de usuário ou matrícula.")]
        [StringLength(LocalIdentityPolicy.EmailMaxLength, ErrorMessage = "O identificador deve ter no máximo 254 caracteres.")]
        [Display(Name = "Usuário ou Matrícula")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Informe a senha.")]
        [StringLength(LocalIdentityPolicy.PasswordMaxLength, ErrorMessage = "A senha deve ter no máximo 128 caracteres.")]
        [DataType(DataType.Password)]
        [Display(Name = "Senha")]
        public string Password { get; set; } = string.Empty;

        [Display(Name = "Lembrar-me")]
        public bool RememberMe { get; set; }
    }
}
