namespace Integracao.ControlID.PoC.ViewModels.Session
{
    public class SessionViewModel
    {
        public long Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string DeviceAddress { get; set; } = string.Empty;
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ExpiresAt { get; set; }
        public string? Token { get; set; }
    }
}
