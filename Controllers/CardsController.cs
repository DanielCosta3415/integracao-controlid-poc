using System.Text.Json;
using Integracao.ControlID.PoC.Models.ControlIDApi;
using Integracao.ControlID.PoC.Services.ControlIDApi;
using Integracao.ControlID.PoC.ViewModels.Cards;
using Microsoft.AspNetCore.Mvc;

namespace Integracao.ControlID.PoC.Controllers
{
    public class CardsController : Controller
    {
        private readonly OfficialControlIdApiService _officialApi;
        private readonly ILogger<CardsController> _logger;

        public CardsController(OfficialControlIdApiService officialApi, ILogger<CardsController> logger)
        {
            _officialApi = officialApi;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            var model = new CardListViewModel();

            if (!_officialApi.TryGetConnection(out _, out _))
            {
                model.ErrorMessage = "É necessário conectar-se e autenticar com um equipamento Control iD.";
                return View(model);
            }

            try
            {
                model.Cards = (await LoadCardsAsync())
                    .Select(ToCardViewModel)
                    .OrderBy(card => card.UserId)
                    .ThenBy(card => card.Value)
                    .ToList();

                if (model.Cards.Count == 0)
                    model.ErrorMessage = "Nenhum cartão encontrado.";
            }
            catch (Exception ex)
            {
                model.ErrorMessage = $"Erro ao consultar cartões: {ex.Message}";
                _logger.LogError(ex, "Erro ao consultar cartões.");
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
                var card = (await LoadCardsAsync(id.Value)).FirstOrDefault();
                if (card != null)
                    return View(ToCardViewModel(card));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao consultar detalhes do cartão {CardId}.", id.Value);
            }

            return NotFound();
        }

        public IActionResult Create()
        {
            return View(new CardEditViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CardEditViewModel model)
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
                var values = BuildCardValues(model);
                var result = await _officialApi.InvokeAsync("create-objects", new
                {
                    @object = "cards",
                    values = new[] { values }
                });

                if (!result.Success)
                {
                    ModelState.AddModelError(string.Empty, BuildErrorMessage("Erro ao criar cartão", result));
                    return View(model);
                }

                TempData["StatusMessage"] = "Cartão criado com sucesso!";
                TempData["StatusType"] = "success";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Erro ao criar cartão: {ex.Message}");
                _logger.LogError(ex, "Erro ao criar cartão.");
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
                var card = (await LoadCardsAsync(id.Value)).FirstOrDefault();
                if (card != null)
                    return View(ToEditViewModel(card));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar cartão {CardId} para edição.", id.Value);
            }

            return NotFound();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(long id, CardEditViewModel model)
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
                var values = BuildCardValues(model);
                var result = await _officialApi.InvokeAsync("modify-objects", new
                {
                    @object = "cards",
                    values,
                    where = new { cards = new { id } }
                });

                if (!result.Success)
                {
                    ModelState.AddModelError(string.Empty, BuildErrorMessage("Erro ao atualizar cartão", result));
                    return View(model);
                }

                TempData["StatusMessage"] = "Cartão atualizado com sucesso!";
                TempData["StatusType"] = "success";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Erro ao atualizar cartão: {ex.Message}");
                _logger.LogError(ex, "Erro ao atualizar cartão {CardId}.", id);
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
                var card = (await LoadCardsAsync(id.Value)).FirstOrDefault();
                if (card != null)
                {
                    return View(new CardDeleteViewModel
                    {
                        Id = card.Id,
                        UserId = card.UserId,
                        Value = card.Value
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar cartão {CardId} para exclusão.", id.Value);
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
                    @object = "cards",
                    where = new { cards = new { id } }
                });

                TempData["StatusMessage"] = result.Success
                    ? "Cartão excluído com sucesso!"
                    : BuildErrorMessage("Erro ao excluir cartão", result);
                TempData["StatusType"] = result.Success ? "success" : "danger";
            }
            catch (Exception ex)
            {
                TempData["StatusMessage"] = $"Erro ao excluir cartão: {ex.Message}";
                TempData["StatusType"] = "danger";
                _logger.LogError(ex, "Erro ao excluir cartão {CardId}.", id);
            }

            return RedirectToAction(nameof(Index));
        }

        private async Task<List<CardRecord>> LoadCardsAsync(long? id = null)
        {
            object payload = id.HasValue
                ? new { @object = "cards", where = new { cards = new { id = id.Value } } }
                : new { @object = "cards" };

            var (result, document) = await _officialApi.InvokeJsonAsync("load-objects", payload);
            if (!result.Success)
                throw new InvalidOperationException(BuildErrorMessage("Erro ao consultar cartões", result));

            if (document == null)
                return [];

            return ExtractArray(document.RootElement, "cards")
                .Select(MapCard)
                .ToList();
        }

        private static Dictionary<string, object?> BuildCardValues(CardEditViewModel model)
        {
            var values = new Dictionary<string, object?>
            {
                ["user_id"] = model.UserId,
                ["value"] = model.Value,
                ["type"] = model.Type,
                ["begin_time"] = ToUnixTime(model.BeginTime),
                ["end_time"] = ToUnixTime(model.EndTime)
            };

            if (!string.IsNullOrWhiteSpace(model.Status))
                values["status"] = model.Status;

            return values;
        }

        private static CardRecord MapCard(JsonElement element)
        {
            return new CardRecord
            {
                Id = GetInt64(element, "id"),
                UserId = GetInt64(element, "user_id", "userId"),
                Value = GetString(element, "value") ?? string.Empty,
                Type = GetString(element, "type") ?? string.Empty,
                Status = GetString(element, "status"),
                BeginTime = GetNullableInt64(element, "begin_time", "beginTime"),
                EndTime = GetNullableInt64(element, "end_time", "endTime"),
                CreatedAt = GetDateTime(element, "created_at", "createdAt")
            };
        }

        private static CardViewModel ToCardViewModel(CardRecord card)
        {
            return new CardViewModel
            {
                Id = card.Id,
                UserId = card.UserId,
                Value = card.Value,
                Type = card.Type,
                Status = card.Status ?? string.Empty,
                BeginTime = FromUnixTime(card.BeginTime),
                EndTime = FromUnixTime(card.EndTime),
                CreatedAt = card.CreatedAt
            };
        }

        private static CardEditViewModel ToEditViewModel(CardRecord card)
        {
            return new CardEditViewModel
            {
                Id = card.Id,
                UserId = card.UserId,
                Value = card.Value,
                Type = card.Type,
                Status = card.Status ?? string.Empty,
                BeginTime = FromUnixTime(card.BeginTime),
                EndTime = FromUnixTime(card.EndTime)
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
                return $"{prefix}: {result.ErrorMessage}";

            if (!string.IsNullOrWhiteSpace(result.ResponseBody) && !result.ResponseBodyIsBase64)
                return $"{prefix}: {result.ResponseBody}";

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

        private sealed class CardRecord
        {
            public long Id { get; init; }
            public long UserId { get; init; }
            public string Value { get; init; } = string.Empty;
            public string Type { get; init; } = string.Empty;
            public string? Status { get; init; }
            public long? BeginTime { get; init; }
            public long? EndTime { get; init; }
            public DateTime? CreatedAt { get; init; }
        }
    }
}
