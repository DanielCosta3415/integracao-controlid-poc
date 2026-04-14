using System.Text.Json;
using Integracao.ControlID.PoC.Services.ControlIDApi;
using Integracao.ControlID.PoC.ViewModels.ProductSpecific;

namespace Integracao.ControlID.PoC.Services.ProductSpecific;

public sealed class ProductSpecificSnapshotService
{
    private readonly IOfficialControlIdApiService _apiService;
    private readonly ProductSpecificJsonReader _reader;

    public ProductSpecificSnapshotService(
        IOfficialControlIdApiService apiService,
        ProductSpecificJsonReader reader)
    {
        _apiService = apiService;
        _reader = reader;
    }

    public async Task PopulateAllAsync(ProductSpecificViewModel model)
    {
        await PopulateFacialAsync(model);
        await PopulateQrAsync(model);
        await PopulatePowerAsync(model);
        await PopulateStreamingAsync(model);
        await PopulateSipAsync(model);
        await PopulateSipAudioAsync(model);
        await PopulateAccessAudioAsync(model);
        await PopulateSignalsAsync(model);
    }

    public async Task PopulateSipAsync(ProductSpecificViewModel model)
    {
        var (_, configDocument) = await _apiService.InvokeJsonAsync("get-configuration", new
        {
            pjsip = new[]
            {
                "enabled", "server_ip", "server_port", "server_outbound_port", "server_outbound_port_range",
                "numeric_branch_enabled", "branch", "login", "password", "peer_to_peer_enabled",
                "reg_status_query_period", "server_retry_interval", "max_call_time", "push_button_debounce",
                "auto_answer_enabled", "auto_answer_delay", "auto_call_button_enabled", "rex_enabled",
                "dialing_display_mode", "auto_call_target", "custom_identifier_auto_call", "video_enabled",
                "pjsip_custom_audio_enabled", "custom_audio_volume_gain", "mic_volume", "speaker_volume",
                "open_door_enabled", "open_door_command", "facial_id_during_call_enabled"
            }
        });

        if (configDocument != null)
        {
            var root = configDocument.RootElement;
            model.SipEnabled = _reader.GetConfigBool(root, "pjsip", "enabled");
            model.SipServerIp = _reader.GetConfigString(root, "pjsip", "server_ip", model.SipServerIp);
            model.SipServerPort = _reader.GetConfigInt(root, "pjsip", "server_port", model.SipServerPort);
            model.SipServerOutboundPort = _reader.GetConfigInt(root, "pjsip", "server_outbound_port", model.SipServerOutboundPort);
            model.SipServerOutboundPortRange = _reader.GetConfigInt(root, "pjsip", "server_outbound_port_range", model.SipServerOutboundPortRange);
            model.SipNumericBranchEnabled = _reader.GetConfigBool(root, "pjsip", "numeric_branch_enabled", true);
            model.SipBranch = _reader.GetConfigString(root, "pjsip", "branch", model.SipBranch);
            model.SipLogin = _reader.GetConfigString(root, "pjsip", "login", model.SipLogin);
            model.SipPassword = _reader.GetConfigString(root, "pjsip", "password", model.SipPassword);
            model.SipPeerToPeerEnabled = _reader.GetConfigBool(root, "pjsip", "peer_to_peer_enabled");
            model.SipRegStatusQueryPeriod = _reader.GetConfigInt(root, "pjsip", "reg_status_query_period", model.SipRegStatusQueryPeriod);
            model.SipServerRetryInterval = _reader.GetConfigInt(root, "pjsip", "server_retry_interval", model.SipServerRetryInterval);
            model.SipMaxCallTime = _reader.GetConfigInt(root, "pjsip", "max_call_time", model.SipMaxCallTime);
            model.SipPushButtonDebounce = _reader.GetConfigInt(root, "pjsip", "push_button_debounce", model.SipPushButtonDebounce);
            model.SipAutoAnswerEnabled = _reader.GetConfigBool(root, "pjsip", "auto_answer_enabled");
            model.SipAutoAnswerDelay = _reader.GetConfigInt(root, "pjsip", "auto_answer_delay", model.SipAutoAnswerDelay);
            model.SipAutoCallButtonEnabled = _reader.GetConfigBool(root, "pjsip", "auto_call_button_enabled", true);
            model.SipRexEnabled = _reader.GetConfigBool(root, "pjsip", "rex_enabled");
            model.SipDialingDisplayMode = _reader.GetConfigString(root, "pjsip", "dialing_display_mode", model.SipDialingDisplayMode);
            model.SipAutoCallTarget = _reader.GetConfigString(root, "pjsip", "auto_call_target", model.SipAutoCallTarget);
            model.SipCustomIdentifierAutoCall = _reader.GetConfigString(root, "pjsip", "custom_identifier_auto_call", model.SipCustomIdentifierAutoCall);
            model.SipVideoEnabled = _reader.GetConfigBool(root, "pjsip", "video_enabled");
            model.SipCustomAudioEnabled = _reader.GetConfigBool(root, "pjsip", "pjsip_custom_audio_enabled");
            model.SipCustomAudioVolumeGain = _reader.GetConfigString(root, "pjsip", "custom_audio_volume_gain", model.SipCustomAudioVolumeGain);
            model.SipMicVolume = _reader.GetConfigInt(root, "pjsip", "mic_volume", model.SipMicVolume);
            model.SipSpeakerVolume = _reader.GetConfigInt(root, "pjsip", "speaker_volume", model.SipSpeakerVolume);
            model.SipOpenDoorEnabled = _reader.GetConfigBool(root, "pjsip", "open_door_enabled");
            model.SipOpenDoorCommand = _reader.GetConfigString(root, "pjsip", "open_door_command", model.SipOpenDoorCommand);
            model.SipFacialIdDuringCallEnabled = _reader.GetConfigBool(root, "pjsip", "facial_id_during_call_enabled");
        }

        var (_, statusDocument) = await _apiService.InvokeJsonAsync("get-sip-status");
        if (statusDocument == null)
            return;

        model.SipStatusCode = _reader.GetRootInt(statusDocument.RootElement, "status", model.SipStatusCode);
        model.SipInCall = _reader.GetRootBool(statusDocument.RootElement, "in_call");
    }

