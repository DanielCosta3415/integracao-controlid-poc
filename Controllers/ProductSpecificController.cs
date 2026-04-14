using System.Text.Json;
using Integracao.ControlID.PoC.Models.ControlIDApi;
using Integracao.ControlID.PoC.Services.ControlIDApi;
using Integracao.ControlID.PoC.ViewModels.ProductSpecific;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Integracao.ControlID.PoC.Controllers
{
    public class ProductSpecificController : Controller
    {
        private readonly OfficialControlIdApiService _apiService;
        private readonly ILogger<ProductSpecificController> _logger;

        public ProductSpecificController(OfficialControlIdApiService apiService, ILogger<ProductSpecificController> logger)
        {
            _apiService = apiService;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            var model = new ProductSpecificViewModel();
            if (!EnsureConnected(model))
                return View(model);

            await PopulateAllAsync(model);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpgradeIdFace(ProductSpecificViewModel model)
        {
            model.ActiveSection = "upgrades";
            if (!EnsureConnected(model))
                return View("Index", model);

            try
            {
                var result = await _apiService.InvokeAsync("upgrade-idface-pro", new { password = model.IdFaceProPassword });
                EnsureSuccess(result, "Erro ao executar upgrade Pro do iDFace");
                model.ResultMessage = "Upgrade Pro do iDFace solicitado com sucesso.";
                model.ResultStatusType = "success";
                model.ResponseJson = string.IsNullOrWhiteSpace(result.ResponseBody) ? "Operacao concluida sem corpo de resposta." : result.ResponseBody;
            }
            catch (Exception ex)
            {
                model.ErrorMessage = ex.Message;
                _logger.LogError(ex, "Erro ao executar upgrade Pro do iDFace.");
            }

            return View("Index", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpgradeEnterprise(ProductSpecificViewModel model)
        {
            model.ActiveSection = "upgrades";
            if (!EnsureConnected(model))
                return View("Index", model);

            try
            {
                var result = await _apiService.InvokeAsync("upgrade-idflex-enterprise", new { password = model.IdFlexEnterprisePassword });
                EnsureSuccess(result, "Erro ao executar upgrade Enterprise");
                model.ResultMessage = "Upgrade Enterprise solicitado com sucesso.";
                model.ResultStatusType = "success";
                model.ResponseJson = string.IsNullOrWhiteSpace(result.ResponseBody) ? "Operacao concluida sem corpo de resposta." : result.ResponseBody;
            }
            catch (Exception ex)
            {
                model.ErrorMessage = ex.Message;
                _logger.LogError(ex, "Erro ao executar upgrade Enterprise.");
            }

            return View("Index", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> FacialSettings(ProductSpecificViewModel model)
        {
            model.ActiveSection = "facial";
            if (!EnsureConnected(model))
                return View("Index", model);

            try
            {
                var (result, document) = await _apiService.InvokeJsonAsync("set-configuration", new
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
                });

                EnsureSuccess(result, "Erro ao aplicar configuracoes faciais");
                model.ResultMessage = "Configuracoes faciais atualizadas com sucesso.";
                model.ResultStatusType = "success";
                model.ResponseJson = FormatJson(result.ResponseBody, document);
            }
            catch (Exception ex)
            {
                model.ErrorMessage = ex.Message;
                _logger.LogError(ex, "Erro ao aplicar configuracoes faciais.");
            }

            return View("Index", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> QrCodeSettings(ProductSpecificViewModel model)
        {
            model.ActiveSection = "qr";
            if (!EnsureConnected(model))
                return View("Index", model);

            try
            {
                var module = model.QrModule.Equals("barras", StringComparison.OrdinalIgnoreCase) ? "barras" : "face_id";
                var payload = new Dictionary<string, object?>
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

                var (result, document) = await _apiService.InvokeJsonAsync("set-configuration", payload);
                EnsureSuccess(result, "Erro ao aplicar configuracoes de QR Code");
                model.ResultMessage = "Configuracoes de QR Code/TOTP atualizadas com sucesso.";
                model.ResultStatusType = "success";
                model.ResponseJson = FormatJson(result.ResponseBody, document);
            }
            catch (Exception ex)
            {
                model.ErrorMessage = ex.Message;
                _logger.LogError(ex, "Erro ao aplicar configuracoes de QR Code.");
            }

            return View("Index", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PowerSettings(ProductSpecificViewModel model)
        {
            model.ActiveSection = "power";
            if (!EnsureConnected(model))
                return View("Index", model);

            try
            {
                var (result, document) = await _apiService.InvokeJsonAsync("set-configuration", new
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
                });

                EnsureSuccess(result, "Erro ao aplicar configuracoes de energia");
                model.ResultMessage = "Configuracoes de energia e screenshot atualizadas com sucesso.";
                model.ResultStatusType = "success";
                model.ResponseJson = FormatJson(result.ResponseBody, document);
            }
            catch (Exception ex)
            {
                model.ErrorMessage = ex.Message;
                _logger.LogError(ex, "Erro ao aplicar configuracoes de energia.");
            }

            return View("Index", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Streaming(ProductSpecificViewModel model)
        {
            model.ActiveSection = "streaming";
            if (!EnsureConnected(model))
                return View("Index", model);

            try
            {
                var (result, document) = await _apiService.InvokeJsonAsync("set-configuration", new
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
                });

                EnsureSuccess(result, "Erro ao aplicar configuracoes de streaming");
                model.ResultMessage = "Configuracoes de streaming atualizadas com sucesso.";
                model.ResultStatusType = "success";
                model.ResponseJson = FormatJson(result.ResponseBody, document);
            }
            catch (Exception ex)
            {
                model.ErrorMessage = ex.Message;
                _logger.LogError(ex, "Erro ao aplicar configuracoes de streaming.");
            }

            return View("Index", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SipSettings(ProductSpecificViewModel model)
        {
            model.ActiveSection = "sip";
            if (!EnsureConnected(model))
                return View("Index", model);

            try
            {
                var (result, document) = await _apiService.InvokeJsonAsync("set-configuration", new
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
                });

                EnsureSuccess(result, "Erro ao aplicar configuracoes SIP");
                await PopulateSipAsync(model);
                model.ResultMessage = "Configuracoes SIP atualizadas com sucesso.";
                model.ResultStatusType = "success";
                model.ResponseJson = FormatJson(result.ResponseBody, document);
            }
            catch (Exception ex)
            {
                model.ErrorMessage = ex.Message;
                _logger.LogError(ex, "Erro ao aplicar configuracoes SIP.");
            }

            return View("Index", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RefreshSipStatus(ProductSpecificViewModel model)
        {
            model.ActiveSection = "sip";
            if (!EnsureConnected(model))
                return View("Index", model);

            try
            {
                var (result, document) = await _apiService.InvokeJsonAsync("get-sip-status");
                EnsureSuccess(result, "Erro ao consultar status do SIP");
                model.SipStatusCode = document == null ? model.SipStatusCode : GetRootInt(document.RootElement, "status", model.SipStatusCode);
                model.SipInCall = document != null && GetRootBool(document.RootElement, "in_call");
                model.ResultMessage = "Status do SIP atualizado.";
                model.ResultStatusType = "success";
                model.ResponseJson = FormatJson(result.ResponseBody, document);
            }
            catch (Exception ex)
            {
                model.ErrorMessage = ex.Message;
                _logger.LogError(ex, "Erro ao consultar status do SIP.");
            }

            return View("Index", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MakeSipCall(ProductSpecificViewModel model)
        {
            model.ActiveSection = "sip";
            if (!EnsureConnected(model))
                return View("Index", model);

            try
            {
                var (result, document) = await _apiService.InvokeJsonAsync("make-sip-call", new { target = model.SipCallTarget });
                EnsureSuccess(result, "Erro ao iniciar chamada SIP");
                await PopulateSipAsync(model);
                model.ResultMessage = $"Chamada SIP iniciada para {model.SipCallTarget}.";
                model.ResultStatusType = "success";
                model.ResponseJson = FormatJson(result.ResponseBody, document);
            }
            catch (Exception ex)
            {
                model.ErrorMessage = ex.Message;
                _logger.LogError(ex, "Erro ao iniciar chamada SIP.");
            }

            return View("Index", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> FinalizeSipCall(ProductSpecificViewModel model)
        {
            model.ActiveSection = "sip";
            if (!EnsureConnected(model))
                return View("Index", model);

            try
            {
                var (result, document) = await _apiService.InvokeJsonAsync("finalize-sip-call");
                EnsureSuccess(result, "Erro ao finalizar chamada SIP");
                await PopulateSipAsync(model);
                model.ResultMessage = "Chamada SIP finalizada com sucesso.";
                model.ResultStatusType = "success";
                model.ResponseJson = FormatJson(result.ResponseBody, document);
            }
            catch (Exception ex)
            {
                model.ErrorMessage = ex.Message;
                _logger.LogError(ex, "Erro ao finalizar chamada SIP.");
            }

            return View("Index", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CheckSipAudio(ProductSpecificViewModel model)
        {
            model.ActiveSection = "sip-audio";
            if (!EnsureConnected(model))
                return View("Index", model);

            try
            {
                var (result, document) = await _apiService.InvokeJsonAsync("has-pjsip-audio-message");
                EnsureSuccess(result, "Erro ao consultar audio customizado do SIP");
                model.SipAudioExists = document != null && GetRootBool(document.RootElement, "file_exists");
                model.ResultMessage = model.SipAudioExists ? "O dispositivo possui um toque customizado." : "Nao ha toque customizado cadastrado.";
                model.ResultStatusType = "success";
                model.ResponseJson = FormatJson(result.ResponseBody, document);
            }
            catch (Exception ex)
            {
                model.ErrorMessage = ex.Message;
                _logger.LogError(ex, "Erro ao consultar audio customizado do SIP.");
            }

            return View("Index", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadSipAudio(ProductSpecificViewModel model)
        {
            model.ActiveSection = "sip-audio";
            if (!EnsureConnected(model))
                return View("Index", model);

            try
            {
                var base64 = await ReadFileAsBase64Async(model.SipAudioFile, "Selecione um arquivo WAV para o toque SIP.");
                var result = await _apiService.InvokeAsync("set-pjsip-audio-message", base64, $"current={model.SipAudioCurrent}&total={model.SipAudioTotal}");
                EnsureSuccess(result, "Erro ao enviar audio customizado do SIP");
                await PopulateSipAudioAsync(model);
                model.ResultMessage = "Audio customizado do SIP enviado com sucesso.";
                model.ResultStatusType = "success";
                model.ResponseJson = string.IsNullOrWhiteSpace(result.ResponseBody) ? "Operacao concluida sem corpo de resposta." : result.ResponseBody;
            }
            catch (Exception ex)
            {
                model.ErrorMessage = ex.Message;
                _logger.LogError(ex, "Erro ao enviar audio customizado do SIP.");
            }

            return View("Index", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DownloadSipAudio(ProductSpecificViewModel model)
        {
            model.ActiveSection = "sip-audio";
            if (!EnsureConnected(model))
                return View("Index", model);

            try
            {
                var result = await _apiService.InvokeAsync("get-pjsip-audio-message");
                EnsureSuccess(result, "Erro ao baixar audio customizado do SIP");
                return DownloadBinaryResult(result, "sip-ring.wav", "audio/wav");
            }
            catch (Exception ex)
            {
                model.ErrorMessage = ex.Message;
                _logger.LogError(ex, "Erro ao baixar audio customizado do SIP.");
                return View("Index", model);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AccessAudioSettings(ProductSpecificViewModel model)
        {
            model.ActiveSection = "access-audio";
            if (!EnsureConnected(model))
                return View("Index", model);

            try
            {
                var (result, document) = await _apiService.InvokeJsonAsync("set-configuration", new
                {
                    buzzer = new
                    {
                        audio_message_not_identified = model.AccessAudioNotIdentified,
                        audio_message_authorized = model.AccessAudioAuthorized,
                        audio_message_not_authorized = model.AccessAudioNotAuthorized,
                        audio_message_use_mask = model.AccessAudioUseMask,
                        audio_message_volume_gain = model.AccessAudioVolumeGain
                    }
                });

                EnsureSuccess(result, "Erro ao aplicar configuracoes de audio de acesso");
                model.ResultMessage = "Configuracoes de audio de acesso atualizadas com sucesso.";
                model.ResultStatusType = "success";
                model.ResponseJson = FormatJson(result.ResponseBody, document);
            }
            catch (Exception ex)
            {
                model.ErrorMessage = ex.Message;
                _logger.LogError(ex, "Erro ao aplicar configuracoes de audio de acesso.");
            }

            return View("Index", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CheckAccessAudio(ProductSpecificViewModel model)
        {
            model.ActiveSection = "access-audio";
            if (!EnsureConnected(model))
                return View("Index", model);

            try
            {
                var (result, document) = await _apiService.InvokeJsonAsync("has-audio-access-messages");
                EnsureSuccess(result, "Erro ao consultar audios de acesso");
                if (document != null)
                {
                    model.AccessAudioHasNotIdentified = GetRootBool(document.RootElement, "not_identified");
                    model.AccessAudioHasAuthorized = GetRootBool(document.RootElement, "authorized");
                    model.AccessAudioHasNotAuthorized = GetRootBool(document.RootElement, "not_authorized");
                    model.AccessAudioHasUseMask = GetRootBool(document.RootElement, "use_mask");
                }

                model.ResultMessage = "Presenca dos audios de acesso atualizada.";
                model.ResultStatusType = "success";
                model.ResponseJson = FormatJson(result.ResponseBody, document);
            }
            catch (Exception ex)
            {
                model.ErrorMessage = ex.Message;
                _logger.LogError(ex, "Erro ao consultar audios de acesso.");
            }

            return View("Index", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadAccessAudio(ProductSpecificViewModel model)
        {
            model.ActiveSection = "access-audio";
            if (!EnsureConnected(model))
                return View("Index", model);

            try
            {
                var base64 = await ReadFileAsBase64Async(model.AccessAudioFile, "Selecione um arquivo WAV para o evento de acesso.");
                var result = await _apiService.InvokeAsync(
                    "set-audio-access-message",
                    base64,
                    $"event={model.AccessAudioEvent}&current={model.AccessAudioCurrent}&total={model.AccessAudioTotal}");

                EnsureSuccess(result, "Erro ao enviar audio de acesso");
                await PopulateAccessAudioAsync(model);
                model.ResultMessage = $"Audio do evento '{model.AccessAudioEvent}' enviado com sucesso.";
                model.ResultStatusType = "success";
                model.ResponseJson = string.IsNullOrWhiteSpace(result.ResponseBody) ? "Operacao concluida sem corpo de resposta." : result.ResponseBody;
            }
            catch (Exception ex)
            {
                model.ErrorMessage = ex.Message;
                _logger.LogError(ex, "Erro ao enviar audio de acesso.");
            }

            return View("Index", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DownloadAccessAudio(ProductSpecificViewModel model)
        {
            model.ActiveSection = "access-audio";
            if (!EnsureConnected(model))
                return View("Index", model);

            try
            {
                var result = await _apiService.InvokeAsync("get-audio-access-message", new { @event = model.AccessAudioEvent });
                EnsureSuccess(result, "Erro ao baixar audio de acesso");
                return DownloadBinaryResult(result, $"access-{model.AccessAudioEvent}.wav", "audio/wav");
            }
            catch (Exception ex)
            {
                model.ErrorMessage = ex.Message;
                _logger.LogError(ex, "Erro ao baixar audio de acesso.");
                return View("Index", model);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Signals(ProductSpecificViewModel model)
        {
            model.ActiveSection = "signals";
            if (!EnsureConnected(model))
                return View("Index", model);

            try
            {
                var (result, document) = await _apiService.InvokeJsonAsync("set-configuration", new
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
                });

                EnsureSuccess(result, "Erro ao aplicar sinais customizados");
                model.ResultMessage = "Sinais customizados atualizados com sucesso.";
                model.ResultStatusType = "success";
                model.ResponseJson = FormatJson(result.ResponseBody, document);
            }
            catch (Exception ex)
            {
                model.ErrorMessage = ex.Message;
                _logger.LogError(ex, "Erro ao aplicar sinais customizados.");
            }

            return View("Index", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RefreshLeds(ProductSpecificViewModel model)
        {
            model.ActiveSection = "signals";
            if (!EnsureConnected(model))
                return View("Index", model);

            try
            {
                var result = await _apiService.InvokeAsync("reread-leds");
                EnsureSuccess(result, "Erro ao recarregar configuracao de LEDs");
                model.ResultMessage = "Configuracao de LEDs reaplicada com sucesso.";
                model.ResultStatusType = "success";
                model.ResponseJson = string.IsNullOrWhiteSpace(result.ResponseBody) ? "Operacao concluida sem corpo de resposta." : result.ResponseBody;
            }
            catch (Exception ex)
            {
                model.ErrorMessage = ex.Message;
                _logger.LogError(ex, "Erro ao recarregar configuracao de LEDs.");
            }

            return View("Index", model);
        }

        private async Task PopulateAllAsync(ProductSpecificViewModel model)
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

            model.FaceScreenAlwaysOn = GetConfigBool(document.RootElement, "general", "screen_always_on", true);
            model.IdentifierCardEnabled = GetConfigBool(document.RootElement, "identifier", "card_identification_enabled", true);
            model.IdentifierFaceEnabled = GetConfigBool(document.RootElement, "identifier", "face_identification_enabled", true);
            model.IdentifierQrCodeEnabled = GetConfigBool(document.RootElement, "identifier", "qrcode_identification_enabled", true);
            model.IdentifierPinEnabled = GetConfigBool(document.RootElement, "identifier", "pin_identification_enabled");
            model.FaceMaskDetectionEnabled = GetConfigString(document.RootElement, "face_id", "mask_detection_enabled", model.FaceMaskDetectionEnabled);
            model.FaceVehicleModeEnabled = GetConfigBool(document.RootElement, "face_id", "vehicle_mode");
            model.FaceMaxIdentifiedDuration = GetConfigInt(document.RootElement, "face_id", "max_identified_duration", model.FaceMaxIdentifiedDuration);
            model.FaceLimitIdentificationToDisplayRegion = GetConfigBool(document.RootElement, "face_id", "limit_identification_to_display_region");
            model.FaceMinDetectBoundsWidth = GetConfigString(document.RootElement, "face_id", "min_detect_bounds_width", model.FaceMinDetectBoundsWidth);
            model.FaceCameraOverlayZoom = GetConfigString(document.RootElement, "camera_overlay", "zoom", model.FaceCameraOverlayZoom);
            model.FaceCameraOverlayVerticalCrop = GetConfigString(document.RootElement, "camera_overlay", "vertical_crop", model.FaceCameraOverlayVerticalCrop);
            model.FaceLightThresholdLedActivation = GetConfigString(document.RootElement, "face_module", "light_threshold_led_activation", model.FaceLightThresholdLedActivation);
            model.FaceLedBrightness = GetConfigInt(document.RootElement, "led_white", "brightness", model.FaceLedBrightness);
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
            var module = root.TryGetProperty("barras", out var barras) && barras.ValueKind == JsonValueKind.Object && barras.TryGetProperty("qrcode_legacy_mode_enabled", out _)
                ? "barras"
                : "face_id";

            model.QrModule = module;
            model.QrCodeLegacyModeEnabled = GetConfigString(root, module, "qrcode_legacy_mode_enabled", model.QrCodeLegacyModeEnabled);
            model.QrTotpEnabled = GetConfigBool(root, module, "totp_enabled");
            model.QrTotpWindowSize = GetConfigInt(root, module, "totp_window_size", model.QrTotpWindowSize);
            model.QrTotpWindowNum = GetConfigInt(root, module, "totp_window_num", model.QrTotpWindowNum);
            model.QrTotpSingleUse = GetConfigBool(root, module, "totp_single_use", true);
            model.QrTotpTzOffset = GetConfigInt(root, module, "totp_tz_offset", model.QrTotpTzOffset);
        }

        private async Task PopulatePowerAsync(ProductSpecificViewModel model)
        {
            var (_, document) = await _apiService.InvokeJsonAsync("get-configuration", new
            {
                general = new[] { "screenshot_resize", "energy_mode", "energy_display_custom", "energy_sound_custom", "energy_ir_custom", "energy_led_custom" }
            });

            if (document == null)
                return;

            model.ScreenshotResize = GetConfigString(document.RootElement, "general", "screenshot_resize", model.ScreenshotResize);
            model.EnergyMode = GetConfigString(document.RootElement, "general", "energy_mode", model.EnergyMode);
            model.EnergyDisplayCustom = GetConfigString(document.RootElement, "general", "energy_display_custom", model.EnergyDisplayCustom);
            model.EnergySoundCustom = GetConfigString(document.RootElement, "general", "energy_sound_custom", model.EnergySoundCustom);
            model.EnergyIrCustom = GetConfigString(document.RootElement, "general", "energy_ir_custom", model.EnergyIrCustom);
            model.EnergyLedCustom = GetConfigString(document.RootElement, "general", "energy_led_custom", model.EnergyLedCustom);
        }

        private async Task PopulateStreamingAsync(ProductSpecificViewModel model)
        {
            var (_, document) = await _apiService.InvokeJsonAsync("get-configuration", new
            {
                onvif = new[] { "rtsp_enabled", "rtsp_port", "rtsp_username", "rtsp_password", "rtsp_rgb", "rtsp_codec", "onvif_enabled", "onvif_port", "rtsp_flipped", "rtsp_watermark_enabled", "rtsp_watermark_logo_enabled", "rtsp_watermark_custom_logo_enabled" }
            });

            if (document == null)
                return;

            model.StreamingRtspEnabled = GetConfigBool(document.RootElement, "onvif", "rtsp_enabled");
            model.StreamingRtspPort = GetConfigInt(document.RootElement, "onvif", "rtsp_port", model.StreamingRtspPort);
            model.StreamingRtspUsername = GetConfigString(document.RootElement, "onvif", "rtsp_username", model.StreamingRtspUsername);
            model.StreamingRtspPassword = GetConfigString(document.RootElement, "onvif", "rtsp_password", model.StreamingRtspPassword);
            model.StreamingRtspRgb = GetConfigBool(document.RootElement, "onvif", "rtsp_rgb", true);
            model.StreamingRtspCodec = GetConfigString(document.RootElement, "onvif", "rtsp_codec", model.StreamingRtspCodec);
            model.StreamingOnvifEnabled = GetConfigBool(document.RootElement, "onvif", "onvif_enabled");
            model.StreamingOnvifPort = GetConfigInt(document.RootElement, "onvif", "onvif_port", model.StreamingOnvifPort);
            model.StreamingRtspFlipped = GetConfigBool(document.RootElement, "onvif", "rtsp_flipped");
            model.StreamingWatermarkEnabled = GetConfigBool(document.RootElement, "onvif", "rtsp_watermark_enabled", true);
            model.StreamingWatermarkLogoEnabled = GetConfigBool(document.RootElement, "onvif", "rtsp_watermark_logo_enabled", true);
            model.StreamingWatermarkCustomLogoEnabled = GetConfigBool(document.RootElement, "onvif", "rtsp_watermark_custom_logo_enabled");
        }

        private async Task PopulateSipAsync(ProductSpecificViewModel model)
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
                model.SipEnabled = GetConfigBool(configDocument.RootElement, "pjsip", "enabled");
                model.SipServerIp = GetConfigString(configDocument.RootElement, "pjsip", "server_ip", model.SipServerIp);
                model.SipServerPort = GetConfigInt(configDocument.RootElement, "pjsip", "server_port", model.SipServerPort);
                model.SipServerOutboundPort = GetConfigInt(configDocument.RootElement, "pjsip", "server_outbound_port", model.SipServerOutboundPort);
                model.SipServerOutboundPortRange = GetConfigInt(configDocument.RootElement, "pjsip", "server_outbound_port_range", model.SipServerOutboundPortRange);
                model.SipNumericBranchEnabled = GetConfigBool(configDocument.RootElement, "pjsip", "numeric_branch_enabled", true);
                model.SipBranch = GetConfigString(configDocument.RootElement, "pjsip", "branch", model.SipBranch);
                model.SipLogin = GetConfigString(configDocument.RootElement, "pjsip", "login", model.SipLogin);
                model.SipPassword = GetConfigString(configDocument.RootElement, "pjsip", "password", model.SipPassword);
                model.SipPeerToPeerEnabled = GetConfigBool(configDocument.RootElement, "pjsip", "peer_to_peer_enabled");
                model.SipRegStatusQueryPeriod = GetConfigInt(configDocument.RootElement, "pjsip", "reg_status_query_period", model.SipRegStatusQueryPeriod);
                model.SipServerRetryInterval = GetConfigInt(configDocument.RootElement, "pjsip", "server_retry_interval", model.SipServerRetryInterval);
                model.SipMaxCallTime = GetConfigInt(configDocument.RootElement, "pjsip", "max_call_time", model.SipMaxCallTime);
                model.SipPushButtonDebounce = GetConfigInt(configDocument.RootElement, "pjsip", "push_button_debounce", model.SipPushButtonDebounce);
                model.SipAutoAnswerEnabled = GetConfigBool(configDocument.RootElement, "pjsip", "auto_answer_enabled");
                model.SipAutoAnswerDelay = GetConfigInt(configDocument.RootElement, "pjsip", "auto_answer_delay", model.SipAutoAnswerDelay);
                model.SipAutoCallButtonEnabled = GetConfigBool(configDocument.RootElement, "pjsip", "auto_call_button_enabled", true);
                model.SipRexEnabled = GetConfigBool(configDocument.RootElement, "pjsip", "rex_enabled");
                model.SipDialingDisplayMode = GetConfigString(configDocument.RootElement, "pjsip", "dialing_display_mode", model.SipDialingDisplayMode);
                model.SipAutoCallTarget = GetConfigString(configDocument.RootElement, "pjsip", "auto_call_target", model.SipAutoCallTarget);
                model.SipCustomIdentifierAutoCall = GetConfigString(configDocument.RootElement, "pjsip", "custom_identifier_auto_call", model.SipCustomIdentifierAutoCall);
                model.SipVideoEnabled = GetConfigBool(configDocument.RootElement, "pjsip", "video_enabled");
                model.SipCustomAudioEnabled = GetConfigBool(configDocument.RootElement, "pjsip", "pjsip_custom_audio_enabled");
                model.SipCustomAudioVolumeGain = GetConfigString(configDocument.RootElement, "pjsip", "custom_audio_volume_gain", model.SipCustomAudioVolumeGain);
                model.SipMicVolume = GetConfigInt(configDocument.RootElement, "pjsip", "mic_volume", model.SipMicVolume);
                model.SipSpeakerVolume = GetConfigInt(configDocument.RootElement, "pjsip", "speaker_volume", model.SipSpeakerVolume);
                model.SipOpenDoorEnabled = GetConfigBool(configDocument.RootElement, "pjsip", "open_door_enabled");
                model.SipOpenDoorCommand = GetConfigString(configDocument.RootElement, "pjsip", "open_door_command", model.SipOpenDoorCommand);
                model.SipFacialIdDuringCallEnabled = GetConfigBool(configDocument.RootElement, "pjsip", "facial_id_during_call_enabled");
            }

            var (_, statusDocument) = await _apiService.InvokeJsonAsync("get-sip-status");
            if (statusDocument == null)
                return;

            model.SipStatusCode = GetRootInt(statusDocument.RootElement, "status", model.SipStatusCode);
            model.SipInCall = GetRootBool(statusDocument.RootElement, "in_call");
        }

        private async Task PopulateSipAudioAsync(ProductSpecificViewModel model)
        {
            var (_, document) = await _apiService.InvokeJsonAsync("has-pjsip-audio-message");
            if (document != null)
                model.SipAudioExists = GetRootBool(document.RootElement, "file_exists");
        }

        private async Task PopulateAccessAudioAsync(ProductSpecificViewModel model)
        {
            var (_, configDocument) = await _apiService.InvokeJsonAsync("get-configuration", new
            {
                buzzer = new[] { "audio_message_not_identified", "audio_message_authorized", "audio_message_not_authorized", "audio_message_use_mask", "audio_message_volume_gain" }
            });

            if (configDocument != null)
            {
                model.AccessAudioNotIdentified = GetConfigString(configDocument.RootElement, "buzzer", "audio_message_not_identified", model.AccessAudioNotIdentified);
                model.AccessAudioAuthorized = GetConfigString(configDocument.RootElement, "buzzer", "audio_message_authorized", model.AccessAudioAuthorized);
                model.AccessAudioNotAuthorized = GetConfigString(configDocument.RootElement, "buzzer", "audio_message_not_authorized", model.AccessAudioNotAuthorized);
                model.AccessAudioUseMask = GetConfigString(configDocument.RootElement, "buzzer", "audio_message_use_mask", model.AccessAudioUseMask);
                model.AccessAudioVolumeGain = GetConfigString(configDocument.RootElement, "buzzer", "audio_message_volume_gain", model.AccessAudioVolumeGain);
            }

            var (_, statusDocument) = await _apiService.InvokeJsonAsync("has-audio-access-messages");
            if (statusDocument == null)
                return;

            model.AccessAudioHasNotIdentified = GetRootBool(statusDocument.RootElement, "not_identified");
            model.AccessAudioHasAuthorized = GetRootBool(statusDocument.RootElement, "authorized");
            model.AccessAudioHasNotAuthorized = GetRootBool(statusDocument.RootElement, "not_authorized");
            model.AccessAudioHasUseMask = GetRootBool(statusDocument.RootElement, "use_mask");
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

            model.SignalSecBoxOutMode = GetConfigString(document.RootElement, "general", "sec_box_out_mode", model.SignalSecBoxOutMode);
            model.SignalRelayOutMode = GetConfigString(document.RootElement, "general", "relay_out_mode", model.SignalRelayOutMode);
            model.SignalRelayEnabled = GetConfigBool(document.RootElement, "general", "relay1_enabled", true);
            model.SignalRelayAutoClose = GetConfigBool(document.RootElement, "general", "relay1_auto_close", true);
            model.SignalRelayTimeout = GetConfigInt(document.RootElement, "general", "relay1_timeout", model.SignalRelayTimeout);
            model.SignalGpioExt1Mode = GetConfigString(document.RootElement, "general", "gpio_ext1_mode", model.SignalGpioExt1Mode);
            model.SignalGpioExt1Idle = GetConfigString(document.RootElement, "general", "gpio_ext1_idle", model.SignalGpioExt1Idle);
            model.SignalGpioExt2Mode = GetConfigString(document.RootElement, "general", "gpio_ext2_mode", model.SignalGpioExt2Mode);
            model.SignalGpioExt2Idle = GetConfigString(document.RootElement, "general", "gpio_ext2_idle", model.SignalGpioExt2Idle);
            model.SignalGpioExt3Mode = GetConfigString(document.RootElement, "general", "gpio_ext3_mode", model.SignalGpioExt3Mode);
            model.SignalGpioExt3Idle = GetConfigString(document.RootElement, "general", "gpio_ext3_idle", model.SignalGpioExt3Idle);
        }

        private bool EnsureConnected(ProductSpecificViewModel model)
        {
            if (_apiService.TryGetConnection(out _, out _))
                return true;

            model.ErrorMessage = "E necessario conectar-se e autenticar com um equipamento Control iD.";
            return false;
        }

        private static void EnsureSuccess(OfficialApiInvocationResult result, string message)
        {
            if (result.Success)
                return;

            if (!string.IsNullOrWhiteSpace(result.ErrorMessage))
                throw new InvalidOperationException($"{message}: {result.ErrorMessage}");

            if (!string.IsNullOrWhiteSpace(result.ResponseBody) && !result.ResponseBodyIsBase64)
                throw new InvalidOperationException($"{message}: {result.ResponseBody}");

            throw new InvalidOperationException($"{message} (status HTTP {result.StatusCode}).");
        }

        private IActionResult DownloadBinaryResult(OfficialApiInvocationResult result, string fileName, string fallbackContentType)
        {
            if (result.ResponseBodyIsBase64 && !string.IsNullOrWhiteSpace(result.ResponseBody))
            {
                var bytes = Convert.FromBase64String(result.ResponseBody);
                return File(bytes, string.IsNullOrWhiteSpace(result.ResponseContentType) ? fallbackContentType : result.ResponseContentType, fileName);
            }

            return File(System.Text.Encoding.UTF8.GetBytes(result.ResponseBody ?? string.Empty), fallbackContentType, fileName);
        }

        private static async Task<string> ReadFileAsBase64Async(IFormFile? file, string emptyMessage)
        {
            if (file == null || file.Length == 0)
                throw new InvalidOperationException(emptyMessage);

            await using var stream = file.OpenReadStream();
            using var memory = new MemoryStream();
            await stream.CopyToAsync(memory);
            return Convert.ToBase64String(memory.ToArray());
        }

        private static string FormatJson(string rawJson, JsonDocument? document)
        {
            if (document == null)
                return rawJson;

            return JsonSerializer.Serialize(document.RootElement, new JsonSerializerOptions
            {
                WriteIndented = true
            });
        }

        private static string BoolString(bool value) => value ? "1" : "0";

        private static string GetConfigString(JsonElement root, string section, string field, string fallback = "")
        {
            if (root.TryGetProperty(section, out var sectionElement) && sectionElement.ValueKind == JsonValueKind.Object && sectionElement.TryGetProperty(field, out var fieldElement))
                return fieldElement.ToString() ?? fallback;

            return fallback;
        }

        private static bool GetConfigBool(JsonElement root, string section, string field, bool fallback = false)
        {
            var value = GetConfigString(root, section, field, fallback ? "1" : "0");
            return value == "1" || value.Equals("true", StringComparison.OrdinalIgnoreCase);
        }

        private static int GetConfigInt(JsonElement root, string section, string field, int fallback)
        {
            var value = GetConfigString(root, section, field, fallback.ToString());
            return int.TryParse(value, out var parsed) ? parsed : fallback;
        }

        private static bool GetRootBool(JsonElement root, string name)
        {
            if (!root.TryGetProperty(name, out var value))
                return false;

            return value.ValueKind switch
            {
                JsonValueKind.True => true,
                JsonValueKind.False => false,
                JsonValueKind.Number => value.TryGetInt32(out var number) && number != 0,
                JsonValueKind.String => value.GetString() is string text && (text == "1" || text.Equals("true", StringComparison.OrdinalIgnoreCase)),
                _ => false
            };
        }

        private static int GetRootInt(JsonElement root, string name, int fallback)
        {
            if (!root.TryGetProperty(name, out var value))
                return fallback;

            if (value.ValueKind == JsonValueKind.Number && value.TryGetInt32(out var numeric))
                return numeric;

            return value.ValueKind == JsonValueKind.String && int.TryParse(value.GetString(), out var parsed) ? parsed : fallback;
        }
    }
}
