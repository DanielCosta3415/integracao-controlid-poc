using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Integracao.ControlID.PoC.ViewModels.OperationModes
{
    public class OperationModesViewModel
    {
        public string ActiveSection { get; set; } = string.Empty;
        public string ErrorMessage { get; set; } = string.Empty;
        public string ResultMessage { get; set; } = string.Empty;
        public string ResultStatusType { get; set; } = string.Empty;
        public string ResponseJson { get; set; } = string.Empty;

        public bool IsConnected { get; set; }
        public bool SessionValidated { get; set; }
        public string SessionStatusSummary { get; set; } = "Sessão não verificada.";
        public string CurrentModeKey { get; set; } = "unknown";
        public string CurrentModeLabel { get; set; } = "Indeterminado";
        public string CurrentModeDescription { get; set; } = "Conecte um equipamento para identificar o modo atual.";
        public string CurrentModeTone { get; set; } = "neutral";
        public string CurrentModeEvidence { get; set; } = string.Empty;
        public string? DetectedProductModel { get; set; }
        public string? DetectedSerialNumber { get; set; }
        public string? CallbackBaseUrl { get; set; }

        public bool OnlineEnabled { get; set; }
        public bool LocalIdentificationEnabled { get; set; }
        public bool ExtractTemplateEnabled { get; set; }

        [Display(Name = "Reutilizar device cadastrado")]
        public bool ReuseExistingDevice { get; set; }

        [Display(Name = "ID do device existente")]
        public long? ExistingDeviceId { get; set; }

        [Display(Name = "Server ID em uso")]
        public long? CurrentServerId { get; set; }

        [Display(Name = "Nome do servidor online")]
        public string ServerName { get; set; } = "Control iD PoC";

        [Display(Name = "URL pública da PoC")]
        public string ServerUrl { get; set; } = string.Empty;

        [Display(Name = "Chave pública opcional")]
        public string PublicKey { get; set; } = string.Empty;

        [Display(Name = "Extrair template")]
        public bool ExtractTemplate { get; set; }

        [Range(1, 30, ErrorMessage = "Informe entre 1 e 30 tentativas máximas.")]
        [Display(Name = "Máximo de tentativas online")]
        public int MaxRequestAttempts { get; set; } = 3;

        [Display(Name = "Senha/licença Pro")]
        public string ProLicensePassword { get; set; } = string.Empty;

        [Display(Name = "Senha/licença Enterprise")]
        public string EnterpriseLicensePassword { get; set; } = string.Empty;

        public IReadOnlyList<OperationModeProfileCardViewModel> Profiles { get; set; } = Array.Empty<OperationModeProfileCardViewModel>();
        public IReadOnlyList<OperationModeReadinessViewModel> Readiness { get; set; } = Array.Empty<OperationModeReadinessViewModel>();
        public IReadOnlyList<OperationModeSignalViewModel> RecentSignals { get; set; } = Array.Empty<OperationModeSignalViewModel>();
    }

    public class OperationModeProfileCardViewModel
    {
        public string Key { get; set; } = string.Empty;
        public string Label { get; set; } = string.Empty;
        public string Summary { get; set; } = string.Empty;
        public string Tone { get; set; } = "neutral";
        public bool IsCurrent { get; set; }
        public string Requirements { get; set; } = string.Empty;
        public string Checklist { get; set; } = string.Empty;
        public string SubmitAction { get; set; } = string.Empty;
        public string SubmitLabel { get; set; } = string.Empty;
        public string SubmitAriaLabel { get; set; } = string.Empty;
    }

    public class OperationModeReadinessViewModel
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Path { get; set; } = string.Empty;
        public string Tone { get; set; } = "neutral";
        public string StatusText { get; set; } = "Aguardando";
        public DateTime? LastReceivedAt { get; set; }
    }

    public class OperationModeSignalViewModel
    {
        public string Label { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Tone { get; set; } = "neutral";
        public DateTime ReceivedAt { get; set; }
    }
}
