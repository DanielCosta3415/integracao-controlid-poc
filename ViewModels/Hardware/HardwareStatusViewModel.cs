using System;

namespace Integracao.ControlID.PoC.ViewModels.Hardware
{
    /// <summary>
    /// ViewModel para exibir o status geral do hardware do equipamento.
    /// </summary>
    public class HardwareStatusViewModel
    {
        public HardwareStatusDto HardwareStatus { get; set; } = new();
        public string ErrorMessage { get; set; } = string.Empty;
    }

    // DTO simplificado para status do hardware
    public class HardwareStatusDto
    {
        public string Status { get; set; } = string.Empty;
        public string Firmware { get; set; } = string.Empty;
        public string Serial { get; set; } = string.Empty;
        public string DeviceId { get; set; } = string.Empty;
        public string IpAddress { get; set; } = string.Empty;
        public int? Temperature { get; set; }
        public bool? DoorOpen { get; set; }
        public bool? Tamper { get; set; }
        public double? PowerVoltage { get; set; }
        public string SensorsInfo { get; set; } = string.Empty;
        public string RawJson { get; set; } = string.Empty;
        public DateTime? RetrievedAt { get; set; }
    }
}

