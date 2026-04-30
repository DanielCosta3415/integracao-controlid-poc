using System.ComponentModel.DataAnnotations;

namespace Integracao.ControlID.PoC.ViewModels.AdvancedOfficial
{
    public class CameraCaptureViewModel
    {
        [Required]
        [Display(Name = "Tipo de frame")]
        public string FrameType { get; set; } = "camera";

        [Required]
        [Display(Name = "Câmera")]
        public string Camera { get; set; } = "rgb";

        public string ErrorMessage { get; set; } = string.Empty;
        public string ResultMessage { get; set; } = string.Empty;
        public string ResultStatusType { get; set; } = string.Empty;
        public string ResponseJson { get; set; } = string.Empty;
        public string Base64Image { get; set; } = string.Empty;
        public string ImageContentType { get; set; } = "image/png";
    }
}
