using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Integracao.ControlID.PoC.Models.Database;
using Integracao.ControlID.PoC.Data; // Adicionado para o contexto correto
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Integracao.ControlID.PoC.Services.Database
{
    public class AccessLogRepository
    {
        private readonly IntegracaoControlIDContext _dbContext;
        private readonly ILogger<AccessLogRepository> _logger;

        public AccessLogRepository(IntegracaoControlIDContext dbContext, ILogger<AccessLogRepository> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        /// <summary>
        /// Adiciona um novo log de acesso local.
        /// </summary>
        public async Task<AccessLogLocal> AddAccessLogAsync(AccessLogLocal log)
        {
            try
            {
                log.CreatedAt = DateTime.UtcNow;
                _dbContext.AccessLogs.Add(log);
                await _dbContext.SaveChangesAsync();
                return log;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao adicionar log de acesso local.");
                throw;
            }
        }

        /// <summary>
        /// Busca log de acesso local pelo Id.
        /// </summary>
        public async Task<AccessLogLocal?> GetAccessLogByIdAsync(long id)
        {
            return await _dbContext.AccessLogs.FindAsync(id);
        }

        /// <summary>
        /// Busca todos os logs de acesso locais.
        /// </summary>
        public async Task<List<AccessLogLocal>> GetAllAccessLogsAsync()
        {
            return await _dbContext.AccessLogs.OrderByDescending(l => l.Time).ToListAsync();
        }

        /// <summary>
        /// Atualiza dados de um log de acesso local.
        /// </summary>
        public async Task<bool> UpdateAccessLogAsync(AccessLogLocal log)
        {
            try
            {
                _dbContext.AccessLogs.Update(log);
                await _dbContext.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Erro ao atualizar log de acesso local {log.Id}.");
                return false;
            }
        }

        /// <summary>
        /// Remove log de acesso local pelo Id.
        /// </summary>
        public async Task<bool> DeleteAccessLogAsync(long id)
        {
            try
            {
                var log = await _dbContext.AccessLogs.FindAsync(id);
                if (log == null)
                    return false;

                _dbContext.AccessLogs.Remove(log);
                await _dbContext.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Erro ao remover log de acesso local {id}.");
                return false;
            }
        }

        /// <summary>
        /// Busca logs de acesso locais por usuário, evento, dispositivo ou data.
        /// </summary>
        public async Task<List<AccessLogLocal>> SearchAccessLogsAsync(
            long? userId = null,
            string? eventCode = null,
            long? deviceId = null,
            DateTime? startDate = null,
            DateTime? endDate = null)
        {
            IQueryable<AccessLogLocal> query = _dbContext.AccessLogs;
            int? parsedEventCode = null;

            if (userId.HasValue)
                query = query.Where(l => l.UserId == userId.Value);

            if (!string.IsNullOrWhiteSpace(eventCode))
            {
                if (int.TryParse(eventCode, out var parsed))
                    parsedEventCode = parsed;
                else
                    return new List<AccessLogLocal>();
            }

            if (parsedEventCode.HasValue)
                query = query.Where(l => l.Event == parsedEventCode.Value);

            if (deviceId.HasValue)
                query = query.Where(l => l.DeviceId == deviceId.Value);

            if (startDate.HasValue)
                query = query.Where(l => l.Time >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(l => l.Time <= endDate.Value);

            return await query.OrderByDescending(l => l.Time).ToListAsync();
        }
    }
}
