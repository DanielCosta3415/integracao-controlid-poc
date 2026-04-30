using System.Text.Json;
using Integracao.ControlID.PoC.Services.ControlIDApi;
using Integracao.ControlID.PoC.Services.DocumentedFeatures;
using Integracao.ControlID.PoC.ViewModels.DocumentedFeatures;
using Integracao.ControlID.PoC.Helpers;
using Microsoft.AspNetCore.Mvc;

namespace Integracao.ControlID.PoC.Controllers
{
    public class DocumentedFeaturesController : Controller
    {
        private readonly OfficialControlIdApiService _apiService;
        private readonly DocumentedFeaturesPayloadFactory _payloadFactory;
        private readonly OfficialApiResultPresentationService _resultPresentationService;
        private readonly OfficialApiBinaryFileResultFactory _binaryFileResultFactory;
        private readonly ILogger<DocumentedFeaturesController> _logger;

        public DocumentedFeaturesController(
            OfficialControlIdApiService apiService,
            DocumentedFeaturesPayloadFactory payloadFactory,
            OfficialApiResultPresentationService resultPresentationService,
            OfficialApiBinaryFileResultFactory binaryFileResultFactory,
            ILogger<DocumentedFeaturesController> logger)
        {
            _apiService = apiService;
            _payloadFactory = payloadFactory;
            _resultPresentationService = resultPresentationService;
            _binaryFileResultFactory = binaryFileResultFactory;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            var model = new DocumentedFeaturesViewModel();
            if (!EnsureConnected(model))
                return View(model);

            await PopulateAllAsync(model);
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Attendance(DocumentedFeaturesViewModel model)
        {
            model.ActiveSection = "attendance";
            if (!EnsureConnected(model))
                return View("Index", model);

            try
            {
                var (result, document) = await _apiService.InvokeJsonAsync("set-configuration", _payloadFactory.BuildAttendanceSettings(model));

                _resultPresentationService.EnsureSuccess(result, "Erro ao aplicar modo ponto");
                model.ResultMessage = "Modo ponto atualizado com sucesso.";
                model.ResultStatusType = "success";
                model.ResponseJson = _resultPresentationService.FormatJson(result.ResponseBody, document);
            }
            catch (Exception ex)
            {
                model.ErrorMessage = SecurityTextHelper.BuildSafeUserMessage("A operação não pôde ser concluída", ex);
                _logger.LogError(ex, "Erro ao aplicar configuracoes de modo ponto.");
            }

            return View("Index", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> OnlineMode(DocumentedFeaturesViewModel model)
        {
            model.ActiveSection = "online";
            if (!EnsureConnected(model))
                return View("Index", model);

            try
            {
                long serverId;
                if (model.OnlineUseExistingDevice && model.OnlineExistingDeviceId.HasValue && model.OnlineExistingDeviceId.Value > 0)
                {
                    serverId = model.OnlineExistingDeviceId.Value;
                }
                else
                {
                    var (createResult, createDocument) = await _apiService.InvokeJsonAsync("create-objects", new
                    {
                        @object = "devices",
                        values = new[]
                        {
                            new
                            {
                                name = model.OnlineServerName,
                                ip = model.OnlineServerUrl,
                                public_key = model.OnlinePublicKey
                            }
                        }
                    });

                    _resultPresentationService.EnsureSuccess(createResult, "Erro ao criar device para modo online");
                    serverId = ReadFirstId(createDocument) ?? throw new InvalidOperationException("A API nao retornou um server_id valido.");
                }

                var (result, document) = await _apiService.InvokeJsonAsync("set-configuration", _payloadFactory.BuildOnlineSettings(model, serverId));

                _resultPresentationService.EnsureSuccess(result, "Erro ao configurar modo online");
                model.OnlineCurrentServerId = serverId;
                model.ResultMessage = $"Modo online configurado com sucesso com server_id {serverId}.";
                model.ResultStatusType = "success";
                model.ResponseJson = _resultPresentationService.FormatJson(result.ResponseBody, document);
            }
            catch (Exception ex)
            {
                model.ErrorMessage = SecurityTextHelper.BuildSafeUserMessage("A operação não pôde ser concluída", ex);
                _logger.LogError(ex, "Erro ao configurar modo online.");
            }

            return View("Index", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Security(DocumentedFeaturesViewModel model)
        {
            model.ActiveSection = "security";
            if (!EnsureConnected(model))
                return View("Index", model);

            try
            {
                var (result, document) = await _apiService.InvokeJsonAsync("set-configuration", _payloadFactory.BuildSecuritySettings(model));

                _resultPresentationService.EnsureSuccess(result, "Erro ao aplicar seguranca operacional");
                model.ResultMessage = "Seguranca operacional atualizada com sucesso.";
                model.ResultStatusType = "success";
                model.ResponseJson = _resultPresentationService.FormatJson(result.ResponseBody, document);
            }
            catch (Exception ex)
            {
                model.ErrorMessage = SecurityTextHelper.BuildSafeUserMessage("A operação não pôde ser concluída", ex);
                _logger.LogError(ex, "Erro ao aplicar configuracoes de seguranca.");
            }

            return View("Index", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Visitors(DocumentedFeaturesViewModel model)
        {
            model.ActiveSection = "visitors";
            if (!EnsureConnected(model))
                return View("Index", model);

            try
            {
                var (result, document) = await _apiService.InvokeJsonAsync("set-configuration", _payloadFactory.BuildVisitorsSettings(model));

                _resultPresentationService.EnsureSuccess(result, "Erro ao aplicar suporte a visitantes");
                model.ResultMessage = "Configuracoes de visitantes atualizadas com sucesso.";
                model.ResultStatusType = "success";
                model.ResponseJson = _resultPresentationService.FormatJson(result.ResponseBody, document);
            }
            catch (Exception ex)
            {
                model.ErrorMessage = SecurityTextHelper.BuildSafeUserMessage("A operação não pôde ser concluída", ex);
                _logger.LogError(ex, "Erro ao aplicar suporte a visitantes.");
            }

            return View("Index", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> IdCloud(DocumentedFeaturesViewModel model)
        {
            model.ActiveSection = "idcloud";
            if (!EnsureConnected(model))
                return View("Index", model);

            try
            {
                var (result, document) = await _apiService.InvokeJsonAsync("set-configuration", _payloadFactory.BuildIdCloudSettings(model));

                _resultPresentationService.EnsureSuccess(result, "Erro ao configurar iDCloud");
                await PopulateIdCloudAsync(model);
                model.ResultMessage = "Configuracao do iDCloud atualizada com sucesso.";
                model.ResultStatusType = "success";
                model.ResponseJson = _resultPresentationService.FormatJson(result.ResponseBody, document);
            }
            catch (Exception ex)
            {
                model.ErrorMessage = SecurityTextHelper.BuildSafeUserMessage("A operação não pôde ser concluída", ex);
                _logger.LogError(ex, "Erro ao configurar iDCloud.");
            }

            return View("Index", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RegenerateIdCloudCode(DocumentedFeaturesViewModel model)
        {
            model.ActiveSection = "idcloud";
            if (!EnsureConnected(model))
                return View("Index", model);

            try
            {
                var result = await _apiService.InvokeAsync("change-idcloud-code");
                _resultPresentationService.EnsureSuccess(result, "Erro ao regenerar codigo do iDCloud");

                await PopulateIdCloudAsync(model);
                model.ResultMessage = "Codigo de verificacao do iDCloud regenerado com sucesso.";
                model.ResultStatusType = "success";
                model.ResponseJson = _resultPresentationService.FormatResponseBody(result);
            }
            catch (Exception ex)
            {
                model.ErrorMessage = SecurityTextHelper.BuildSafeUserMessage("A operação não pôde ser concluída", ex);
                _logger.LogError(ex, "Erro ao regenerar codigo do iDCloud.");
            }

            return View("Index", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Alarm(DocumentedFeaturesViewModel model)
        {
            model.ActiveSection = "alarm";
            if (!EnsureConnected(model))
                return View("Index", model);

            try
            {
                var (result, document) = await _apiService.InvokeJsonAsync("set-configuration", _payloadFactory.BuildAlarmSettings(model));

                _resultPresentationService.EnsureSuccess(result, "Erro ao configurar parametros de alarme");
                await PopulateAlarmAsync(model);
                model.ResultMessage = "Configuracoes de alarme atualizadas com sucesso.";
                model.ResultStatusType = "success";
                model.ResponseJson = _resultPresentationService.FormatJson(result.ResponseBody, document);
            }
            catch (Exception ex)
            {
                model.ErrorMessage = SecurityTextHelper.BuildSafeUserMessage("A operação não pôde ser concluída", ex);
                _logger.LogError(ex, "Erro ao configurar alarme.");
            }

            return View("Index", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> StopAlarm(DocumentedFeaturesViewModel model)
        {
            model.ActiveSection = "alarm";
            if (!EnsureConnected(model))
                return View("Index", model);

            try
            {
                var (result, document) = await _apiService.InvokeJsonAsync("alarm-status", new { stop = true });
                _resultPresentationService.EnsureSuccess(result, "Erro ao interromper alarme");
                await PopulateAlarmAsync(model);
                model.ResultMessage = "Alarme interrompido com sucesso.";
                model.ResultStatusType = "success";
                model.ResponseJson = _resultPresentationService.FormatJson(result.ResponseBody, document);
            }
            catch (Exception ex)
            {
                model.ErrorMessage = SecurityTextHelper.BuildSafeUserMessage("A operação não pôde ser concluída", ex);
                _logger.LogError(ex, "Erro ao interromper alarme.");
            }

            return View("Index", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GenerateReport(DocumentedFeaturesViewModel model)
        {
            model.ActiveSection = "reports";
            if (!EnsureConnected(model))
                return View("Index", model);

            try
            {
                JsonDocument.Parse(model.ReportPayload);
                var result = await _apiService.InvokeAsync("report-generate", model.ReportPayload);
                _resultPresentationService.EnsureSuccess(result, "Erro ao gerar relatorio customizado");
                return _binaryFileResultFactory.Create(result, "report-generate.txt", "text/plain");
            }
            catch (Exception ex)
            {
                model.ErrorMessage = SecurityTextHelper.BuildSafeUserMessage("A operação não pôde ser concluída", ex);
                _logger.LogError(ex, "Erro ao gerar relatorio customizado.");
                return View("Index", model);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ExportAfd(DocumentedFeaturesViewModel model)
        {
            model.ActiveSection = "reports";
            if (!EnsureConnected(model))
                return View("Index", model);

            try
            {
                var result = await _apiService.InvokeAsync(
                    "export-afd",
                    _payloadFactory.BuildAfdExport(model),
                    model.AfdMode == "671" ? "mode=671" : string.Empty);

                _resultPresentationService.EnsureSuccess(result, "Erro ao exportar AFD");
                return _binaryFileResultFactory.Create(result, $"AFD-{model.AfdMode}.txt", "text/plain");
            }
            catch (Exception ex)
            {
                model.ErrorMessage = SecurityTextHelper.BuildSafeUserMessage("A operação não pôde ser concluída", ex);
                _logger.LogError(ex, "Erro ao exportar AFD.");
                return View("Index", model);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ExportAuditLogs(DocumentedFeaturesViewModel model)
        {
            model.ActiveSection = "reports";
            if (!EnsureConnected(model))
                return View("Index", model);

            try
            {
                var result = await _apiService.InvokeAsync("export-audit-logs", _payloadFactory.BuildAuditLogsExport(model));

                _resultPresentationService.EnsureSuccess(result, "Erro ao exportar logs de auditoria");
                return _binaryFileResultFactory.Create(result, "audit-logs.txt", "text/plain");
            }
            catch (Exception ex)
            {
                model.ErrorMessage = SecurityTextHelper.BuildSafeUserMessage("A operação não pôde ser concluída", ex);
                _logger.LogError(ex, "Erro ao exportar logs de auditoria.");
                return View("Index", model);
            }

        }

        private async Task PopulateAllAsync(DocumentedFeaturesViewModel model)
        {
            await PopulateAttendanceAsync(model);
            await PopulateOnlineAsync(model);
            await PopulateSecurityAsync(model);
            await PopulateVisitorsAsync(model);
            await PopulateIdCloudAsync(model);
            await PopulateAlarmAsync(model);
        }

        private async Task PopulateAttendanceAsync(DocumentedFeaturesViewModel model)
        {
            var (_, document) = await _apiService.InvokeJsonAsync("get-configuration", new
            {
                general = new[] { "attendance_mode", "clear_expired_users" },
                identifier = new[] { "log_type" }
            });

            if (document == null)
                return;

            model.AttendanceModeEnabled = GetConfigBool(document.RootElement, "general", "attendance_mode");
            model.AttendanceCustomLogTypesEnabled = GetConfigBool(document.RootElement, "identifier", "log_type");
            model.AttendanceClearExpiredUsers = GetConfigString(document.RootElement, "general", "clear_expired_users", model.AttendanceClearExpiredUsers);
        }

        private async Task PopulateOnlineAsync(DocumentedFeaturesViewModel model)
        {
            var (_, document) = await _apiService.InvokeJsonAsync("get-configuration", new
            {
                general = new[] { "online", "local_identification" },
                online_client = new[] { "server_id", "extract_template", "max_request_attempts" }
            });

            if (document == null)
                return;

            model.OnlineEnabled = GetConfigBool(document.RootElement, "general", "online");
            model.OnlineLocalIdentification = GetConfigBool(document.RootElement, "general", "local_identification", true);
            model.OnlineExtractTemplate = GetConfigBool(document.RootElement, "online_client", "extract_template");
            model.OnlineMaxRequestAttempts = GetConfigInt(document.RootElement, "online_client", "max_request_attempts", model.OnlineMaxRequestAttempts);
            model.OnlineCurrentServerId = GetConfigLong(document.RootElement, "online_client", "server_id");
            model.OnlineExistingDeviceId = model.OnlineCurrentServerId;
        }

        private async Task PopulateSecurityAsync(DocumentedFeaturesViewModel model)
        {
            var (_, document) = await _apiService.InvokeJsonAsync("get-configuration", new
            {
                general = new[] { "ssh_enabled", "usb_port_enabled", "web_server_enabled" },
                snmp_agent = new[] { "snmp_enabled" }
            });

            if (document == null)
                return;

            model.SecuritySshEnabled = GetConfigBool(document.RootElement, "general", "ssh_enabled");
            model.SecurityUsbPortEnabled = GetConfigBool(document.RootElement, "general", "usb_port_enabled", true);
            model.SecurityWebServerEnabled = GetConfigBool(document.RootElement, "general", "web_server_enabled", true);
            model.SecuritySnmpEnabled = GetConfigBool(document.RootElement, "snmp_agent", "snmp_enabled");
        }

        private async Task PopulateVisitorsAsync(DocumentedFeaturesViewModel model)
        {
            var (_, document) = await _apiService.InvokeJsonAsync("get-configuration", new
            {
                general = new[] { "clear_expired_users" },
                sec_box = new[] { "catra_collect_visitor_card" }
            });

            if (document == null)
                return;

            model.VisitorsClearExpiredUsers = GetConfigString(document.RootElement, "general", "clear_expired_users", model.VisitorsClearExpiredUsers);
            model.VisitorsCollectCardOnExit = GetConfigBool(document.RootElement, "sec_box", "catra_collect_visitor_card");
        }

        private async Task PopulateIdCloudAsync(DocumentedFeaturesViewModel model)
        {
            var (_, configDocument) = await _apiService.InvokeJsonAsync("get-configuration", new
            {
                push_server = new[] { "push_remote_address", "push_request_timeout", "push_request_period" }
            });

            if (configDocument != null)
            {
                model.IdCloudPushRemoteAddress = GetConfigString(configDocument.RootElement, "push_server", "push_remote_address", model.IdCloudPushRemoteAddress);
                model.IdCloudPushRequestTimeout = GetConfigInt(configDocument.RootElement, "push_server", "push_request_timeout", model.IdCloudPushRequestTimeout);
                model.IdCloudPushRequestPeriod = GetConfigInt(configDocument.RootElement, "push_server", "push_request_period", model.IdCloudPushRequestPeriod);
            }

            var (_, systemDocument) = await _apiService.InvokeJsonAsync("system-information");
            if (systemDocument != null)
                model.IdCloudVerificationCode = GetRootString(systemDocument.RootElement, "iDCloud_code", "idcloud_code");
        }

        private async Task PopulateAlarmAsync(DocumentedFeaturesViewModel model)
        {
            var (_, statusDocument) = await _apiService.InvokeJsonAsync("alarm-status", new { });
            if (statusDocument != null)
            {
                model.AlarmActive = GetRootBool(statusDocument.RootElement, "active");
                model.AlarmCause = GetRootInt(statusDocument.RootElement, "cause", 0);
            }

            var (_, configDocument) = await _apiService.InvokeJsonAsync("get-configuration", new
            {
                alarm = new[]
                {
                    "device_violation_enabled",
                    "door_sensor_alarm_timeout_after_closure",
                    "door_sensor_delay",
                    "door_sensor_enabled",
                    "forced_access_debounce",
                    "forced_access_enabled",
                    "panic_card_enabled",
                    "panic_finger_delay",
                    "panic_finger_enabled",
                    "panic_password_enabled",
                    "panic_pin_enabled"
                }
            });

            if (configDocument == null)
                return;

            model.AlarmDeviceViolationEnabled = GetConfigBool(configDocument.RootElement, "alarm", "device_violation_enabled");
            model.AlarmDoorSensorAlarmTimeoutAfterClosure = GetConfigInt(configDocument.RootElement, "alarm", "door_sensor_alarm_timeout_after_closure", 0);
            model.AlarmDoorSensorDelay = GetConfigInt(configDocument.RootElement, "alarm", "door_sensor_delay", model.AlarmDoorSensorDelay);
            model.AlarmDoorSensorEnabled = GetConfigBool(configDocument.RootElement, "alarm", "door_sensor_enabled", true);
            model.AlarmForcedAccessDebounce = GetConfigInt(configDocument.RootElement, "alarm", "forced_access_debounce", model.AlarmForcedAccessDebounce);
            model.AlarmForcedAccessEnabled = GetConfigBool(configDocument.RootElement, "alarm", "forced_access_enabled");
            model.AlarmPanicCardEnabled = GetConfigBool(configDocument.RootElement, "alarm", "panic_card_enabled", true);
            model.AlarmPanicFingerDelay = GetConfigInt(configDocument.RootElement, "alarm", "panic_finger_delay", model.AlarmPanicFingerDelay);
            model.AlarmPanicFingerEnabled = GetConfigBool(configDocument.RootElement, "alarm", "panic_finger_enabled", true);
            model.AlarmPanicPasswordEnabled = GetConfigBool(configDocument.RootElement, "alarm", "panic_password_enabled", true);
            model.AlarmPanicPinEnabled = GetConfigBool(configDocument.RootElement, "alarm", "panic_pin_enabled", true);
        }

        private bool EnsureConnected(DocumentedFeaturesViewModel model)
        {
            if (_apiService.TryGetConnection(out _, out _))
                return true;

            model.ErrorMessage = "E necessario conectar-se e autenticar com um equipamento Control iD.";
            return false;
        }

        private static long? ReadFirstId(JsonDocument? document)
        {
            if (document == null || !document.RootElement.TryGetProperty("ids", out var ids) || ids.ValueKind != JsonValueKind.Array || ids.GetArrayLength() == 0)
                return null;

            var first = ids[0];
            if (first.ValueKind == JsonValueKind.Number && first.TryGetInt64(out var numeric))
                return numeric;

            if (first.ValueKind == JsonValueKind.String && long.TryParse(first.GetString(), out var parsed))
                return parsed;

            return null;
        }

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

        private static long? GetConfigLong(JsonElement root, string section, string field)
        {
            var value = GetConfigString(root, section, field, string.Empty);
            return long.TryParse(value, out var parsed) ? parsed : null;
        }

        private static string GetRootString(JsonElement root, params string[] names)
        {
            foreach (var name in names)
            {
                if (root.TryGetProperty(name, out var value))
                    return value.ToString() ?? string.Empty;
            }

            return string.Empty;
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



