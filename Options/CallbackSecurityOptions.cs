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
        public bool RequireSignedRequests { get; set; }
        public string SignatureHeaderName { get; set; } = "X-ControlID-Signature";
        public string TimestampHeaderName { get; set; } = "X-ControlID-Timestamp";
        public string NonceHeaderName { get; set; } = "X-ControlID-Nonce";
        public int MaxClockSkewSeconds { get; set; } = 300;
        public int NonceTtlSeconds { get; set; } = 600;
    }
}
