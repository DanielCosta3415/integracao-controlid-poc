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
    public class LogRepository
    {
        private readonly IntegracaoControlIDContext _dbContext;
        private readonly ILogger<LogRepository> _logger;

        public LogRepository(IntegracaoControlIDContext dbContext, ILogger<LogRepository> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        /// <summary>
        /// Adiciona um novo log local.
        /// </summary>
        public async Task<LogLocal> AddLogAsync(LogLocal log)
        {
            try
            {
                log.CreatedAt = DateTime.UtcNow;
                _dbContext.Logs.Add(log);
                await _dbContext.SaveChangesAsync();
                return log;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao adicionar log local.");
                throw;
            }
        }

        /// <summary>
        /// Busca log local pelo Id.
        /// </summary>
        public async Task<LogLocal?> GetLogByIdAsync(long id)
        {
            return await _dbContext.Logs.FindAsync(id);
        }

        /// <summary>
        /// Busca todos os logs locais.
        /// </summary>
        public async Task<List<LogLocal>> GetAllLogsAsync()
        {
            return await _dbContext.Logs.OrderByDescending(l => l.CreatedAt).ToListAsync();
        }

        /// <summary>
        /// Atualiza dados de um log local.
        /// </summary>
        public async Task<bool> UpdateLogAsync(LogLocal log)
        {
            try
            {
                _dbContext.Logs.Update(log);
                await _dbContext.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Erro ao atualizar log local {log.Id}.");
                return false;
            }
        }

        /// <summary>
        /// Remove log local pelo Id.
        /// </summary>
        public async Task<bool> DeleteLogAsync(long id)
        {
            try
            {
                var log = await _dbContext.Logs.FindAsync(id);
                if (log == null)
                    return false;

                _dbContext.Logs.Remove(log);
                await _dbContext.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Erro ao remover log local {id}.");
                return false;
            }
        }

        /// <summary>
        /// Busca logs locais por nível, origem ou período.
        /// </summary>
        public async Task<List<LogLocal>> SearchLogsAsync(
            string? level = null,
            string? source = null,
            DateTime? startDate = null,
            DateTime? endDate = null)
        {
            IQueryable<LogLocal> query = _dbContext.Logs;

            if (!string.IsNullOrWhiteSpace(level))
                query = query.Where(l => l.Level == level);

            if (!string.IsNullOrWhiteSpace(source))
                query = query.Where(l => l.Source == source);

            if (startDate.HasValue)
                query = query.Where(l => l.CreatedAt >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(l => l.CreatedAt <= endDate.Value);

            return await query.OrderByDescending(l => l.CreatedAt).ToListAsync();
        }
    }
}