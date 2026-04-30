using System;
using System.Text.Json;
using Integracao.ControlID.PoC.Models.ControlIDApi;
using Integracao.ControlID.PoC.Services.ControlIDApi;
using Integracao.ControlID.PoC.ViewModels.Session;
using Microsoft.AspNetCore.Http;
using Integracao.ControlID.PoC.Helpers;
using Microsoft.AspNetCore.Mvc;

namespace Integracao.ControlID.PoC.Controllers
{
    public class SessionController : Controller
    {
        private readonly OfficialControlIdApiService _officialApi;
        private readonly ILogger<SessionController> _logger;
        private const string SessionDeviceAddressKey = "ControlID_DeviceAddress";
        private const string SessionSessionStringKey = "ControlID_SessionString";

        public SessionController(OfficialControlIdApiService officialApi, ILogger<SessionController> logger)
        {
            _officialApi = officialApi;
            _logger = logger;
        }

        private bool TryGetSession(out string deviceAddress, out string sessionString)
        {
            deviceAddress = HttpContext.Session.GetString(SessionDeviceAddressKey) ?? string.Empty;
            sessionString = HttpContext.Session.GetString(SessionSessionStringKey) ?? string.Empty;
            return !(string.IsNullOrWhiteSpace(deviceAddress) || string.IsNullOrWhiteSpace(sessionString));
        }

        // GET: /Session/Status
        public IActionResult Status()
        {
            TryGetSession(out string deviceAddress, out string sessionString);

            var model = new SessionStatusViewModel
            {
                DeviceAddress = deviceAddress,
                SessionString = sessionString,
                SessionValid = null,
                StatusMessage = TempData["StatusMessage"] as string,
                StatusType = TempData["StatusType"] as string
            };
            return View(model);
        }

        // POST: /Session/Validate
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Validate()
        {
            if (!TryGetSession(out string deviceAddress, out string sessionString))
            {
                TempData["StatusMessage"] = "Não há uma sessão ativa para validar.";
                TempData["StatusType"] = "warning";
                return RedirectToAction(nameof(Status));
            }

            try
            {
                var (result, document) = await _officialApi.InvokeJsonAsync("session-is-valid");

                if (!result.Success)
                {
                    TempData["StatusMessage"] = BuildErrorMessage("Falha ao validar a sessão", result);
                    TempData["StatusType"] = "danger";
                    _logger.LogWarning("Falha ao validar sessão no dispositivo {DeviceAddress}, status: {StatusCode}", deviceAddress, result.StatusCode);
                    return RedirectToAction(nameof(Status));
                }

                var sessionIsValid = document != null && TryGetBoolean(document.RootElement, out var isValid, "session_is_valid", "sessionIsValid", "valid") && isValid;

                if (sessionIsValid)
                {
                    TempData["StatusMessage"] = "Sessão válida!";
                    TempData["StatusType"] = "success";
                }
                else
                {
                    TempData["StatusMessage"] = "Sessão inválida ou expirada.";
                    TempData["StatusType"] = "danger";
                }
            }
            catch (Exception ex)
            {
                TempData["StatusMessage"] = SecurityTextHelper.BuildSafeUserMessage("Erro ao validar sessão", ex);
                TempData["StatusType"] = "danger";
                _logger.LogError(ex, "Erro ao validar sessão no dispositivo {DeviceAddress}", deviceAddress);
            }

            return RedirectToAction(nameof(Status));
        }

        // POST: /Session/Clear
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Clear()
        {
            HttpContext.Session.Remove(SessionSessionStringKey);
            HttpContext.Session.Remove(SessionDeviceAddressKey);

            TempData["StatusMessage"] = "Sessão local removida com sucesso.";
            TempData["StatusType"] = "success";

            return RedirectToAction(nameof(Status));
        }

        private static string BuildErrorMessage(string prefix, OfficialApiInvocationResult result)
        {
            if (!string.IsNullOrWhiteSpace(result.ErrorMessage))
                return SecurityTextHelper.BuildApiFailureMessage(result, prefix);

            if (!string.IsNullOrWhiteSpace(result.ResponseBody) && !result.ResponseBodyIsBase64)
                return SecurityTextHelper.BuildApiFailureMessage(result, prefix);

            return $"{prefix} (status: {result.StatusCode}).";
        }

        private static bool TryGetBoolean(JsonElement element, out bool value, params string[] propertyNames)
        {
            foreach (var propertyName in propertyNames)
            {
                if (!element.TryGetProperty(propertyName, out var property))
                    continue;

                switch (property.ValueKind)
                {
                    case JsonValueKind.True:
                    case JsonValueKind.False:
                        value = property.GetBoolean();
                        return true;
                    case JsonValueKind.String:
                        if (bool.TryParse(property.GetString(), out value))
                            return true;

                        if (property.GetString() == "1")
                        {
                            value = true;
                            return true;
                        }

                        if (property.GetString() == "0")
                        {
                            value = false;
                            return true;
                        }
                        break;
                    case JsonValueKind.Number:
                        if (property.TryGetInt32(out var numericValue))
                        {
                            value = numericValue != 0;
                            return true;
                        }
                        break;
                }
            }

            value = false;
            return false;
        }
    }
}




