using System.Collections.Generic;

namespace Integracao.ControlID.PoC.ViewModels.QRCodes
{
    /// <summary>
    /// ViewModel para exibir a lista de QR Codes.
    /// </summary>
    public class QRCodeListViewModel
    {
        public List<QRCodeViewModel> QRCodes { get; set; } = new List<QRCodeViewModel>();
        public string ErrorMessage { get; set; } = string.Empty;
    }
}
