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
    public class MonitorEventRepository
    {
        private readonly IntegracaoControlIDContext _dbContext;
        private readonly ILogger<MonitorEventRepository> _logger;

        public MonitorEventRepository(IntegracaoControlIDContext dbContext, ILogger<MonitorEventRepository> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        /// <summary>
        /// Adiciona um novo evento monitorado local.
        /// </summary>
        public async Task<MonitorEventLocal> AddMonitorEventAsync(MonitorEventLocal monitorEvent)
        {
            try
            {
                monitorEvent.ReceivedAt = DateTime.UtcNow;
                _dbContext.MonitorEvents.Add(monitorEvent);
                await _dbContext.SaveChangesAsync();
                return monitorEvent;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao adicionar evento monitorado local.");
                throw;
            }
        }

        /// <summary>
        /// Busca evento monitorado local pelo Id.
        /// </summary>
        public async Task<MonitorEventLocal?> GetMonitorEventByIdAsync(Guid id)
        {
            return await _dbContext.MonitorEvents.FirstOrDefaultAsync(e => e.EventId == id);
        }

        /// <summary>
        /// Busca todos os eventos monitorados locais.
        /// </summary>
        public async Task<List<MonitorEventLocal>> GetAllMonitorEventsAsync()
        {
            return await GetRecentMonitorEventsAsync();
        }

        public async Task<List<MonitorEventLocal>> GetRecentMonitorEventsAsync(int? limit = null)
        {
            var normalizedLimit = LocalDataQueryLimits.NormalizeLimit(limit);

            return await _dbContext.MonitorEvents
                .AsNoTracking()
                .OrderByDescending(e => e.ReceivedAt)
                .Take(normalizedLimit)
                .ToListAsync();
        }

        public async Task<int> CountMonitorEventsAsync()
        {
            return await _dbContext.MonitorEvents.CountAsync();
        }

        /// <summary>
        /// Atualiza dados de um evento monitorado local.
        /// </summary>
        public async Task<bool> UpdateMonitorEventAsync(MonitorEventLocal monitorEvent)
        {
            try
            {
                _dbContext.MonitorEvents.Update(monitorEvent);
                await _dbContext.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao atualizar evento monitorado local {EventId}.", monitorEvent.EventId);
                return false;
            }
        }

        /// <summary>
        /// Remove evento monitorado local pelo Id.
        /// </summary>
        public async Task<bool> DeleteMonitorEventAsync(Guid id)
        {
            try
            {
                var evt = await _dbContext.MonitorEvents.FirstOrDefaultAsync(e => e.EventId == id);
                if (evt == null)
                    return false;

                _dbContext.MonitorEvents.Remove(evt);
                await _dbContext.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Erro ao remover evento monitorado local {id}.");
                return false;
            }
        }

        public async Task<int> DeleteAllMonitorEventsAsync()
        {
            return await _dbContext.MonitorEvents.ExecuteDeleteAsync();
        }

        public async Task<int> DeleteMonitorEventsOlderThanAsync(DateTime cutoffUtc)
        {
            return await _dbContext.MonitorEvents
                .Where(item => item.ReceivedAt < cutoffUtc)
                .ExecuteDeleteAsync();
        }

        /// <summary>
        /// Busca eventos monitorados locais por tipo, status ou período.
        /// </summary>
        public async Task<List<MonitorEventLocal>> SearchMonitorEventsAsync(
            string? eventType = null,
            string? status = null,
            DateTime? startDate = null,
            DateTime? endDate = null)
        {
            IQueryable<MonitorEventLocal> query = _dbContext.MonitorEvents.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(eventType))
                query = query.Where(e => e.EventType == eventType);

            if (!string.IsNullOrWhiteSpace(status))
                query = query.Where(e => e.Status == status);

            if (startDate.HasValue)
                query = query.Where(e => e.ReceivedAt >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(e => e.ReceivedAt <= endDate.Value);

            return await query
                .OrderByDescending(e => e.ReceivedAt)
                .Take(LocalDataQueryLimits.DefaultListLimit)
                .ToListAsync();
        }
    }
}
