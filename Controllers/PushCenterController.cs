using System.Text;
using Integracao.ControlID.PoC.Helpers;
using Integracao.ControlID.PoC.Models.Database;
using Integracao.ControlID.PoC.Services.Callbacks;
using Integracao.ControlID.PoC.Services.Database;
using Integracao.ControlID.PoC.Services.Push;
using Integracao.ControlID.PoC.ViewModels.Push;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Logging;

namespace Integracao.ControlID.PoC.Controllers
{
    /// <summary>
    /// Centraliza a fila Push persistida, incluindo enfileiramento manual,
    /// polling pelo equipamento e armazenamento do resultado de execução.
    /// </summary>
    public class PushCenterController : Controller
    {
        private const int EventListLimit = LocalDataQueryLimits.DefaultListLimit;
        private readonly PushCommandWorkflowService _pushWorkflowService;
        private readonly CallbackSecurityEvaluator _securityEvaluator;
        private readonly CallbackRequestBodyReader _bodyReader;
        private readonly PushIdempotencyKeyResolver _idempotencyKeyResolver;
        private readonly ILogger<PushCenterController> _logger;

        public PushCenterController(
            PushCommandWorkflowService pushWorkflowService,
            CallbackSecurityEvaluator securityEvaluator,
            CallbackRequestBodyReader bodyReader,
            PushIdempotencyKeyResolver idempotencyKeyResolver,
            ILogger<PushCenterController> logger)
        {
            _pushWorkflowService = pushWorkflowService;
            _securityEvaluator = securityEvaluator;
            _bodyReader = bodyReader;
            _idempotencyKeyResolver = idempotencyKeyResolver;
            _logger = logger;
        }

        /// <summary>
        /// Renderiza a central Push com a fila persistida e o formulário de novo comando.
        /// </summary>
        /// <returns>View com histórico, mensagens operacionais e formulário de enfileiramento.</returns>
        public async Task<IActionResult> Index()
        {
            return View(new PushEventListViewModel
            {
                Events = (await _pushWorkflowService.GetRecentAsync(EventListLimit)).Select(ToViewModel).ToList(),
                TotalCount = await _pushWorkflowService.CountAsync(),
                DisplayLimit = EventListLimit,
                StatusMessage = TempData["StatusMessage"] as string ?? string.Empty,
                StatusType = TempData["StatusType"] as string ?? string.Empty
            });
        }

        /// <summary>
        /// Exibe os detalhes de um comando ou resultado Push persistido.
        /// </summary>
        /// <param name="id">Identificador local do comando Push.</param>
        /// <returns>View de detalhes ou NotFound quando o item não existe.</returns>
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                _logger.LogWarning("Push details requested without a command id.");
                return NotFound();
            }

            var command = await _pushWorkflowService.GetByIdAsync(id.Value);
            if (command == null)
            {
                _logger.LogWarning("Push details requested for missing command {CommandId}.", id.Value);
                return NotFound();
            }

