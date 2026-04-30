using Integracao.ControlID.PoC.ViewModels.Monitor;
using System.Collections.Generic;

namespace Integracao.ControlID.PoC.ViewModels.Push
{
    /// <summary>
    /// ViewModel para exibir a lista de eventos/comandos Push recebidos.
    /// </summary>
    public class PushEventListViewModel
    {
        public List<PushEventViewModel> Events { get; set; } = new List<PushEventViewModel>();
        public PushQueueCommandViewModel QueueCommand { get; set; } = new PushQueueCommandViewModel();
        public string ErrorMessage { get; set; } = string.Empty;
        public string StatusMessage { get; set; } = string.Empty;
        public string StatusType { get; set; } = string.Empty;
        public string ClearConfirmationPhrase { get; set; } = string.Empty;
        public int TotalCount { get; set; }
        public int DisplayLimit { get; set; }
        public bool IsTruncated => TotalCount > Events.Count;
        public int RetentionDays { get; set; } = 30;
    }
}

