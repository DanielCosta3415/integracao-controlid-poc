using System;
using System.Text.Json;
using Integracao.ControlID.PoC.Models.ControlIDApi;
using Integracao.ControlID.PoC.Services.ControlIDApi;
using Integracao.ControlID.PoC.ViewModels.Config;
using Integracao.ControlID.PoC.Helpers;
using Microsoft.AspNetCore.Mvc;

namespace Integracao.ControlID.PoC.Controllers
{
    public class ConfigController : Controller
    {
        private readonly OfficialControlIdApiService _apiService;
        private readonly ILogger<ConfigController> _logger;

        public ConfigController(OfficialControlIdApiService apiService, ILogger<ConfigController> logger)
        {
            _apiService = apiService;
            _logger = logger;
        }

        // GET: /Config
        public async Task<IActionResult> Index()
        {
            var model = new ConfigListViewModel();
            if (!_apiService.TryGetConnection(out _, out _))
            {
                model.ErrorMessage = "É necessário conectar-se e autenticar com um equipamento Control iD.";
                return View(model);
            }

            try
            {
                model.Configs = await LoadConfigsAsync();
            }
            catch (Exception ex)
            {
                model.ErrorMessage = SecurityTextHelper.BuildSafeUserMessage("A operação não pôde ser concluída", ex);
                _logger.LogError(ex, "Erro ao consultar configurações pela API oficial.");
            }

            return View(model);
        }

        // GET: /Config/Diagnostics
        public IActionResult Diagnostics()
        {
            return View(new ConfigDiagnosticsViewModel());
        }

        // POST: /Config/ConnectionTest
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ConnectionTest(ConfigDiagnosticsViewModel model)
        {
            if (!_apiService.TryGetConnection(out _, out _))
            {
                model.ErrorMessage = "É necessário conectar-se e autenticar com um equipamento Control iD.";
                return View("Diagnostics", model);
            }

            ModelState.Remove(nameof(model.PingHost));
            ModelState.Remove(nameof(model.NslookupHost));

            if (!ModelState.IsValid)
                return View("Diagnostics", model);

            try
            {
                var (result, document) = await _apiService.InvokeJsonAsync("connection-test", new
                {
                    host = model.ConnectionHost,
                    port = model.ConnectionPort
                });

                EnsureSuccess(result, "Erro ao executar teste de conexão");
                model.ConnectionResultJson = FormatResponse(result.ResponseBody, document);
            }
            catch (Exception ex)
            {
                model.ErrorMessage = SecurityTextHelper.BuildSafeUserMessage("A operação não pôde ser concluída", ex);
                _logger.LogError(ex, "Erro ao executar teste de conexão oficial.");
            }

            return View("Diagnostics", model);
        }

        // POST: /Config/PingTest
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PingTest(ConfigDiagnosticsViewModel model)
        {
            if (!_apiService.TryGetConnection(out _, out _))
            {
                model.ErrorMessage = "É necessário conectar-se e autenticar com um equipamento Control iD.";
                return View("Diagnostics", model);
            }

            ModelState.Remove(nameof(model.ConnectionHost));
            ModelState.Remove(nameof(model.ConnectionPort));
            ModelState.Remove(nameof(model.NslookupHost));

            if (!ModelState.IsValid)
                return View("Diagnostics", model);

            try
            {
                var (result, document) = await _apiService.InvokeJsonAsync("ping-test", new
                {
                    host = model.PingHost
                });

                EnsureSuccess(result, "Erro ao executar ping");
                model.PingResultJson = FormatResponse(result.ResponseBody, document);
            }
            catch (Exception ex)
            {
                model.ErrorMessage = SecurityTextHelper.BuildSafeUserMessage("A operação não pôde ser concluída", ex);
                _logger.LogError(ex, "Erro ao executar ping oficial.");
            }

            return View("Diagnostics", model);
        }

        // POST: /Config/NslookupTest
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> NslookupTest(ConfigDiagnosticsViewModel model)
        {
            if (!_apiService.TryGetConnection(out _, out _))
            {
                model.ErrorMessage = "É necessário conectar-se e autenticar com um equipamento Control iD.";
                return View("Diagnostics", model);
            }

            ModelState.Remove(nameof(model.ConnectionHost));
            ModelState.Remove(nameof(model.ConnectionPort));
            ModelState.Remove(nameof(model.PingHost));

            if (!ModelState.IsValid)
                return View("Diagnostics", model);

            try
            {
                var (result, document) = await _apiService.InvokeJsonAsync("nslookup-test", new
                {
                    host = model.NslookupHost
                });

                EnsureSuccess(result, "Erro ao executar nslookup");
                model.NslookupResultJson = FormatResponse(result.ResponseBody, document);
            }
            catch (Exception ex)
            {
                model.ErrorMessage = SecurityTextHelper.BuildSafeUserMessage("A operação não pôde ser concluída", ex);
                _logger.LogError(ex, "Erro ao executar nslookup oficial.");
            }

            return View("Diagnostics", model);
        }

