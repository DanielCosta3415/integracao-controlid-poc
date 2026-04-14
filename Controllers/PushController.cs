using System;
using System.Text.Json;
using System.Threading.Tasks;
using Integracao.ControlID.PoC.Models.Database;
using Integracao.ControlID.PoC.Services.Database;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Integracao.ControlID.PoC.Controllers
{
    public class PushController : Controller
    {
        private readonly ILogger<PushController> _logger;
        private readonly PushCommandRepository _pushCommandRepository;

        public PushController(ILogger<PushController> logger, PushCommandRepository pushCommandRepository)
        {
            _logger = logger;
            _pushCommandRepository = pushCommandRepository;
        }

        // GET: /Push
        public IActionResult Index()
        {
            return RedirectToAction(nameof(PushCenterController.Index), "PushCenter");
        }

        // GET: /Push/Details/{guid}
        public IActionResult Details(Guid? id)
        {
            return id == null
                ? NotFound()
                : RedirectToAction(nameof(PushCenterController.Details), "PushCenter", new { id });
        }

        // POST: /Push/Receive
        [HttpPost]
        [Route("Push/Receive")]
        public async Task<IActionResult> Receive()
        {
            string? body = null;
            try
            {
                using var reader = new System.IO.StreamReader(Request.Body);
                body = await reader.ReadToEndAsync();

                // Mantem um limite razoavel para proteger a PoC de cargas acidentais excessivas.
                if (body.Length > 1024 * 1024)
                    return BadRequest("Payload muito grande.");

                var command = BuildPushCommand(body);
                await _pushCommandRepository.AddPushCommandAsync(command);

                _logger.LogInformation("Evento Push legado recebido em {Time}: {Summary}",
                    command.ReceivedAt, Truncate(body, 500));

                return Ok(new { status = "received", eventId = command.CommandId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Falha ao processar evento Push legado. Body (trunc): {Body}", Truncate(body, 500));
                return StatusCode(500, "Erro ao processar evento Push");
            }
        }

        // POST: /Push/Clear
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Clear()
        {
            var commands = await _pushCommandRepository.GetAllPushCommandsAsync();
            foreach (var command in commands)
                await _pushCommandRepository.DeletePushCommandAsync(command.CommandId);

            TempData["StatusMessage"] = "Eventos Push limpos com sucesso.";
            TempData["StatusType"] = "success";
            return RedirectToAction(nameof(PushCenterController.Index), "PushCenter");
        }

        /// <summary>
        /// Trunca texto longo para logs.
        /// </summary>
        private static string Truncate(string? value, int maxLength)
        {
            if (string.IsNullOrEmpty(value)) return string.Empty;
            return value.Length <= maxLength ? value : value.Substring(0, maxLength) + "...";
        }

        private PushCommandLocal BuildPushCommand(string body)
        {
            var command = new PushCommandLocal
            {
                CommandId = Guid.NewGuid(),
                ReceivedAt = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
                RawJson = body,
                Payload = body,
                CommandType = "legacy_push_event",
                Status = "received",
                DeviceId = string.Empty,
                UserId = string.Empty
            };

            try
            {
                using var document = JsonDocument.Parse(body);
                var root = document.RootElement;

                command.CommandType =
                    TryGetJsonString(root, "command_type") ??
                    TryGetJsonString(root, "type") ??
                    TryGetJsonString(root, "event") ??
                    command.CommandType;
                command.Status = TryGetJsonString(root, "status") ?? command.Status;
                command.DeviceId =
                    TryGetJsonString(root, "device_id") ??
                    TryGetJsonString(root, "deviceid") ??
                    string.Empty;
                command.UserId =
                    TryGetJsonString(root, "user_id") ??
                    TryGetJsonString(root, "userid") ??
                    string.Empty;
                command.Payload =
                    TryGetJsonString(root, "payload") ??
                    TryGetJsonString(root, "data") ??
                    body;
            }
            catch (JsonException je)
            {
                _logger.LogWarning(je, "Erro ao desserializar JSON do evento Push legado.");
            }

            return command;
        }

        private static string? TryGetJsonString(JsonElement root, string propertyName)
        {
            if (!root.TryGetProperty(propertyName, out var property))
                return null;

            return property.ValueKind == JsonValueKind.String
                ? property.GetString()
                : property.GetRawText();
        }
    }
}
