using System.Collections.Generic;

namespace Integracao.ControlID.PoC.Options
{
    public sealed class ControlIdEgressOptions
    {
        public bool RequireAllowedDeviceHosts { get; set; }
        public bool RequireHttpsDeviceUrls { get; set; }
        public List<string> AllowedDeviceHosts { get; set; } = new();
    }
}
