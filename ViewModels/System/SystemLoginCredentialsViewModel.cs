using System.ComponentModel.DataAnnotations;

namespace Integracao.ControlID.PoC.ViewModels.System
{
    public class SystemLoginCredentialsViewModel
    {
        [Required(ErrorMessage = "Informe o novo usuário de login.")]
        [Display(Name = "Novo Usuário")]
        public string Login { get; set; } = string.Empty;

        [Required(ErrorMessage = "Informe a nova senha de login.")]
        [Display(Name = "Nova Senha")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        public string ResultMessage { get; set; } = string.Empty;
        public string ResultStatusType { get; set; } = string.Empty;
        public string ResponseJson { get; set; } = string.Empty;
        public string ErrorMessage { get; set; } = string.Empty;
    }
}
