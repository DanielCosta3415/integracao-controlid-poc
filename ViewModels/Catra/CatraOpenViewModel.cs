using System.ComponentModel.DataAnnotations;

namespace Integracao.ControlID.PoC.ViewModels.Catra
{
    /// <summary>
    /// ViewModel para enviar comando de abertura de catraca.
    /// </summary>
    public class CatraOpenViewModel
    {
        [Required(ErrorMessage = "Tipo de comando obrigatório.")]
        [Display(Name = "Tipo de comando")]
        public string ActionType { get; set; } = "allow";

        [Display(Name = "Sentido liberado")]
        public string AllowDirection { get; set; } = "clockwise";

        [Display(Name = "Relé da catraca")]
        [Range(1, 2, ErrorMessage = "O relé da catraca deve ser 1 ou 2.")]
        public int? Relay { get; set; }
    }
}
