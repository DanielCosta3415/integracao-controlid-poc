using System.ComponentModel.DataAnnotations;

namespace Integracao.ControlID.PoC.ViewModels.Hardware
{
    /// <summary>
    /// ViewModel para acionar um relé.
    /// </summary>
    public class RelayActionViewModel
    {
        [Required(ErrorMessage = "Informe o número da porta.")]
        [Display(Name = "Número da Porta")]
        public int DoorNumber { get; set; }
    }
}
