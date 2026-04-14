using System.ComponentModel.DataAnnotations;

namespace Integracao.ControlID.PoC.ViewModels.Auth
{
    /// <summary>
    /// ViewModel para troca de senha do usuário local.
    /// </summary>
    public class ChangePasswordViewModel
    {
        [Required(ErrorMessage = "Senha atual obrigatória.")]
        [DataType(DataType.Password)]
        [Display(Name = "Senha atual")]
        public string CurrentPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Nova senha obrigatória.")]
        [DataType(DataType.Password)]
        [Display(Name = "Nova senha")]
        public string NewPassword { get; set; } = string.Empty;

        [DataType(DataType.Password)]
        [Display(Name = "Confirmação da nova senha")]
        [Compare("NewPassword", ErrorMessage = "As senhas não conferem.")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