            return View(ToViewModel(command));
        }

        /// <summary>
        /// Cria um comando Push pendente para ser consumido posteriormente pelo equipamento via GET /push.
        /// </summary>
        /// <param name="model">Dados do comando, dispositivo alvo e payload JSON.</param>
        /// <returns>Redirecionamento para a central com mensagem de sucesso ou erro de validação.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Queue([Bind(Prefix = nameof(PushEventListViewModel.QueueCommand))] PushQueueCommandViewModel model)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning(
                    "Rejected push queue request for device {DeviceId} and type {CommandType} because the model state is invalid.",
                    model.DeviceId,
                    model.CommandType);

                return View(nameof(Index), await BuildIndexViewModelAsync(model, "Revise os dados do comando antes de enfileirar."));
            }

            try
            {
                var queueResult = await _pushWorkflowService.QueueAsync(model);
                if (!queueResult.IsQueued)
                    return View(nameof(Index), await BuildIndexViewModelAsync(model, queueResult.ErrorMessage ?? "Informe um payload JSON valido antes de enfileirar."));

                TempData["StatusMessage"] = "Comando push enfileirado com sucesso.";
                TempData["StatusType"] = "success";
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Failed to queue push command for device {DeviceId}. Type {CommandType}.",
                    model.DeviceId,
                    model.CommandType);

                TempData["StatusMessage"] = "Nao foi possivel enfileirar o comando push.";
                TempData["StatusType"] = "danger";
            }

            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// Remove todos os comandos Push persistidos, preservando apenas o registro em log da operação manual.
        /// </summary>
        /// <returns>Redirecionamento para a central Push com o resultado da limpeza.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Clear(string confirmationPhrase)
        {
            if (!HighImpactOperationGuard.IsConfirmed(confirmationPhrase, HighImpactOperationGuard.ConfirmClearPushQueue))
            {
                TempData["StatusMessage"] = HighImpactOperationGuard.BuildRequiredMessage(HighImpactOperationGuard.ConfirmClearPushQueue);
                TempData["StatusType"] = "warning";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                await _pushWorkflowService.ClearAsync();
                TempData["StatusMessage"] = "Fila de push limpa com sucesso.";
                TempData["StatusType"] = "success";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to clear the push queue manually.");
                TempData["StatusMessage"] = "Nao foi possivel limpar a fila de push.";
                TempData["StatusType"] = "danger";
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Purge(int retentionDays, string confirmationPhrase)
        {
            if (!HighImpactOperationGuard.IsConfirmed(confirmationPhrase, HighImpactOperationGuard.ConfirmPurgePushQueue))
            {
                TempData["StatusMessage"] = HighImpactOperationGuard.BuildRequiredMessage(HighImpactOperationGuard.ConfirmPurgePushQueue);
                TempData["StatusType"] = "warning";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                var normalizedRetentionDays = LocalDataQueryLimits.NormalizeRetentionDays(retentionDays);
                var cutoffUtc = DateTime.UtcNow.AddDays(-normalizedRetentionDays);
                var removedCount = await _pushWorkflowService.PurgeOlderThanAsync(cutoffUtc);

                TempData["StatusMessage"] = $"Expurgo concluido. Retencao: {normalizedRetentionDays} dias. Registros removidos: {removedCount}.";
                TempData["StatusType"] = "success";
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to purge the push queue manually.");
                TempData["StatusMessage"] = "Nao foi possivel expurgar a fila de push.";
                TempData["StatusType"] = "danger";
            }

            return RedirectToAction(nameof(Index));
        }

        /// <summary>
        /// Endpoint consumido pelo equipamento para buscar o próximo comando pendente da fila.
        /// </summary>
        /// <param name="deviceId">Identificador preferencial do equipamento que está consultando a fila.</param>
        /// <param name="legacyDeviceId">Identificador legado aceito por compatibilidade.</param>
        /// <returns>Payload JSON do comando pendente ou objeto vazio quando não houver trabalho disponível.</returns>
        [HttpGet("/push")]
        [EnableRateLimiting("CallbackIngress")]
        public async Task<IActionResult> Poll([FromQuery(Name = "device_id")] string? deviceId, [FromQuery(Name = "deviceid")] string? legacyDeviceId)
        {
            var ingressRejection = ValidateIngressRequest();
            if (ingressRejection != null)
                return ingressRejection;

            var resolvedDeviceId = string.IsNullOrWhiteSpace(deviceId) ? legacyDeviceId : deviceId;
            try
            {
                var command = await _pushWorkflowService.DeliverNextAsync(resolvedDeviceId);
                if (command == null)
                    return Ok(new { });

                _logger.LogInformation(
                    "Push poll delivered command {CommandId} to device {DeviceId}. PayloadBytes {PayloadBytes}.",
                    command.CommandId,
                    resolvedDeviceId,
                    Encoding.UTF8.GetByteCount(command.Payload ?? string.Empty));

                return Content(string.IsNullOrWhiteSpace(command.Payload) ? "{}" : command.Payload, "application/json", Encoding.UTF8);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to deliver push command to device {DeviceId}.", resolvedDeviceId);
                return StatusCode(StatusCodes.Status500InternalServerError, new { error = "Nao foi possivel consultar a fila push." });
            }
        }

        /// <summary>
        /// Recebe o resultado de execução enviado pelo equipamento após consumir um comando Push.
        /// </summary>
        /// <param name="commandId">Identificador do comando entregue previamente, quando informado pelo equipamento.</param>
        /// <returns>OK quando o resultado foi persistido; erro 500 quando a persistência falha.</returns>
        [HttpPost("/result")]
        [EnableRateLimiting("CallbackIngress")]
        public async Task<IActionResult> Result([FromQuery(Name = "command_id")] Guid? commandId)
        {
            var ingressRejection = ValidateIngressRequest();
            if (ingressRejection != null)
                return ingressRejection;

            var bodyResult = await _bodyReader.ReadAsync(Request);
            if (!bodyResult.IsSuccessful)
            {
                _logger.LogWarning(
                    "Rejected push result body for {Path}. Status {StatusCode}. Reason: {Reason}",
                    Request.Path,
                    bodyResult.StatusCode,
                    bodyResult.Message);

                return StatusCode(bodyResult.StatusCode, new { error = bodyResult.Message });
            }

            var body = bodyResult.Body;

            try
            {
                var resolvedCommandId = commandId ?? _idempotencyKeyResolver.Resolve(Request);

                if (!commandId.HasValue && resolvedCommandId.HasValue)
                {
                    _logger.LogInformation(
                        "Push result received with idempotency key resolved to command {CommandId}. Device {DeviceId}.",
                        resolvedCommandId.Value,
                        Request.Query["device_id"].ToString());
                }

                if (!resolvedCommandId.HasValue)
                {
                    _logger.LogWarning(
                        "Push result received without command_id. Device {DeviceId}. BodyBytes {BodyBytes}.",
                        Request.Query["device_id"].ToString(),
                        Encoding.UTF8.GetByteCount(body));
                }

                var command = await _pushWorkflowService.StoreResultAsync(
                    resolvedCommandId,
                    body,
                    Request.Query["device_id"].ToString(),
                    Request.Query["user_id"].ToString(),
                    Request.Query["status"].ToString());

                _logger.LogInformation(
                    "Push result stored for command {CommandId}. Device {DeviceId}. Status {Status}. BodyBytes {BodyBytes}.",
                    command.CommandId,
                    command.DeviceId,
                    command.Status,
                    Encoding.UTF8.GetByteCount(body));

                return Ok();
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Failed to persist push result for command {CommandId}. Device {DeviceId}.",
                    commandId,
                    Request.Query["device_id"].ToString());

                return StatusCode(StatusCodes.Status500InternalServerError, new { error = "Nao foi possivel persistir o resultado push." });
            }
        }

        private IActionResult? ValidateIngressRequest()
        {
            var securityResult = _securityEvaluator.Evaluate(HttpContext);
            if (securityResult.IsAllowed)
                return null;

            _logger.LogWarning(
                "Blocked push ingress request for {Path}. Status {StatusCode}. Reason: {Reason}",
                Request.Path,
                securityResult.StatusCode,
                securityResult.Message);

            return StatusCode(securityResult.StatusCode, new { error = securityResult.Message });
        }

        private async Task<PushEventListViewModel> BuildIndexViewModelAsync(PushQueueCommandViewModel queueCommand, string errorMessage)
        {
            return new PushEventListViewModel
            {
                Events = (await _pushWorkflowService.GetRecentAsync(EventListLimit)).Select(ToViewModel).ToList(),
                TotalCount = await _pushWorkflowService.CountAsync(),
                DisplayLimit = EventListLimit,
                QueueCommand = queueCommand,
                ErrorMessage = errorMessage
            };
        }

        /// <summary>
        /// Converte a entidade persistida em ViewModel sem expor regra de banco para a camada Razor.
        /// </summary>
        /// <param name="command">Comando Push local carregado do SQLite.</param>
        /// <returns>ViewModel pronta para listagem ou detalhe.</returns>
        private static PushEventViewModel ToViewModel(PushCommandLocal command)
        {
            return new PushEventViewModel
            {
                EventId = command.CommandId,
                ReceivedAt = command.ReceivedAt,
                CommandType = command.CommandType,
                RawJson = command.RawJson,
                Status = command.Status,
                Payload = command.Payload,
                DeviceId = command.DeviceId,
                UserId = command.UserId
            };
        }
    }
}
