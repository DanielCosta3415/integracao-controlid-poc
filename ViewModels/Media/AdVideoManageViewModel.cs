using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace Integracao.ControlID.PoC.ViewModels.Media
{
    public class AdVideoManageViewModel
    {
        [Display(Name = "Vídeo MP4")]
        public IFormFile? VideoFile { get; set; }

        [Range(128, 4096, ErrorMessage = "O tamanho do bloco deve ficar entre 128 KB e 4096 KB.")]
        [Display(Name = "Tamanho do bloco (KB)")]
        public int ChunkSizeKb { get; set; } = 512;

        [Display(Name = "Ativar vídeo após upload")]
        public bool EnableAfterUpload { get; set; } = true;

        public bool? CustomVideoEnabled { get; set; }
        public string StatusJson { get; set; } = string.Empty;
        public string ResultMessage { get; set; } = string.Empty;
        public string ResultStatusType { get; set; } = string.Empty;
        public string LastResponseJson { get; set; } = string.Empty;
        public string ErrorMessage { get; set; } = string.Empty;
    }
}
