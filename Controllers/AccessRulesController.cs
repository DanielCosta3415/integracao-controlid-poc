using System;
using System.Text.Json;
using Integracao.ControlID.PoC.Models.ControlIDApi;
using Integracao.ControlID.PoC.Services.ControlIDApi;
using Integracao.ControlID.PoC.Services.Security;
using Integracao.ControlID.PoC.ViewModels.AccessRules;
using Integracao.ControlID.PoC.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Integracao.ControlID.PoC.Controllers
{
    [Authorize(Roles = AppSecurityRoles.Administrator)]
    public class AccessRulesController : Controller
    {
        private readonly OfficialControlIdApiService _apiService;
        private readonly ILogger<AccessRulesController> _logger;

        public AccessRulesController(OfficialControlIdApiService apiService, ILogger<AccessRulesController> logger)
        {
            _apiService = apiService;
            _logger = logger;
        }

        // GET: /AccessRules
        public async Task<IActionResult> Index()
        {
            var model = new AccessRuleListViewModel();
            if (!_apiService.TryGetConnection(out _, out _))
            {
                model.ErrorMessage = "É necessário conectar-se e autenticar com um equipamento Control iD.";
                return View(model);
            }

            try
            {
                model.AccessRules = await LoadRulesAsync();
            }
            catch (Exception ex)
            {
                model.ErrorMessage = SecurityTextHelper.BuildSafeUserMessage("A operação não pôde ser concluída", ex);
                _logger.LogError(ex, "Erro ao consultar regras de acesso pela API oficial.");
            }

            return View(model);
        }

        // GET: /AccessRules/Details/5
        public async Task<IActionResult> Details(long? id)
        {
            if (id == null)
                return NotFound();

            if (!_apiService.TryGetConnection(out _, out _))
                return NotFound();

            try
            {
                var rule = await LoadSingleRuleAsync(id.Value);
                if (rule != null)
                    return View(rule);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao consultar detalhes da regra de acesso {RuleId}.", id);
            }

            return NotFound();
        }

        // GET: /AccessRules/Create
        public IActionResult Create()
        {
            return View(new AccessRuleEditViewModel());
        }

        // POST: /AccessRules/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AccessRuleEditViewModel model)
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
                    @object = "access_rules",
                    values = new[] { new { name = model.Name, type = model.Type, priority = model.Priority } }
                });

                EnsureSuccess(result, "Erro ao criar regra de acesso");

                TempData["StatusMessage"] = "Regra de acesso criada com sucesso.";
                TempData["StatusType"] = "success";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, SecurityTextHelper.BuildSafeUserMessage("A operação não pôde ser concluída", ex));
                _logger.LogError(ex, "Erro ao criar regra de acesso.");
                return View(model);
            }
        }

        // GET: /AccessRules/Edit/5
        public async Task<IActionResult> Edit(long? id)
        {
            if (id == null)
                return NotFound();

            if (!_apiService.TryGetConnection(out _, out _))
                return NotFound();

            try
            {
                var rule = await LoadSingleRuleAsync(id.Value);
                if (rule != null)
                {
                    var editModel = new AccessRuleEditViewModel
                    {
                        Id = rule.Id,
                        Name = rule.Name,
                        Type = rule.Type,
                        Priority = rule.Priority
                    };
                    return View(editModel);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar regra de acesso {RuleId} para edição.", id);
            }
            return NotFound();
        }

        // POST: /AccessRules/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(long id, AccessRuleEditViewModel model)
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
                    @object = "access_rules",
                    values = new
                    {
                        name = model.Name,
                        type = model.Type,
                        priority = model.Priority
                    },
                    where = new
                    {
                        access_rules = new
                        {
                            id
                        }
                    }
                });

                EnsureSuccess(result, "Erro ao atualizar regra de acesso");

                TempData["StatusMessage"] = "Regra de acesso atualizada com sucesso.";
                TempData["StatusType"] = "success";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, SecurityTextHelper.BuildSafeUserMessage("A operação não pôde ser concluída", ex));
                _logger.LogError(ex, "Erro ao atualizar regra de acesso {RuleId}.", id);
                return View(model);
            }
        }

        // GET: /AccessRules/Delete/5
        public async Task<IActionResult> Delete(long? id)
        {
            if (id == null)
                return NotFound();

            if (!_apiService.TryGetConnection(out _, out _))
                return NotFound();

            try
            {
                var rule = await LoadSingleRuleAsync(id.Value);
                if (rule != null)
                {
                    return View(new AccessRuleDeleteViewModel
                    {
                        Id = rule.Id,
                        Name = rule.Name
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar regra de acesso {RuleId} para exclusão.", id);
            }
            return NotFound();
        }

        // POST: /AccessRules/Delete/5
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
                    @object = "access_rules",
                    where = new
                    {
                        access_rules = new
                        {
                            id
                        }
                    }
                });

                EnsureSuccess(result, "Erro ao excluir regra de acesso");

                TempData["StatusMessage"] = "Regra de acesso excluída com sucesso.";
                TempData["StatusType"] = "success";
            }
            catch (Exception ex)
            {
                TempData["StatusMessage"] = SecurityTextHelper.BuildSafeUserMessage("A operação não pôde ser concluída", ex);
                TempData["StatusType"] = "danger";
                _logger.LogError(ex, "Erro ao excluir regra de acesso {RuleId}.", id);
            }

            return RedirectToAction(nameof(Index));
        }

        #region Métodos Auxiliares

        private async Task<List<AccessRuleViewModel>> LoadRulesAsync(long? id = null)
        {
            object payload = id.HasValue
                ? new
                {
                    @object = "access_rules",
                    where = new
                    {
                        access_rules = new
                        {
                            id = id.Value
                        }
                    }
                }
                : new
                {
                    @object = "access_rules"
                };

            var (result, document) = await _apiService.InvokeJsonAsync("load-objects", payload);
            EnsureSuccess(result, "Erro ao consultar regras de acesso");

            if (document == null || !document.RootElement.TryGetProperty("access_rules", out var rulesElement))
                return [];

            var rules = JsonSerializer.Deserialize<List<AccessRuleRecord>>(rulesElement.GetRawText(), JsonOptions()) ?? [];

            return rules.Select(dto => new AccessRuleViewModel
            {
                Id = dto.Id,
                Name = dto.Name,
                Type = dto.Type,
                Priority = dto.Priority
            }).ToList();
        }

        private async Task<AccessRuleViewModel?> LoadSingleRuleAsync(long id)
        {
            var rules = await LoadRulesAsync(id);
            return rules.FirstOrDefault();
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

        private static JsonSerializerOptions JsonOptions()
        {
            return new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
        }

        private sealed class AccessRuleRecord
        {
            public long Id { get; set; }
            public string Name { get; set; } = string.Empty;
            public int Type { get; set; }
            public int Priority { get; set; }
        }

        #endregion
    }
}



