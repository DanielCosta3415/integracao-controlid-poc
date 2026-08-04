using System.Net.Http.Json;
using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using Integracao.ControlID.PoC.Services.Security;
using Integracao.ControlID.PoC.ViewModels.Development;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Integracao.ControlID.PoC.Controllers;

[Authorize(Roles = AppSecurityRoles.Administrator)]
[ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
public sealed class DevelopmentController : Controller
{
    private static readonly string[] DefaultScenarios = ["normal", "slow", "timeout", "bad-request", "unauthorized", "forbidden", "not-found", "conflict", "rate-limited", "server-error", "invalid-json", "truncated-json", "unexpected-json", "wrong-content-type", "oversized-response", "session-expired", "feature-unavailable", "network-drop"];
    private static readonly string[] DefaultProfiles = ["idface", "idflex", "idbox", "legacy"];
    private static readonly HashSet<int> SupportedDatasetSizes = [1, 100, 1000, 10000, 100000];
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IWebHostEnvironment _environment;
    private readonly IConfiguration _configuration;
    private readonly ILogger<DevelopmentController> _logger;

    public DevelopmentController(
        IHttpClientFactory httpClientFactory,
        IWebHostEnvironment environment,
        IConfiguration configuration,
        ILogger<DevelopmentController> logger)
    {
        _httpClientFactory = httpClientFactory;
        _environment = environment;
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<IActionResult> Simulator(CancellationToken cancellationToken)
    {
        if (!_environment.IsDevelopment())
            return NotFound();

        var model = new SimulatorViewModel
        {
            StubUrl = ResolveStubUrl().ToString(),
            StatusMessage = TempData["StatusMessage"] as string,
            StatusType = TempData["StatusType"] as string ?? "info",
            AvailableScenarios = DefaultScenarios,
            AvailableProfiles = DefaultProfiles,
            AvailableDatasetSizes = SupportedDatasetSizes.Order().ToArray()
        };

        try
        {
            var client = CreateClient();
            var statusTask = client.GetFromJsonAsync<StubStatusDto>("/__stub/status", cancellationToken);
            var catalogTask = client.GetFromJsonAsync<StubCatalogDto>("/__stub/catalog", cancellationToken);
            await Task.WhenAll(statusTask, catalogTask);

            var status = await statusTask;
            var catalog = await catalogTask;
            if (status == null || catalog == null)
                throw new InvalidDataException("O simulador retornou um estado incompleto.");

            model.IsReachable = true;
            model.Scenario = status.Scenario.Name;
            model.Profile = status.Profile.Name;
            model.DatasetSize = status.DatasetSize;
            model.SelectedScenario = status.Scenario.Name;
            model.SelectedProfile = status.Profile.Name;
            model.SelectedDatasetSize = status.DatasetSize;
            model.DelayMilliseconds = status.Scenario.DelayMs;
            model.Endpoint = status.Scenario.Endpoint;
            model.ResponseBytes = status.Scenario.ResponseBytes;
            model.AvailableScenarios = catalog.Scenarios;
            model.AvailableProfiles = catalog.Profiles;
            model.AvailableDatasetSizes = catalog.DatasetSizes;
            model.Requests = status.Requests
                .Select(item => new SimulatorRequestMetricViewModel(
                    item.Key,
                    item.Value.Count,
                    item.Value.AverageMilliseconds,
                    item.Value.MaximumMilliseconds))
                .OrderByDescending(static item => item.Count)
                .ThenBy(static item => item.Path, StringComparer.Ordinal)
                .ToArray();
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or JsonException or InvalidDataException)
        {
            model.StatusMessage ??= "O simulador local não está disponível. Inicie-o e tente novamente.";
            model.StatusType = "warning";
            _logger.LogWarning(ex, "Development simulator status could not be loaded from loopback.");
        }

        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ApplyScenario(SimulatorViewModel model, CancellationToken cancellationToken)
    {
        if (!_environment.IsDevelopment())
            return NotFound();

        if (!ModelState.IsValid)
            return await Simulator(cancellationToken);

        try
        {
            var response = await CreateClient().PostAsJsonAsync(
                "/__stub/scenario",
                new
                {
                    name = model.SelectedScenario,
                    delayMs = model.DelayMilliseconds,
                    endpoint = string.IsNullOrWhiteSpace(model.Endpoint) ? null : model.Endpoint,
                    responseBytes = model.ResponseBytes
                },
                cancellationToken);
            response.EnsureSuccessStatusCode();
            TempData["StatusMessage"] = $"Cenário '{model.SelectedScenario}' aplicado ao simulador.";
            TempData["StatusType"] = "success";
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
        {
            TempData["StatusMessage"] = "Não foi possível configurar o cenário no simulador local.";
            TempData["StatusType"] = "danger";
            _logger.LogWarning(ex, "Development simulator scenario update failed.");
        }

        return RedirectToAction(nameof(Simulator));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResetSimulator(SimulatorViewModel model, CancellationToken cancellationToken)
    {
        if (!_environment.IsDevelopment())
            return NotFound();

        if (!SupportedDatasetSizes.Contains(model.SelectedDatasetSize))
            ModelState.AddModelError(nameof(model.SelectedDatasetSize), "Selecione uma massa suportada pelo simulador.");

        if (!ModelState.IsValid)
            return await Simulator(cancellationToken);

        try
        {
            var response = await CreateClient().PostAsJsonAsync(
                "/__stub/reset",
                new { profile = model.SelectedProfile, datasetSize = model.SelectedDatasetSize },
                cancellationToken);
            response.EnsureSuccessStatusCode();
            TempData["StatusMessage"] = "Simulador reinicializado com perfil e massa determinísticos.";
            TempData["StatusType"] = "success";
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
        {
            TempData["StatusMessage"] = "Não foi possível reinicializar o simulador local.";
            TempData["StatusType"] = "danger";
            _logger.LogWarning(ex, "Development simulator reset failed.");
        }

        return RedirectToAction(nameof(Simulator));
    }

    private HttpClient CreateClient()
    {
        var client = _httpClientFactory.CreateClient();
        client.BaseAddress = ResolveStubUrl();
        client.Timeout = TimeSpan.FromSeconds(5);
        return client;
    }

    private Uri ResolveStubUrl()
    {
        var configured = _configuration["Demo:StubUrl"] ?? "http://127.0.0.1:6600";
        if (!Uri.TryCreate(configured, UriKind.Absolute, out var uri) || uri == null)
        {
            throw new InvalidOperationException("Demo:StubUrl deve apontar para HTTP em loopback.");
        }

        var isLoopback = uri.IsLoopback ||
                         uri.Host.Equals("localhost", StringComparison.OrdinalIgnoreCase) ||
                         (IPAddress.TryParse(uri.Host, out var address) && IPAddress.IsLoopback(address));
        if (uri.Scheme != Uri.UriSchemeHttp || !isLoopback)
            throw new InvalidOperationException("Demo:StubUrl deve apontar para HTTP em loopback.");

        return new Uri(uri.GetLeftPart(UriPartial.Authority));
    }

    private sealed record StubStatusDto(
        StubScenarioDto Scenario,
        StubProfileDto Profile,
        [property: JsonPropertyName("dataset_size")] int DatasetSize,
        IReadOnlyDictionary<string, StubRequestMetricDto> Requests);

    private sealed record StubScenarioDto(string Name, int DelayMs, string Endpoint, int ResponseBytes);
    private sealed record StubProfileDto(string Name);
    private sealed record StubCatalogDto(
        IReadOnlyList<string> Scenarios,
        IReadOnlyList<string> Profiles,
        [property: JsonPropertyName("dataset_sizes")] IReadOnlyList<int> DatasetSizes);
    private sealed record StubRequestMetricDto(
        long Count,
        [property: JsonPropertyName("average_ms")] double AverageMilliseconds,
        [property: JsonPropertyName("max_ms")] double MaximumMilliseconds);
}
