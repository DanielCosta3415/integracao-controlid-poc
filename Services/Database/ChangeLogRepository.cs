using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Integracao.ControlID.PoC.Models.Database;
using Integracao.ControlID.PoC.Data; // Usando o contexto correto
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Integracao.ControlID.PoC.Services.Database
{
    public class ChangeLogRepository
    {
        private readonly IntegracaoControlIDContext _dbContext;
        private readonly ILogger<ChangeLogRepository> _logger;

        public ChangeLogRepository(IntegracaoControlIDContext dbContext, ILogger<ChangeLogRepository> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        /// <summary>
        /// Adiciona um novo log de alteração local.
        /// </summary>
        public async Task<ChangeLogLocal> AddChangeLogAsync(ChangeLogLocal log)
        {
            try
            {
                log.Timestamp = DateTime.UtcNow;
                _dbContext.ChangeLogs.Add(log);
                await _dbContext.SaveChangesAsync();
                return log;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao adicionar log de alteração local.");
                throw;
            }
        }

        /// <summary>
        /// Busca log de alteração local pelo Id.
        /// </summary>
        public async Task<ChangeLogLocal?> GetChangeLogByIdAsync(long id)
        {
            return await _dbContext.ChangeLogs.FindAsync(id);
        }

        /// <summary>
        /// Busca todos os logs de alteração locais.
        /// </summary>
        public async Task<List<ChangeLogLocal>> GetAllChangeLogsAsync()
        {
            return await _dbContext.ChangeLogs.OrderByDescending(l => l.Timestamp).ToListAsync();
        }

        /// <summary>
        /// Atualiza dados de um log de alteração local.
        /// </summary>
        public async Task<bool> UpdateChangeLogAsync(ChangeLogLocal log)
        {
            try
            {
                _dbContext.ChangeLogs.Update(log);
                await _dbContext.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Erro ao atualizar log de alteração local {log.Id}.");
                return false;
            }
        }

        /// <summary>
        /// Remove log de alteração local pelo Id.
        /// </summary>
        public async Task<bool> DeleteChangeLogAsync(long id)
        {
            try
            {
                var log = await _dbContext.ChangeLogs.FindAsync(id);
                if (log == null)
                    return false;

                _dbContext.ChangeLogs.Remove(log);
                await _dbContext.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Erro ao remover log de alteração local {id}.");
                return false;
            }
        }

        /// <summary>
        /// Busca logs de alteração locais por operação, tabela, usuário ou período.
        /// </summary>
        public async Task<List<ChangeLogLocal>> SearchChangeLogsAsync(
            string? operationType = null,
            string? tableName = null,
            string? performedBy = null,
            DateTime? startDate = null,
            DateTime? endDate = null)
        {
            IQueryable<ChangeLogLocal> query = _dbContext.ChangeLogs;

            if (!string.IsNullOrWhiteSpace(operationType))
                query = query.Where(l => l.OperationType == operationType);

            if (!string.IsNullOrWhiteSpace(tableName))
                query = query.Where(l => l.TableName == tableName);

            if (!string.IsNullOrWhiteSpace(performedBy))
                query = query.Where(l => l.PerformedBy == performedBy);

            if (startDate.HasValue)
                query = query.Where(l => l.Timestamp >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(l => l.Timestamp <= endDate.Value);

            return await query.OrderByDescending(l => l.Timestamp).ToListAsync();
        }
    }
}
