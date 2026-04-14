using System.ComponentModel.DataAnnotations;

namespace Integracao.ControlID.PoC.ViewModels.Push
{
    public class PushQueueCommandViewModel
    {
        [Required(ErrorMessage = "O dispositivo de destino é obrigatório.")]
        [Display(Name = "Dispositivo")]
        public string DeviceId { get; set; } = string.Empty;

        [Required(ErrorMessage = "O tipo do comando é obrigatório.")]
        [Display(Name = "Tipo do comando")]
        public string CommandType { get; set; } = "custom";

        [Display(Name = "Usuário relacionado")]
        public string UserId { get; set; } = string.Empty;

        [Required(ErrorMessage = "O payload JSON é obrigatório.")]
        [Display(Name = "Payload")]
        public string Payload { get; set; } = "{\n  \"actions\": []\n}";
    }
}