    public async Task PopulateSipAudioAsync(ProductSpecificViewModel model)
    {
        var (_, document) = await _apiService.InvokeJsonAsync("has-pjsip-audio-message");
        if (document != null)
            model.SipAudioExists = _reader.GetRootBool(document.RootElement, "file_exists");
    }

    public async Task PopulateAccessAudioAsync(ProductSpecificViewModel model)
    {
        var (_, configDocument) = await _apiService.InvokeJsonAsync("get-configuration", new
        {
            buzzer = new[] { "audio_message_not_identified", "audio_message_authorized", "audio_message_not_authorized", "audio_message_use_mask", "audio_message_volume_gain" }
        });

        if (configDocument != null)
        {
            var root = configDocument.RootElement;
            model.AccessAudioNotIdentified = _reader.GetConfigString(root, "buzzer", "audio_message_not_identified", model.AccessAudioNotIdentified);
            model.AccessAudioAuthorized = _reader.GetConfigString(root, "buzzer", "audio_message_authorized", model.AccessAudioAuthorized);
            model.AccessAudioNotAuthorized = _reader.GetConfigString(root, "buzzer", "audio_message_not_authorized", model.AccessAudioNotAuthorized);
            model.AccessAudioUseMask = _reader.GetConfigString(root, "buzzer", "audio_message_use_mask", model.AccessAudioUseMask);
            model.AccessAudioVolumeGain = _reader.GetConfigString(root, "buzzer", "audio_message_volume_gain", model.AccessAudioVolumeGain);
        }

        var (_, statusDocument) = await _apiService.InvokeJsonAsync("has-audio-access-messages");
        if (statusDocument == null)
            return;

        model.AccessAudioHasNotIdentified = _reader.GetRootBool(statusDocument.RootElement, "not_identified");
        model.AccessAudioHasAuthorized = _reader.GetRootBool(statusDocument.RootElement, "authorized");
        model.AccessAudioHasNotAuthorized = _reader.GetRootBool(statusDocument.RootElement, "not_authorized");
        model.AccessAudioHasUseMask = _reader.GetRootBool(statusDocument.RootElement, "use_mask");
    }

