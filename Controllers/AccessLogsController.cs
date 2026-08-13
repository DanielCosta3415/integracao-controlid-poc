using System;
using System.Text.Json;
using Integracao.ControlID.PoC.Models.ControlIDApi;
using Integracao.ControlID.PoC.Services.ControlIDApi;
using Integracao.ControlID.PoC.Services.Security;
using Integracao.ControlID.PoC.ViewModels.AccessLogs;
using Integracao.ControlID.PoC.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Integracao.ControlID.PoC.Controllers
{
    [Authorize(Roles = AppSecurityRoles.Administrator)]
    public class AccessLogsController : Controller
    {
        private readonly OfficialControlIdApiService _officialApi;
        private readonly ILogger<AccessLogsController> _logger;

        public AccessLogsController(OfficialControlIdApiService officialApi, ILogger<AccessLogsController> logger)
        {
            _officialApi = officialApi;
            _logger = logger;
        }

        public async Task<IActionResult> Index(long? userId, long? deviceId, int? @event, DateTime? startDate, DateTime? endDate)
        {
            var model = new AccessLogListViewModel();

            if (!_officialApi.TryGetConnection(out _, out _))
            {
                model.ErrorMessage = "É necessário conectar-se e autenticar com um equipamento Control iD.";
                return View(model);
            }

            try
            {
                var logs = await LoadLogsAsync(
                    userId: userId,
                    deviceId: deviceId,
                    eventCode: @event,
                    startDate: startDate,
                    endDate: endDate);
                model.AccessLogs = logs
                    .OrderByDescending(log => log.Time ?? DateTime.MinValue)
                    .ToList();

                if (model.AccessLogs.Count == 0)
                    model.ErrorMessage = "Nenhum log de acesso encontrado com os filtros informados.";
            }
            catch (Exception ex)
            {
                model.ErrorMessage = SecurityTextHelper.BuildSafeUserMessage("Erro ao consultar logs de acesso", ex);
                _logger.LogError(ex, "Erro ao consultar logs de acesso.");
            }

            return View(model);
        }

        // GET: /AccessLogs/Details/5
        public async Task<IActionResult> Details(long? id)
        {
            if (id == null)
                return NotFound("Id do log não informado.");

            if (!_officialApi.TryGetConnection(out _, out _))
                return NotFound("Sessão ou dispositivo não encontrados.");

            try
            {
                var log = (await LoadLogsAsync(id: id.Value)).FirstOrDefault();
                if (log != null)
                    return View(log);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao consultar detalhes do log de acesso.");
            }
            return NotFound("Log não encontrado ou erro inesperado.");
        }

        private async Task<List<AccessLogViewModel>> LoadLogsAsync(
            long? id = null,
            long? userId = null,
            long? deviceId = null,
            int? eventCode = null,
            DateTime? startDate = null,
            DateTime? endDate = null)
        {
            var payload = AccessLogQueryBuilder.Build(id, userId, deviceId, eventCode, startDate, endDate);

            var (result, document) = await _officialApi.InvokeJsonAsync("load-objects", payload);
            if (!result.Success)
                throw new InvalidOperationException(BuildErrorMessage("Erro ao consultar logs de acesso", result));

            if (document == null)
                return [];

            return ExtractArray(document.RootElement, "access_logs")
                .Select(MapAccessLog)
                .ToList();
        }

        private static AccessLogViewModel MapAccessLog(JsonElement element)
        {
            var timestamp = GetDateTime(element, "time", "timestamp");

            return new AccessLogViewModel
            {
                Id = GetInt64(element, "id"),
                Event = GetInt32(element, "event"),
                DeviceId = GetNullableInt64(element, "device_id", "deviceId"),
                UserId = GetNullableInt64(element, "user_id", "userId"),
                PortalId = GetNullableInt32(element, "portal_id", "portalId"),
                Info = GetString(element, "info", "message", "description") ?? string.Empty,
                Time = timestamp,
                CreatedAt = timestamp
            };
        }

        private static IEnumerable<JsonElement> ExtractArray(JsonElement root, string propertyName)
        {
            if (root.TryGetProperty(propertyName, out var array) && array.ValueKind == JsonValueKind.Array)
                return array.EnumerateArray();

            return [];
        }

        private static string BuildErrorMessage(string prefix, OfficialApiInvocationResult result)
        {
            return SecurityTextHelper.BuildApiFailureMessage(result, prefix);
        }

        private static long GetInt64(JsonElement element, params string[] propertyNames)
        {
            foreach (var propertyName in propertyNames)
            {
                if (element.TryGetProperty(propertyName, out var property))
                {
                    if (property.ValueKind == JsonValueKind.Number && property.TryGetInt64(out var number))
                        return number;

                    if (property.ValueKind == JsonValueKind.String && long.TryParse(property.GetString(), out number))
                        return number;
                }
            }

            return 0;
        }

        private static long? GetNullableInt64(JsonElement element, params string[] propertyNames)
        {
            var value = GetInt64(element, propertyNames);
            return value == 0 ? null : value;
        }

        private static int GetInt32(JsonElement element, params string[] propertyNames)
        {
            foreach (var propertyName in propertyNames)
            {
                if (element.TryGetProperty(propertyName, out var property))
                {
                    if (property.ValueKind == JsonValueKind.Number && property.TryGetInt32(out var number))
                        return number;

                    if (property.ValueKind == JsonValueKind.String && int.TryParse(property.GetString(), out number))
                        return number;
                }
            }

            return 0;
        }

        private static int? GetNullableInt32(JsonElement element, params string[] propertyNames)
        {
            foreach (var propertyName in propertyNames)
            {
                if (element.TryGetProperty(propertyName, out var property))
                {
                    if (property.ValueKind == JsonValueKind.Number && property.TryGetInt32(out var number))
                        return number;

                    if (property.ValueKind == JsonValueKind.String && int.TryParse(property.GetString(), out number))
                        return number;
                }
            }

            return null;
        }

        private static DateTime? GetDateTime(JsonElement element, params string[] propertyNames)
        {
            foreach (var propertyName in propertyNames)
            {
                if (element.TryGetProperty(propertyName, out var property) &&
                    property.ValueKind == JsonValueKind.Number &&
                    property.TryGetInt64(out var unixSeconds))
                {
                    return DateTimeOffset.FromUnixTimeSeconds(unixSeconds).LocalDateTime;
                }

                if (element.TryGetProperty(propertyName, out property) &&
                    property.ValueKind == JsonValueKind.String &&
                    DateTime.TryParse(property.GetString(), out var parsed))
                {
                    return parsed;
                }
            }

            return null;
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
    }
}




