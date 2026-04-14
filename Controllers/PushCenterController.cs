using System.Text;
using Integracao.ControlID.PoC.Models.Database;
using Integracao.ControlID.PoC.Services.Database;
using Integracao.ControlID.PoC.ViewModels.Push;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Integracao.ControlID.PoC.Controllers
{
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

        public async Task<IActionResult> Index()
        {
            return View(new PushEventListViewModel
            {
                Events = (await _pushCommandRepository.GetAllPushCommandsAsync()).Select(ToViewModel).ToList(),
                StatusMessage = TempData["StatusMessage"] as string ?? string.Empty,
                StatusType = TempData["StatusType"] as string ?? string.Empty
            });
        }

        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var command = await _pushCommandRepository.GetPushCommandByIdAsync(id.Value);
            return command == null ? NotFound() : View(ToViewModel(command));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Queue(PushQueueCommandViewModel model)
        {
            if (!ModelState.IsValid)
            {
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

        [HttpGet("/push")]
        public async Task<IActionResult> Poll([FromQuery(Name = "device_id")] string? deviceId, [FromQuery(Name = "deviceid")] string? legacyDeviceId)
        {
            var resolvedDeviceId = string.IsNullOrWhiteSpace(deviceId) ? legacyDeviceId : deviceId;
            try
            {
                var command = await _pushCommandRepository.GetNextPendingCommandAsync(resolvedDeviceId);

                if (command == null)
                {
                    _logger.LogDebug("Push poll found no pending command for device {DeviceId}.", resolvedDeviceId);
                    return Ok(new { });
                }

                command.Status = "delivered";
                command.UpdatedAt = DateTime.UtcNow;
                await _pushCommandRepository.UpdatePushCommandAsync(command);

                _logger.LogInformation(
                    "Push poll delivered command {CommandId} to device {DeviceId}.",
                    command.CommandId,
                    resolvedDeviceId);

                return Content(string.IsNullOrWhiteSpace(command.Payload) ? "{}" : command.Payload, "application/json", Encoding.UTF8);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to deliver push command to device {DeviceId}.", resolvedDeviceId);
                return StatusCode(StatusCodes.Status500InternalServerError, new { error = "Nao foi possivel consultar a fila push." });
            }
        }

        [HttpPost("/result")]
        public async Task<IActionResult> Result([FromQuery(Name = "command_id")] Guid? commandId)
        {
            using var reader = new StreamReader(Request.Body);
            var body = await reader.ReadToEndAsync();

            try
            {
                PushCommandLocal? command = commandId.HasValue
                    ? await _pushCommandRepository.GetPushCommandByIdAsync(commandId.Value)
                    : null;

                if (command == null)
                {
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
                    await _pushCommandRepository.UpdatePushCommandAsync(command);
                }

                _logger.LogInformation(
                    "Push result stored for command {CommandId}. Device {DeviceId}. Status {Status}.",
                    command.CommandId,
                    command.DeviceId,
                    command.Status);

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