    private async Task PopulateFacialAsync(ProductSpecificViewModel model)
    {
        var (_, document) = await _apiService.InvokeJsonAsync("get-configuration", new
        {
            general = new[] { "screen_always_on" },
            identifier = new[] { "card_identification_enabled", "face_identification_enabled", "qrcode_identification_enabled", "pin_identification_enabled" },
            face_id = new[] { "mask_detection_enabled", "vehicle_mode", "max_identified_duration", "limit_identification_to_display_region", "min_detect_bounds_width" },
            camera_overlay = new[] { "zoom", "vertical_crop" },
            face_module = new[] { "light_threshold_led_activation" },
            led_white = new[] { "brightness" }
        });

        if (document == null)
            return;

        var root = document.RootElement;
        model.FaceScreenAlwaysOn = _reader.GetConfigBool(root, "general", "screen_always_on", true);
        model.IdentifierCardEnabled = _reader.GetConfigBool(root, "identifier", "card_identification_enabled", true);
        model.IdentifierFaceEnabled = _reader.GetConfigBool(root, "identifier", "face_identification_enabled", true);
        model.IdentifierQrCodeEnabled = _reader.GetConfigBool(root, "identifier", "qrcode_identification_enabled", true);
        model.IdentifierPinEnabled = _reader.GetConfigBool(root, "identifier", "pin_identification_enabled");
        model.FaceMaskDetectionEnabled = _reader.GetConfigString(root, "face_id", "mask_detection_enabled", model.FaceMaskDetectionEnabled);
        model.FaceVehicleModeEnabled = _reader.GetConfigBool(root, "face_id", "vehicle_mode");
        model.FaceMaxIdentifiedDuration = _reader.GetConfigInt(root, "face_id", "max_identified_duration", model.FaceMaxIdentifiedDuration);
        model.FaceLimitIdentificationToDisplayRegion = _reader.GetConfigBool(root, "face_id", "limit_identification_to_display_region");
        model.FaceMinDetectBoundsWidth = _reader.GetConfigString(root, "face_id", "min_detect_bounds_width", model.FaceMinDetectBoundsWidth);
        model.FaceCameraOverlayZoom = _reader.GetConfigString(root, "camera_overlay", "zoom", model.FaceCameraOverlayZoom);
        model.FaceCameraOverlayVerticalCrop = _reader.GetConfigString(root, "camera_overlay", "vertical_crop", model.FaceCameraOverlayVerticalCrop);
        model.FaceLightThresholdLedActivation = _reader.GetConfigString(root, "face_module", "light_threshold_led_activation", model.FaceLightThresholdLedActivation);
        model.FaceLedBrightness = _reader.GetConfigInt(root, "led_white", "brightness", model.FaceLedBrightness);
    }

    private async Task PopulateQrAsync(ProductSpecificViewModel model)
    {
        var (_, document) = await _apiService.InvokeJsonAsync("get-configuration", new
        {
            face_id = new[] { "qrcode_legacy_mode_enabled", "totp_enabled", "totp_window_size", "totp_window_num", "totp_single_use", "totp_tz_offset" },
            barras = new[] { "qrcode_legacy_mode_enabled", "totp_enabled", "totp_window_size", "totp_window_num", "totp_single_use", "totp_tz_offset" }
        });

        if (document == null)
            return;

        var root = document.RootElement;
        var module = root.TryGetProperty("barras", out var barras) &&
            barras.ValueKind == JsonValueKind.Object &&
            barras.TryGetProperty("qrcode_legacy_mode_enabled", out _)
            ? "barras"
            : "face_id";

        model.QrModule = module;
        model.QrCodeLegacyModeEnabled = _reader.GetConfigString(root, module, "qrcode_legacy_mode_enabled", model.QrCodeLegacyModeEnabled);
        model.QrTotpEnabled = _reader.GetConfigBool(root, module, "totp_enabled");
        model.QrTotpWindowSize = _reader.GetConfigInt(root, module, "totp_window_size", model.QrTotpWindowSize);
        model.QrTotpWindowNum = _reader.GetConfigInt(root, module, "totp_window_num", model.QrTotpWindowNum);
        model.QrTotpSingleUse = _reader.GetConfigBool(root, module, "totp_single_use", true);
        model.QrTotpTzOffset = _reader.GetConfigInt(root, module, "totp_tz_offset", model.QrTotpTzOffset);
    }

    private async Task PopulatePowerAsync(ProductSpecificViewModel model)
    {
        var (_, document) = await _apiService.InvokeJsonAsync("get-configuration", new
        {
            general = new[] { "screenshot_resize", "energy_mode", "energy_display_custom", "energy_sound_custom", "energy_ir_custom", "energy_led_custom" }
        });

        if (document == null)
            return;

        var root = document.RootElement;
        model.ScreenshotResize = _reader.GetConfigString(root, "general", "screenshot_resize", model.ScreenshotResize);
        model.EnergyMode = _reader.GetConfigString(root, "general", "energy_mode", model.EnergyMode);
        model.EnergyDisplayCustom = _reader.GetConfigString(root, "general", "energy_display_custom", model.EnergyDisplayCustom);
        model.EnergySoundCustom = _reader.GetConfigString(root, "general", "energy_sound_custom", model.EnergySoundCustom);
        model.EnergyIrCustom = _reader.GetConfigString(root, "general", "energy_ir_custom", model.EnergyIrCustom);
        model.EnergyLedCustom = _reader.GetConfigString(root, "general", "energy_led_custom", model.EnergyLedCustom);
    }

