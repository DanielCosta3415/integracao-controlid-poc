namespace Integracao.ControlID.PoC.ViewModels.Session
{
    public class SessionDeactivateViewModel
    {
        public long Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string DeviceAddress { get; set; } = string.Empty;
        public bool IsActive { get; set; }
    }
}