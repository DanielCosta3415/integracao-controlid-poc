using System;

namespace Integracao.ControlID.PoC.ViewModels.Errors
{
    /// <summary>
    /// ViewModel para exibir informações detalhadas de um erro.
    /// </summary>
    public class ErrorViewModel
    {
        public string RequestId { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string Path { get; set; } = string.Empty;
        public string StackTrace { get; set; } = string.Empty;
        public int? StatusCode { get; set; }
        public string InnerException { get; set; } = string.Empty;
        public string User { get; set; } = string.Empty;
        public string AdditionalData { get; set; } = string.Empty;
        public DateTime? OccurredAt { get; set; }

        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
    }
}
