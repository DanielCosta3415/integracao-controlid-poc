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
    public class LogoRepository
    {
        private readonly IntegracaoControlIDContext _dbContext;
        private readonly ILogger<LogoRepository> _logger;

        public LogoRepository(IntegracaoControlIDContext dbContext, ILogger<LogoRepository> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        /// <summary>
        /// Adiciona um novo logo local.
        /// </summary>
        public async Task<LogoLocal> AddLogoAsync(LogoLocal logo)
        {
            try
            {
                logo.CreatedAt = DateTime.UtcNow;
                _dbContext.Logos.Add(logo);
                await _dbContext.SaveChangesAsync();
                return logo;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao adicionar logo local.");
                throw;
            }
        }

        /// <summary>
        /// Busca logo local pelo Id.
        /// </summary>
        public async Task<LogoLocal?> GetLogoByIdAsync(long id)
        {
            return await _dbContext.Logos.FindAsync(id);
        }

        /// <summary>
        /// Busca todos os logos locais.
        /// </summary>
        public async Task<List<LogoLocal>> GetAllLogosAsync()
        {
            return await _dbContext.Logos.OrderBy(l => l.Id).ToListAsync();
        }

        /// <summary>
        /// Atualiza dados de um logo local.
        /// </summary>
        public async Task<bool> UpdateLogoAsync(LogoLocal logo)
        {
            try
            {
                _dbContext.Logos.Update(logo);
                await _dbContext.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Erro ao atualizar logo local {logo.Id}.");
                return false;
            }
        }

        /// <summary>
        /// Remove logo local pelo Id.
        /// </summary>
        public async Task<bool> DeleteLogoAsync(long id)
        {
            try
            {
                var logo = await _dbContext.Logos.FindAsync(id);
                if (logo == null)
                    return false;

                _dbContext.Logos.Remove(logo);
                await _dbContext.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Erro ao remover logo local {id}.");
                return false;
            }
        }

        /// <summary>
        /// Busca logos locais por formato, descrição ou período.
        /// </summary>
        public async Task<List<LogoLocal>> SearchLogosAsync(
            string? format = null,
            string? description = null,
            DateTime? startDate = null,
            DateTime? endDate = null)
        {
            IQueryable<LogoLocal> query = _dbContext.Logos;

            if (!string.IsNullOrWhiteSpace(format))
                query = query.Where(l => l.Format == format);

            if (!string.IsNullOrWhiteSpace(description))
                query = query.Where(l => l.Description.Contains(description));

            if (startDate.HasValue)
                query = query.Where(l => l.CreatedAt >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(l => l.CreatedAt <= endDate.Value);

            return await query.OrderBy(l => l.Id).ToListAsync();
        }
    }
}
