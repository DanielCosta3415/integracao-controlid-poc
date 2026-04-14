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

        [Display(Name = "Senha/licenca do upgrade Pro")]
        public string IdFaceProPassword { get; set; } = "ABCDE12345";

        [Display(Name = "Senha/licenca do upgrade Enterprise")]
        public string IdFlexEnterprisePassword { get; set; } = "ABCDE12345";

        [Display(Name = "Mascara")]
        public string FaceMaskDetectionEnabled { get; set; } = "0";

        [Display(Name = "Modo veicular")]
        public bool FaceVehicleModeEnabled { get; set; }

        [Range(0, 120000)]
        [Display(Name = "Tempo para reabrir a mesma face (ms)")]
        public int FaceMaxIdentifiedDuration { get; set; } = 30000;

        [Display(Name = "Tela sempre ligada")]
        public bool FaceScreenAlwaysOn { get; set; } = true;

        [Display(Name = "Zoom da camera")]
        public string FaceCameraOverlayZoom { get; set; } = "1.00";

        [Display(Name = "Recorte vertical")]
        public string FaceCameraOverlayVerticalCrop { get; set; } = "0.00";

        [Display(Name = "Restringir identificacao a area da tela")]
        public bool FaceLimitIdentificationToDisplayRegion { get; set; }

        [Display(Name = "Distancia minima (min_detect_bounds_width)")]
        public string FaceMinDetectBoundsWidth { get; set; } = "0.29";

        [Display(Name = "Threshold dos LEDs brancos")]
        public string FaceLightThresholdLedActivation { get; set; } = "40";

        [Range(0, 100)]
        [Display(Name = "Brilho dos LEDs brancos (%)")]
        public int FaceLedBrightness { get; set; } = 70;

        [Display(Name = "Cartao habilitado")]
        public bool IdentifierCardEnabled { get; set; } = true;

        [Display(Name = "Face habilitada")]
        public bool IdentifierFaceEnabled { get; set; } = true;

        [Display(Name = "QR Code habilitado")]
        public bool IdentifierQrCodeEnabled { get; set; } = true;

        [Display(Name = "PIN habilitado")]
        public bool IdentifierPinEnabled { get; set; }

        [Display(Name = "Modulo do QR Code")]
        public string QrModule { get; set; } = "face_id";

        [Display(Name = "Modo do QR Code")]
        public string QrCodeLegacyModeEnabled { get; set; } = "0";

        [Display(Name = "TOTP habilitado")]
        public bool QrTotpEnabled { get; set; }

        [Range(1, 3600)]
        [Display(Name = "Janela TOTP (s)")]
        public int QrTotpWindowSize { get; set; } = 30;

        [Range(1, 20)]
        [Display(Name = "Numero de janelas TOTP")]
        public int QrTotpWindowNum { get; set; } = 5;

        [Display(Name = "Cartao de uso unico")]
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

        [Display(Name = "Usuario RTSP")]
        public string StreamingRtspUsername { get; set; } = "admin";

        [Display(Name = "Senha RTSP")]
        public string StreamingRtspPassword { get; set; } = "admin";

        [Display(Name = "Usar camera RGB")]
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

        [Display(Name = "Logo da watermark habilitado")]
        public bool StreamingWatermarkLogoEnabled { get; set; } = true;

        [Display(Name = "Logo customizado da watermark habilitado")]
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

        [Display(Name = "Ramal numerico")]
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
        [Display(Name = "Periodo de consulta do registro (s)")]
        public int SipRegStatusQueryPeriod { get; set; } = 60;

        [Range(1, 3600)]
        [Display(Name = "Keep-alive (s)")]
        public int SipServerRetryInterval { get; set; } = 5;

        [Range(1, 7200)]
        [Display(Name = "Duracao maxima da chamada (s)")]
        public int SipMaxCallTime { get; set; } = 300;

        [Range(0, 10000)]
        [Display(Name = "Debounce do botao (ms)")]
        public int SipPushButtonDebounce { get; set; } = 50;

        [Display(Name = "Auto answer")]
        public bool SipAutoAnswerEnabled { get; set; }

        [Range(0, 120)]
        [Display(Name = "Delay do auto answer (s)")]
        public int SipAutoAnswerDelay { get; set; } = 5;

        [Display(Name = "Botao de chamada na tela")]
        public bool SipAutoCallButtonEnabled { get; set; } = true;

        [Display(Name = "Botao externo habilitado")]
        public bool SipRexEnabled { get; set; }

        [Display(Name = "Modo de discagem")]
        public string SipDialingDisplayMode { get; set; } = "0";

        [Display(Name = "Destino auto dial")]
        public string SipAutoCallTarget { get; set; } = "456";

        [Display(Name = "Identificador visivel")]
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

        [Display(Name = "Liberacao por DTMF habilitada")]
        public bool SipOpenDoorEnabled { get; set; }

        [Display(Name = "Codigo DTMF")]
        public string SipOpenDoorCommand { get; set; } = "12345";

        [Display(Name = "Identificacao durante a chamada")]
        public bool SipFacialIdDuringCallEnabled { get; set; }

        [Display(Name = "Ramal para ligar agora")]
        public string SipCallTarget { get; set; } = "503";

        [Range(1, 100)]
        [Display(Name = "Bloco atual do audio SIP")]
        public int SipAudioCurrent { get; set; } = 1;

        [Range(1, 100)]
        [Display(Name = "Total de blocos do audio SIP")]
        public int SipAudioTotal { get; set; } = 1;

        [Display(Name = "Arquivo WAV do SIP")]
        public IFormFile? SipAudioFile { get; set; }

        [Display(Name = "Existe audio SIP customizado")]
        public bool SipAudioExists { get; set; }

        [Display(Name = "Status SIP")]
        public int SipStatusCode { get; set; } = -1;

        [Display(Name = "Em chamada")]
        public bool SipInCall { get; set; }

        [Display(Name = "Nao identificado")]
        public string AccessAudioNotIdentified { get; set; } = "default";

        [Display(Name = "Autorizado")]
        public string AccessAudioAuthorized { get; set; } = "default";

        [Display(Name = "Nao autorizado")]
        public string AccessAudioNotAuthorized { get; set; } = "default";

        [Display(Name = "Use mascara")]
        public string AccessAudioUseMask { get; set; } = "default";

        [Display(Name = "Ganho do volume")]
        public string AccessAudioVolumeGain { get; set; } = "2";

        [Display(Name = "Evento do audio")]
        public string AccessAudioEvent { get; set; } = "authorized";

        [Range(1, 100)]
        [Display(Name = "Bloco atual do audio de acesso")]
        public int AccessAudioCurrent { get; set; } = 1;

        [Range(1, 100)]
        [Display(Name = "Total de blocos do audio de acesso")]
        public int AccessAudioTotal { get; set; } = 1;

        [Display(Name = "Arquivo WAV do evento")]
        public IFormFile? AccessAudioFile { get; set; }

        [Display(Name = "Existe audio para Nao identificado")]
        public bool AccessAudioHasNotIdentified { get; set; }

        [Display(Name = "Existe audio para Autorizado")]
        public bool AccessAudioHasAuthorized { get; set; }

        [Display(Name = "Existe audio para Nao autorizado")]
        public bool AccessAudioHasNotAuthorized { get; set; }

        [Display(Name = "Existe audio para Use mascara")]
        public bool AccessAudioHasUseMask { get; set; }

        [Display(Name = "Modo da SecBox")]
        public string SignalSecBoxOutMode { get; set; } = "0";

        [Display(Name = "Modo do rele interno")]
        public string SignalRelayOutMode { get; set; } = "0";

        [Display(Name = "Rele interno habilitado")]
        public bool SignalRelayEnabled { get; set; } = true;

        [Display(Name = "Fechamento automatico do rele")]
        public bool SignalRelayAutoClose { get; set; } = true;

        [Range(100, 10000)]
        [Display(Name = "Timeout do rele (ms)")]
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
