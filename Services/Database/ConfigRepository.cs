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
    public class ConfigRepository
    {
        private readonly IntegracaoControlIDContext _dbContext;
        private readonly ILogger<ConfigRepository> _logger;

        public ConfigRepository(IntegracaoControlIDContext dbContext, ILogger<ConfigRepository> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        /// <summary>
        /// Adiciona uma nova configuração local.
        /// </summary>
        public async Task<ConfigLocal> AddConfigAsync(ConfigLocal config)
        {
            try
            {
                config.CreatedAt = DateTime.UtcNow;
                _dbContext.Configs.Add(config);
                await _dbContext.SaveChangesAsync();
                return config;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao adicionar configuração local.");
                throw;
            }
        }

        /// <summary>
        /// Busca configuração local pelo Id.
        /// </summary>
        public async Task<ConfigLocal?> GetConfigByIdAsync(long id)
        {
            return await _dbContext.Configs.FindAsync(id);
        }

        /// <summary>
        /// Busca todas as configurações locais.
        /// </summary>
        public async Task<List<ConfigLocal>> GetAllConfigsAsync()
        {
            return await _dbContext.Configs.OrderBy(c => c.Id).ToListAsync();
        }

        /// <summary>
        /// Atualiza dados de uma configuração local.
        /// </summary>
        public async Task<bool> UpdateConfigAsync(ConfigLocal config)
        {
            try
            {
                _dbContext.Configs.Update(config);
                await _dbContext.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Erro ao atualizar configuração local {config.Id}.");
                return false;
            }
        }

        /// <summary>
        /// Remove configuração local pelo Id.
        /// </summary>
        public async Task<bool> DeleteConfigAsync(long id)
        {
            try
            {
                var config = await _dbContext.Configs.FindAsync(id);
                if (config == null)
                    return false;

                _dbContext.Configs.Remove(config);
                await _dbContext.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Erro ao remover configuração local {id}.");
                return false;
            }
        }

        /// <summary>
        /// Busca configurações locais por grupo, chave ou valor.
        /// </summary>
        public async Task<List<ConfigLocal>> SearchConfigsAsync(string? group = null, string? key = null, string? value = null)
        {
            IQueryable<ConfigLocal> query = _dbContext.Configs;

            if (!string.IsNullOrWhiteSpace(group))
                query = query.Where(c => c.Group == group);

            if (!string.IsNullOrWhiteSpace(key))
                query = query.Where(c => c.Key == key);

            if (!string.IsNullOrWhiteSpace(value))
                query = query.Where(c => c.Value == value);

            return await query.OrderBy(c => c.Id).ToListAsync();
        }
    }
}
