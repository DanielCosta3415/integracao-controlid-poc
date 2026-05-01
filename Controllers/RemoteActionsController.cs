using System;
using Integracao.ControlID.PoC.Models.ControlIDApi;
using Integracao.ControlID.PoC.Services.ControlIDApi;
using Integracao.ControlID.PoC.Services.Security;
using Integracao.ControlID.PoC.ViewModels.RemoteActions;
using Integracao.ControlID.PoC.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Integracao.ControlID.PoC.Controllers
{
    [Authorize(Roles = AppSecurityRoles.Administrator)]
    public class RemoteActionsController : Controller
    {
        private readonly OfficialControlIdApiService _apiService;
        private readonly ILogger<RemoteActionsController> _logger;

        public RemoteActionsController(OfficialControlIdApiService apiService, ILogger<RemoteActionsController> logger)
        {
            _apiService = apiService;
            _logger = logger;
        }

        // Centralização das ações disponíveis
        private static readonly List<RemoteActionViewModel> AvailableActions = new()
        {
            new() { Action = "open_door", DisplayName = "Abrir Porta", Parameters = "Usa execute_actions com action=door" },
            new() { Action = "activate_buzzer", DisplayName = "Acionar Buzzer", Parameters = "Usa buzzer_buzz com parâmetros padrão seguros" },
            new() { Action = "send_message", DisplayName = "Enviar Mensagem", Parameters = "Usa message_to_screen com timeout de 3 segundos" },
            new() { Action = "remote_authorization", DisplayName = "Autorização Remota", Parameters = "Usa remote_user_authorization com portal, usuário, evento e ação final" },
            new() { Action = "remote_enroll", DisplayName = "Cadastro Remoto", Parameters = "Usa remote_enroll para face, biometria, cartão, PIN ou senha" },
            new() { Action = "cancel_remote_enroll", DisplayName = "Cancelar Cadastro Remoto", Parameters = "Usa cancel_remote_enroll para interromper a captura em andamento" }
        };

        // GET: /RemoteActions
        public IActionResult Index()
        {
            var model = new RemoteActionListViewModel
            {
                Actions = AvailableActions
            };
            return View(model);
        }

        // GET: /RemoteActions/Enroll
        public IActionResult Enroll()
        {
            return View(new RemoteEnrollViewModel());
        }

        // GET: /RemoteActions/Authorization
        public IActionResult Authorization()
        {
            return View(new RemoteAuthorizationViewModel());
        }

        // GET: /RemoteActions/Details?action=open_door
        public IActionResult Details([FromQuery(Name = "action")] string actionName)
        {
            if (string.IsNullOrWhiteSpace(actionName))
                return NotFound();

            var found = AvailableActions.Find(a => a.Action == actionName);
            if (found == null)
                return NotFound();

            var model = new RemoteActionViewModel { Action = actionName, DisplayName = found.DisplayName };
            return View(model);
        }

        // POST: /RemoteActions/Execute
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Execute(RemoteActionExecuteViewModel model)
        {
            if (!ModelState.IsValid)
            {
                TempData["StatusMessage"] = "Parâmetros inválidos para execução da ação remota.";
                TempData["StatusType"] = "danger";
                return RedirectToAction(nameof(Index));
            }

            if (!_apiService.TryGetConnection(out _, out _))
            {
                TempData["StatusMessage"] = "É necessário conectar-se e autenticar com um equipamento Control iD.";
                TempData["StatusType"] = "danger";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                OfficialApiInvocationResult result;

                switch (model.Action)
                {
                    case "open_door":
                        if (model.Door == null)
                        {
                            TempData["StatusMessage"] = "Informe o número da porta para abrir.";
                            TempData["StatusType"] = "danger";
                            return RedirectToAction(nameof(Index));
                        }

                        result = await _apiService.InvokeAsync("execute-actions", new
                        {
                            actions = new[]
                            {
                                new
                                {
                                    action = "door",
                                    parameters = $"door={model.Door.Value}"
                                }
                            }
                        });
                        break;

                    case "activate_buzzer":
                        result = await _apiService.InvokeAsync("buzzer-buzz", new
                        {
                            frequency = 4000,
                            duty_cycle = 50,
                            timeout = 1000
                        });
                        break;

                    case "send_message":
                        if (string.IsNullOrWhiteSpace(model.Message))
                        {
                            TempData["StatusMessage"] = "Informe a mensagem a ser exibida.";
                            TempData["StatusType"] = "danger";
                            return RedirectToAction(nameof(Index));
                        }

                        result = await _apiService.InvokeAsync("message-to-screen", new
                        {
                            message = model.Message,
                            timeout = 3000
                        });
                        break;

                    case "cancel_remote_enroll":
                        result = await _apiService.InvokeAsync("cancel-remote-enroll");
                        break;

                    default:
                        TempData["StatusMessage"] = "Ação remota não suportada pela camada oficial atual.";
                        TempData["StatusType"] = "danger";
                        return RedirectToAction(nameof(Index));
                }

                if (!result.Success)
                    throw new InvalidOperationException(BuildErrorMessage(result));

                TempData["StatusMessage"] = "Ação remota executada com sucesso.";
                TempData["StatusType"] = "success";
            }
            catch (Exception ex)
            {
                TempData["StatusMessage"] = SecurityTextHelper.BuildSafeUserMessage("Erro ao executar ação remota", ex);
                TempData["StatusType"] = "danger";
                _logger.LogError(ex, "Erro ao executar ação remota.");
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: /RemoteActions/Enroll
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Enroll(RemoteEnrollViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            if (!_apiService.TryGetConnection(out _, out _))
            {
                model.ErrorMessage = "É necessário conectar-se e autenticar com um equipamento Control iD.";
                return View(model);
            }

            try
            {
                var result = await _apiService.InvokeAsync("remote-enroll", new
                {
                    type = model.Type,
                    user_id = model.UserId,
                    save = model.Save,
                    sync = model.Sync
                });

                if (!result.Success)
                    throw new InvalidOperationException(BuildErrorMessage(result));

                model.ResultMessage = "Cadastro remoto iniciado com sucesso. Acompanhe os callbacks oficiais para receber o resultado.";
                model.ResultStatusType = "success";
                model.ResponseJson = string.IsNullOrWhiteSpace(result.ResponseBody)
                    ? "Operação iniciada sem corpo de resposta."
                    : result.ResponseBody;
            }
            catch (Exception ex)
            {
                model.ResultMessage = SecurityTextHelper.BuildSafeUserMessage("Erro ao iniciar cadastro remoto", ex);
                model.ResultStatusType = "danger";
                _logger.LogError(ex, "Erro ao iniciar cadastro remoto.");
            }

            return View(model);
        }

        // POST: /RemoteActions/Authorization
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Authorization(RemoteAuthorizationViewModel model)
        {
            if (!ModelState.IsValid)
                return View(model);

            if (!_apiService.TryGetConnection(out _, out _))
            {
                model.ErrorMessage = "É necessário conectar-se e autenticar com um equipamento Control iD.";
                return View(model);
            }

            try
            {
                var result = await _apiService.InvokeAsync("remote-user-authorization", new
                {
                    @event = model.Event,
                    user_id = model.UserId,
                    user_name = model.UserName,
                    user_image = model.UserImage,
                    portal_id = model.PortalId,
                    actions = new[]
                    {
                        new
                        {
                            action = model.ActionName,
                            parameters = model.ActionParameters
                        }
                    }
                });

                if (!result.Success)
                    throw new InvalidOperationException(BuildErrorMessage(result));

                model.ResultMessage = "Autorização remota enviada com sucesso.";
                model.ResultStatusType = "success";
                model.ResponseJson = string.IsNullOrWhiteSpace(result.ResponseBody)
                    ? "Operação concluída sem corpo de resposta."
                    : result.ResponseBody;
            }
            catch (Exception ex)
            {
                model.ResultMessage = SecurityTextHelper.BuildSafeUserMessage("Erro ao executar autorização remota", ex);
                model.ResultStatusType = "danger";
                _logger.LogError(ex, "Erro ao executar autorização remota.");
            }

            return View(model);
        }

        // Helper para nome de exibição de cada ação
        private string GetActionDisplayName(string action)
        {
            var found = AvailableActions.Find(a => a.Action == action);
            return found?.DisplayName ?? "Ação desconhecida";
        }

        private static string BuildErrorMessage(OfficialApiInvocationResult result)
        {
            if (!string.IsNullOrWhiteSpace(result.ErrorMessage))
                return result.ErrorMessage;

            if (!string.IsNullOrWhiteSpace(result.ResponseBody))
                return result.ResponseBody;

            return $"Falha HTTP {result.StatusCode}.";
        }
    }
}



