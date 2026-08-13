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
            config.CreatedAt = DateTime.UtcNow;
            _dbContext.Configs.Add(config);
            await _dbContext.SaveChangesAsync();
            return config;
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
            return await _dbContext.Configs
                .AsNoTracking()
                .OrderBy(c => c.Id)
                .Take(LocalDataQueryLimits.DefaultListLimit)
                .ToListAsync();
        }

        /// <summary>
        /// Atualiza dados de uma configuração local.
        /// </summary>
        public async Task<bool> UpdateConfigAsync(ConfigLocal config)
        {
            _dbContext.Configs.Update(config);
            await _dbContext.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// Remove configuração local pelo Id.
        /// </summary>
        public async Task<bool> DeleteConfigAsync(long id)
        {
            var config = await _dbContext.Configs.FindAsync(id);
            if (config == null)
                return false;

            _dbContext.Configs.Remove(config);
            await _dbContext.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// Busca configurações locais por grupo, chave ou valor.
        /// </summary>
        public async Task<List<ConfigLocal>> SearchConfigsAsync(string? group = null, string? key = null, string? value = null)
        {
            IQueryable<ConfigLocal> query = _dbContext.Configs.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(group))
                query = query.Where(c => c.Group == group);

            if (!string.IsNullOrWhiteSpace(key))
                query = query.Where(c => c.Key == key);

            query = query.OrderBy(c => c.Id);

            if (string.IsNullOrWhiteSpace(value))
            {
                return await query
                    .Take(LocalDataQueryLimits.DefaultListLimit)
                    .ToListAsync();
            }

            var matches = new List<ConfigLocal>();
            await foreach (var config in query.AsAsyncEnumerable())
            {
                if (!string.Equals(config.Value, value, StringComparison.Ordinal))
                    continue;

                matches.Add(config);
                if (matches.Count == LocalDataQueryLimits.DefaultListLimit)
                    break;
            }

            return matches;
        }
    }
}
