using Integracao.ControlID.PoC.ViewModels.ProductSpecific;

namespace Integracao.ControlID.PoC.Services.ProductSpecific
{
    public sealed class ProductSpecificConfigurationPayloadFactory
    {
        public object BuildFacialSettings(ProductSpecificViewModel model)
        {
            return new
            {
                general = new
                {
                    screen_always_on = BoolString(model.FaceScreenAlwaysOn)
                },
                identifier = new
                {
                    card_identification_enabled = BoolString(model.IdentifierCardEnabled),
                    face_identification_enabled = BoolString(model.IdentifierFaceEnabled),
                    qrcode_identification_enabled = BoolString(model.IdentifierQrCodeEnabled),
                    pin_identification_enabled = BoolString(model.IdentifierPinEnabled)
                },
                face_id = new
                {
                    mask_detection_enabled = model.FaceMaskDetectionEnabled,
                    vehicle_mode = BoolString(model.FaceVehicleModeEnabled),
                    max_identified_duration = model.FaceMaxIdentifiedDuration.ToString(),
                    limit_identification_to_display_region = BoolString(model.FaceLimitIdentificationToDisplayRegion),
                    min_detect_bounds_width = model.FaceMinDetectBoundsWidth
                },
                camera_overlay = new
                {
                    zoom = model.FaceCameraOverlayZoom,
                    vertical_crop = model.FaceCameraOverlayVerticalCrop
                },
                face_module = new
                {
                    light_threshold_led_activation = model.FaceLightThresholdLedActivation
                },
                led_white = new
                {
                    brightness = model.FaceLedBrightness.ToString()
                }
            };
        }

        public object BuildQrCodeSettings(ProductSpecificViewModel model)
        {
            var module = model.QrModule.Equals("barras", StringComparison.OrdinalIgnoreCase) ? "barras" : "face_id";
            return new Dictionary<string, object?>
            {
                [module] = new
                {
                    qrcode_legacy_mode_enabled = model.QrCodeLegacyModeEnabled,
                    totp_enabled = BoolString(model.QrTotpEnabled),
                    totp_window_size = model.QrTotpWindowSize.ToString(),
                    totp_window_num = model.QrTotpWindowNum.ToString(),
                    totp_single_use = BoolString(model.QrTotpSingleUse),
                    totp_tz_offset = model.QrTotpTzOffset.ToString()
                }
            };
        }

        public object BuildPowerSettings(ProductSpecificViewModel model)
        {
            return new
            {
                general = new
                {
                    screenshot_resize = model.ScreenshotResize,
                    energy_mode = model.EnergyMode,
                    energy_display_custom = model.EnergyDisplayCustom,
                    energy_sound_custom = model.EnergySoundCustom,
                    energy_ir_custom = model.EnergyIrCustom,
                    energy_led_custom = model.EnergyLedCustom
                }
            };
        }

        public object BuildStreamingSettings(ProductSpecificViewModel model)
        {
            return new
            {
                onvif = new
                {
                    rtsp_enabled = BoolString(model.StreamingRtspEnabled),
                    rtsp_port = model.StreamingRtspPort.ToString(),
                    rtsp_username = model.StreamingRtspUsername,
                    rtsp_password = model.StreamingRtspPassword,
                    rtsp_rgb = BoolString(model.StreamingRtspRgb),
                    rtsp_codec = model.StreamingRtspCodec,
                    onvif_enabled = BoolString(model.StreamingOnvifEnabled),
                    onvif_port = model.StreamingOnvifPort.ToString(),
                    rtsp_flipped = BoolString(model.StreamingRtspFlipped),
                    rtsp_watermark_enabled = BoolString(model.StreamingWatermarkEnabled),
                    rtsp_watermark_logo_enabled = BoolString(model.StreamingWatermarkLogoEnabled),
                    rtsp_watermark_custom_logo_enabled = BoolString(model.StreamingWatermarkCustomLogoEnabled)
                }
            };
        }

        public object BuildSipSettings(ProductSpecificViewModel model)
        {
            return new
            {
                pjsip = new
                {
                    enabled = BoolString(model.SipEnabled),
                    server_ip = model.SipServerIp,
                    server_port = model.SipServerPort.ToString(),
                    server_outbound_port = model.SipServerOutboundPort.ToString(),
                    server_outbound_port_range = model.SipServerOutboundPortRange.ToString(),
                    numeric_branch_enabled = BoolString(model.SipNumericBranchEnabled),
                    branch = model.SipBranch,
                    login = model.SipLogin,
                    password = model.SipPassword,
                    peer_to_peer_enabled = BoolString(model.SipPeerToPeerEnabled),
                    reg_status_query_period = model.SipRegStatusQueryPeriod.ToString(),
                    server_retry_interval = model.SipServerRetryInterval.ToString(),
                    max_call_time = model.SipMaxCallTime.ToString(),
                    push_button_debounce = model.SipPushButtonDebounce.ToString(),
                    auto_answer_enabled = BoolString(model.SipAutoAnswerEnabled),
                    auto_answer_delay = model.SipAutoAnswerDelay.ToString(),
                    auto_call_button_enabled = BoolString(model.SipAutoCallButtonEnabled),
                    rex_enabled = BoolString(model.SipRexEnabled),
                    dialing_display_mode = model.SipDialingDisplayMode,
                    auto_call_target = model.SipAutoCallTarget,
                    custom_identifier_auto_call = model.SipCustomIdentifierAutoCall,
                    video_enabled = BoolString(model.SipVideoEnabled),
                    pjsip_custom_audio_enabled = BoolString(model.SipCustomAudioEnabled),
                    custom_audio_volume_gain = model.SipCustomAudioVolumeGain,
                    mic_volume = model.SipMicVolume.ToString(),
                    speaker_volume = model.SipSpeakerVolume.ToString(),
                    open_door_enabled = BoolString(model.SipOpenDoorEnabled),
                    open_door_command = model.SipOpenDoorCommand,
                    facial_id_during_call_enabled = BoolString(model.SipFacialIdDuringCallEnabled)
                }
            };
        }

        public object BuildAccessAudioSettings(ProductSpecificViewModel model)
        {
            return new
            {
                buzzer = new
                {
                    audio_message_not_identified = model.AccessAudioNotIdentified,
                    audio_message_authorized = model.AccessAudioAuthorized,
                    audio_message_not_authorized = model.AccessAudioNotAuthorized,
                    audio_message_use_mask = model.AccessAudioUseMask,
                    audio_message_volume_gain = model.AccessAudioVolumeGain
                }
            };
        }

        public object BuildSignalsSettings(ProductSpecificViewModel model)
        {
            return new
            {
                general = new
                {
                    sec_box_out_mode = model.SignalSecBoxOutMode,
                    relay_out_mode = model.SignalRelayOutMode,
                    relay1_enabled = BoolString(model.SignalRelayEnabled),
                    relay1_auto_close = BoolString(model.SignalRelayAutoClose),
                    relay1_timeout = model.SignalRelayTimeout.ToString(),
                    gpio_ext1_mode = model.SignalGpioExt1Mode,
                    gpio_ext1_idle = model.SignalGpioExt1Idle,
                    gpio_ext2_mode = model.SignalGpioExt2Mode,
                    gpio_ext2_idle = model.SignalGpioExt2Idle,
                    gpio_ext3_mode = model.SignalGpioExt3Mode,
                    gpio_ext3_idle = model.SignalGpioExt3Idle
                }
            };
        }

        private static string BoolString(bool value) => value ? "1" : "0";
    }
}
