using System;
using System.Text.Json;
using Integracao.ControlID.PoC.Models.ControlIDApi;
using Integracao.ControlID.PoC.Services.ControlIDApi;
using Integracao.ControlID.PoC.ViewModels.Devices;
using Integracao.ControlID.PoC.Helpers;
using Microsoft.AspNetCore.Mvc;

namespace Integracao.ControlID.PoC.Controllers
{
    public class DevicesController : Controller
    {
        private readonly OfficialControlIdApiService _officialApi;
        private readonly ILogger<DevicesController> _logger;

        public DevicesController(OfficialControlIdApiService officialApi, ILogger<DevicesController> logger)
        {
            _officialApi = officialApi;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            var model = new DeviceListViewModel();

            if (!_officialApi.TryGetConnection(out _, out _))
            {
                model.ErrorMessage = "É necessário conectar-se e autenticar com um equipamento Control iD.";
                return View(model);
            }

            try
            {
                model.Devices = (await LoadDevicesAsync())
                    .Select(ToDeviceViewModel)
                    .OrderBy(device => device.Name)
                    .ThenBy(device => device.Id)
                    .ToList();

                if (model.Devices.Count == 0)
                    model.ErrorMessage = "Nenhum dispositivo encontrado.";
            }
            catch (Exception ex)
            {
                model.ErrorMessage = SecurityTextHelper.BuildSafeUserMessage("Erro ao consultar dispositivos", ex);
                _logger.LogError(ex, "Erro ao consultar dispositivos.");
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
                var device = (await LoadDevicesAsync(id.Value)).FirstOrDefault();
                if (device != null)
                    return View(ToDeviceViewModel(device));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao consultar detalhes do dispositivo {DeviceId}.", id.Value);
            }

            return NotFound();
        }

        public IActionResult Create()
        {
            return View(new DeviceEditViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(DeviceEditViewModel model)
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
                var result = await _officialApi.InvokeAsync("create-objects", new
                {
                    @object = "devices",
                    values = new[] { BuildDeviceValues(model) }
                });

                EnsureSuccess(result, "Erro ao criar dispositivo");

                TempData["StatusMessage"] = "Dispositivo criado com sucesso!";
                TempData["StatusType"] = "success";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, SecurityTextHelper.BuildSafeUserMessage("A operação não pôde ser concluída", ex));
                _logger.LogError(ex, "Erro ao criar dispositivo.");
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
                var device = (await LoadDevicesAsync(id.Value)).FirstOrDefault();
                if (device != null)
                    return View(ToEditViewModel(device));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar dispositivo {DeviceId} para edição.", id.Value);
            }

            return NotFound();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(long id, DeviceEditViewModel model)
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
                var result = await _officialApi.InvokeAsync("modify-objects", new
                {
                    @object = "devices",
                    values = BuildDeviceValues(model),
                    where = new { devices = new { id } }
                });

                EnsureSuccess(result, "Erro ao atualizar dispositivo");

                TempData["StatusMessage"] = "Dispositivo atualizado com sucesso!";
                TempData["StatusType"] = "success";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, SecurityTextHelper.BuildSafeUserMessage("A operação não pôde ser concluída", ex));
                _logger.LogError(ex, "Erro ao atualizar dispositivo {DeviceId}.", id);
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
                var device = (await LoadDevicesAsync(id.Value)).FirstOrDefault();
                if (device != null)
                {
                    return View(new DeviceDeleteViewModel
                    {
                        Id = device.Id,
                        Name = device.Name
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar dispositivo {DeviceId} para exclusão.", id.Value);
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
                    @object = "devices",
                    where = new { devices = new { id } }
                });

                EnsureSuccess(result, "Erro ao excluir dispositivo");

                TempData["StatusMessage"] = "Dispositivo excluído com sucesso!";
                TempData["StatusType"] = "success";
            }
            catch (Exception ex)
            {
                TempData["StatusMessage"] = SecurityTextHelper.BuildSafeUserMessage("Erro ao excluir dispositivo", ex);
                TempData["StatusType"] = "danger";
                _logger.LogError(ex, "Erro ao excluir dispositivo {DeviceId}.", id);
            }

            return RedirectToAction(nameof(Index));
        }

        private async Task<List<DeviceRecord>> LoadDevicesAsync(long? id = null)
        {
            object payload = id.HasValue
                ? new
                {
                    @object = "devices",
                    where = new { devices = new { id = id.Value } }
                }
                : new { @object = "devices" };

            var (result, document) = await _officialApi.InvokeJsonAsync("load-objects", payload);
            EnsureSuccess(result, "Erro ao consultar dispositivos");

            if (document == null)
                return [];

            return ExtractArray(document.RootElement, "devices")
                .Select(MapDevice)
                .ToList();
        }

        private static Dictionary<string, object?> BuildDeviceValues(DeviceEditViewModel model)
        {
            var values = new Dictionary<string, object?>
            {
                ["name"] = model.Name,
                ["ip"] = model.Ip
            };

            if (!string.IsNullOrWhiteSpace(model.SerialNumber))
                values["serial_number"] = model.SerialNumber;

            if (!string.IsNullOrWhiteSpace(model.Firmware))
                values["firmware"] = model.Firmware;

            if (!string.IsNullOrWhiteSpace(model.Status))
                values["status"] = model.Status;

            if (!string.IsNullOrWhiteSpace(model.Description))
                values["description"] = model.Description;

            return values;
        }

        private static DeviceRecord MapDevice(JsonElement element)
        {
            return new DeviceRecord
            {
                Id = GetInt64(element, "id"),
                Name = GetString(element, "name") ?? string.Empty,
                Ip = GetString(element, "ip", "ip_address") ?? string.Empty,
                SerialNumber = GetString(element, "serial_number", "serialNumber") ?? string.Empty,
                Firmware = GetString(element, "firmware") ?? string.Empty,
                Status = GetString(element, "status") ?? string.Empty,
                Description = GetString(element, "description") ?? string.Empty,
                RegisteredAt = GetFlexibleDateTime(element, "registered_at", "created_at", "registeredAt", "createdAt"),
                LastSeenAt = GetFlexibleDateTime(element, "last_seen_at", "updated_at", "lastSeenAt", "updatedAt")
            };
        }

        private static DeviceViewModel ToDeviceViewModel(DeviceRecord device)
        {
            return new DeviceViewModel
            {
                Id = device.Id,
                Name = device.Name,
                Ip = device.Ip,
                SerialNumber = device.SerialNumber,
                Firmware = device.Firmware,
                Status = device.Status,
                RegisteredAt = device.RegisteredAt,
                LastSeenAt = device.LastSeenAt,
                Description = device.Description
            };
        }

        private static DeviceEditViewModel ToEditViewModel(DeviceRecord device)
        {
            return new DeviceEditViewModel
            {
                Id = device.Id,
                Name = device.Name,
                Ip = device.Ip,
                SerialNumber = device.SerialNumber,
                Firmware = device.Firmware,
                Status = device.Status,
                Description = device.Description
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

        private sealed class DeviceRecord
        {
            public long Id { get; init; }
            public string Name { get; init; } = string.Empty;
            public string Ip { get; init; } = string.Empty;
            public string SerialNumber { get; init; } = string.Empty;
            public string Firmware { get; init; } = string.Empty;
            public string Status { get; init; } = string.Empty;
            public DateTime? RegisteredAt { get; init; }
            public DateTime? LastSeenAt { get; init; }
            public string Description { get; init; } = string.Empty;
        }
    }
}




