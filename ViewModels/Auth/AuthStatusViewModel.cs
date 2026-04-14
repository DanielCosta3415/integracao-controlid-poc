using Integracao.ControlID.PoC.Helpers;

namespace Integracao.ControlID.PoC.ViewModels.Auth
{
    public class AuthStatusViewModel
    {
        public string? DeviceAddress { get; set; }
        public string? SessionString { get; set; }
        public bool IsAuthenticated { get; set; }
        public string MaskedSessionString => SecurityTextHelper.MaskSensitiveValue(SessionString);
    }
}
