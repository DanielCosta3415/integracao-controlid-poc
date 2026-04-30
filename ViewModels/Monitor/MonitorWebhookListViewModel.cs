using System.Collections.Generic;

namespace Integracao.ControlID.PoC.ViewModels.Monitor
{
    /// <summary>
    /// ViewModel para exibir a lista de eventos recebidos via webhook.
    /// </summary>
    public class MonitorWebhookListViewModel
    {
        public List<WebhookEventViewModel> Events { get; set; } = new List<WebhookEventViewModel>();
        public string ErrorMessage { get; set; } = string.Empty;
        public string ClearConfirmationPhrase { get; set; } = string.Empty;
        public int TotalCount { get; set; }
        public int DisplayLimit { get; set; }
        public bool IsTruncated => TotalCount > Events.Count;
        public int RetentionDays { get; set; } = 30;
    }
}
