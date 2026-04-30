using System.ComponentModel.DataAnnotations;

namespace Integracao.ControlID.PoC.ViewModels.Auth
{
    /// <summary>
    /// ViewModel para autenticação/login do usuário.
    /// </summary>
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Informe o nome de usuário ou matrícula.")]
        [Display(Name = "Usuário ou Matrícula")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Informe a senha.")]
        [DataType(DataType.Password)]
        [Display(Name = "Senha")]
        public string Password { get; set; } = string.Empty;

        [Display(Name = "Lembrar-me")]
        public bool RememberMe { get; set; }
    }
}
