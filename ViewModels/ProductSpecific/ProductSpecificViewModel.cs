using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace Integracao.ControlID.PoC.ViewModels.ProductSpecific
{
    public class ProductSpecificViewModel
    {
        public string ActiveSection { get; set; } = string.Empty;
        public string ErrorMessage { get; set; } = string.Empty;
        public string ResultMessage { get; set; } = string.Empty;
        public string ResultStatusType { get; set; } = string.Empty;
        public string ResponseJson { get; set; } = string.Empty;

        [Display(Name = "Senha/licença do upgrade Pro")]
        public string IdFaceProPassword { get; set; } = "ABCDE12345";

        [Display(Name = "Senha/licença do upgrade Enterprise")]
        public string IdFlexEnterprisePassword { get; set; } = "ABCDE12345";

        [Display(Name = "Máscara")]
        public string FaceMaskDetectionEnabled { get; set; } = "0";

        [Display(Name = "Modo veicular")]
        public bool FaceVehicleModeEnabled { get; set; }

        [Range(0, 120000)]
        [Display(Name = "Tempo para reabrir a mesma face (ms)")]
        public int FaceMaxIdentifiedDuration { get; set; } = 30000;

        [Display(Name = "Tela sempre ligada")]
        public bool FaceScreenAlwaysOn { get; set; } = true;

        [Display(Name = "Zoom da câmera")]
        public string FaceCameraOverlayZoom { get; set; } = "1.00";

        [Display(Name = "Recorte vertical")]
        public string FaceCameraOverlayVerticalCrop { get; set; } = "0.00";

        [Display(Name = "Restringir identificação à área da tela")]
        public bool FaceLimitIdentificationToDisplayRegion { get; set; }

        [Display(Name = "Distância mínima (min_detect_bounds_width)")]
        public string FaceMinDetectBoundsWidth { get; set; } = "0.29";

        [Display(Name = "Threshold dos LEDs brancos")]
        public string FaceLightThresholdLedActivation { get; set; } = "40";

        [Range(0, 100)]
        [Display(Name = "Brilho dos LEDs brancos (%)")]
        public int FaceLedBrightness { get; set; } = 70;

        [Display(Name = "Cartão habilitado")]
        public bool IdentifierCardEnabled { get; set; } = true;

        [Display(Name = "Face habilitada")]
        public bool IdentifierFaceEnabled { get; set; } = true;

        [Display(Name = "QR Code habilitado")]
        public bool IdentifierQrCodeEnabled { get; set; } = true;

        [Display(Name = "PIN habilitado")]
        public bool IdentifierPinEnabled { get; set; }

        [Display(Name = "Módulo do QR Code")]
        public string QrModule { get; set; } = "face_id";

        [Display(Name = "Modo do QR Code")]
        public string QrCodeLegacyModeEnabled { get; set; } = "0";

        [Display(Name = "TOTP habilitado")]
        public bool QrTotpEnabled { get; set; }

        [Range(1, 3600)]
        [Display(Name = "Janela TOTP (s)")]
        public int QrTotpWindowSize { get; set; } = 30;

        [Range(1, 20)]
        [Display(Name = "Número de janelas TOTP")]
        public int QrTotpWindowNum { get; set; } = 5;

        [Display(Name = "Cartão de uso único")]
        public bool QrTotpSingleUse { get; set; } = true;

        [Display(Name = "Offset de fuso TOTP (s)")]
        public int QrTotpTzOffset { get; set; }

        [Display(Name = "Resize de screenshot")]
        public string ScreenshotResize { get; set; } = "0.42";

        [Display(Name = "Modo de energia")]
        public string EnergyMode { get; set; } = "0";

        [Display(Name = "Display custom")]
        public string EnergyDisplayCustom { get; set; } = "6";

        [Display(Name = "Som custom")]
        public string EnergySoundCustom { get; set; } = "6";

        [Display(Name = "IR custom")]
        public string EnergyIrCustom { get; set; } = "10";

        [Display(Name = "LED branco custom")]
        public string EnergyLedCustom { get; set; } = "10";

        [Display(Name = "RTSP habilitado")]
        public bool StreamingRtspEnabled { get; set; }

        [Range(1, 65535)]
        [Display(Name = "Porta RTSP")]
        public int StreamingRtspPort { get; set; } = 554;

        [Display(Name = "Usuário RTSP")]
        public string StreamingRtspUsername { get; set; } = "admin";

        [Display(Name = "Senha RTSP")]
        public string StreamingRtspPassword { get; set; } = "admin";

        [Display(Name = "Usar câmera RGB")]
        public bool StreamingRtspRgb { get; set; } = true;

        [Display(Name = "Codec RTSP")]
        public string StreamingRtspCodec { get; set; } = "h264";

        [Display(Name = "ONVIF habilitado")]
        public bool StreamingOnvifEnabled { get; set; }

        [Range(1, 65535)]
        [Display(Name = "Porta ONVIF")]
        public int StreamingOnvifPort { get; set; } = 8000;

        [Display(Name = "Imagem espelhada")]
        public bool StreamingRtspFlipped { get; set; }

        [Display(Name = "Watermark habilitado")]
        public bool StreamingWatermarkEnabled { get; set; } = true;

        [Display(Name = "Logo da marca d'água habilitado")]
        public bool StreamingWatermarkLogoEnabled { get; set; } = true;

        [Display(Name = "Logo personalizado da marca d'água habilitado")]
        public bool StreamingWatermarkCustomLogoEnabled { get; set; }

        [Display(Name = "Intercom SIP habilitado")]
        public bool SipEnabled { get; set; }

        [Display(Name = "Servidor SIP")]
        public string SipServerIp { get; set; } = "sip.example.com";

        [Range(1, 65535)]
        [Display(Name = "Porta SIP")]
        public int SipServerPort { get; set; } = 5060;

        [Range(0, 65535)]
        [Display(Name = "Porta inicial RTP")]
        public int SipServerOutboundPort { get; set; } = 10000;

        [Range(0, 65535)]
        [Display(Name = "Faixa RTP")]
        public int SipServerOutboundPortRange { get; set; } = 1000;

        [Display(Name = "Ramal numérico")]
        public bool SipNumericBranchEnabled { get; set; } = true;

        [Display(Name = "Ramal")]
        public string SipBranch { get; set; } = "987";

        [Display(Name = "Login SIP")]
        public string SipLogin { get; set; } = "987";

        [Display(Name = "Senha SIP")]
        public string SipPassword { get; set; } = "123456";

        [Display(Name = "Peer to peer")]
        public bool SipPeerToPeerEnabled { get; set; }

        [Range(1, 3600)]
        [Display(Name = "Período de consulta do registro (s)")]
        public int SipRegStatusQueryPeriod { get; set; } = 60;

        [Range(1, 3600)]
        [Display(Name = "Keep-alive (s)")]
        public int SipServerRetryInterval { get; set; } = 5;

        [Range(1, 7200)]
        [Display(Name = "Duração máxima da chamada (s)")]
        public int SipMaxCallTime { get; set; } = 300;

        [Range(0, 10000)]
        [Display(Name = "Debounce do botão (ms)")]
        public int SipPushButtonDebounce { get; set; } = 50;

        [Display(Name = "Auto answer")]
        public bool SipAutoAnswerEnabled { get; set; }

        [Range(0, 120)]
        [Display(Name = "Delay do auto answer (s)")]
        public int SipAutoAnswerDelay { get; set; } = 5;

        [Display(Name = "Botão de chamada na tela")]
        public bool SipAutoCallButtonEnabled { get; set; } = true;

        [Display(Name = "Botão externo habilitado")]
        public bool SipRexEnabled { get; set; }

        [Display(Name = "Modo de discagem")]
        public string SipDialingDisplayMode { get; set; } = "0";

        [Display(Name = "Destino de discagem automática")]
        public string SipAutoCallTarget { get; set; } = "456";

        [Display(Name = "Identificador visível")]
        public string SipCustomIdentifierAutoCall { get; set; } = "Reception";

        [Display(Name = "Video SIP habilitado")]
        public bool SipVideoEnabled { get; set; }

        [Display(Name = "Toque customizado habilitado")]
        public bool SipCustomAudioEnabled { get; set; }

        [Display(Name = "Ganho do toque customizado")]
        public string SipCustomAudioVolumeGain { get; set; } = "1";

        [Range(1, 10)]
        [Display(Name = "Volume do microfone")]
        public int SipMicVolume { get; set; } = 5;

        [Range(1, 10)]
        [Display(Name = "Volume do alto-falante")]
        public int SipSpeakerVolume { get; set; } = 7;

        [Display(Name = "Liberação por DTMF habilitada")]
        public bool SipOpenDoorEnabled { get; set; }

        [Display(Name = "Código DTMF")]
        public string SipOpenDoorCommand { get; set; } = "12345";

        [Display(Name = "Identificação durante a chamada")]
        public bool SipFacialIdDuringCallEnabled { get; set; }

        [Display(Name = "Ramal para ligar agora")]
        public string SipCallTarget { get; set; } = "503";

        [Range(1, 100)]
        [Display(Name = "Bloco atual do áudio SIP")]
        public int SipAudioCurrent { get; set; } = 1;

        [Range(1, 100)]
        [Display(Name = "Total de blocos do áudio SIP")]
        public int SipAudioTotal { get; set; } = 1;

        [Display(Name = "Arquivo WAV do SIP")]
        public IFormFile? SipAudioFile { get; set; }

        [Display(Name = "Existe áudio SIP personalizado")]
        public bool SipAudioExists { get; set; }

        [Display(Name = "Status SIP")]
        public int SipStatusCode { get; set; } = -1;

        [Display(Name = "Em chamada")]
        public bool SipInCall { get; set; }

        [Display(Name = "Não identificado")]
        public string AccessAudioNotIdentified { get; set; } = "default";

        [Display(Name = "Autorizado")]
        public string AccessAudioAuthorized { get; set; } = "default";

        [Display(Name = "Não autorizado")]
        public string AccessAudioNotAuthorized { get; set; } = "default";

        [Display(Name = "Uso de máscara")]
        public string AccessAudioUseMask { get; set; } = "default";

        [Display(Name = "Ganho do volume")]
        public string AccessAudioVolumeGain { get; set; } = "2";

        [Display(Name = "Evento do áudio")]
        public string AccessAudioEvent { get; set; } = "authorized";

        [Range(1, 100)]
        [Display(Name = "Bloco atual do áudio de acesso")]
        public int AccessAudioCurrent { get; set; } = 1;

        [Range(1, 100)]
        [Display(Name = "Total de blocos do áudio de acesso")]
        public int AccessAudioTotal { get; set; } = 1;

        [Display(Name = "Arquivo WAV do evento")]
        public IFormFile? AccessAudioFile { get; set; }

        [Display(Name = "Existe áudio para não identificado")]
        public bool AccessAudioHasNotIdentified { get; set; }

        [Display(Name = "Existe áudio para autorizado")]
        public bool AccessAudioHasAuthorized { get; set; }

        [Display(Name = "Existe áudio para não autorizado")]
        public bool AccessAudioHasNotAuthorized { get; set; }

        [Display(Name = "Existe áudio para uso de máscara")]
        public bool AccessAudioHasUseMask { get; set; }

        [Display(Name = "Modo da SecBox")]
        public string SignalSecBoxOutMode { get; set; } = "0";

        [Display(Name = "Modo do relé interno")]
        public string SignalRelayOutMode { get; set; } = "0";

        [Display(Name = "Relé interno habilitado")]
        public bool SignalRelayEnabled { get; set; } = true;

        [Display(Name = "Fechamento automático do relé")]
        public bool SignalRelayAutoClose { get; set; } = true;

        [Range(100, 10000)]
        [Display(Name = "Timeout do relé (ms)")]
        public int SignalRelayTimeout { get; set; } = 3000;

        [Display(Name = "GPIO1 modo")]
        public string SignalGpioExt1Mode { get; set; } = "0";

        [Display(Name = "GPIO1 idle")]
        public string SignalGpioExt1Idle { get; set; } = "0";

        [Display(Name = "GPIO2 modo")]
        public string SignalGpioExt2Mode { get; set; } = "0";

        [Display(Name = "GPIO2 idle")]
        public string SignalGpioExt2Idle { get; set; } = "0";

        [Display(Name = "GPIO3 modo")]
        public string SignalGpioExt3Mode { get; set; } = "0";

        [Display(Name = "GPIO3 idle")]
        public string SignalGpioExt3Idle { get; set; } = "0";
    }
}
