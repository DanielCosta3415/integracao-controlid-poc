using System.ComponentModel.DataAnnotations;

namespace Integracao.ControlID.PoC.ViewModels.Session
{
    public class SessionEditViewModel
    {
        [Required]
        public long Id { get; set; }

        [Required(ErrorMessage = "O usuário é obrigatório.")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "O endereço do equipamento é obrigatório.")]
        public string DeviceAddress { get; set; } = string.Empty;

        public bool IsActive { get; set; }
        public DateTime? ExpiresAt { get; set; }
        public string? Token { get; set; }
    }
}
