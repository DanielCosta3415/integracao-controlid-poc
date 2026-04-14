using System;
using System.Text.Json;
using Integracao.ControlID.PoC.Models.ControlIDApi;
using Integracao.ControlID.PoC.Services.ControlIDApi;
using Integracao.ControlID.PoC.ViewModels.Hardware;
using Integracao.ControlID.PoC.Helpers;
using Microsoft.AspNetCore.Mvc;

namespace Integracao.ControlID.PoC.Controllers
{
    public class HardwareController : Controller
    {
        private readonly OfficialControlIdApiService _apiService;
        private readonly ILogger<HardwareController> _logger;

        public HardwareController(OfficialControlIdApiService apiService, ILogger<HardwareController> logger)
        {
            _apiService = apiService;
            _logger = logger;
        }

        // GET: /Hardware/Status
        public async Task<IActionResult> Status()
        {
            var model = new HardwareStatusViewModel();
            string deviceAddress = _apiService.GetDeviceAddress();

            if (string.IsNullOrWhiteSpace(deviceAddress))
            {
                model.ErrorMessage = "É necessário conectar-se com um equipamento Control iD.";
                return View(model);
            }

            try
            {
                var (systemResult, systemDocument) = await _apiService.InvokeJsonAsync("system-information");
                if (!systemResult.Success || systemDocument == null)
                {
                    model.ErrorMessage = BuildErrorMessage(systemResult, "Erro ao consultar status do hardware");
                    return View(model);
                }

                bool? doorOpen = null;
                string doorStateText = "Estado da porta indisponível";
                string doorRawJson = string.Empty;

                if (_apiService.TryGetConnection(out _, out _))
                {
                    var (doorResult, doorDocument) = await _apiService.InvokeJsonAsync("door-state", new { });
                    if (doorResult.Success && doorDocument != null)
                    {
                        doorOpen = TryExtractDoorOpen(doorDocument.RootElement);
                        doorStateText = doorOpen.HasValue ? (doorOpen.Value ? "Aberta" : "Fechada") : "Sem porta reportada";
                        doorRawJson = doorResult.ResponseBody;
                    }
                }

                var root = systemDocument.RootElement;
                model.HardwareStatus = new HardwareStatusDto
                {
                    Status = FormatOnlineMode(root),
                    Firmware = GetString(root, "version"),
                    Serial = GetString(root, "serial"),
                    DeviceId = GetString(root, "device_id"),
                    IpAddress = GetNestedString(root, "network", "ip"),
                    DoorOpen = doorOpen,
                    SensorsInfo = $"{doorStateText} | MAC: {GetNestedString(root, "network", "mac")}",
                    RawJson = string.IsNullOrWhiteSpace(doorRawJson)
                        ? systemResult.ResponseBody
                        : $"{systemResult.ResponseBody}\n\n{doorRawJson}",
                    RetrievedAt = DateTime.UtcNow
                };
            }
            catch (Exception ex)
            {
                model.ErrorMessage = SecurityTextHelper.BuildSafeUserMessage("Erro ao consultar status do hardware", ex);
                _logger.LogError(ex, "Erro ao consultar status do hardware.");
            }

            return View(model);
        }

        // GET: /Hardware/Gpio
        public async Task<IActionResult> Gpio()
        {
            var model = new GpioStateViewModel();
            string deviceAddress = _apiService.GetDeviceAddress();

            if (string.IsNullOrWhiteSpace(deviceAddress))
            {
                model.ErrorMessage = "É necessário conectar-se com um equipamento Control iD.";
                return View(model);
            }

            if (!_apiService.TryGetConnection(out _, out _))
            {
                model.ErrorMessage = "É necessário conectar-se e autenticar com um equipamento Control iD.";
                return View(model);
            }

            try
            {
                var inputs = new Dictionary<string, bool>();
                var outputs = new Dictionary<string, bool>();
                var rawResponses = new List<string>();

                foreach (var gpioNumber in Enumerable.Range(1, 8))
                {
                    var (result, document) = await _apiService.InvokeJsonAsync("gpio-state", new { gpio = gpioNumber });
                    if (!result.Success || document == null)
                        continue;

                    rawResponses.Add(result.ResponseBody);
                    var root = document.RootElement;
                    var enabled = GetInt(root, "enabled");
                    var isInput = GetInt(root, "in") == 1;
                    var value = GetInt(root, "value") == 1;
                    var pinName = GetString(root, "pin");
                    var key = string.IsNullOrWhiteSpace(pinName) ? $"GPIO {gpioNumber}" : $"GPIO {gpioNumber} ({pinName})";

                    if (enabled == 0)
                        key += " [desabilitado]";

                    if (isInput)
                        inputs[key] = value;
                    else
                        outputs[key] = value;
                }

                if (inputs.Count == 0 && outputs.Count == 0)
                {
                    model.ErrorMessage = "Nenhum GPIO pôde ser consultado com a sessão atual.";
                    return View(model);
                }

                model.GpioState = new GpioStateDto
                {
                    Inputs = inputs,
                    Outputs = outputs,
                    RetrievedAt = DateTime.UtcNow,
                    Info = $"Consulta oficial consolidada dos GPIOs 1 a 8. Entradas: {inputs.Count}. Saídas: {outputs.Count}.",
                    RawJson = string.Join("\n\n", rawResponses)
                };
            }
            catch (Exception ex)
            {
                model.ErrorMessage = SecurityTextHelper.BuildSafeUserMessage("Erro ao consultar estado dos GPIOs", ex);
                _logger.LogError(ex, "Erro ao consultar estado dos GPIOs.");
            }

            return View(model);
        }

