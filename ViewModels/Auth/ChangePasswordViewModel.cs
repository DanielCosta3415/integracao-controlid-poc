using System.ComponentModel.DataAnnotations;
using Integracao.ControlID.PoC.Models.Security;

namespace Integracao.ControlID.PoC.ViewModels.Auth
{
    public class ChangePasswordViewModel
    {
        [Required(ErrorMessage = "Usuário obrigatório.")]
        [StringLength(LocalIdentityPolicy.EmailMaxLength, ErrorMessage = "O identificador deve ter no máximo 254 caracteres.")]
        [Display(Name = "Usuário local")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Senha atual obrigatória.")]
        [StringLength(LocalIdentityPolicy.PasswordMaxLength, ErrorMessage = "A senha atual deve ter no máximo 128 caracteres.")]
        [DataType(DataType.Password)]
        [Display(Name = "Senha atual")]
        public string CurrentPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Nova senha obrigatória.")]
        [StringLength(LocalIdentityPolicy.PasswordMaxLength, MinimumLength = LocalIdentityPolicy.PasswordMinLength, ErrorMessage = "A nova senha deve ter entre 12 e 128 caracteres.")]
        [DataType(DataType.Password)]
        [Display(Name = "Nova senha")]
        public string NewPassword { get; set; } = string.Empty;

        [DataType(DataType.Password)]
        [StringLength(LocalIdentityPolicy.PasswordMaxLength, ErrorMessage = "A confirmação deve ter no máximo 128 caracteres.")]
        [Display(Name = "Confirmação da nova senha")]
        [Compare("NewPassword", ErrorMessage = "As senhas não conferem.")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
