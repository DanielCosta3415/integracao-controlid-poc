using System;

namespace Integracao.ControlID.PoC.ViewModels.Media
{
    /// <summary>
    /// ViewModel simplificado para exibição/detalhe de foto.
    /// </summary>
    public class PhotoViewModel
    {
        public long Id { get; set; }
        public long UserId { get; set; }
        public string Base64Image { get; set; } = string.Empty;
        public string ContentType { get; set; } = "image/jpeg";
        public string FileName { get; set; } = string.Empty;
        public string Format { get; set; } = "jpg";
        public bool HasImage { get; set; }
        public DateTime? CreatedAt { get; set; }
    }
}