        // GET: /Hardware/DoorState
        public async Task<IActionResult> DoorState(int? doorNumber = null)
        {
            var model = new DoorStateViewModel
            {
                DoorNumber = doorNumber
            };

            if (!_apiService.TryGetConnection(out _, out _))
            {
                model.ErrorMessage = "É necessário conectar-se e autenticar com um equipamento Control iD.";
                return View(model);
            }

            try
            {
                object payload = doorNumber.HasValue ? new { door = doorNumber.Value } : new { };
                var (result, document) = await _apiService.InvokeJsonAsync("door-state", payload);
                if (!result.Success)
                {
                    model.ErrorMessage = BuildErrorMessage(result, "Erro ao consultar o estado da porta");
                    return View(model);
                }

                model.ResponseJson = FormatJson(result.ResponseBody, document);
                model.Summary = BuildDoorSummary(document?.RootElement);
            }
            catch (Exception ex)
            {
                model.ErrorMessage = SecurityTextHelper.BuildSafeUserMessage("Erro ao consultar o estado da porta", ex);
                _logger.LogError(ex, "Erro ao consultar o estado das portas.");
            }

            return View(model);
        }

        // GET: /Hardware/RelayAction
        public IActionResult RelayAction()
        {
            return View(new RelayActionViewModel());
        }

        // GET: /Hardware/ValidateBiometry
        public IActionResult ValidateBiometry()
        {
            return View(new BiometryValidationViewModel());
        }

        // POST: /Hardware/RelayAction
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RelayAction(RelayActionViewModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["StatusMessage"] = "Parâmetros inválidos para abrir a saída.";
                TempData["StatusType"] = "danger";
                return RedirectToAction(nameof(Status));
            }

            if (!_apiService.TryGetConnection(out _, out _))
            {
                TempData["StatusMessage"] = "É necessário conectar-se e autenticar com um equipamento Control iD.";
                TempData["StatusType"] = "danger";
                return RedirectToAction(nameof(Status));
            }

            if (model.DoorNumber < 1)
            {
                TempData["StatusMessage"] = "Número de porta inválido.";
                TempData["StatusType"] = "danger";
                return RedirectToAction(nameof(Status));
            }

            try
            {
                var result = await _apiService.InvokeAsync("execute-actions", new
                {
                    actions = new[]
                    {
                        new
                        {
                            action = "door",
                            parameters = $"door={model.DoorNumber}"
                        }
                    }
                });

                if (!result.Success)
                    throw new InvalidOperationException(BuildErrorMessage(result, "Erro ao abrir a saída"));

                TempData["StatusMessage"] = "Saída acionada com sucesso.";
                TempData["StatusType"] = "success";
            }
            catch (Exception ex)
            {
                TempData["StatusMessage"] = SecurityTextHelper.BuildSafeUserMessage("Erro ao abrir a saída", ex);
                TempData["StatusType"] = "danger";
                _logger.LogError(ex, "Erro ao abrir a saída remota.");
            }

            return RedirectToAction(nameof(Status));
        }

        // POST: /Hardware/RereadLeds
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RereadLeds()
        {
            if (!_apiService.TryGetConnection(out _, out _))
            {
                TempData["StatusMessage"] = "É necessário conectar-se e autenticar com um equipamento Control iD.";
                TempData["StatusType"] = "danger";
                return RedirectToAction(nameof(Status));
            }

            try
            {
                var result = await _apiService.InvokeAsync("reread-leds");
                if (!result.Success)
                    throw new InvalidOperationException(BuildErrorMessage(result, "Erro ao recarregar a configuração de LEDs"));

                TempData["StatusMessage"] = "Configuração de LEDs recarregada com sucesso.";
                TempData["StatusType"] = "success";
            }
            catch (Exception ex)
            {
                TempData["StatusMessage"] = SecurityTextHelper.BuildSafeUserMessage("Erro ao recarregar LEDs", ex);
                TempData["StatusType"] = "danger";
                _logger.LogError(ex, "Erro ao recarregar configuração de LEDs.");
            }

            return RedirectToAction(nameof(Status));
        }

