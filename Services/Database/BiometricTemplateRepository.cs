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
    public class BiometricTemplateRepository
    {
        private readonly IntegracaoControlIDContext _dbContext;
        private readonly ILogger<BiometricTemplateRepository> _logger;

        public BiometricTemplateRepository(IntegracaoControlIDContext dbContext, ILogger<BiometricTemplateRepository> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        /// <summary>
        /// Adiciona um novo template biométrico local.
        /// </summary>
        public async Task<BiometricTemplateLocal> AddTemplateAsync(BiometricTemplateLocal template)
        {
            try
            {
                template.CreatedAt = DateTime.UtcNow;
                _dbContext.BiometricTemplates.Add(template);
                await _dbContext.SaveChangesAsync();
                return template;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao adicionar template biométrico local.");
                throw;
            }
        }

        /// <summary>
        /// Busca template biométrico local pelo Id.
        /// </summary>
        public async Task<BiometricTemplateLocal?> GetTemplateByIdAsync(long id)
        {
            return await _dbContext.BiometricTemplates.FindAsync(id);
        }

        /// <summary>
        /// Busca todos os templates biométricos locais.
        /// </summary>
        public async Task<List<BiometricTemplateLocal>> GetAllTemplatesAsync()
        {
            return await _dbContext.BiometricTemplates
                .OrderBy(t => t.Id)
                .Take(LocalDataQueryLimits.DefaultListLimit)
                .ToListAsync();
        }

        /// <summary>
        /// Atualiza dados de um template biométrico local.
        /// </summary>
        public async Task<bool> UpdateTemplateAsync(BiometricTemplateLocal template)
        {
            try
            {
                _dbContext.BiometricTemplates.Update(template);
                await _dbContext.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Erro ao atualizar template biométrico local {template.Id}.");
                return false;
            }
        }

        /// <summary>
        /// Remove template biométrico local pelo Id.
        /// </summary>
        public async Task<bool> DeleteTemplateAsync(long id)
        {
            try
            {
                var template = await _dbContext.BiometricTemplates.FindAsync(id);
                if (template == null)
                    return false;

                _dbContext.BiometricTemplates.Remove(template);
                await _dbContext.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Erro ao remover template biométrico local {id}.");
                return false;
            }
        }

        /// <summary>
        /// Busca templates biométricos locais por usuário.
        /// </summary>
        public async Task<List<BiometricTemplateLocal>> GetTemplatesByUserIdAsync(long userId)
        {
            return await _dbContext.BiometricTemplates
                .Where(t => t.UserId == userId)
                .OrderBy(t => t.Id)
                .Take(LocalDataQueryLimits.DefaultListLimit)
                .ToListAsync();
        }

        /// <summary>
        /// Busca templates biométricos por tipo.
        /// </summary>
        public async Task<List<BiometricTemplateLocal>> SearchTemplatesAsync(string? type = null)
        {
            IQueryable<BiometricTemplateLocal> query = _dbContext.BiometricTemplates;

            if (!string.IsNullOrWhiteSpace(type))
            {
                if (int.TryParse(type, out var parsedType))
                    query = query.Where(t => t.Type == parsedType);
                else
                    return new List<BiometricTemplateLocal>();
            }

            return await query
                .OrderBy(t => t.Id)
                .Take(LocalDataQueryLimits.DefaultListLimit)
                .ToListAsync();
        }
    }
}