        // GET: /Config/Official
        public IActionResult Official()
        {
            return View(new ConfigOfficialViewModel());
        }

        // POST: /Config/GetOfficial
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> GetOfficial(ConfigOfficialViewModel model)
        {
            if (!_apiService.TryGetConnection(out _, out _))
            {
                model.ErrorMessage = "É necessário conectar-se e autenticar com um equipamento Control iD.";
                return View("Official", model);
            }

            ModelState.Remove(nameof(model.SetPayload));

            if (!ModelState.IsValid)
                return View("Official", model);

            try
            {
                ValidateJsonPayload(model.GetPayload, nameof(model.GetPayload));

                var (result, document) = await _apiService.InvokeJsonAsync("get-configuration", model.GetPayload);
                EnsureSuccess(result, "Erro ao consultar configurações oficiais");

                model.GetResponseJson = FormatResponse(result.ResponseBody, document);
            }
            catch (Exception ex)
            {
                model.ErrorMessage = SecurityTextHelper.BuildSafeUserMessage("A operação não pôde ser concluída", ex);
                _logger.LogError(ex, "Erro ao consultar get_configuration oficial.");
            }

            return View("Official", model);
        }

        // POST: /Config/SetOfficial
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SetOfficial(ConfigOfficialViewModel model)
        {
            if (!_apiService.TryGetConnection(out _, out _))
            {
                model.ErrorMessage = "É necessário conectar-se e autenticar com um equipamento Control iD.";
                return View("Official", model);
            }

            ModelState.Remove(nameof(model.GetPayload));

            if (!ModelState.IsValid)
                return View("Official", model);

            try
            {
                ValidateJsonPayload(model.SetPayload, nameof(model.SetPayload));

                var (result, document) = await _apiService.InvokeJsonAsync("set-configuration", model.SetPayload);
                EnsureSuccess(result, "Erro ao alterar configurações oficiais");

                model.SetResponseJson = FormatResponse(result.ResponseBody, document);
            }
            catch (Exception ex)
            {
                model.ErrorMessage = SecurityTextHelper.BuildSafeUserMessage("A operação não pôde ser concluída", ex);
                _logger.LogError(ex, "Erro ao consultar set_configuration oficial.");
            }

            return View("Official", model);
        }

        // GET: /Config/Details/5
        public async Task<IActionResult> Details(long? id)
        {
            if (id == null)
                return NotFound();

            if (!_apiService.TryGetConnection(out _, out _))
                return NotFound();

            try
            {
                var config = await LoadSingleConfigAsync(id.Value);
                if (config != null)
                    return View(config);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao consultar detalhes da configuração {ConfigId}.", id);
            }

            return NotFound();
        }

        // GET: /Config/Create
        public IActionResult Create()
        {
            return View(new ConfigEditViewModel());
        }

        // POST: /Config/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ConfigEditViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            if (!_apiService.TryGetConnection(out _, out _))
            {
                ModelState.AddModelError(string.Empty, "É necessário conectar-se e autenticar com um equipamento Control iD.");
                return View(model);
            }

