using Integracao.ControlID.PoC.Helpers;

namespace Integracao.ControlID.PoC.ViewModels.Session
{
    public class SessionStatusViewModel
    {
        public string? DeviceAddress { get; set; }
        public string? SessionString { get; set; }
        public bool? SessionValid { get; set; }
        public string? StatusMessage { get; set; }
        public string? StatusType { get; set; }
        public string MaskedSessionString => SecurityTextHelper.MaskSensitiveValue(SessionString);
    }
}
