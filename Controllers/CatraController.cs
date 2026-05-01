using System;
using System.Text.Json;
using Integracao.ControlID.PoC.Models.ControlIDApi;
using Integracao.ControlID.PoC.Services.ControlIDApi;
using Integracao.ControlID.PoC.Services.Security;
using Integracao.ControlID.PoC.ViewModels.Catra;
using Integracao.ControlID.PoC.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Integracao.ControlID.PoC.Controllers
{
    [Authorize(Roles = AppSecurityRoles.Administrator)]
    public class CatraController : Controller
    {
        private readonly OfficialControlIdApiService _officialApi;
        private readonly ILogger<CatraController> _logger;

        public CatraController(OfficialControlIdApiService officialApi, ILogger<CatraController> logger)
        {
            _officialApi = officialApi;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            var model = new CatraEventListViewModel
            {
                OpenCommand = new CatraOpenViewModel()
            };

            if (!_officialApi.TryGetConnection(out _, out _))
            {
                model.ErrorMessage = "É necessário conectar-se e autenticar com um equipamento Control iD.";
                return View(model);
            }

            try
            {
                model.CatraEvents = (await LoadCatraEventsAsync())
                    .Select(ToCatraEventViewModel)
                    .OrderByDescending(evt => evt.Time ?? evt.CreatedAt ?? DateTime.MinValue)
                    .ThenByDescending(evt => evt.Id)
                    .ToList();

                await PopulateCatraInfoAsync(model);
            }
            catch (Exception ex)
            {
                model.ErrorMessage = SecurityTextHelper.BuildSafeUserMessage("Erro ao consultar eventos da catraca", ex);
                _logger.LogError(ex, "Erro ao consultar eventos da catraca.");
            }

            return View(model);
        }

        public async Task<IActionResult> Details(long? id)
        {
            if (id == null)
                return NotFound();

            if (!_officialApi.TryGetConnection(out _, out _))
                return NotFound();

            try
            {
                var catraEvent = (await LoadCatraEventsAsync(id.Value)).FirstOrDefault();
                if (catraEvent != null)
                    return View(ToCatraEventViewModel(catraEvent));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao consultar detalhes do evento da catraca {EventId}.", id.Value);
            }

            return NotFound();
        }

        public async Task<IActionResult> Delete(long? id)
        {
            if (id == null)
                return NotFound();

            if (!_officialApi.TryGetConnection(out _, out _))
                return NotFound();

            try
            {
                var catraEvent = (await LoadCatraEventsAsync(id.Value)).FirstOrDefault();
                if (catraEvent != null)
                    return View(ToCatraEventViewModel(catraEvent));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar evento de catraca {EventId} para exclusão.", id.Value);
            }

            return NotFound();
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(long id)
        {
            if (!_officialApi.TryGetConnection(out _, out _))
            {
                TempData["StatusMessage"] = "É necessário conectar-se e autenticar com um equipamento Control iD.";
                TempData["StatusType"] = "danger";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                var result = await _officialApi.InvokeAsync("destroy-objects", new
                {
                    @object = "catra_events",
                    where = new { catra_events = new { id } }
                });

                EnsureSuccess(result, "Erro ao excluir evento da catraca");

                TempData["StatusMessage"] = "Evento da catraca excluído com sucesso.";
                TempData["StatusType"] = "success";
            }
            catch (Exception ex)
            {
                TempData["StatusMessage"] = SecurityTextHelper.BuildSafeUserMessage("Erro ao excluir evento da catraca", ex);
                TempData["StatusType"] = "danger";
                _logger.LogError(ex, "Erro ao excluir evento da catraca {EventId}.", id);
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Open(CatraOpenViewModel model)
        {
            if (!_officialApi.TryGetConnection(out _, out _))
            {
                TempData["StatusMessage"] = "É necessário conectar-se e autenticar com um equipamento Control iD.";
                TempData["StatusType"] = "danger";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                var (action, parameters) = BuildCommand(model);
                var result = await _officialApi.InvokeAsync("execute-actions", new
                {
                    actions = new[]
                    {
                        new
                        {
                            action,
                            parameters
                        }
                    }
                });

                EnsureSuccess(result, "Erro ao enviar comando para a catraca");

                TempData["StatusMessage"] = "Comando da catraca enviado com sucesso!";
                TempData["StatusType"] = "success";
            }
            catch (Exception ex)
            {
                TempData["StatusMessage"] = SecurityTextHelper.BuildSafeUserMessage("Erro ao enviar comando da catraca", ex);
                TempData["StatusType"] = "danger";
                _logger.LogError(ex, "Erro ao executar comando da catraca.");
            }

            return RedirectToAction(nameof(Index));
        }

        private async Task<List<CatraEventRecord>> LoadCatraEventsAsync(long? id = null)
        {
            object payload = id.HasValue
                ? new
                {
                    @object = "catra_events",
                    where = new { catra_events = new { id = id.Value } }
                }
                : new { @object = "catra_events" };

            var (result, document) = await _officialApi.InvokeJsonAsync("load-objects", payload);
            EnsureSuccess(result, "Erro ao consultar eventos da catraca");

            if (document == null)
                return [];

            return ExtractArray(document.RootElement, "catra_events")
                .Select(MapCatraEvent)
                .ToList();
        }

        private async Task PopulateCatraInfoAsync(CatraEventListViewModel model)
        {
            var (result, document) = await _officialApi.InvokeJsonAsync("get-catra-info");
            if (!result.Success || document == null)
                return;

            var root = document.RootElement;
            model.LeftTurns = GetNullableInt64(root, "left_turns", "leftTurns");
            model.RightTurns = GetNullableInt64(root, "right_turns", "rightTurns");
            model.EntranceTurns = GetNullableInt64(root, "entrance_turns", "entranceTurns");
            model.ExitTurns = GetNullableInt64(root, "exit_turns", "exitTurns");
            model.TotalTurns = GetNullableInt64(root, "total_turns", "totalTurns");
            model.CatraInfoRawJson = result.ResponseBody;
        }

        private static (string action, string parameters) BuildCommand(CatraOpenViewModel model)
        {
            var actionType = model.ActionType?.Trim().ToLowerInvariant();

            return actionType switch
            {
                "allow" => ("catra", $"allow={NormalizeDirection(model.AllowDirection)}"),
                "relay" when model.Relay is 1 or 2 => ("catra", $"relay={model.Relay.Value}"),
                "collector" => ("open_collector", string.Empty),
                "relay" => throw new InvalidOperationException("Informe um relé válido da catraca: 1 ou 2."),
                _ => throw new InvalidOperationException("Tipo de comando de catraca inválido.")
            };
        }

        private static string NormalizeDirection(string? direction)
        {
            var normalized = direction?.Trim().ToLowerInvariant();
            return normalized switch
            {
                "anticlockwise" or "clockwise" or "both" => normalized,
                _ => throw new InvalidOperationException("Direção inválida para a catraca. Use clockwise, anticlockwise ou both.")
            };
        }

        private static CatraEventRecord MapCatraEvent(JsonElement element)
        {
            return new CatraEventRecord
            {
                Id = GetInt64(element, "id"),
                Direction = GetInt32(element, "direction"),
                Time = GetFlexibleDateTime(element, "time", "event_time", "timestamp"),
                Info = GetString(element, "info", "message", "reason") ?? string.Empty,
                UserId = GetNullableInt64(element, "user_id", "userId"),
                DeviceId = GetNullableInt64(element, "device_id", "deviceId"),
                CreatedAt = GetFlexibleDateTime(element, "created_at", "createdAt")
            };
        }

        private static CatraEventViewModel ToCatraEventViewModel(CatraEventRecord catraEvent)
        {
            return new CatraEventViewModel
            {
                Id = catraEvent.Id,
                Direction = catraEvent.Direction,
                Time = catraEvent.Time,
                Info = catraEvent.Info,
                UserId = catraEvent.UserId,
                DeviceId = catraEvent.DeviceId,
                CreatedAt = catraEvent.CreatedAt
            };
        }

        private static IEnumerable<JsonElement> ExtractArray(JsonElement root, string propertyName)
        {
            if (root.TryGetProperty(propertyName, out var array) && array.ValueKind == JsonValueKind.Array)
                return array.EnumerateArray();

            return [];
        }

        private static void EnsureSuccess(OfficialApiInvocationResult result, string prefix)
        {
            if (result.Success)
                return;

            throw new InvalidOperationException(BuildErrorMessage(result, prefix));
        }

        private static string BuildErrorMessage(OfficialApiInvocationResult result, string prefix)
        {
            if (!string.IsNullOrWhiteSpace(result.ErrorMessage))
                return SecurityTextHelper.BuildApiFailureMessage(result, prefix);

            if (!string.IsNullOrWhiteSpace(result.ResponseBody) && !result.ResponseBodyIsBase64)
                return SecurityTextHelper.BuildApiFailureMessage(result, prefix);

            return SecurityTextHelper.BuildApiFailureMessage(result, prefix);
        }

        private static string? GetString(JsonElement element, params string[] propertyNames)
        {
            foreach (var propertyName in propertyNames)
            {
                if (element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.String)
                    return property.GetString();
            }

            return null;
        }

        private static long GetInt64(JsonElement element, params string[] propertyNames)
        {
            foreach (var propertyName in propertyNames)
            {
                if (!element.TryGetProperty(propertyName, out var property))
                    continue;

                if (property.ValueKind == JsonValueKind.Number && property.TryGetInt64(out var number))
                    return number;

                if (property.ValueKind == JsonValueKind.String && long.TryParse(property.GetString(), out number))
                    return number;
            }

            return 0;
        }

        private static long? GetNullableInt64(JsonElement element, params string[] propertyNames)
        {
            foreach (var propertyName in propertyNames)
            {
                if (!element.TryGetProperty(propertyName, out var property))
                    continue;

                if (property.ValueKind == JsonValueKind.Number && property.TryGetInt64(out var number))
                    return number;

                if (property.ValueKind == JsonValueKind.String && long.TryParse(property.GetString(), out number))
                    return number;
            }

            return null;
        }

        private static int GetInt32(JsonElement element, params string[] propertyNames)
        {
            foreach (var propertyName in propertyNames)
            {
                if (!element.TryGetProperty(propertyName, out var property))
                    continue;

                if (property.ValueKind == JsonValueKind.Number && property.TryGetInt32(out var number))
                    return number;

                if (property.ValueKind == JsonValueKind.String && int.TryParse(property.GetString(), out number))
                    return number;
            }

            return 0;
        }

        private static DateTime? GetFlexibleDateTime(JsonElement element, params string[] propertyNames)
        {
            foreach (var propertyName in propertyNames)
            {
                if (!element.TryGetProperty(propertyName, out var property))
                    continue;

                if (property.ValueKind == JsonValueKind.String && DateTime.TryParse(property.GetString(), out var parsed))
                    return parsed;

                if (property.ValueKind == JsonValueKind.Number && property.TryGetInt64(out var unixValue))
                    return FromUnixLike(unixValue);

                if (property.ValueKind == JsonValueKind.String && long.TryParse(property.GetString(), out unixValue))
                    return FromUnixLike(unixValue);
            }

            return null;
        }

        private static DateTime FromUnixLike(long value)
        {
            return value > 9_999_999_999
                ? DateTimeOffset.FromUnixTimeMilliseconds(value).LocalDateTime
                : DateTimeOffset.FromUnixTimeSeconds(value).LocalDateTime;
        }

        private sealed class CatraEventRecord
        {
            public long Id { get; init; }
            public int Direction { get; init; }
            public DateTime? Time { get; init; }
            public string Info { get; init; } = string.Empty;
            public long? UserId { get; init; }
            public long? DeviceId { get; init; }
            public DateTime? CreatedAt { get; init; }
        }
    }
}




