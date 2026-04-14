using System.ComponentModel.DataAnnotations;

namespace Integracao.ControlID.PoC.ViewModels.RemoteActions
{
    public class RemoteAuthorizationViewModel
    {
        [Range(1, int.MaxValue, ErrorMessage = "Informe o código do evento.")]
        [Display(Name = "Evento")]
        public int Event { get; set; } = 7;

        [Range(1, long.MaxValue, ErrorMessage = "Informe o ID do usuário.")]
        [Display(Name = "ID do Usuário")]
        public long UserId { get; set; }

        [Required(ErrorMessage = "Informe o nome do usuário.")]
        [Display(Name = "Nome do Usuário")]
        public string UserName { get; set; } = string.Empty;

        [Display(Name = "Enviar indicador de imagem do usuário")]
        public bool UserImage { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Informe o portal.")]
        [Display(Name = "Portal")]
        public int PortalId { get; set; } = 1;

        [Required(ErrorMessage = "Informe a ação remota.")]
        [Display(Name = "Ação")]
        public string ActionName { get; set; } = "door";

        [Required(ErrorMessage = "Informe os parâmetros da ação.")]
        [Display(Name = "Parâmetros da Ação")]
        public string ActionParameters { get; set; } = "door=1";

        public string ResultMessage { get; set; } = string.Empty;
        public string ResultStatusType { get; set; } = string.Empty;
        public string ResponseJson { get; set; } = string.Empty;
        public string ErrorMessage { get; set; } = string.Empty;
    }
}