    private async Task PopulateStreamingAsync(ProductSpecificViewModel model)
    {
        var (_, document) = await _apiService.InvokeJsonAsync("get-configuration", new
        {
            onvif = new[] { "rtsp_enabled", "rtsp_port", "rtsp_username", "rtsp_password", "rtsp_rgb", "rtsp_codec", "onvif_enabled", "onvif_port", "rtsp_flipped", "rtsp_watermark_enabled", "rtsp_watermark_logo_enabled", "rtsp_watermark_custom_logo_enabled" }
        });

        if (document == null)
            return;

        var root = document.RootElement;
        model.StreamingRtspEnabled = _reader.GetConfigBool(root, "onvif", "rtsp_enabled");
        model.StreamingRtspPort = _reader.GetConfigInt(root, "onvif", "rtsp_port", model.StreamingRtspPort);
        model.StreamingRtspUsername = _reader.GetConfigString(root, "onvif", "rtsp_username", model.StreamingRtspUsername);
        model.StreamingRtspPassword = _reader.GetConfigString(root, "onvif", "rtsp_password", model.StreamingRtspPassword);
        model.StreamingRtspRgb = _reader.GetConfigBool(root, "onvif", "rtsp_rgb", true);
        model.StreamingRtspCodec = _reader.GetConfigString(root, "onvif", "rtsp_codec", model.StreamingRtspCodec);
        model.StreamingOnvifEnabled = _reader.GetConfigBool(root, "onvif", "onvif_enabled");
        model.StreamingOnvifPort = _reader.GetConfigInt(root, "onvif", "onvif_port", model.StreamingOnvifPort);
        model.StreamingRtspFlipped = _reader.GetConfigBool(root, "onvif", "rtsp_flipped");
        model.StreamingWatermarkEnabled = _reader.GetConfigBool(root, "onvif", "rtsp_watermark_enabled", true);
        model.StreamingWatermarkLogoEnabled = _reader.GetConfigBool(root, "onvif", "rtsp_watermark_logo_enabled", true);
        model.StreamingWatermarkCustomLogoEnabled = _reader.GetConfigBool(root, "onvif", "rtsp_watermark_custom_logo_enabled");
    }

    private async Task PopulateSignalsAsync(ProductSpecificViewModel model)
    {
        var (_, document) = await _apiService.InvokeJsonAsync("get-configuration", new
        {
            general = new[]
            {
                "sec_box_out_mode", "relay_out_mode", "relay1_enabled", "relay1_auto_close", "relay1_timeout",
                "gpio_ext1_mode", "gpio_ext1_idle", "gpio_ext2_mode", "gpio_ext2_idle", "gpio_ext3_mode", "gpio_ext3_idle"
            }
        });

        if (document == null)
            return;

        var root = document.RootElement;
        model.SignalSecBoxOutMode = _reader.GetConfigString(root, "general", "sec_box_out_mode", model.SignalSecBoxOutMode);
        model.SignalRelayOutMode = _reader.GetConfigString(root, "general", "relay_out_mode", model.SignalRelayOutMode);
        model.SignalRelayEnabled = _reader.GetConfigBool(root, "general", "relay1_enabled", true);
        model.SignalRelayAutoClose = _reader.GetConfigBool(root, "general", "relay1_auto_close", true);
        model.SignalRelayTimeout = _reader.GetConfigInt(root, "general", "relay1_timeout", model.SignalRelayTimeout);
        model.SignalGpioExt1Mode = _reader.GetConfigString(root, "general", "gpio_ext1_mode", model.SignalGpioExt1Mode);
        model.SignalGpioExt1Idle = _reader.GetConfigString(root, "general", "gpio_ext1_idle", model.SignalGpioExt1Idle);
        model.SignalGpioExt2Mode = _reader.GetConfigString(root, "general", "gpio_ext2_mode", model.SignalGpioExt2Mode);
        model.SignalGpioExt2Idle = _reader.GetConfigString(root, "general", "gpio_ext2_idle", model.SignalGpioExt2Idle);
        model.SignalGpioExt3Mode = _reader.GetConfigString(root, "general", "gpio_ext3_mode", model.SignalGpioExt3Mode);
        model.SignalGpioExt3Idle = _reader.GetConfigString(root, "general", "gpio_ext3_idle", model.SignalGpioExt3Idle);
    }
}
