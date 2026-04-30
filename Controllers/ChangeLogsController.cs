using System;
using System.Text.Json;
using Integracao.ControlID.PoC.Models.ControlIDApi;
using Integracao.ControlID.PoC.Services.ControlIDApi;
using Integracao.ControlID.PoC.ViewModels.ChangeLogs;
using Integracao.ControlID.PoC.Helpers;
using Microsoft.AspNetCore.Mvc;

namespace Integracao.ControlID.PoC.Controllers
{
    public class ChangeLogsController : Controller
    {
        private readonly OfficialControlIdApiService _officialApi;
        private readonly ILogger<ChangeLogsController> _logger;

        public ChangeLogsController(OfficialControlIdApiService officialApi, ILogger<ChangeLogsController> logger)
        {
            _officialApi = officialApi;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            var model = new ChangeLogListViewModel();

            if (!_officialApi.TryGetConnection(out _, out _))
            {
                model.ErrorMessage = "É necessário conectar-se e autenticar com um equipamento Control iD.";
                return View(model);
            }

            try
            {
                model.ChangeLogs = (await LoadChangeLogsAsync())
                    .OrderByDescending(log => log.Timestamp ?? DateTime.MinValue)
                    .ToList();

                if (model.ChangeLogs.Count == 0)
                    model.ErrorMessage = "Nenhum log de alteração encontrado.";
            }
            catch (Exception ex)
            {
                model.ErrorMessage = SecurityTextHelper.BuildSafeUserMessage("Erro ao consultar logs de alterações", ex);
                _logger.LogError(ex, "Erro ao consultar logs de alterações.");
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
                var changeLog = (await LoadChangeLogsAsync(id.Value)).FirstOrDefault();
                if (changeLog != null)
                    return View(changeLog);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao consultar detalhes do log de alteração.");
            }

            return NotFound();
        }

        private async Task<List<ChangeLogViewModel>> LoadChangeLogsAsync(long? id = null)
        {
            object payload = id.HasValue
                ? new { @object = "change_logs", where = new { change_logs = new { id = id.Value } } }
                : new { @object = "change_logs" };

            var (result, document) = await _officialApi.InvokeJsonAsync("load-objects", payload);
            if (!result.Success)
                throw new InvalidOperationException(BuildErrorMessage("Erro ao consultar logs de alterações", result));

            if (document == null)
                return [];

            return ExtractArray(document.RootElement, "change_logs")
                .Select(MapChangeLog)
                .ToList();
        }

        private static ChangeLogViewModel MapChangeLog(JsonElement element)
        {
            var timestamp = GetDateTime(element, "timestamp", "time");

            return new ChangeLogViewModel
            {
                Id = GetInt64(element, "id"),
                OperationType = GetString(element, "operation", "operation_type", "type") ?? string.Empty,
                TableName = GetString(element, "table", "table_name", "object") ?? string.Empty,
                TableId = GetNullableInt64(element, "table_id", "record_id", "object_id"),
                Timestamp = timestamp,
                PerformedBy = GetString(element, "performed_by", "user_name", "login") ?? string.Empty,
                Description = GetString(element, "description", "details", "info") ?? string.Empty,
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
            if (!string.IsNullOrWhiteSpace(result.ErrorMessage))
                return SecurityTextHelper.BuildApiFailureMessage(result, prefix);

            if (!string.IsNullOrWhiteSpace(result.ResponseBody) && !result.ResponseBodyIsBase64)
                return SecurityTextHelper.BuildApiFailureMessage(result, prefix);

            return $"{prefix} (status: {result.StatusCode}).";
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

            return null;
        }

        private static DateTime? GetDateTime(JsonElement element, params string[] propertyNames)
        {
            foreach (var propertyName in propertyNames)
            {
                if (element.TryGetProperty(propertyName, out var property) &&
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




