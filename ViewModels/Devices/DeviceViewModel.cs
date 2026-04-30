using System;

namespace Integracao.ControlID.PoC.ViewModels.Devices
{
    /// <summary>
    /// ViewModel simplificado para exibição/detalhe de dispositivo.
    /// </summary>
    public class DeviceViewModel
    {
        public long Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Ip { get; set; } = string.Empty;
        public string SerialNumber { get; set; } = string.Empty;
        public string Firmware { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime? RegisteredAt { get; set; }
        public DateTime? LastSeenAt { get; set; }
        public string Description { get; set; } = string.Empty;
    }
}
