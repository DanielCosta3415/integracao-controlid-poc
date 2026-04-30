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
    public class UserRepository
    {
        private readonly IntegracaoControlIDContext _dbContext;
        private readonly ILogger<UserRepository> _logger;

        public UserRepository(IntegracaoControlIDContext dbContext, ILogger<UserRepository> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        /// <summary>
        /// Adiciona um novo usuário local.
        /// </summary>
        public async Task<UserLocal> AddUserAsync(UserLocal user)
        {
            try
            {
                user.CreatedAt = DateTime.UtcNow;
                _dbContext.Users.Add(user);
                await _dbContext.SaveChangesAsync();
                return user;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao adicionar usuário local.");
                throw;
            }
        }

        /// <summary>
        /// Busca usuário local pelo Id.
        /// </summary>
        public async Task<UserLocal?> GetUserByIdAsync(long id)
        {
            return await _dbContext.Users.FindAsync(id);
        }

        /// <summary>
        /// Busca todos os usuários locais.
        /// </summary>
        public async Task<List<UserLocal>> GetAllUsersAsync()
        {
            return await _dbContext.Users
                .OrderBy(u => u.Id)
                .Take(LocalDataQueryLimits.DefaultListLimit)
                .ToListAsync();
        }

        /// <summary>
        /// Atualiza dados de um usuário local.
        /// </summary>
        public async Task<bool> UpdateUserAsync(UserLocal user)
        {
            try
            {
                _dbContext.Users.Update(user);
                await _dbContext.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Erro ao atualizar usuário local {user.Id}.");
                return false;
            }
        }

        /// <summary>
        /// Remove usuário local pelo Id.
        /// </summary>
        public async Task<bool> DeleteUserAsync(long id)
        {
            try
            {
                var user = await _dbContext.Users.FindAsync(id);
                if (user == null)
                    return false;

                _dbContext.Users.Remove(user);
                await _dbContext.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Erro ao remover usuário local {id}.");
                return false;
            }
        }

        /// <summary>
        /// Busca usuários locais por nome ou matrícula.
        /// </summary>
        public async Task<List<UserLocal>> SearchUsersAsync(string? name = null, string? registration = null)
        {
            IQueryable<UserLocal> query = _dbContext.Users;

            if (!string.IsNullOrWhiteSpace(name))
                query = query.Where(u => u.Name.Contains(name));

            if (!string.IsNullOrWhiteSpace(registration))
                query = query.Where(u => u.Registration == registration);

            return await query
                .OrderBy(u => u.Id)
                .Take(LocalDataQueryLimits.DefaultListLimit)
                .ToListAsync();
        }
    }
}
