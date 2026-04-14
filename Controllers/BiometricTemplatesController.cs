using System.Text.Json;
using Integracao.ControlID.PoC.Models.ControlIDApi;
using Integracao.ControlID.PoC.Services.ControlIDApi;
using Integracao.ControlID.PoC.ViewModels.BiometricTemplates;
using Integracao.ControlID.PoC.Helpers;
using Microsoft.AspNetCore.Mvc;

namespace Integracao.ControlID.PoC.Controllers
{
    public class BiometricTemplatesController : Controller
    {
        private readonly OfficialControlIdApiService _officialApi;
        private readonly ILogger<BiometricTemplatesController> _logger;

        public BiometricTemplatesController(OfficialControlIdApiService officialApi, ILogger<BiometricTemplatesController> logger)
        {
            _officialApi = officialApi;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            var model = new BiometricTemplateListViewModel();

            if (!_officialApi.TryGetConnection(out _, out _))
            {
                model.ErrorMessage = "É necessário conectar-se e autenticar com um equipamento Control iD.";
                return View(model);
            }

            try
            {
                model.Templates = (await LoadTemplatesAsync())
                    .Select(ToTemplateViewModel)
                    .OrderBy(template => template.UserId)
                    .ThenBy(template => template.Id)
                    .ToList();

                if (model.Templates.Count == 0)
                    model.ErrorMessage = "Nenhum template biométrico encontrado.";
            }
            catch (Exception ex)
            {
                model.ErrorMessage = SecurityTextHelper.BuildSafeUserMessage("Erro ao consultar templates biométricos", ex);
                _logger.LogError(ex, "Erro ao consultar templates biométricos.");
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
                var template = (await LoadTemplatesAsync(id.Value)).FirstOrDefault();
                if (template != null)
                    return View(ToTemplateViewModel(template));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao consultar detalhes do template biométrico {TemplateId}.", id.Value);
            }

            return NotFound();
        }

