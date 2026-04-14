namespace Integracao.ControlID.PoC.ViewModels.QRCodes
{
    /// <summary>
    /// ViewModel para confirmação de exclusão de QR Code.
    /// </summary>
    public class QRCodeDeleteViewModel
    {
        public long Id { get; set; }
        public long UserId { get; set; }
        public string Value { get; set; } = string.Empty;
    }
}
