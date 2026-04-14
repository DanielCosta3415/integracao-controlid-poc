using System;

namespace Integracao.ControlID.PoC.ViewModels.Logo
{
    /// <summary>
    /// ViewModel simplificado para exibição/detalhe de logo.
    /// </summary>
    public class LogoViewModel
    {
        public long Id { get; set; }
        public string Base64Image { get; set; } = string.Empty;
        public string ContentType { get; set; } = "image/png";
        public string FileName { get; set; } = string.Empty;
        public string Format { get; set; } = "png";
        public bool HasImage { get; set; }
    }
}