        public IActionResult Create()
        {
            return View(new BiometricTemplateEditViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(BiometricTemplateEditViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            if (!_officialApi.TryGetConnection(out _, out _))
            {
                ModelState.AddModelError(string.Empty, "É necessário conectar-se e autenticar com um equipamento Control iD.");
                return View(model);
            }

            try
            {
                var values = BuildTemplateValues(model);
                var result = await _officialApi.InvokeAsync("create-objects", new
                {
                    @object = "templates",
                    values = new[] { values }
                });

                if (!result.Success)
                {
                    ModelState.AddModelError(string.Empty, BuildErrorMessage("Erro ao criar template biométrico", result));
                    return View(model);
                }

                TempData["StatusMessage"] = "Template biométrico criado com sucesso!";
                TempData["StatusType"] = "success";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, SecurityTextHelper.BuildSafeUserMessage("Erro ao criar template biométrico", ex));
                _logger.LogError(ex, "Erro ao criar template biométrico.");
                return View(model);
            }
        }

        public async Task<IActionResult> Edit(long? id)
        {
            if (id == null)
                return NotFound();

            if (!_officialApi.TryGetConnection(out _, out _))
                return NotFound();

            try
            {
                var template = (await LoadTemplatesAsync(id.Value)).FirstOrDefault();
                if (template != null)
                    return View(ToEditViewModel(template));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar template {TemplateId} para edição.", id.Value);
            }

            return NotFound();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(long id, BiometricTemplateEditViewModel model)
        {
            if (id != model.Id)
                return NotFound();

            if (!ModelState.IsValid)
                return View(model);

            if (!_officialApi.TryGetConnection(out _, out _))
            {
                ModelState.AddModelError(string.Empty, "É necessário conectar-se e autenticar com um equipamento Control iD.");
                return View(model);
            }

            try
            {
                var values = BuildTemplateValues(model);
                var result = await _officialApi.InvokeAsync("modify-objects", new
                {
                    @object = "templates",
                    values,
                    where = new { templates = new { id } }
                });

                if (!result.Success)
                {
                    ModelState.AddModelError(string.Empty, BuildErrorMessage("Erro ao atualizar template biométrico", result));
                    return View(model);
                }

                TempData["StatusMessage"] = "Template biométrico atualizado com sucesso!";
                TempData["StatusType"] = "success";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, SecurityTextHelper.BuildSafeUserMessage("Erro ao atualizar template biométrico", ex));
                _logger.LogError(ex, "Erro ao atualizar template biométrico {TemplateId}.", id);
                return View(model);
            }
        }

        public async Task<IActionResult> Delete(long? id)
        {
            if (id == null)
                return NotFound();

            if (!_officialApi.TryGetConnection(out _, out _))
                return NotFound();

            try
            {
                var template = (await LoadTemplatesAsync(id.Value)).FirstOrDefault();
                if (template != null)
                {
                    return View(new BiometricTemplateDeleteViewModel
                    {
                        Id = template.Id,
                        UserId = template.UserId
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar template {TemplateId} para exclusão.", id.Value);
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
                    @object = "templates",
                    where = new { templates = new { id } }
                });

                TempData["StatusMessage"] = result.Success
                    ? "Template biométrico excluído com sucesso!"
                    : BuildErrorMessage("Erro ao excluir template biométrico", result);
                TempData["StatusType"] = result.Success ? "success" : "danger";
            }
            catch (Exception ex)
            {
                TempData["StatusMessage"] = SecurityTextHelper.BuildSafeUserMessage("Erro ao excluir template biométrico", ex);
                TempData["StatusType"] = "danger";
                _logger.LogError(ex, "Erro ao excluir template biométrico {TemplateId}.", id);
            }

            return RedirectToAction(nameof(Index));
        }

        private async Task<List<BiometricTemplateRecord>> LoadTemplatesAsync(long? id = null)
        {
            object payload = id.HasValue
                ? new { @object = "templates", where = new { templates = new { id = id.Value } } }
                : new { @object = "templates" };

            var (result, document) = await _officialApi.InvokeJsonAsync("load-objects", payload);
            if (!result.Success)
                throw new InvalidOperationException(BuildErrorMessage("Erro ao consultar templates biométricos", result));

            if (document == null)
                return [];

            return ExtractArray(document.RootElement, "templates")
                .Select(MapTemplate)
                .ToList();
        }

        private static Dictionary<string, object?> BuildTemplateValues(BiometricTemplateEditViewModel model)
        {
            return new Dictionary<string, object?>
            {
                ["user_id"] = model.UserId,
                ["template"] = model.Template,
                ["type"] = model.Type,
                ["finger_position"] = model.FingerPosition,
                ["finger_type"] = model.FingerType,
                ["begin_time"] = ToUnixTime(model.BeginTime),
                ["end_time"] = ToUnixTime(model.EndTime)
            };
        }

        private static BiometricTemplateRecord MapTemplate(JsonElement element)
        {
            return new BiometricTemplateRecord
            {
                Id = GetInt64(element, "id"),
                UserId = GetInt64(element, "user_id", "userId"),
                Template = GetString(element, "template") ?? string.Empty,
                Type = GetInt32(element, "type"),
                FingerPosition = GetInt32(element, "finger_position", "fingerPosition"),
                FingerType = GetInt32(element, "finger_type", "fingerType"),
                BeginTime = GetNullableInt64(element, "begin_time", "beginTime"),
                EndTime = GetNullableInt64(element, "end_time", "endTime"),
                CreatedAt = GetDateTime(element, "created_at", "createdAt")
            };
        }

        private static BiometricTemplateViewModel ToTemplateViewModel(BiometricTemplateRecord template)
        {
            return new BiometricTemplateViewModel
            {
                Id = template.Id,
                UserId = template.UserId,
                Template = template.Template,
                Type = template.Type,
                FingerPosition = template.FingerPosition,
                FingerType = template.FingerType,
                BeginTime = FromUnixTime(template.BeginTime),
                EndTime = FromUnixTime(template.EndTime),
                CreatedAt = template.CreatedAt
            };
        }

        private static BiometricTemplateEditViewModel ToEditViewModel(BiometricTemplateRecord template)
        {
            return new BiometricTemplateEditViewModel
            {
                Id = template.Id,
                UserId = template.UserId,
                Template = template.Template,
                Type = template.Type,
                FingerPosition = template.FingerPosition,
                FingerType = template.FingerType,
                BeginTime = FromUnixTime(template.BeginTime),
                EndTime = FromUnixTime(template.EndTime)
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

        private static DateTime? FromUnixTime(long? value)
        {
            return value.HasValue ? DateTimeOffset.FromUnixTimeSeconds(value.Value).LocalDateTime : null;
        }

        private static long? ToUnixTime(DateTime? value)
        {
            return value.HasValue ? new DateTimeOffset(value.Value).ToUnixTimeSeconds() : null;
        }

        private sealed class BiometricTemplateRecord
        {
            public long Id { get; init; }
            public long UserId { get; init; }
            public string Template { get; init; } = string.Empty;
            public int Type { get; init; }
            public int FingerPosition { get; init; }
            public int FingerType { get; init; }
            public long? BeginTime { get; init; }
            public long? EndTime { get; init; }
            public DateTime? CreatedAt { get; init; }
        }
    }
}




