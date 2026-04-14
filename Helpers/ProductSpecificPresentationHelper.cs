using Integracao.ControlID.PoC.ViewModels.ProductSpecific;
using Integracao.ControlID.PoC.ViewModels.Shared;
using Integracao.ControlID.PoC.Services.ProductSpecific;

namespace Integracao.ControlID.PoC.Helpers;

// Centralize page-specific copy and counters so the Razor view stays declarative and testable.
public static class ProductSpecificPresentationHelper
{
    public static int CountConfiguredAccessAudioEvents(ProductSpecificViewModel model)
    {
        return (model.AccessAudioHasNotIdentified ? 1 : 0)
             + (model.AccessAudioHasAuthorized ? 1 : 0)
             + (model.AccessAudioHasNotAuthorized ? 1 : 0)
             + (model.AccessAudioHasUseMask ? 1 : 0);
    }

    public static RawResponsePanelViewModel? BuildResponsePanel(ProductSpecificViewModel model)
    {
        if (string.IsNullOrWhiteSpace(model.ResponseJson))
            return null;

        var (title, description) = GetResponseCopy(model.ActiveSection);
        return new RawResponsePanelViewModel
        {
            Title = title,
            Description = description,
            Content = model.ResponseJson,
            PanelId = "product-specific-response-json",
            BadgeText = string.IsNullOrWhiteSpace(model.ResultStatusType) ? "Retorno técnico" : model.ResultStatusType,
            BadgeTone = string.IsNullOrWhiteSpace(model.ResultStatusType) ? "neutral" : model.ResultStatusType
        };
    }

    private static (string Title, string Description) GetResponseCopy(string? activeSection)
    {
        return (activeSection ?? string.Empty) switch
        {
            ProductSpecificSections.Upgrades => ("Resposta dos upgrades", "Retorno bruto das rotinas de upgrade de licença."),
            ProductSpecificSections.Facial => ("Resposta das configurações faciais", "Retorno bruto do conjunto de reconhecimento facial e métodos de identificação."),
            ProductSpecificSections.Qr => ("Resposta do QR Code e TOTP", "Retorno bruto das definições de QR Code, legado e TOTP."),
            ProductSpecificSections.Power => ("Resposta de energia e screenshot", "Retorno bruto do ajuste de energia, display, áudio e screenshot."),
            ProductSpecificSections.Streaming => ("Resposta do streaming", "Retorno bruto das definições de RTSP, ONVIF e marca d'água."),
            ProductSpecificSections.Sip => ("Resposta do intercom SIP", "Retorno bruto da configuração do intercom e dos comandos de chamada."),
            ProductSpecificSections.SipAudio => ("Resposta do toque personalizado do SIP", "Retorno bruto das operações de envio, verificação ou download do toque SIP."),
            ProductSpecificSections.AccessAudio => ("Resposta dos áudios de eventos", "Retorno bruto das operações de áudio por evento do iDFace."),
            ProductSpecificSections.Signals => ("Resposta dos sinais configuráveis", "Retorno bruto das saídas customizadas e do refresh de LEDs."),
            _ => ("Última resposta oficial", "JSON bruto da última operação executada nesta superfície.")
        };
    }
}
