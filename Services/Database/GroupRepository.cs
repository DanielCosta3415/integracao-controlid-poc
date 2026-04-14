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
    public class GroupRepository
    {
        private readonly IntegracaoControlIDContext _dbContext;
        private readonly ILogger<GroupRepository> _logger;

        public GroupRepository(IntegracaoControlIDContext dbContext, ILogger<GroupRepository> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        /// <summary>
        /// Adiciona um novo grupo local.
        /// </summary>
        public async Task<GroupLocal> AddGroupAsync(GroupLocal group)
        {
            try
            {
                group.CreatedAt = DateTime.UtcNow;
                _dbContext.Groups.Add(group);
                await _dbContext.SaveChangesAsync();
                return group;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao adicionar grupo local.");
                throw;
            }
        }

        /// <summary>
        /// Busca grupo local pelo Id.
        /// </summary>
        public async Task<GroupLocal?> GetGroupByIdAsync(long id)
        {
            return await _dbContext.Groups.FindAsync(id);
        }

        /// <summary>
        /// Busca todos os grupos locais.
        /// </summary>
        public async Task<List<GroupLocal>> GetAllGroupsAsync()
        {
            return await _dbContext.Groups.OrderBy(g => g.Id).ToListAsync();
        }

        /// <summary>
        /// Atualiza dados de um grupo local.
        /// </summary>
        public async Task<bool> UpdateGroupAsync(GroupLocal group)
        {
            try
            {
                _dbContext.Groups.Update(group);
                await _dbContext.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Erro ao atualizar grupo local {group.Id}.");
                return false;
            }
        }

        /// <summary>
        /// Remove grupo local pelo Id.
        /// </summary>
        public async Task<bool> DeleteGroupAsync(long id)
        {
            try
            {
                var group = await _dbContext.Groups.FindAsync(id);
                if (group == null)
                    return false;

                _dbContext.Groups.Remove(group);
                await _dbContext.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Erro ao remover grupo local {id}.");
                return false;
            }
        }

        /// <summary>
        /// Busca grupos locais por nome ou status.
        /// </summary>
        public async Task<List<GroupLocal>> SearchGroupsAsync(string? name = null, string? status = null)
        {
            IQueryable<GroupLocal> query = _dbContext.Groups;

            if (!string.IsNullOrWhiteSpace(name))
                query = query.Where(g => g.Name.Contains(name));

            if (!string.IsNullOrWhiteSpace(status))
                query = query.Where(g => g.Status == status);

            return await query.OrderBy(g => g.Id).ToListAsync();
        }
    }
}