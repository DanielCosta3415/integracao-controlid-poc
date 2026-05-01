using System.Text.Json;
using Integracao.ControlID.PoC.Helpers;
using Integracao.ControlID.PoC.Models.Database;
using Integracao.ControlID.PoC.Services.Database;
using Integracao.ControlID.PoC.ViewModels.Push;

namespace Integracao.ControlID.PoC.Services.Push;

public sealed class PushCommandWorkflowService
{
    private readonly PushCommandRepository _pushCommandRepository;
    private readonly ILogger<PushCommandWorkflowService> _logger;

    public PushCommandWorkflowService(
        PushCommandRepository pushCommandRepository,
        ILogger<PushCommandWorkflowService> logger)
    {
        _pushCommandRepository = pushCommandRepository;
        _logger = logger;
    }

    public async Task<List<PushCommandLocal>> GetAllAsync()
    {
        return await _pushCommandRepository.GetAllPushCommandsAsync();
    }

    public async Task<List<PushCommandLocal>> GetRecentAsync(int? limit = null)
    {
        return await _pushCommandRepository.GetRecentPushCommandsAsync(limit);
    }

    public async Task<int> CountAsync()
    {
        return await _pushCommandRepository.CountPushCommandsAsync();
    }

    public async Task<PushCommandLocal?> GetByIdAsync(Guid commandId)
    {
        return await _pushCommandRepository.GetPushCommandByIdAsync(commandId);
    }

    public async Task<PushQueueResult> QueueAsync(PushQueueCommandViewModel model)
    {
        if (!IsValidJsonPayload(model.Payload))
        {
            _logger.LogWarning(
                "Rejected push queue request for device {DeviceRef} and type {CommandType} because the payload is not valid JSON.",
                PrivacyLogHelper.PseudonymizeIdentifier(model.DeviceId),
                model.CommandType);

            return PushQueueResult.Invalid("Informe um payload JSON valido antes de enfileirar.");
        }

        var command = new PushCommandLocal
        {
            CommandId = Guid.NewGuid(),
            CommandType = model.CommandType,
            DeviceId = model.DeviceId,
            UserId = model.UserId,
            Payload = model.Payload,
            RawJson = model.Payload,
            Status = PushCommandStatuses.Pending,
            CreatedAt = DateTime.UtcNow
        };

        await _pushCommandRepository.AddPushCommandAsync(command);

        _logger.LogInformation(
            "Push command {CommandId} queued for device {DeviceRef} with type {CommandType}.",
            command.CommandId,
            PrivacyLogHelper.PseudonymizeIdentifier(command.DeviceId),
            command.CommandType);

        return PushQueueResult.Queued(command);
    }

    public async Task<int> ClearAsync()
    {
        var removedCount = await _pushCommandRepository.DeleteAllPushCommandsAsync();

        _logger.LogWarning("Push queue cleared manually. Removed {Count} commands.", removedCount);
        return removedCount;
    }

    public async Task<int> PurgeOlderThanAsync(DateTime cutoffUtc)
    {
        var removedCount = await _pushCommandRepository.DeletePushCommandsOlderThanAsync(cutoffUtc);

        _logger.LogWarning(
            "Push queue retention purge removed {Count} commands older than {CutoffUtc}.",
            removedCount,
            cutoffUtc);

        return removedCount;
    }

    public async Task<PushCommandLocal?> DeliverNextAsync(string? deviceId)
    {
        _logger.LogDebug("Push poll started for device {DeviceRef}.", PrivacyLogHelper.PseudonymizeIdentifier(deviceId));

        var command = await _pushCommandRepository.GetNextPendingCommandAsync(deviceId);
        if (command == null)
        {
            _logger.LogDebug("Push poll found no pending command for device {DeviceRef}.", PrivacyLogHelper.PseudonymizeIdentifier(deviceId));
            return null;
        }

        command.Status = PushCommandStatuses.Delivered;
        command.UpdatedAt = DateTime.UtcNow;
        var persisted = await _pushCommandRepository.UpdatePushCommandAsync(command);
        if (!persisted)
        {
            _logger.LogError(
                "Push command {CommandId} was selected for delivery to device {DeviceRef}, but status persistence failed.",
                command.CommandId,
                PrivacyLogHelper.PseudonymizeIdentifier(deviceId));
        }

        return command;
    }

    public async Task<PushCommandLocal> StoreResultAsync(
        Guid? commandId,
        string body,
        string deviceId,
        string userId,
        string? status)
    {
        var resolvedStatus = string.IsNullOrWhiteSpace(status) ? PushCommandStatuses.Completed : status;
        var command = commandId.HasValue
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
                DeviceId = deviceId,
                UserId = userId,
                Payload = body,
                RawJson = body,
                Status = resolvedStatus,
                CreatedAt = DateTime.UtcNow
            };

            await _pushCommandRepository.AddPushCommandAsync(command);
            return command;
        }

        command.RawJson = body;
        command.Payload = body;
        command.Status = resolvedStatus;
        command.UpdatedAt = DateTime.UtcNow;
        var persisted = await _pushCommandRepository.UpdatePushCommandAsync(command);
        if (!persisted)
        {
            _logger.LogError(
                "Push result for command {CommandId} could not persist status {Status}.",
                command.CommandId,
                command.Status);
        }

        return command;
    }

    public async Task<PushCommandLocal> StoreLegacyEventAsync(string body, Guid? commandId = null)
    {
        var command = BuildLegacyCommand(body, commandId);

        if (commandId.HasValue)
        {
            var existing = await _pushCommandRepository.GetPushCommandByIdAsync(commandId.Value);
            if (existing != null)
            {
                existing.ReceivedAt = DateTime.UtcNow;
                existing.RawJson = command.RawJson;
                existing.Payload = command.Payload;
                existing.CommandType = command.CommandType;
                existing.Status = command.Status;
                existing.DeviceId = command.DeviceId;
                existing.UserId = command.UserId;
                existing.UpdatedAt = DateTime.UtcNow;

                await _pushCommandRepository.UpdatePushCommandAsync(existing);
                return existing;
            }
        }

        await _pushCommandRepository.AddPushCommandAsync(command);
        return command;
    }

    public static bool IsValidJsonPayload(string payload)
    {
        if (string.IsNullOrWhiteSpace(payload))
            return false;

        try
        {
            using var document = JsonDocument.Parse(payload);
            return true;
        }
        catch (JsonException)
        {
            return false;
        }
    }

    private PushCommandLocal BuildLegacyCommand(string body, Guid? commandId)
    {
        var command = new PushCommandLocal
        {
            CommandId = commandId ?? Guid.NewGuid(),
            ReceivedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow,
            RawJson = body,
            Payload = body,
            CommandType = "legacy_push_event",
            Status = PushCommandStatuses.Received,
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

public sealed record PushQueueResult(bool IsQueued, string? ErrorMessage, PushCommandLocal? Command)
{
    public static PushQueueResult Queued(PushCommandLocal command) => new(true, null, command);

    public static PushQueueResult Invalid(string errorMessage) => new(false, errorMessage, null);
}
