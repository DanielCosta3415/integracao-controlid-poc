using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Integracao.ControlID.PoC.Models.Database;
using Integracao.ControlID.PoC.Data; // Contexto correto
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Integracao.ControlID.PoC.Services.Database
{
    public class SyncRepository
    {
        private readonly IntegracaoControlIDContext _dbContext;
        private readonly ILogger<SyncRepository> _logger;

        public SyncRepository(IntegracaoControlIDContext dbContext, ILogger<SyncRepository> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        /// <summary>
        /// Adiciona um novo registro de sincronização local.
        /// </summary>
        public async Task<SyncLocal> AddSyncAsync(SyncLocal sync)
        {
            try
            {
                sync.StartedAt = DateTime.UtcNow;
                _dbContext.Syncs.Add(sync);
                await _dbContext.SaveChangesAsync();
                return sync;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao adicionar registro de sincronização local.");
                throw;
            }
        }

        /// <summary>
        /// Busca registro de sincronização local pelo Id.
        /// </summary>
        public async Task<SyncLocal?> GetSyncByIdAsync(long id)
        {
            return await _dbContext.Syncs.FindAsync(id);
        }

        /// <summary>
        /// Busca todos os registros de sincronização locais.
        /// </summary>
        public async Task<List<SyncLocal>> GetAllSyncsAsync()
        {
            return await _dbContext.Syncs.OrderByDescending(s => s.StartedAt).ToListAsync();
        }

        /// <summary>
        /// Atualiza dados de um registro de sincronização local.
        /// </summary>
        public async Task<bool> UpdateSyncAsync(SyncLocal sync)
        {
            try
            {
                _dbContext.Syncs.Update(sync);
                await _dbContext.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Erro ao atualizar registro de sincronização local {sync.Id}.");
                return false;
            }
        }

        /// <summary>
        /// Remove registro de sincronização local pelo Id.
        /// </summary>
        public async Task<bool> DeleteSyncAsync(long id)
        {
            try
            {
                var sync = await _dbContext.Syncs.FindAsync(id);
                if (sync == null)
                    return false;

                _dbContext.Syncs.Remove(sync);
                await _dbContext.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Erro ao remover registro de sincronização local {id}.");
                return false;
            }
        }

        /// <summary>
        /// Busca registros de sincronização locais por tipo, status ou período.
        /// </summary>
        public async Task<List<SyncLocal>> SearchSyncsAsync(
            string? syncType = null,
            string? status = null,
            DateTime? startDate = null,
            DateTime? endDate = null)
        {
            IQueryable<SyncLocal> query = _dbContext.Syncs;

            if (!string.IsNullOrWhiteSpace(syncType))
                query = query.Where(s => s.SyncType == syncType);

            if (!string.IsNullOrWhiteSpace(status))
                query = query.Where(s => s.Status == status);

            if (startDate.HasValue)
                query = query.Where(s => s.StartedAt >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(s => s.StartedAt <= endDate.Value);

            return await query.OrderByDescending(s => s.StartedAt).ToListAsync();
        }
    }
}