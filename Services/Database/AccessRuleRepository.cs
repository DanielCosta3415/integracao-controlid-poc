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
    public class AccessRuleRepository
    {
        private readonly IntegracaoControlIDContext _dbContext;
        private readonly ILogger<AccessRuleRepository> _logger;

        public AccessRuleRepository(IntegracaoControlIDContext dbContext, ILogger<AccessRuleRepository> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        /// <summary>
        /// Adiciona uma nova regra de acesso local.
        /// </summary>
        public async Task<AccessRuleLocal> AddAccessRuleAsync(AccessRuleLocal rule)
        {
            try
            {
                rule.CreatedAt = DateTime.UtcNow;
                _dbContext.AccessRules.Add(rule);
                await _dbContext.SaveChangesAsync();
                return rule;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao adicionar regra de acesso local.");
                throw;
            }
        }

        /// <summary>
        /// Busca regra de acesso local pelo Id.
        /// </summary>
        public async Task<AccessRuleLocal?> GetAccessRuleByIdAsync(long id)
        {
            return await _dbContext.AccessRules.FindAsync(id);
        }

        /// <summary>
        /// Busca todas as regras de acesso locais.
        /// </summary>
        public async Task<List<AccessRuleLocal>> GetAllAccessRulesAsync()
        {
            return await _dbContext.AccessRules.OrderBy(r => r.Id).ToListAsync();
        }

        /// <summary>
        /// Atualiza dados de uma regra de acesso local.
        /// </summary>
        public async Task<bool> UpdateAccessRuleAsync(AccessRuleLocal rule)
        {
            try
            {
                _dbContext.AccessRules.Update(rule);
                await _dbContext.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Erro ao atualizar regra de acesso local {rule.Id}.");
                return false;
            }
        }

        /// <summary>
        /// Remove regra de acesso local pelo Id.
        /// </summary>
        public async Task<bool> DeleteAccessRuleAsync(long id)
        {
            try
            {
                var rule = await _dbContext.AccessRules.FindAsync(id);
                if (rule == null)
                    return false;

                _dbContext.AccessRules.Remove(rule);
                await _dbContext.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Erro ao remover regra de acesso local {id}.");
                return false;
            }
        }

        /// <summary>
        /// Busca regras de acesso locais por nome, status ou prioridade.
        /// </summary>
        public async Task<List<AccessRuleLocal>> SearchAccessRulesAsync(string? name = null, string? status = null, int? priority = null)
        {
            IQueryable<AccessRuleLocal> query = _dbContext.AccessRules;

            if (!string.IsNullOrWhiteSpace(name))
                query = query.Where(r => r.Name.Contains(name));

            if (!string.IsNullOrWhiteSpace(status))
                query = query.Where(r => r.Status == status);

            if (priority.HasValue)
                query = query.Where(r => r.Priority == priority.Value);

            return await query.OrderBy(r => r.Id).ToListAsync();
        }
    }
}
