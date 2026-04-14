using System;
using System.Text.Json;
using Integracao.ControlID.PoC.Models.ControlIDApi;
using Integracao.ControlID.PoC.Services.ControlIDApi;
using Integracao.ControlID.PoC.ViewModels.Auth;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Integracao.ControlID.PoC.Controllers
{
    public class AuthController : Controller
    {
        private readonly OfficialControlIdApiService _officialApi;
        private readonly ILogger<AuthController> _logger;
        private const string SessionDeviceAddressKey = "ControlID_DeviceAddress";
        private const string SessionSessionStringKey = "ControlID_SessionString";

        public AuthController(OfficialControlIdApiService officialApi, ILogger<AuthController> logger)
        {
            _officialApi = officialApi;
            _logger = logger;
        }

        // GET: /Auth/Login
        [HttpGet]
        public IActionResult Login()
        {
            HttpContext.Session.Remove(SessionSessionStringKey);
            return View(new LoginViewModel());
        }

        // POST: /Auth/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            var deviceAddress = HttpContext.Session.GetString(SessionDeviceAddressKey);
            if (string.IsNullOrWhiteSpace(deviceAddress))
            {
                ModelState.AddModelError(string.Empty, "Nenhum dispositivo conectado. Conecte-se a um equipamento Control iD primeiro.");
                return View(model);
            }

            HttpContext.Session.Remove(SessionSessionStringKey);

            try
            {
                var (result, document) = await _officialApi.InvokeJsonDirectAsync(
                    "login",
                    deviceAddress,
                    payload: new { login = model.Username, password = model.Password });

                if (!result.Success)
                {
                    ModelState.AddModelError(string.Empty, BuildErrorMessage("Falha ao autenticar no dispositivo", result));
                    _logger.LogWarning("Falha de login no dispositivo Control iD {DeviceAddress}. Status: {StatusCode}", deviceAddress, result.StatusCode);
                    return View(model);
                }

                if (document == null || !TryGetString(document.RootElement, out var sessionString, "session"))
                {
                    ModelState.AddModelError(string.Empty, "A resposta do dispositivo não continha uma sessão válida.");
                    _logger.LogWarning("Resposta inesperada no login do dispositivo {DeviceAddress}: {ResponseBody}", deviceAddress, result.ResponseBody);
                    return View(model);
                }

                HttpContext.Session.SetString(SessionSessionStringKey, sessionString);

                TempData["StatusMessage"] = "Login realizado com sucesso!";
                TempData["StatusType"] = "success";
                _logger.LogInformation("Login realizado no dispositivo {Device} para o usuário {User}", deviceAddress, model.Username);

                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"Erro ao autenticar: {ex.Message}");
                _logger.LogError(ex, "Erro ao fazer login no dispositivo Control iD em {DeviceAddress}", deviceAddress);
                return View(model);
            }
        }

        // GET: /Auth/Logout
        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            var deviceAddress = HttpContext.Session.GetString(SessionDeviceAddressKey);
            var sessionString = HttpContext.Session.GetString(SessionSessionStringKey);

            if (string.IsNullOrWhiteSpace(deviceAddress) || string.IsNullOrWhiteSpace(sessionString))
            {
                TempData["StatusMessage"] = "Nenhuma sessão de login ativa.";
                TempData["StatusType"] = "warning";
                return RedirectToAction("Index", "Home");
            }

            try
            {
                var result = await _officialApi.InvokeAsync("logout");

                if (result.Success)
                {
                    TempData["StatusMessage"] = "Logout realizado com sucesso.";
                    TempData["StatusType"] = "success";
                    _logger.LogInformation("Logout realizado do dispositivo {Device}", deviceAddress);
                }
                else
                {
                    TempData["StatusMessage"] = BuildErrorMessage("Falha ao sair da sessão", result);
                    TempData["StatusType"] = "danger";
                    _logger.LogWarning("Falha ao realizar logout no dispositivo {Device}, status: {StatusCode}", deviceAddress, result.StatusCode);
                }
            }
            catch (Exception ex)
            {
                TempData["StatusMessage"] = $"Erro ao realizar logout: {ex.Message}";
                TempData["StatusType"] = "danger";
                _logger.LogError(ex, "Erro ao realizar logout do dispositivo Control iD em {DeviceAddress}", deviceAddress);
            }

            HttpContext.Session.Remove(SessionSessionStringKey);

            return RedirectToAction("Index", "Home");
        }

        // GET: /Auth/Status
        [HttpGet]
        public IActionResult Status()
        {
            var sessionString = HttpContext.Session.GetString(SessionSessionStringKey);
            var deviceAddress = HttpContext.Session.GetString(SessionDeviceAddressKey);
            var viewModel = new AuthStatusViewModel
            {
                DeviceAddress = deviceAddress,
                SessionString = sessionString,
                IsAuthenticated = !string.IsNullOrWhiteSpace(sessionString)
            };
            return View(viewModel);
        }

        private static string BuildErrorMessage(string prefix, OfficialApiInvocationResult result)
        {
            if (!string.IsNullOrWhiteSpace(result.ErrorMessage))
                return $"{prefix}: {result.ErrorMessage}";

            if (!string.IsNullOrWhiteSpace(result.ResponseBody) && !result.ResponseBodyIsBase64)
                return $"{prefix}: {result.ResponseBody}";

            return $"{prefix} (status: {result.StatusCode}).";
        }

        private static bool TryGetString(JsonElement element, out string value, params string[] propertyNames)
        {
            foreach (var propertyName in propertyNames)
            {
                if (element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.String)
                {
                    value = property.GetString() ?? string.Empty;
                    return !string.IsNullOrWhiteSpace(value);
                }
            }

            value = string.Empty;
            return false;
        }
    }
}