        // POST: /Hardware/ValidateBiometry
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ValidateBiometry(BiometryValidationViewModel model)
        {
            if (!_apiService.TryGetConnection(out _, out _))
            {
                model.ErrorMessage = "É necessário conectar-se e autenticar com um equipamento Control iD.";
                return View(model);
            }

            if (model.BiometryFile == null || model.BiometryFile.Length == 0)
            {
                model.ResultMessage = "Selecione um arquivo de template ou imagem para validação.";
                model.ResultStatusType = "danger";
                return View(model);
            }

            try
            {
                await using var stream = model.BiometryFile.OpenReadStream();
                using var memoryStream = new MemoryStream();
                await stream.CopyToAsync(memoryStream);

                var result = await _apiService.InvokeAsync("validate-biometry", Convert.ToBase64String(memoryStream.ToArray()));
                if (!result.Success)
                    throw new InvalidOperationException(BuildErrorMessage(result, "Erro ao validar biometria"));

                model.ResultMessage = "Biometria enviada para validação com sucesso.";
                model.ResultStatusType = "success";
                model.ResponseJson = string.IsNullOrWhiteSpace(result.ResponseBody)
                    ? "Operação concluída sem corpo de resposta."
                    : result.ResponseBody;
            }
            catch (Exception ex)
            {
                model.ResultMessage = SecurityTextHelper.BuildSafeUserMessage("A operação não pôde ser concluída", ex);
                model.ResultStatusType = "danger";
                _logger.LogError(ex, "Erro ao validar biometria.");
            }

            return View(model);
        }

        private static string BuildErrorMessage(OfficialApiInvocationResult result, string prefix)
        {
            return SecurityTextHelper.BuildApiFailureMessage(result, prefix);
        }

        private static string GetString(JsonElement element, params string[] propertyNames)
        {
            foreach (var propertyName in propertyNames)
            {
                if (element.TryGetProperty(propertyName, out var value))
                    return value.ValueKind == JsonValueKind.String ? value.GetString() ?? string.Empty : value.ToString();
            }

            return string.Empty;
        }

        private static string GetNestedString(JsonElement element, string objectName, string propertyName)
        {
            if (element.TryGetProperty(objectName, out var nestedElement) && nestedElement.ValueKind == JsonValueKind.Object)
                return GetString(nestedElement, propertyName);

            return string.Empty;
        }

        private static int GetInt(JsonElement element, string propertyName)
        {
            if (!element.TryGetProperty(propertyName, out var value) || value.ValueKind != JsonValueKind.Number)
                return 0;

            return value.GetInt32();
        }

        private static string FormatOnlineMode(JsonElement element)
        {
            if (!element.TryGetProperty("online", out var onlineValue))
                return string.Empty;

            return onlineValue.ValueKind == JsonValueKind.True ? "Online" :
                   onlineValue.ValueKind == JsonValueKind.False ? "Standalone" :
                   onlineValue.ToString();
        }

        private static bool? TryExtractDoorOpen(JsonElement element)
        {
            if (element.ValueKind == JsonValueKind.Object)
            {
                if (element.TryGetProperty("open", out var openValue))
                {
                    return openValue.ValueKind switch
                    {
                        JsonValueKind.True => true,
                        JsonValueKind.False => false,
                        JsonValueKind.Number => openValue.GetInt32() == 1,
                        _ => null
                    };
                }

                foreach (var property in element.EnumerateObject())
                {
                    if (property.Value.ValueKind == JsonValueKind.Object)
                    {
                        var nested = TryExtractDoorOpen(property.Value);
                        if (nested.HasValue)
                            return nested;
                    }
                }
            }

            if (element.ValueKind == JsonValueKind.Array)
            {
                foreach (var item in element.EnumerateArray())
                {
                    var nested = TryExtractDoorOpen(item);
                    if (nested.HasValue)
                        return nested;
                }
            }

            return null;
        }

        private static string BuildDoorSummary(JsonElement? element)
        {
            if (element == null)
                return "Sem dados retornados pela API.";

            var openState = TryExtractDoorOpen(element.Value);
            if (openState.HasValue)
                return openState.Value ? "Pelo menos uma porta reportada está aberta." : "As portas reportadas estão fechadas.";

            return "A API retornou dados de porta, mas sem um campo `open` claramente identificável.";
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




