using System;

namespace Integracao.ControlID.PoC.ViewModels.QRCodes
{
    /// <summary>
    /// ViewModel simplificado para exibição/detalhe de QR Code.
    /// </summary>
    public class QRCodeViewModel
    {
        public long Id { get; set; }
        public long UserId { get; set; }
        public string Value { get; set; } = string.Empty;
    }
}

