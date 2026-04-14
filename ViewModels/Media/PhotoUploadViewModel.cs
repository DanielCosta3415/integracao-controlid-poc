using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace Integracao.ControlID.PoC.ViewModels.Media
{
    /// <summary>
    /// ViewModel para upload/envio de foto de usuário.
    /// </summary>
    public class PhotoUploadViewModel
    {
        [Required(ErrorMessage = "Usuário obrigatório.")]
        [Display(Name = "Usuário")]
        public long UserId { get; set; }

        [Required(ErrorMessage = "Selecione um arquivo de foto.")]
        [Display(Name = "Foto")]
        public IFormFile PhotoFile { get; set; } = default!;
    }
}
