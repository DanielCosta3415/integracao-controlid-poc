using System;

namespace Integracao.ControlID.PoC.ViewModels.System
{
    /// <summary>
    /// ViewModel para exibir informações do sistema do equipamento.
    /// </summary>
    public class SystemInfoViewModel
    {
        public SystemInfoDto? SystemInfo { get; set; }
        public string ErrorMessage { get; set; } = string.Empty;
    }

    // DTO simplificado para informações do sistema
    public class SystemInfoDto
    {
        public string Serial { get; set; } = string.Empty;
        public string Version { get; set; } = string.Empty;
        public string Model { get; set; } = string.Empty;
        public string Hostname { get; set; } = string.Empty;
        public string Firmware { get; set; } = string.Empty;
        public string Uptime { get; set; } = string.Empty;
        public string CurrentTime { get; set; } = string.Empty;
        public string IpAddress { get; set; } = string.Empty;
        public string Gateway { get; set; } = string.Empty;
        public string MacAddress { get; set; } = string.Empty;
        public string DeviceId { get; set; } = string.Empty;
        public string OnlineMode { get; set; } = string.Empty;
        public string RawJson { get; set; } = string.Empty;
        public DateTime? RetrievedAt { get; set; }
    }
}
