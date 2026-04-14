using System;

namespace Integracao.ControlID.PoC.ViewModels.Monitor
{
    /// <summary>
    /// ViewModel para exibir os detalhes de um evento de webhook.
    /// </summary>
    public class WebhookEventViewModel
    {
        public Guid EventId { get; set; }
        public DateTime ReceivedAt { get; set; }
        public string RawJson { get; set; } = string.Empty;
        public string EventType { get; set; } = string.Empty;
        public string DeviceId { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public string Payload { get; set; } = string.Empty;
    }
}