            try
            {
                var result = await _apiService.InvokeAsync("create-objects", new
                {
                    @object = "config_groups",
                    values = new[]
                    {
                        new
                        {
                            group = model.Group,
                            key = model.Key,
                            value = model.Value
                        }
                    }
                });

                EnsureSuccess(result, "Erro ao criar configuração");

                TempData["StatusMessage"] = "Configuração criada com sucesso.";
                TempData["StatusType"] = "success";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, SecurityTextHelper.BuildSafeUserMessage("A operação não pôde ser concluída", ex));
                _logger.LogError(ex, "Erro ao criar configuração.");
                return View(model);
            }
        }

        // GET: /Config/Edit/5
        public async Task<IActionResult> Edit(long? id)
        {
            if (id == null)
                return NotFound();

            if (!_apiService.TryGetConnection(out _, out _))
                return NotFound();

            try
            {
                var config = await LoadSingleConfigAsync(id.Value);
                if (config != null)
                {
                    return View(new ConfigEditViewModel
                    {
                        Id = config.Id,
                        Group = config.Group,
                        Key = config.Key,
                        Value = config.Value
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar configuração {ConfigId} para edição.", id);
            }

            return NotFound();
        }

        // POST: /Config/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(long id, ConfigEditViewModel model)
        {
            if (model.Id != id)
                return NotFound();

            if (!ModelState.IsValid)
                return View(model);

            if (!_apiService.TryGetConnection(out _, out _))
            {
                ModelState.AddModelError(string.Empty, "É necessário conectar-se e autenticar com um equipamento Control iD.");
                return View(model);
            }

            try
            {
                var result = await _apiService.InvokeAsync("modify-objects", new
                {
                    @object = "config_groups",
                    values = new
                    {
                        group = model.Group,
                        key = model.Key,
                        value = model.Value
                    },
                    where = new
                    {
                        config_groups = new
                        {
                            id
                        }
                    }
                });

                EnsureSuccess(result, "Erro ao atualizar configuração");

                TempData["StatusMessage"] = "Configuração atualizada com sucesso.";
                TempData["StatusType"] = "success";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, SecurityTextHelper.BuildSafeUserMessage("A operação não pôde ser concluída", ex));
                _logger.LogError(ex, "Erro ao atualizar configuração {ConfigId}.", id);
                return View(model);
            }
        }

        // GET: /Config/Delete/5
        public async Task<IActionResult> Delete(long? id)
        {
            if (id == null)
                return NotFound();

            if (!_apiService.TryGetConnection(out _, out _))
                return NotFound();

            try
            {
                var config = await LoadSingleConfigAsync(id.Value);
                if (config != null)
                {
                    return View(new ConfigDeleteViewModel
                    {
                        Id = config.Id,
                        Group = config.Group,
                        Key = config.Key
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar configuração {ConfigId} para exclusão.", id);
            }

            return NotFound();
        }

        // POST: /Config/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(long id)
        {
            if (!_apiService.TryGetConnection(out _, out _))
            {
                TempData["StatusMessage"] = "É necessário conectar-se e autenticar com um equipamento Control iD.";
                TempData["StatusType"] = "danger";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                var result = await _apiService.InvokeAsync("destroy-objects", new
                {
                    @object = "config_groups",
                    where = new
                    {
                        config_groups = new
                        {
                            id
                        }
                    }
                });

                EnsureSuccess(result, "Erro ao excluir configuração");

                TempData["StatusMessage"] = "Configuração excluída com sucesso.";
                TempData["StatusType"] = "success";
            }
            catch (Exception ex)
            {
                TempData["StatusMessage"] = SecurityTextHelper.BuildSafeUserMessage("A operação não pôde ser concluída", ex);
                TempData["StatusType"] = "danger";
                _logger.LogError(ex, "Erro ao excluir configuração {ConfigId}.", id);
            }

            return RedirectToAction(nameof(Index));
        }

        #region Métodos Auxiliares

        private async Task<List<ConfigViewModel>> LoadConfigsAsync(long? id = null)
        {
            object payload = id.HasValue
                ? new
                {
                    @object = "config_groups",
                    where = new
                    {
                        config_groups = new
                        {
                            id = id.Value
                        }
                    }
                }
                : new
                {
                    @object = "config_groups"
                };

            var (result, document) = await _apiService.InvokeJsonAsync("load-objects", payload);
            EnsureSuccess(result, "Erro ao consultar configurações");

            if (document == null || !document.RootElement.TryGetProperty("config_groups", out var configsElement))
                return [];

            var configs = JsonSerializer.Deserialize<List<ConfigRecord>>(configsElement.GetRawText(), JsonOptions()) ?? [];

            return configs.Select(dto => new ConfigViewModel
            {
                Id = dto.Id,
                Group = dto.Group,
                Key = dto.Key,
                Value = dto.Value
            }).ToList();
        }

        private async Task<ConfigViewModel?> LoadSingleConfigAsync(long id)
        {
            var configs = await LoadConfigsAsync(id);
            return configs.FirstOrDefault();
        }

        private static void EnsureSuccess(OfficialApiInvocationResult result, string errorPrefix)
        {
            if (result.Success)
                return;

            throw new InvalidOperationException(BuildErrorMessage(result, errorPrefix));
        }

        private static string BuildErrorMessage(OfficialApiInvocationResult result, string errorPrefix)
        {
            return SecurityTextHelper.BuildApiFailureMessage(result, errorPrefix);
        }

        private static void ValidateJsonPayload(string payload, string fieldName)
        {
            if (string.IsNullOrWhiteSpace(payload))
                throw new InvalidOperationException($"Informe o JSON do campo {fieldName}.");

            try
            {
                using var _ = JsonDocument.Parse(payload);
            }
            catch (JsonException ex)
            {
                throw new InvalidOperationException($"JSON inválido em {fieldName}: {ex.Message}", ex);
            }
        }

        private static string FormatResponse(string rawJson, JsonDocument? document)
        {
            if (document == null)
                return rawJson;

            return JsonSerializer.Serialize(document.RootElement, new JsonSerializerOptions
            {
                WriteIndented = true
            });
        }

        private static JsonSerializerOptions JsonOptions()
        {
            return new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
        }

        private sealed class ConfigRecord
        {
            public long Id { get; set; }
            public string Group { get; set; } = string.Empty;
            public string Key { get; set; } = string.Empty;
            public string Value { get; set; } = string.Empty;
        }

        #endregion
    }
}



