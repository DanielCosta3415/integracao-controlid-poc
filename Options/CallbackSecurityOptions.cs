using System.Collections.Generic;

namespace Integracao.ControlID.PoC.Options
{
    public class CallbackSecurityOptions
    {
        public int MaxBodyBytes { get; set; } = 1024 * 1024;
        public bool RequireSharedKey { get; set; }
        public string SharedKeyHeaderName { get; set; } = "X-ControlID-Callback-Key";
        public string SharedKey { get; set; } = string.Empty;
        public bool AllowLoopback { get; set; } = true;
        public List<string> AllowedRemoteIps { get; set; } = new();
    }
}
