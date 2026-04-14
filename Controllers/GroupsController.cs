using System;
using System.Text.Json;
using Integracao.ControlID.PoC.Models.ControlIDApi;
using Integracao.ControlID.PoC.Services.ControlIDApi;
using Integracao.ControlID.PoC.ViewModels.Groups;
using Integracao.ControlID.PoC.Helpers;
using Microsoft.AspNetCore.Mvc;

namespace Integracao.ControlID.PoC.Controllers
{
    public class GroupsController : Controller
    {
        private readonly OfficialControlIdApiService _apiService;
        private readonly ILogger<GroupsController> _logger;

        public GroupsController(OfficialControlIdApiService apiService, ILogger<GroupsController> logger)
        {
            _apiService = apiService;
            _logger = logger;
        }

        // GET: /Groups
        public async Task<IActionResult> Index()
        {
            var model = new GroupListViewModel();

            if (!_apiService.TryGetConnection(out _, out _))
            {
                model.ErrorMessage = "É necessário conectar-se e autenticar com um equipamento Control iD.";
                return View(model);
            }

            try
            {
                model.Groups = await LoadGroupsAsync();
            }
            catch (Exception ex)
            {
                model.ErrorMessage = SecurityTextHelper.BuildSafeUserMessage("A operação não pôde ser concluída", ex);
                _logger.LogError(ex, "Erro ao consultar grupos pela API oficial.");
            }

            return View(model);
        }

        // GET: /Groups/Details/5
        public async Task<IActionResult> Details(long? id)
        {
            if (id == null || id <= 0)
                return NotFound();

            if (!_apiService.TryGetConnection(out _, out _))
                return NotFound();

            try
            {
                var group = await LoadSingleGroupAsync(id.Value);
                if (group != null)
                    return View(group);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao consultar detalhes do grupo {GroupId}.", id);
            }

            return NotFound();
        }

        // GET: /Groups/Create
        public IActionResult Create() => View(new GroupEditViewModel());

        // POST: /Groups/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(GroupEditViewModel model)
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
                    @object = "groups",
                    values = new[] { new { name = model.Name } }
                });

                EnsureSuccess(result, "Erro ao criar grupo");

                TempData["StatusMessage"] = "Grupo criado com sucesso.";
                TempData["StatusType"] = "success";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, SecurityTextHelper.BuildSafeUserMessage("A operação não pôde ser concluída", ex));
                _logger.LogError(ex, "Erro ao criar grupo.");
            }

            return View(model);
        }

        // GET: /Groups/Edit/5
        public async Task<IActionResult> Edit(long? id)
        {
            if (id == null || id <= 0)
                return NotFound();

            if (!_apiService.TryGetConnection(out _, out _))
                return NotFound();

            try
            {
                var group = await LoadSingleGroupAsync(id.Value);
                if (group != null)
                {
                    var editModel = new GroupEditViewModel
                    {
                        Id = group.Id,
                        Name = group.Name
                    };
                    return View(editModel);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar grupo {GroupId} para edição.", id);
            }

            return NotFound();
        }

        // POST: /Groups/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(long id, GroupEditViewModel model)
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
                    @object = "groups",
                    values = new { name = model.Name },
                    where = new { groups = new { id } }
                });

                EnsureSuccess(result, "Erro ao atualizar grupo");

                TempData["StatusMessage"] = "Grupo atualizado com sucesso.";
                TempData["StatusType"] = "success";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, SecurityTextHelper.BuildSafeUserMessage("A operação não pôde ser concluída", ex));
                _logger.LogError(ex, "Erro ao atualizar grupo {GroupId}.", id);
            }

            return View(model);
        }

        // GET: /Groups/Delete/5
        public async Task<IActionResult> Delete(long? id)
        {
            if (id == null || id <= 0)
                return NotFound();

            if (!_apiService.TryGetConnection(out _, out _))
                return NotFound();

            try
            {
                var group = await LoadSingleGroupAsync(id.Value);
                if (group != null)
                {
                    return View(new GroupDeleteViewModel
                    {
                        Id = group.Id,
                        Name = group.Name
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar grupo {GroupId} para exclusão.", id);
            }

            return NotFound();
        }

        // POST: /Groups/Delete/5
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
                    @object = "groups",
                    where = new { groups = new { id } }
                });

                EnsureSuccess(result, "Erro ao excluir grupo");

                TempData["StatusMessage"] = "Grupo excluído com sucesso.";
                TempData["StatusType"] = "success";
            }
            catch (Exception ex)
            {
                TempData["StatusMessage"] = SecurityTextHelper.BuildSafeUserMessage("A operação não pôde ser concluída", ex);
                TempData["StatusType"] = "danger";
                _logger.LogError(ex, "Erro ao excluir grupo {GroupId}.", id);
            }

            return RedirectToAction(nameof(Index));
        }

        private async Task<List<GroupViewModel>> LoadGroupsAsync(long? id = null)
        {
            object payload = id.HasValue
                ? new
                {
                    @object = "groups",
                    where = new
                    {
                        groups = new
                        {
                            id = id.Value
                        }
                    }
                }
                : new
                {
                    @object = "groups"
                };

            var (result, document) = await _apiService.InvokeJsonAsync("load-objects", payload);
            EnsureSuccess(result, "Erro ao consultar grupos");

            if (document == null || !document.RootElement.TryGetProperty("groups", out var groupsElement))
                return [];

            var groups = JsonSerializer.Deserialize<List<GroupRecord>>(groupsElement.GetRawText(), JsonOptions()) ?? [];

            return groups.Select(group => new GroupViewModel
            {
                Id = group.Id,
                Name = group.Name
            }).ToList();
        }

        private async Task<GroupViewModel?> LoadSingleGroupAsync(long id)
        {
            var groups = await LoadGroupsAsync(id);
            return groups.FirstOrDefault();
        }

        private static void EnsureSuccess(OfficialApiInvocationResult result, string prefix)
        {
            if (result.Success)
                return;

            throw new InvalidOperationException(BuildErrorMessage(result, prefix));
        }

        private static string BuildErrorMessage(OfficialApiInvocationResult result, string prefix)
        {
            return SecurityTextHelper.BuildApiFailureMessage(result, prefix);
        }

        private static JsonSerializerOptions JsonOptions()
        {
            return new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
        }

        private sealed class GroupRecord
        {
            public long Id { get; set; }
            public string Name { get; set; } = string.Empty;
        }
    }
}




