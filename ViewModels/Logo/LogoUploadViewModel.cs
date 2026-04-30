using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace Integracao.ControlID.PoC.ViewModels.Logo
{
    /// <summary>
    /// ViewModel para upload/envio de logo do dispositivo.
    /// </summary>
    public class LogoUploadViewModel
    {
        [Range(1, 8, ErrorMessage = "Informe um slot entre 1 e 8.")]
        [Display(Name = "Slot do logo")]
        public long Id { get; set; } = 1;

        [Required(ErrorMessage = "Selecione um arquivo de logo.")]
        [Display(Name = "Logo")]
        public IFormFile LogoFile { get; set; } = default!;
    }
}
