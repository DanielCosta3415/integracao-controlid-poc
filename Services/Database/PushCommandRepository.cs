using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Integracao.ControlID.PoC.Models.Database;
using Integracao.ControlID.PoC.Data; // Contexto correto
using Integracao.ControlID.PoC.Services.Push;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Integracao.ControlID.PoC.Services.Database
{
    public class PushCommandRepository
    {
        private readonly IntegracaoControlIDContext _dbContext;
        private readonly ILogger<PushCommandRepository> _logger;

        public PushCommandRepository(IntegracaoControlIDContext dbContext, ILogger<PushCommandRepository> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        /// <summary>
        /// Adiciona um novo comando Push local.
        /// </summary>
        public async Task<PushCommandLocal> AddPushCommandAsync(PushCommandLocal pushCommand)
        {
            try
            {
                pushCommand.ReceivedAt = DateTime.UtcNow;
                _dbContext.PushCommands.Add(pushCommand);
                await _dbContext.SaveChangesAsync();
                return pushCommand;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao adicionar comando Push local.");
                throw;
            }
        }

        /// <summary>
        /// Busca comando Push local pelo Id.
        /// </summary>
        public async Task<PushCommandLocal?> GetPushCommandByIdAsync(Guid id)
        {
            return await _dbContext.PushCommands.FirstOrDefaultAsync(c => c.CommandId == id);
        }

        /// <summary>
        /// Busca todos os comandos Push locais.
        /// </summary>
        public async Task<List<PushCommandLocal>> GetAllPushCommandsAsync()
        {
            return await GetRecentPushCommandsAsync();
        }

        public async Task<List<PushCommandLocal>> GetRecentPushCommandsAsync(int? limit = null)
        {
            var normalizedLimit = LocalDataQueryLimits.NormalizeLimit(limit);

            return await _dbContext.PushCommands
                .AsNoTracking()
                .OrderByDescending(c => c.ReceivedAt)
                .Take(normalizedLimit)
                .ToListAsync();
        }

        public async Task<int> CountPushCommandsAsync()
        {
            return await _dbContext.PushCommands.CountAsync();
        }

        public async Task<int> CountPendingPushCommandsAsync()
        {
            return await _dbContext.PushCommands.CountAsync(command => command.Status == PushCommandStatuses.Pending);
        }

        /// <summary>
        /// Busca o proximo comando pendente elegivel para um equipamento especifico.
        /// </summary>
        /// <param name="deviceId">Identificador do equipamento que esta consultando a fila; quando vazio, busca comandos globais.</param>
        /// <returns>Comando mais antigo com status `pending`, ou null quando nao houver comando disponivel.</returns>
        public async Task<PushCommandLocal?> GetNextPendingCommandAsync(string? deviceId)
        {
            var query = _dbContext.PushCommands
                .Where(command => command.Status == PushCommandStatuses.Pending);

            if (!string.IsNullOrWhiteSpace(deviceId))
            {
                query = query.Where(command => command.DeviceId == deviceId || string.IsNullOrEmpty(command.DeviceId));
            }

            return await query
                .OrderBy(command => command.CreatedAt)
                .FirstOrDefaultAsync();
        }

        public async Task<PushCommandLocal?> ClaimNextPendingCommandForDeliveryAsync(string? deviceId)
        {
            while (true)
            {
                var query = _dbContext.PushCommands
                    .AsNoTracking()
                    .Where(command => command.Status == PushCommandStatuses.Pending);

                if (!string.IsNullOrWhiteSpace(deviceId))
                {
                    query = query.Where(command => command.DeviceId == deviceId || string.IsNullOrEmpty(command.DeviceId));
                }

                var candidateId = await query
                    .OrderBy(command => command.CreatedAt)
                    .Select(command => (Guid?)command.CommandId)
                    .FirstOrDefaultAsync();

                if (!candidateId.HasValue)
                    return null;

                var now = DateTime.UtcNow;
                var updatedRows = await _dbContext.PushCommands
                    .Where(command => command.CommandId == candidateId.Value && command.Status == PushCommandStatuses.Pending)
                    .ExecuteUpdateAsync(setters => setters
                        .SetProperty(command => command.Status, PushCommandStatuses.Delivered)
                        .SetProperty(command => command.UpdatedAt, now));

                if (updatedRows == 1)
                {
                    var trackedEntry = _dbContext.ChangeTracker
                        .Entries<PushCommandLocal>()
                        .FirstOrDefault(entry => entry.Entity.CommandId == candidateId.Value);

                    if (trackedEntry != null)
                    {
                        trackedEntry.State = EntityState.Detached;
                    }

                    return await GetPushCommandByIdAsync(candidateId.Value);
                }
            }
        }

        /// <summary>
        /// Atualiza dados de um comando Push local.
        /// </summary>
        public async Task<bool> UpdatePushCommandAsync(PushCommandLocal pushCommand)
        {
            try
            {
                _dbContext.PushCommands.Update(pushCommand);
                await _dbContext.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao atualizar comando Push local {CommandId}.", pushCommand.CommandId);
                return false;
            }
        }

        /// <summary>
        /// Remove comando Push local pelo Id.
        /// </summary>
        public async Task<bool> DeletePushCommandAsync(Guid id)
        {
            try
            {
                var cmd = await _dbContext.PushCommands.FirstOrDefaultAsync(c => c.CommandId == id);
                if (cmd == null)
                    return false;

                _dbContext.PushCommands.Remove(cmd);
                await _dbContext.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Erro ao remover comando Push local {id}.");
                return false;
            }
        }

        public async Task<int> DeleteAllPushCommandsAsync()
        {
            return await _dbContext.PushCommands.ExecuteDeleteAsync();
        }

        public async Task<int> DeletePushCommandsOlderThanAsync(DateTime cutoffUtc)
        {
            return await _dbContext.PushCommands
                .Where(item => item.ReceivedAt < cutoffUtc)
                .ExecuteDeleteAsync();
        }

        /// <summary>
        /// Busca comandos Push locais por tipo, status, usuÃ¡rio ou perÃ­odo.
        /// </summary>
        public async Task<List<PushCommandLocal>> SearchPushCommandsAsync(
            string? commandType = null,
            string? status = null,
            long? userId = null,
            DateTime? startDate = null,
            DateTime? endDate = null)
        {
            IQueryable<PushCommandLocal> query = _dbContext.PushCommands.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(commandType))
                query = query.Where(c => c.CommandType == commandType);

            if (!string.IsNullOrWhiteSpace(status))
                query = query.Where(c => c.Status == status);

            if (userId.HasValue)
                query = query.Where(c => c.UserId == userId.Value.ToString());

            if (startDate.HasValue)
                query = query.Where(c => c.ReceivedAt >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(c => c.ReceivedAt <= endDate.Value);

            return await query
                .OrderByDescending(c => c.ReceivedAt)
                .Take(LocalDataQueryLimits.DefaultListLimit)
                .ToListAsync();
        }
    }
}
