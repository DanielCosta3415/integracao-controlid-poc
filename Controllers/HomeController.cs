using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.Json;
using Integracao.ControlID.PoC.Helpers;
using Integracao.ControlID.PoC.Models.ControlIDApi;
using Integracao.ControlID.PoC.Services.ControlIDApi;
using Integracao.ControlID.PoC.Services.Database;
using Integracao.ControlID.PoC.Services.Navigation;
using Integracao.ControlID.PoC.Services.Performance;
using Integracao.ControlID.PoC.ViewModels.Home;
using Integracao.ControlID.PoC.ViewModels.Shared;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;

namespace Integracao.ControlID.PoC.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly OfficialControlIdApiService _officialApi;
        private readonly OfficialApiCatalogService _officialApiCatalogService;
        private readonly MonitorEventRepository _monitorEventRepository;
        private readonly PushCommandRepository _pushCommandRepository;
        private readonly NavigationCatalogService _navigationCatalogService;
        private readonly PageShellService _pageShellService;
        private const string SessionDeviceAddressKey = "ControlID_DeviceAddress";
        private const string SessionDeviceNameKey = "ControlID_DeviceName";
        private const string SessionDeviceSerialKey = "ControlID_DeviceSerial";
        private const string SessionDeviceFirmwareKey = "ControlID_DeviceFirmware";
        private const string SessionSessionStringKey = "ControlID_SessionString";
        private const int RecentActivitySourceLimit = 4;
        private const string DashboardLocalMetricsTimingName = "dashboard-local-metrics";

        public HomeController(
            ILogger<HomeController> logger,
            OfficialControlIdApiService officialApi,
            OfficialApiCatalogService officialApiCatalogService,
            MonitorEventRepository monitorEventRepository,
            PushCommandRepository pushCommandRepository,
            NavigationCatalogService navigationCatalogService,
            PageShellService pageShellService)
        {
            _logger = logger;
            _officialApi = officialApi;
            _officialApiCatalogService = officialApiCatalogService;
            _monitorEventRepository = monitorEventRepository;
            _pushCommandRepository = pushCommandRepository;
            _navigationCatalogService = navigationCatalogService;
            _pageShellService = pageShellService;
        }

        // GET: /
        public async Task<IActionResult> Index()
        {
            var endpoints = _officialApiCatalogService.GetAll();
            var domains = _navigationCatalogService.GetDomains();
            var modules = _navigationCatalogService.GetAllModules();
            var model = new HomeDashboardViewModel
            {
                DeviceAddress = HttpContext.Session.GetString(SessionDeviceAddressKey),
                DeviceName = HttpContext.Session.GetString(SessionDeviceNameKey),
                DeviceSerial = HttpContext.Session.GetString(SessionDeviceSerialKey),
                DeviceFirmware = HttpContext.Session.GetString(SessionDeviceFirmwareKey),
                IsSessionActive = !string.IsNullOrWhiteSpace(HttpContext.Session.GetString(SessionSessionStringKey)),
                StatusMessage = TempData["StatusMessage"] as string,
                StatusType = TempData["StatusType"] as string ?? "info",
                OfficialEndpointCount = endpoints.Count,
                InvokableEndpointCount = endpoints.Count(endpoint => endpoint.Invokable),
                CallbackEndpointCount = endpoints.Count(endpoint => endpoint.Direction == "server-callback"),
                ConnectionPanel = _pageShellService.BuildConnectionPanel(HttpContext, Url.Action("Index", "Home")),
                Domains = domains,
                FeaturedModules = modules
                    .Where(module => module.Visibility == "primary" && module.Controller != "Home")
                    .OrderBy(module => module.Priority)
                    .Take(8)
                    .ToList(),
                QuickFlows =
                [
                    new HomeQuickFlowViewModel
                    {
                        Title = "Conferir sessão",
                        Description = "Valide a sessão atual antes de operar endpoints sensíveis do equipamento.",
                        Href = Url.Action("Status", "Session")!,
                        ButtonLabel = "Abrir sessão"
                    },
                    new HomeQuickFlowViewModel
                    {
                        Title = "Autenticar dispositivo",
                        Description = "Faça login oficial no equipamento conectado e libere chamadas autenticadas.",
                        Href = Url.Action("Login", "Auth")!,
                        ButtonLabel = "Entrar"
                    },
                    new HomeQuickFlowViewModel
                    {
                        Title = "Explorar API oficial",
                        Description = "Navegue pelo inventário vivo de endpoints sem poluir a experiência operacional.",
                        Href = Url.Action("Index", "OfficialApi")!,
                        ButtonLabel = "Abrir catálogo"
                    },
                    new HomeQuickFlowViewModel
                    {
                        Title = "Ver eventos",
                        Description = "Acompanhe o que a PoC recebeu recentemente via monitor, callbacks e push.",
                        Href = Url.Action("Index", "OfficialEvents")!,
                        ButtonLabel = "Abrir eventos"
                    },
                    new HomeQuickFlowViewModel
                    {
                        Title = "Operar usuários",
                        Description = "Acesse a base principal de usuários, validade e credenciais ativas.",
                        Href = Url.Action("Index", "Users")!,
                        ButtonLabel = "Gerenciar usuários"
                    },
                    new HomeQuickFlowViewModel
                    {
                        Title = "Abrir mapa funcional",
                        Description = "Encontre qualquer domínio, módulo ou experiência implementada dentro da PoC.",
                        Href = Url.Action("Index", "Workspace")!,
                        ButtonLabel = "Explorar módulos"
                    }
                ]
            };

            var metricsStopwatch = Stopwatch.StartNew();

            try
            {
                var eventCount = await _monitorEventRepository.CountMonitorEventsAsync();
                var pendingPushCount = await _pushCommandRepository.CountPendingPushCommandsAsync();
                var events = await _monitorEventRepository.GetRecentMonitorEventsAsync(RecentActivitySourceLimit);
                var pushCommands = await _pushCommandRepository.GetRecentPushCommandsAsync(RecentActivitySourceLimit);

                model.RecentEventCount = eventCount;
                model.PendingPushCount = pendingPushCount;
                model.RecentActivities = BuildRecentActivities(events, pushCommands);

                metricsStopwatch.Stop();
                ServerTimingHeaderWriter.AppendMetric(Response, DashboardLocalMetricsTimingName, metricsStopwatch.Elapsed);
                _logger.LogInformation(
                    "Métricas locais da dashboard carregadas em {ElapsedMilliseconds} ms. Eventos: {EventCount}; Push pendentes: {PendingPushCount}; Atividades renderizadas: {RecentActivityCount}.",
                    metricsStopwatch.ElapsedMilliseconds,
                    eventCount,
                    pendingPushCount,
                    model.RecentActivities.Count);
            }
            catch (Exception ex)
            {
                metricsStopwatch.Stop();
                ServerTimingHeaderWriter.AppendMetric(Response, DashboardLocalMetricsTimingName, metricsStopwatch.Elapsed);
                _logger.LogWarning(
                    ex,
                    "Não foi possível carregar as métricas locais da dashboard em {ElapsedMilliseconds} ms.",
                    metricsStopwatch.ElapsedMilliseconds);
                if (string.IsNullOrWhiteSpace(model.StatusMessage))
                {
                    model.StatusMessage = "A dashboard foi carregada, mas algumas métricas locais não puderam ser atualizadas.";
                    model.StatusType = "warning";
                }
            }

            return View(model);
        }

        // POST: /Home/ConnectToDevice
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConnectToDevice(ConnectionPanelViewModel connection)
        {
            if (!ModelState.IsValid)
            {
                TempData["StatusMessage"] = SecurityTextHelper.NormalizeForDisplay(
                    ModelState.Values.SelectMany(entry => entry.Errors).Select(error => error.ErrorMessage).FirstOrDefault(),
                    "Revise protocolo, host e porta antes de tentar conectar o equipamento.");
                TempData["StatusType"] = "danger";
                return RedirectToLocal(connection.ReturnUrl);
            }

            if (!_pageShellService.TryNormalizeConnection(connection, out var baseUrl, out var validationMessage))
            {
                TempData["StatusMessage"] = validationMessage;
                TempData["StatusType"] = "danger";
                return RedirectToLocal(connection.ReturnUrl);
            }

            try
            {
                var (result, document) = await _officialApi.InvokeJsonDirectAsync("system-information", baseUrl);

                if (!result.Success)
                {
                    TempData["StatusMessage"] = BuildErrorMessage("Não foi possível conectar ao equipamento", result);
                    TempData["StatusType"] = "danger";
                    _logger.LogWarning(
                        "Falha ao conectar no dispositivo Control iD em {DeviceRef}. Status: {StatusCode}",
                        PrivacyLogHelper.PseudonymizeEndpoint(baseUrl),
                        result.StatusCode);
                    return RedirectToLocal(connection.ReturnUrl);
                }

                if (document == null)
                {
                    TempData["StatusMessage"] = "Conexão realizada, mas não foi possível processar os dados do equipamento (resposta inválida da API).";
                    TempData["StatusType"] = "warning";
                    _logger.LogWarning(
                        "Resposta inesperada ou invalida ao conectar no equipamento {DeviceRef}. Status: {StatusCode}. ResponseLength: {ResponseLength}.",
                        PrivacyLogHelper.PseudonymizeEndpoint(baseUrl),
                        result.StatusCode,
                        result.ResponseBody?.Length ?? 0);
                    return RedirectToLocal(connection.ReturnUrl);
                }

                var serial = GetString(document.RootElement, "serial", "device_serial", "sn");
                var version = GetString(document.RootElement, "version", "firmware_version");
                var deviceName = GetString(document.RootElement, "product_name", "device_name", "name");

                if (string.IsNullOrWhiteSpace(serial) && string.IsNullOrWhiteSpace(deviceName))
                {
                    TempData["StatusMessage"] = "Conexão realizada, mas não foi possível identificar o equipamento (número de série ausente).";
                    TempData["StatusType"] = "warning";
                    _logger.LogWarning(
                        "Conexao ao equipamento retornou resposta inesperada. Status: {StatusCode}. ResponseLength: {ResponseLength}.",
                        result.StatusCode,
                        result.ResponseBody?.Length ?? 0);
                }
                else
                {
                    if (string.IsNullOrWhiteSpace(deviceName))
                        deviceName = DeviceModelMapper.GetDeviceNameBySerial(serial ?? string.Empty);

                    HttpContext.Session.SetString(SessionDeviceAddressKey, baseUrl);
                    HttpContext.Session.SetString(SessionDeviceNameKey, deviceName);
                    HttpContext.Session.SetString(SessionDeviceSerialKey, serial ?? string.Empty);
                    HttpContext.Session.SetString(SessionDeviceFirmwareKey, version ?? string.Empty);

                    TempData["StatusMessage"] = $"Conectado com sucesso ao equipamento: {deviceName} (Serial: {serial ?? "n/d"})";
                    TempData["StatusType"] = "success";
                    _logger.LogInformation(
                        "Conexão bem-sucedida com o equipamento {DeviceRef}, Firmware: {Version}",
                        PrivacyLogHelper.Pseudonymize(serial ?? baseUrl, "device:unknown"),
                        version);
                }
            }
            catch (HttpRequestException ex)
            {
                TempData["StatusMessage"] = SecurityTextHelper.BuildSafeUserMessage("Falha de rede ao tentar conectar ao equipamento", ex);
                TempData["StatusType"] = "danger";
                _logger.LogError(ex, "Erro de rede ao conectar no dispositivo Control iD em {DeviceRef}", PrivacyLogHelper.PseudonymizeEndpoint(baseUrl));
            }
            catch (TaskCanceledException)
            {
                TempData["StatusMessage"] = "Tempo de resposta excedido ao tentar conectar ao equipamento.";
                TempData["StatusType"] = "danger";
                _logger.LogWarning("Timeout ao conectar no dispositivo Control iD em {DeviceRef}", PrivacyLogHelper.PseudonymizeEndpoint(baseUrl));
            }
            catch (Exception ex)
            {
                TempData["StatusMessage"] = SecurityTextHelper.BuildSafeUserMessage("Erro ao conectar ao equipamento", ex);
                TempData["StatusType"] = "danger";
                _logger.LogError(ex, "Erro inesperado ao conectar no dispositivo Control iD em {DeviceRef}", PrivacyLogHelper.PseudonymizeEndpoint(baseUrl));
            }

            return RedirectToLocal(connection.ReturnUrl);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> TestDeviceConnectivity(ConnectionPanelViewModel connection)
        {
            if (!ModelState.IsValid)
            {
                TempData["StatusMessage"] = SecurityTextHelper.NormalizeForDisplay(
                    ModelState.Values.SelectMany(entry => entry.Errors).Select(error => error.ErrorMessage).FirstOrDefault(),
                    "Revise protocolo, host e porta antes de testar a comunicação.");
                TempData["StatusType"] = "danger";
                return RedirectToLocal(connection.ReturnUrl);
            }

            if (!_pageShellService.TryNormalizeConnection(connection, out var baseUrl, out var validationMessage))
            {
                TempData["StatusMessage"] = validationMessage;
                TempData["StatusType"] = "danger";
                return RedirectToLocal(connection.ReturnUrl);
            }

            try
            {
                var (result, _) = await _officialApi.InvokeJsonDirectAsync("system-information", baseUrl);
                if (!result.Success)
                {
                    TempData["StatusMessage"] = BuildErrorMessage("Comunicação testada, mas o equipamento não respondeu corretamente", result);
                    TempData["StatusType"] = "warning";
                    return RedirectToLocal(connection.ReturnUrl);
                }

                TempData["StatusMessage"] = "Comunicação OK com o equipamento informado. Agora você pode conectar esse equipamento ao contexto da PoC.";
                TempData["StatusType"] = "success";
            }
            catch (Exception ex)
            {
                TempData["StatusMessage"] = SecurityTextHelper.BuildSafeUserMessage("Falha ao testar comunicação com o equipamento", ex);
                TempData["StatusType"] = "danger";
                _logger.LogError(ex, "Erro ao testar comunicação com o equipamento {DeviceRef}.", PrivacyLogHelper.PseudonymizeEndpoint(baseUrl));
            }

            return RedirectToLocal(connection.ReturnUrl);
        }

        private static string BuildErrorMessage(string prefix, OfficialApiInvocationResult result)
        {
            return SecurityTextHelper.BuildApiFailureMessage(result, prefix);
        }

        private static string? GetString(JsonElement element, params string[] propertyNames)
        {
            foreach (var propertyName in propertyNames)
            {
                if (element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.String)
                {
                    var value = property.GetString();
                    if (!string.IsNullOrWhiteSpace(value))
                        return value;
                }
            }

            return null;
        }

        private static IReadOnlyList<DashboardActivityItemViewModel> BuildRecentActivities(
            IReadOnlyList<Models.Database.MonitorEventLocal> events,
            IReadOnlyList<Models.Database.PushCommandLocal> pushCommands)
        {
            var activities = new List<DashboardActivityItemViewModel>();

            activities.AddRange(events.Take(4).Select(evt => new DashboardActivityItemViewModel
            {
                Title = string.IsNullOrWhiteSpace(evt.EventType) ? "Evento oficial recebido" : evt.EventType,
                Description = string.IsNullOrWhiteSpace(evt.Payload)
                    ? "Evento persistido pela trilha oficial de monitor."
                    : $"Payload: {Trim(evt.Payload, 92)}",
                Meta = evt.ReceivedAt.ToLocalTime().ToString("dd/MM/yyyy HH:mm:ss"),
                Href = "/OfficialEvents",
                Tone = "danger",
                OccurredAt = evt.ReceivedAt
            }));

            activities.AddRange(pushCommands.Take(4).Select(command => new DashboardActivityItemViewModel
            {
                Title = string.IsNullOrWhiteSpace(command.CommandType) ? "Comando push enfileirado" : command.CommandType,
                Description = string.IsNullOrWhiteSpace(command.Status)
                    ? "Comando aguardando processamento."
                    : $"Status atual: {command.Status}",
                Meta = command.ReceivedAt.ToLocalTime().ToString("dd/MM/yyyy HH:mm:ss"),
                Href = "/PushCenter",
                Tone = string.Equals(command.Status, "pending", StringComparison.OrdinalIgnoreCase) ? "warning" : "neutral",
                OccurredAt = command.ReceivedAt
            }));

            return activities
                .OrderByDescending(activity => activity.OccurredAt)
                .Take(6)
                .ToList();
        }

        private static string Trim(string value, int maxLength)
        {
            if (string.IsNullOrWhiteSpace(value) || value.Length <= maxLength)
                return value;

            return value.Substring(0, maxLength - 3) + "...";
        }

        private IActionResult RedirectToLocal(string? returnUrl)
        {
            if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            return RedirectToAction(nameof(Index));
        }
    }

    public static class DeviceModelMapper
    {
        public static string GetDeviceNameBySerial(string serial)
        {
            if (string.IsNullOrWhiteSpace(serial))
                return "Equipamento desconhecido";

            // Exemplos: ajuste conforme seus padrões reais
            if (serial.StartsWith("19"))
                return "iDFace";
            if (serial.StartsWith("12"))
                return "iDAccess";
            if (serial.StartsWith("20"))
                return "iDBlock";
            // Adicione mais padrões aqui conforme necessário

            return "Modelo não identificado";
        }
    }
}
