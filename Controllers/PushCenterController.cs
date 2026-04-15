using System.Text;
using Integracao.ControlID.PoC.Models.Database;
using Integracao.ControlID.PoC.Services.Database;
using Integracao.ControlID.PoC.ViewModels.Push;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Integracao.ControlID.PoC.Controllers
{
    /// <summary>
    /// Centraliza a fila Push persistida, incluindo enfileiramento manual,
    /// polling pelo equipamento e armazenamento do resultado de execução.
    /// </summary>
    public class PushCenterController : Controller
    {
        private readonly PushCommandRepository _pushCommandRepository;
        private readonly ILogger<PushCenterController> _logger;

        public PushCenterController(
            PushCommandRepository pushCommandRepository,
            ILogger<PushCenterController> logger)
        {
            _pushCommandRepository = pushCommandRepository;
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
                Events = (await _pushCommandRepository.GetAllPushCommandsAsync()).Select(ToViewModel).ToList(),
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

            var command = await _pushCommandRepository.GetPushCommandByIdAsync(id.Value);
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
        public async Task<IActionResult> Queue(PushQueueCommandViewModel model)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning(
                    "Rejected push queue request for device {DeviceId} and type {CommandType} because the model state is invalid.",
                    model.DeviceId,
                    model.CommandType);

                return View(nameof(Index), new PushEventListViewModel
                {
                    Events = (await _pushCommandRepository.GetAllPushCommandsAsync()).Select(ToViewModel).ToList(),
                    QueueCommand = model,
                    ErrorMessage = "Revise os dados do comando antes de enfileirar."
                });
            }

            try
            {
                var command = new PushCommandLocal
                {
                    CommandId = Guid.NewGuid(),
                    CommandType = model.CommandType,
                    DeviceId = model.DeviceId,
                    UserId = model.UserId,
                    Payload = model.Payload,
                    RawJson = model.Payload,
                    Status = "pending",
                    CreatedAt = DateTime.UtcNow
                };

                await _pushCommandRepository.AddPushCommandAsync(command);

                _logger.LogInformation(
                    "Push command {CommandId} queued for device {DeviceId} with type {CommandType}.",
                    command.CommandId,
                    command.DeviceId,
                    command.CommandType);

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
        public async Task<IActionResult> Clear()
        {
            try
            {
                var commands = await _pushCommandRepository.GetAllPushCommandsAsync();
                foreach (var command in commands)
                {
                    await _pushCommandRepository.DeletePushCommandAsync(command.CommandId);
                }

                _logger.LogWarning("Push queue cleared manually. Removed {Count} commands.", commands.Count());

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

        /// <summary>
        /// Endpoint consumido pelo equipamento para buscar o próximo comando pendente da fila.
        /// </summary>
        /// <param name="deviceId">Identificador preferencial do equipamento que está consultando a fila.</param>
        /// <param name="legacyDeviceId">Identificador legado aceito por compatibilidade.</param>
        /// <returns>Payload JSON do comando pendente ou objeto vazio quando não houver trabalho disponível.</returns>
        [HttpGet("/push")]
        public async Task<IActionResult> Poll([FromQuery(Name = "device_id")] string? deviceId, [FromQuery(Name = "deviceid")] string? legacyDeviceId)
        {
            var resolvedDeviceId = string.IsNullOrWhiteSpace(deviceId) ? legacyDeviceId : deviceId;
            try
            {
                _logger.LogDebug("Push poll started for device {DeviceId}.", resolvedDeviceId);

                var command = await _pushCommandRepository.GetNextPendingCommandAsync(resolvedDeviceId);

                if (command == null)
                {
                    _logger.LogDebug("Push poll found no pending command for device {DeviceId}.", resolvedDeviceId);
                    return Ok(new { });
                }

                command.Status = "delivered";
                command.UpdatedAt = DateTime.UtcNow;
                var persisted = await _pushCommandRepository.UpdatePushCommandAsync(command);
                if (!persisted)
                {
                    _logger.LogError(
                        "Push command {CommandId} was selected for delivery to device {DeviceId}, but status persistence failed.",
                        command.CommandId,
                        resolvedDeviceId);
                }

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
        public async Task<IActionResult> Result([FromQuery(Name = "command_id")] Guid? commandId)
        {
            using var reader = new StreamReader(Request.Body);
            var body = await reader.ReadToEndAsync();

            try
            {
                if (!commandId.HasValue)
                {
                    _logger.LogWarning(
                        "Push result received without command_id. Device {DeviceId}. BodyBytes {BodyBytes}.",
                        Request.Query["device_id"].ToString(),
                        Encoding.UTF8.GetByteCount(body));
                }

                PushCommandLocal? command = commandId.HasValue
                    ? await _pushCommandRepository.GetPushCommandByIdAsync(commandId.Value)
                    : null;

                if (command == null)
                {
                    if (commandId.HasValue)
                    {
                        _logger.LogWarning(
                            "Push result received for unknown command {CommandId}. A standalone result record will be created.",
                            commandId.Value);
                    }

                    command = new PushCommandLocal
                    {
                        CommandId = commandId ?? Guid.NewGuid(),
                        CommandType = "result",
                        DeviceId = Request.Query["device_id"].ToString(),
                        UserId = Request.Query["user_id"].ToString(),
                        Payload = body,
                        RawJson = body,
                        Status = string.IsNullOrWhiteSpace(Request.Query["status"]) ? "completed" : Request.Query["status"].ToString(),
                        CreatedAt = DateTime.UtcNow
                    };

                    await _pushCommandRepository.AddPushCommandAsync(command);
                }
                else
                {
                    command.RawJson = body;
                    command.Payload = body;
                    command.Status = string.IsNullOrWhiteSpace(Request.Query["status"]) ? "completed" : Request.Query["status"].ToString();
                    command.UpdatedAt = DateTime.UtcNow;
                    var persisted = await _pushCommandRepository.UpdatePushCommandAsync(command);
                    if (!persisted)
                    {
                        _logger.LogError(
                            "Push result for command {CommandId} could not persist status {Status}.",
                            command.CommandId,
                            command.Status);
                    }
                }

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
