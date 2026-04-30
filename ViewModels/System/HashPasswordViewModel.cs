using System.ComponentModel.DataAnnotations;

namespace Integracao.ControlID.PoC.ViewModels.System
{
    /// <summary>
    /// ViewModel para geração de hash de senha via API Control iD.
    /// </summary>
    public class HashPasswordViewModel
    {
        [Required(ErrorMessage = "Informe a senha.")]
        [Display(Name = "Senha")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [Display(Name = "Hash gerado")]
        public string Hash { get; set; } = string.Empty;

        [Display(Name = "Salt gerado")]
        public string Salt { get; set; } = string.Empty;
    }
}
