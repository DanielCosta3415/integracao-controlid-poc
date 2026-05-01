using System.Text.Json;
using Integracao.ControlID.PoC.Models.ControlIDApi;
using Integracao.ControlID.PoC.Services.ControlIDApi;
using Integracao.ControlID.PoC.Services.Security;
using Integracao.ControlID.PoC.ViewModels.OfficialObjects;
using Integracao.ControlID.PoC.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Integracao.ControlID.PoC.Controllers
{
    public class OfficialObjectsController : Controller
    {
        private static readonly IReadOnlyList<OfficialObjectDefinition> Definitions = BuildDefinitions();

        private readonly OfficialControlIdApiService _apiService;
        private readonly ILogger<OfficialObjectsController> _logger;

        public OfficialObjectsController(OfficialControlIdApiService apiService, ILogger<OfficialObjectsController> logger)
        {
            _apiService = apiService;
            _logger = logger;
        }

        public IActionResult Index(string? objectName = null)
        {
            var model = new OfficialObjectsViewModel
            {
                SelectedObjectName = string.IsNullOrWhiteSpace(objectName) ? "areas" : objectName
            };

            PopulateDefinitions(model, resetSamples: true);

            if (!EnsureConnected(model))
                return View(model);

            model.ResultMessage = "Selecione qualquer objeto oficial documentado e execute load/create/create-or-modify/modify/destroy com os exemplos prontos.";
            model.ResultStatusType = "info";
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult SelectObject(OfficialObjectsViewModel model)
        {
            model.ActiveSection = "catalog";
            PopulateDefinitions(model, resetSamples: true);

            if (!EnsureConnected(model))
                return View("Index", model);

            model.ResultMessage = $"Exemplos carregados para o objeto '{model.SelectedObjectName}'.";
            model.ResultStatusType = "info";
            return View("Index", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Load(OfficialObjectsViewModel model)
        {
            model.ActiveSection = "load";
            PopulateDefinitions(model);
            if (!EnsureConnected(model))
                return View("Index", model);

            try
            {
                var payload = string.IsNullOrWhiteSpace(model.LoadWhereJson)
                    ? $$"""
                    {
                      "object": "{{model.SelectedObjectName}}"
                    }
                    """
                    : $$"""
                    {
                      "object": "{{model.SelectedObjectName}}",
                      "where": {{model.LoadWhereJson}}
                    }
                    """;

                var (result, document) = await _apiService.InvokeJsonAsync("load-objects", payload);
                EnsureSuccess(result, "Erro ao carregar objetos");
                model.ResultMessage = $"Consulta do objeto '{model.SelectedObjectName}' concluida com sucesso.";
                model.ResultStatusType = "success";
                model.ResponseJson = FormatJson(result.ResponseBody, document);
            }
            catch (Exception ex)
            {
                model.ErrorMessage = SecurityTextHelper.BuildSafeUserMessage("A operação não pôde ser concluída", ex);
                _logger.LogError(ex, "Erro ao consultar objeto oficial {ObjectName}.", model.SelectedObjectName);
            }

            return View("Index", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = AppSecurityRoles.Administrator)]
        public async Task<IActionResult> Create(OfficialObjectsViewModel model)
        {
            model.ActiveSection = "create";
            PopulateDefinitions(model);
            if (!EnsureConnected(model))
                return View("Index", model);

            try
            {
                JsonDocument.Parse(model.CreateValuesJson);

                var payload = $$"""
                {
                  "object": "{{model.SelectedObjectName}}",
                  "values": {{model.CreateValuesJson}}
                }
                """;

                var (result, document) = await _apiService.InvokeJsonAsync("create-objects", payload);
                EnsureSuccess(result, "Erro ao criar objetos");
                model.ResultMessage = $"Criacao do objeto '{model.SelectedObjectName}' concluida com sucesso.";
                model.ResultStatusType = "success";
                model.ResponseJson = FormatJson(result.ResponseBody, document);
            }
            catch (Exception ex)
            {
                model.ErrorMessage = SecurityTextHelper.BuildSafeUserMessage("A operação não pôde ser concluída", ex);
                _logger.LogError(ex, "Erro ao criar objeto oficial {ObjectName}.", model.SelectedObjectName);
            }

            return View("Index", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = AppSecurityRoles.Administrator)]
        public async Task<IActionResult> CreateOrModify(OfficialObjectsViewModel model)
        {
            model.ActiveSection = "upsert";
            PopulateDefinitions(model);
            if (!EnsureConnected(model))
                return View("Index", model);

            try
            {
                JsonDocument.Parse(model.UpsertValuesJson);

                var payload = $$"""
                {
                  "object": "{{model.SelectedObjectName}}",
                  "values": {{model.UpsertValuesJson}}
                }
                """;

                var (result, document) = await _apiService.InvokeJsonAsync("create-or-modify-objects", payload);
                EnsureSuccess(result, "Erro ao executar create-or-modify");
                model.ResultMessage = $"Create-or-modify do objeto '{model.SelectedObjectName}' concluido com sucesso.";
                model.ResultStatusType = "success";
                model.ResponseJson = FormatJson(result.ResponseBody, document);
            }
            catch (Exception ex)
            {
                model.ErrorMessage = SecurityTextHelper.BuildSafeUserMessage("A operação não pôde ser concluída", ex);
                _logger.LogError(ex, "Erro ao executar create-or-modify para {ObjectName}.", model.SelectedObjectName);
            }

            return View("Index", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = AppSecurityRoles.Administrator)]
        public async Task<IActionResult> Modify(OfficialObjectsViewModel model)
        {
            model.ActiveSection = "modify";
            PopulateDefinitions(model);
            if (!EnsureConnected(model))
                return View("Index", model);

            try
            {
                JsonDocument.Parse(model.ModifyWhereJson);
                JsonDocument.Parse(model.ModifyValuesJson);

                var payload = $$"""
                {
                  "object": "{{model.SelectedObjectName}}",
                  "where": {{model.ModifyWhereJson}},
                  "values": {{model.ModifyValuesJson}}
                }
                """;

                var (result, document) = await _apiService.InvokeJsonAsync("modify-objects", payload);
                EnsureSuccess(result, "Erro ao modificar objetos");
                model.ResultMessage = $"Alteracao do objeto '{model.SelectedObjectName}' concluida com sucesso.";
                model.ResultStatusType = "success";
                model.ResponseJson = FormatJson(result.ResponseBody, document);
            }
            catch (Exception ex)
            {
                model.ErrorMessage = SecurityTextHelper.BuildSafeUserMessage("A operação não pôde ser concluída", ex);
                _logger.LogError(ex, "Erro ao modificar objeto oficial {ObjectName}.", model.SelectedObjectName);
            }

            return View("Index", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = AppSecurityRoles.Administrator)]
        public async Task<IActionResult> Destroy(OfficialObjectsViewModel model)
        {
            model.ActiveSection = "destroy";
            PopulateDefinitions(model);
            if (!EnsureConnected(model))
                return View("Index", model);

            var expectedConfirmation = HighImpactOperationGuard.BuildDestroyObjectsConfirmation(model.SelectedObjectName);
            if (!HighImpactOperationGuard.IsConfirmed(model.DestroyConfirmationPhrase, expectedConfirmation))
            {
                model.ErrorMessage = HighImpactOperationGuard.BuildRequiredMessage(expectedConfirmation);
                return View("Index", model);
            }

            try
            {
                JsonDocument.Parse(model.DestroyWhereJson);

                var payload = $$"""
                {
                  "object": "{{model.SelectedObjectName}}",
                  "where": {{model.DestroyWhereJson}}
                }
                """;

                var (result, document) = await _apiService.InvokeJsonAsync("destroy-objects", payload);
                EnsureSuccess(result, "Erro ao remover objetos");
                model.ResultMessage = $"Remocao do objeto '{model.SelectedObjectName}' concluida com sucesso.";
                model.ResultStatusType = "success";
                model.ResponseJson = FormatJson(result.ResponseBody, document);
            }
            catch (Exception ex)
            {
                model.ErrorMessage = SecurityTextHelper.BuildSafeUserMessage("A operação não pôde ser concluída", ex);
                _logger.LogError(ex, "Erro ao remover objeto oficial {ObjectName}.", model.SelectedObjectName);
            }

            return View("Index", model);
        }

        private static IReadOnlyList<OfficialObjectDefinition> BuildDefinitions()
        {
            static OfficialObjectDefinition D(string name, string summary, string keys, string createJson, string loadWhereJson, string modifyWhereJson, string modifyValuesJson, string dedicated = "") =>
                new(name, summary, keys, createJson, loadWhereJson, modifyWhereJson, modifyValuesJson, dedicated);

            return
            [
                D("users", "Usuarios cadastrados no equipamento.", "id", """[{ "id": 101, "registration": "9001", "name": "User Smoke" }]""", """{ "users": { "id": 101 } }""", """{ "users": { "id": 101 } }""", """{ "name": "User Smoke Updated" }""", "Users"),
                D("change_logs", "Historico de insercao, alteracao e remocao de usuarios, templates, faces e cartoes.", "id", """[{ "id": 1, "operation_type": "create", "table_name": "users", "table_id": 101, "timestamp": 1713000000 }]""", """{ "change_logs": { "id": 1 } }""", """{ "change_logs": { "id": 1 } }""", """{ "table_name": "cards" }""", "ChangeLogs"),
                D("templates", "Templates biometricos de digitais.", "id", """[{ "id": 1, "user_id": 101, "finger_type": 0, "template": "ZmFrZQ==" }]""", """{ "templates": { "id": 1 } }""", """{ "templates": { "id": 1 } }""", """{ "finger_type": 1 }""", "BiometricTemplates"),
                D("cards", "Cartoes de proximidade.", "id", """[{ "id": 1, "user_id": 101, "value": 819876543210 }]""", """{ "cards": { "id": 1 } }""", """{ "cards": { "id": 1 } }""", """{ "value": 819876543211 }""", "Cards"),
                D("qrcodes", "QR Codes alfanumericos de identificacao.", "id", """[{ "id": 1, "user_id": 101, "value": "QR-OFFICIAL-1" }]""", """{ "qrcodes": { "id": 1 } }""", """{ "qrcodes": { "id": 1 } }""", """{ "value": "QR-OFFICIAL-2" }""", "QRCodes"),
                D("uhf_tags", "Tags UHF em modo estendido.", "id", """[{ "id": 1, "user_id": 101, "value": "CAFEDAD0" }]""", """{ "uhf_tags": { "id": 1 } }""", """{ "uhf_tags": { "id": 1 } }""", """{ "value": "CAFEDAD1" }"""),
                D("pins", "PINs usados para identificacao.", "id ou user_id", """[{ "id": 1, "user_id": 101, "value": "<pin-exemplo>" }]""", """{ "pins": { "id": 1 } }""", """{ "pins": { "id": 1 } }""", """{ "value": "<pin-atualizado>" }"""),
                D("alarm_zones", "Zonas de alarme e parametros da entrada.", "zone", """[{ "zone": 1, "enabled": 1, "active_level": 0, "alarm_delay": 5 }]""", """{ "alarm_zones": { "zone": 1 } }""", """{ "alarm_zones": { "zone": 1 } }""", """{ "enabled": 0 }"""),
                D("user_roles", "Privilegios administrativos por usuario.", "user_id", """[{ "user_id": 101, "role": 1 }]""", """{ "user_roles": { "user_id": 101 } }""", """{ "user_roles": { "user_id": 101 } }""", """{ "role": 0 }"""),
                D("groups", "Grupos/departamentos de acesso.", "id", """[{ "id": 10, "name": "Visitors" }]""", """{ "groups": { "id": 10 } }""", """{ "groups": { "id": 10 } }""", """{ "name": "Visitors Updated" }""", "Groups"),
                D("user_groups", "Relaciona usuarios e grupos.", "user_id + group_id", """[{ "user_id": 101, "group_id": 10 }]""", """{ "user_groups": { "user_id": 101 } }""", """{ "user_groups": { "user_id": 101 } }""", """{ "group_id": 11 }"""),
                D("scheduled_unlocks", "Liberacoes agendadas.", "id", """[{ "id": 1, "name": "Morning Open", "message": "Open lobby" }]""", """{ "scheduled_unlocks": { "id": 1 } }""", """{ "scheduled_unlocks": { "id": 1 } }""", """{ "message": "Open lobby updated" }"""),
                D("actions", "Scripts de acao remota.", "group_id", """[{ "group_id": 1, "name": "Open Door", "action": "door", "parameters": "door=1", "run_at": 0 }]""", """{ "actions": { "group_id": 1 } }""", """{ "actions": { "group_id": 1 } }""", """{ "parameters": "door=2" }"""),
                D("areas", "Areas cujo acesso sera controlado.", "id", """[{ "id": 1, "name": "Lobby" }]""", """{ "areas": { "id": 1 } }""", """{ "areas": { "id": 1 } }""", """{ "name": "Lobby Norte" }"""),
                D("portals", "Portais entre areas.", "id", """[{ "id": 1, "name": "Portal A", "area_from_id": 1, "area_to_id": 2 }]""", """{ "portals": { "id": 1 } }""", """{ "portals": { "id": 1 } }""", """{ "name": "Portal A Updated" }"""),
                D("portal_actions", "Relaciona portais e scripts de acao.", "portal_id + action_id", """[{ "portal_id": 1, "action_id": 1 }]""", """{ "portal_actions": { "portal_id": 1 } }""", """{ "portal_actions": { "portal_id": 1 } }""", """{ "action_id": 2 }"""),
                D("access_rules", "Regras de acesso.", "id", """[{ "id": 1, "name": "Always Released", "type": 1, "priority": 1 }]""", """{ "access_rules": { "id": 1 } }""", """{ "access_rules": { "id": 1 } }""", """{ "name": "Always Released Updated" }""", "AccessRules"),
                D("portal_access_rules", "Relaciona portais e regras de acesso.", "portal_id + access_rule_id", """[{ "portal_id": 1, "access_rule_id": 1 }]""", """{ "portal_access_rules": { "portal_id": 1 } }""", """{ "portal_access_rules": { "portal_id": 1 } }""", """{ "access_rule_id": 2 }"""),
                D("group_access_rules", "Relaciona grupos e regras de acesso.", "group_id + access_rule_id", """[{ "group_id": 10, "access_rule_id": 1 }]""", """{ "group_access_rules": { "group_id": 10 } }""", """{ "group_access_rules": { "group_id": 10 } }""", """{ "access_rule_id": 2 }"""),
                D("scheduled_unlock_access_rules", "Relaciona liberacoes agendadas e regras de acesso.", "scheduled_unlock_id + access_rule_id", """[{ "scheduled_unlock_id": 1, "access_rule_id": 1 }]""", """{ "scheduled_unlock_access_rules": { "scheduled_unlock_id": 1 } }""", """{ "scheduled_unlock_access_rules": { "scheduled_unlock_id": 1 } }""", """{ "access_rule_id": 2 }"""),
                D("time_zones", "Agendas/intervalos usados em regras de acesso.", "id", """[{ "id": 1, "name": "Business Hours" }]""", """{ "time_zones": { "id": 1 } }""", """{ "time_zones": { "id": 1 } }""", """{ "name": "Business Hours Updated" }"""),
                D("time_spans", "Intervalos de um horario.", "id", """[{ "id": 1, "time_zone_id": 1, "start": 28800, "end": 64800, "sun": 0, "mon": 1, "tue": 1, "wed": 1, "thu": 1, "fri": 1, "sat": 0, "hol1": 0, "hol2": 0, "hol3": 0 }]""", """{ "time_spans": { "id": 1 } }""", """{ "time_spans": { "id": 1 } }""", """{ "end": 68400 }"""),
                D("contingency_cards", "Cartoes validos em contingencia.", "id", """[{ "id": 1, "value": 819876543210 }]""", """{ "contingency_cards": { "id": 1 } }""", """{ "contingency_cards": { "id": 1 } }""", """{ "value": 819876543211 }"""),
                D("contingency_card_access_rules", "Regra de acesso usada pelos cartoes de contingencia.", "access_rule_id", """[{ "access_rule_id": 1 }]""", """{ "contingency_card_access_rules": { "access_rule_id": 1 } }""", """{ "contingency_card_access_rules": { "access_rule_id": 1 } }""", """{ "access_rule_id": 2 }"""),
                D("holidays", "Feriados e seus grupos.", "id", """[{ "id": 1, "name": "Holiday", "start": 1735689600, "end": 1735776000, "hol1": 1, "hol2": 0, "hol3": 0, "repeats": 1 }]""", """{ "holidays": { "id": 1 } }""", """{ "holidays": { "id": 1 } }""", """{ "name": "Holiday Updated" }"""),
                D("alarm_zone_time_zones", "Relaciona zonas de alarme e horarios.", "alarm_zone_id + time_zone_id", """[{ "alarm_zone_id": 1, "time_zone_id": 1 }]""", """{ "alarm_zone_time_zones": { "alarm_zone_id": 1 } }""", """{ "alarm_zone_time_zones": { "alarm_zone_id": 1 } }""", """{ "time_zone_id": 2 }"""),
                D("access_rule_time_zones", "Relaciona regras de acesso e horarios.", "access_rule_id + time_zone_id", """[{ "access_rule_id": 1, "time_zone_id": 1 }]""", """{ "access_rule_time_zones": { "access_rule_id": 1 } }""", """{ "access_rule_time_zones": { "access_rule_id": 1 } }""", """{ "time_zone_id": 2 }"""),
                D("access_logs", "Logs de acesso produzidos pelo equipamento.", "id", """[{ "id": 1, "time": 1713000000, "event": 7, "device_id": 1, "user_id": 101, "portal_id": 1 }]""", """{ "access_logs": { "id": 1 } }""", """{ "access_logs": { "id": 1 } }""", """{ "event": 6 }""", "AccessLogs"),
                D("access_log_access_rules", "Regras associadas a um log de acesso.", "access_log_id + access_rule_id", """[{ "access_log_id": 1, "access_rule_id": 1 }]""", """{ "access_log_access_rules": { "access_log_id": 1 } }""", """{ "access_log_access_rules": { "access_log_id": 1 } }""", """{ "access_rule_id": 2 }"""),
                D("alarm_logs", "Logs de ocorrencias de alarme.", "id", """[{ "id": 1, "event": 1, "cause": 7, "user_id": 101, "time": 1713000000, "door_id": 1 }]""", """{ "alarm_logs": { "id": 1 } }""", """{ "alarm_logs": { "id": 1 } }""", """{ "event": 2 }"""),
                D("devices", "Devices reconhecidos pelo equipamento.", "id", """[{ "id": 1, "name": "Server", "ip": "127.0.0.1" }]""", """{ "devices": { "id": 1 } }""", """{ "devices": { "id": 1 } }""", """{ "ip": "127.0.0.2" }""", "Devices"),
                D("user_access_rules", "Relaciona usuarios e regras de acesso.", "user_id + access_rule_id", """[{ "user_id": 101, "access_rule_id": 1 }]""", """{ "user_access_rules": { "user_id": 101 } }""", """{ "user_access_rules": { "user_id": 101 } }""", """{ "access_rule_id": 2 }"""),
                D("area_access_rules", "Relaciona areas e regras de acesso.", "area_id + access_rule_id", """[{ "area_id": 1, "access_rule_id": 1 }]""", """{ "area_access_rules": { "area_id": 1 } }""", """{ "area_access_rules": { "area_id": 1 } }""", """{ "access_rule_id": 2 }"""),
                D("catra_infos", "Informacoes agregadas de giro da catraca.", "id", """[{ "id": 1, "left_turns": 10, "right_turns": 8, "entrance_turns": 6, "exit_turns": 5 }]""", """{ "catra_infos": { "id": 1 } }""", """{ "catra_infos": { "id": 1 } }""", """{ "left_turns": 11 }""", "Catra"),
                D("log_types", "Tipos de log de ponto.", "id", """[{ "id": 1, "name": "Check In" }]""", """{ "log_types": { "id": 1 } }""", """{ "log_types": { "id": 1 } }""", """{ "name": "Check Out" }"""),
                D("sec_boxs", "Configuracao do modulo externo Security Box.", "id", """[{ "id": 65793, "version": 2, "name": "SecBox", "enabled": true, "relay_timeout": 3000, "door_sensor_enabled": true, "door_sensor_idle": false, "auto_close_enabled": 1 }]""", """{ "sec_boxs": { "id": 65793 } }""", """{ "sec_boxs": { "id": 65793 } }""", """{ "relay_timeout": 3500 }"""),
                D("contacts", "Contatos usados nas chamadas SIP.", "id", """[{ "id": 1, "name": "Reception", "number": "500" }]""", """{ "contacts": { "id": 1 } }""", """{ "contacts": { "id": 1 } }""", """{ "number": "501" }"""),
                D("timed_alarms", "Agendamentos fixos de alarme.", "id", """[{ "id": 1, "name": "Alarm Morning", "start": 28800, "sun": 0, "mon": 1, "tue": 1, "wed": 1, "thu": 1, "fri": 1, "sat": 0 }]""", """{ "timed_alarms": { "id": 1 } }""", """{ "timed_alarms": { "id": 1 } }""", """{ "start": 32400 }"""),
                D("access_events", "Eventos operacionais como porta, secbox e catra.", "id", """[{ "id": 1, "event": "door", "type": "OPEN", "identification": "1", "device_id": 1, "timestamp": 1713000000 }]""", """{ "access_events": { "id": 1 } }""", """{ "access_events": { "id": 1 } }""", """{ "type": "CLOSE" }"""),
                D("custom_thresholds", "Rigidez individual de identificacao facial.", "user_id", """[{ "id": 1, "user_id": 101, "threshold": 1200 }]""", """{ "custom_thresholds": { "user_id": 101 } }""", """{ "custom_thresholds": { "user_id": 101 } }""", """{ "threshold": 1300 }"""),
                D("network_interlocking_rules", "Regras de intertravamento remoto entre devices.", "id", """[{ "id": 1, "ip": "192.168.0.20", "login": "<login>", "password": "<senha>", "portal_name": "Airlock B", "enabled": 1 }]""", """{ "network_interlocking_rules": { "id": 1 } }""", """{ "network_interlocking_rules": { "id": 1 } }""", """{ "enabled": 0 }""")
            ];
        }

        private void PopulateDefinitions(OfficialObjectsViewModel model, bool resetSamples = false)
        {
            model.Definitions = Definitions;

            var definition = Definitions.FirstOrDefault(item => item.Name.Equals(model.SelectedObjectName, StringComparison.OrdinalIgnoreCase))
                ?? Definitions.First(item => item.Name.Equals("areas", StringComparison.OrdinalIgnoreCase));

            model.SelectedObjectName = definition.Name;

            if (resetSamples || string.IsNullOrWhiteSpace(model.CreateValuesJson))
                model.CreateValuesJson = definition.CreateSampleJson;
            if (resetSamples || string.IsNullOrWhiteSpace(model.UpsertValuesJson))
                model.UpsertValuesJson = definition.CreateSampleJson;
            if (resetSamples || string.IsNullOrWhiteSpace(model.LoadWhereJson))
                model.LoadWhereJson = definition.LoadWhereSampleJson;
            if (resetSamples || string.IsNullOrWhiteSpace(model.ModifyWhereJson))
                model.ModifyWhereJson = definition.ModifyWhereSampleJson;
            if (resetSamples || string.IsNullOrWhiteSpace(model.ModifyValuesJson))
                model.ModifyValuesJson = definition.ModifyValuesSampleJson;
            if (resetSamples || string.IsNullOrWhiteSpace(model.DestroyWhereJson))
                model.DestroyWhereJson = definition.ModifyWhereSampleJson;
        }

        private bool EnsureConnected(OfficialObjectsViewModel model)
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

        private static string FormatJson(string rawJson, JsonDocument? document)
        {
            if (document == null)
                return rawJson;

            return JsonSerializer.Serialize(document.RootElement, new JsonSerializerOptions
            {
                WriteIndented = true
            });
        }
    }
}



