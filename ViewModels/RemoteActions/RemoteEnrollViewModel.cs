using System.ComponentModel.DataAnnotations;

namespace Integracao.ControlID.PoC.ViewModels.RemoteActions
{
    public class RemoteEnrollViewModel
    {
        [Required(ErrorMessage = "Informe o tipo de cadastro remoto.")]
        [Display(Name = "Tipo")]
        public string Type { get; set; } = "face";

        [Required(ErrorMessage = "Informe o ID do usuário.")]
        [Display(Name = "ID do usuário")]
        public long UserId { get; set; }

        [Display(Name = "Salvar no equipamento")]
        public bool Save { get; set; } = true;

        [Display(Name = "Sincronizar com callbacks")]
        public bool Sync { get; set; } = true;

        public string ResultMessage { get; set; } = string.Empty;
        public string ResultStatusType { get; set; } = string.Empty;
        public string ResponseJson { get; set; } = string.Empty;
        public string ErrorMessage { get; set; } = string.Empty;
    }
}
