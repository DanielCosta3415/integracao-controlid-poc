using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Integracao.ControlID.PoC.Data;
using Integracao.ControlID.PoC.Models.Database;
using Integracao.ControlID.PoC.Models.Security;
using Integracao.ControlID.PoC.Services.Security;
using Microsoft.Data.Sqlite;
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
        /// Adiciona um novo usuario local.
        /// </summary>
        public async Task<UserLocal> AddUserAsync(UserLocal user)
        {
            NormalizeIdentity(user);
            user.CreatedAt = DateTime.UtcNow;
            _dbContext.Users.Add(user);
            await _dbContext.SaveChangesAsync();
            return user;
        }

        /// <summary>
        /// Busca usuario local pelo Id.
        /// </summary>
        public async Task<UserLocal?> GetUserByIdAsync(long id)
        {
            return await _dbContext.Users.FindAsync(id);
        }

        public async Task<int> CountUsersAsync()
        {
            return await _dbContext.Users.CountAsync();
        }

        public async Task<UserLocal?> GetUserByUsernameOrEmailAsync(string usernameOrEmail)
        {
            var normalized = LocalIdentityPolicy.NormalizeIdentifier(usernameOrEmail);
            return await _dbContext.Users
                .AsNoTracking()
                .OrderBy(u => u.Id)
                .FirstOrDefaultAsync(u =>
                    u.NormalizedUsername == normalized ||
                    u.NormalizedEmail == normalized);
        }

        public async Task<LocalUserRegistrationResult> RegisterLocalUserAsync(
            UserLocal user,
            bool allowAdditionalUsers,
            CancellationToken cancellationToken = default)
        {
            NormalizeIdentity(user);

            if (_dbContext.Database.GetDbConnection() is not SqliteConnection connection)
                throw new InvalidOperationException("Atomic local user registration requires SQLite.");

            var openedHere = connection.State != ConnectionState.Open;
            if (openedHere)
                await connection.OpenAsync(cancellationToken);

            await using var transaction = connection.BeginTransaction(deferred: false);
            await _dbContext.Database.UseTransactionAsync(transaction, cancellationToken);

            try
            {
                if (await _dbContext.Users.AnyAsync(
                        item => item.NormalizedUsername == user.NormalizedUsername,
                        cancellationToken))
                {
                    await transaction.RollbackAsync(cancellationToken);
                    return LocalUserRegistrationResult.DuplicateUsername();
                }

                if (await _dbContext.Users.AnyAsync(
                        item => item.NormalizedEmail == user.NormalizedEmail,
                        cancellationToken))
                {
                    await transaction.RollbackAsync(cancellationToken);
                    return LocalUserRegistrationResult.DuplicateEmail();
                }

                var hasUsers = await _dbContext.Users.AnyAsync(cancellationToken);
                if (hasUsers && !allowAdditionalUsers)
                {
                    await transaction.RollbackAsync(cancellationToken);
                    return LocalUserRegistrationResult.RegistrationClosed();
                }

                user.Role = hasUsers ? AppSecurityRoles.Operator : AppSecurityRoles.Administrator;
                user.CreatedAt = DateTime.UtcNow;
                _dbContext.Users.Add(user);
                await _dbContext.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);

                return LocalUserRegistrationResult.Created(user, isBootstrapAdministrator: !hasUsers);
            }
            catch (DbUpdateException ex) when (ex.InnerException is SqliteException { SqliteErrorCode: 19 })
            {
                await transaction.RollbackAsync(CancellationToken.None);
                _dbContext.ChangeTracker.Clear();
                _logger.LogWarning("Conflito de unicidade ao registrar identidade local.");
                return LocalUserRegistrationResult.DuplicateIdentity();
            }
            finally
            {
                await _dbContext.Database.UseTransactionAsync(null, CancellationToken.None);
                if (openedHere)
                    await connection.CloseAsync();
            }
        }

        /// <summary>
        /// Busca todos os usuarios locais.
        /// </summary>
        public async Task<List<UserLocal>> GetAllUsersAsync()
        {
            return await _dbContext.Users
                .OrderBy(u => u.Id)
                .Take(LocalDataQueryLimits.DefaultListLimit)
                .ToListAsync();
        }

        /// <summary>
        /// Atualiza dados de um usuario local.
        /// </summary>
        public async Task<bool> UpdateUserAsync(UserLocal user)
        {
            NormalizeIdentity(user);
            user.UpdatedAt = DateTime.UtcNow;
            _dbContext.Users.Update(user);
            await _dbContext.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// Remove usuario local pelo Id.
        /// </summary>
        public async Task<bool> DeleteUserAsync(long id)
        {
            var user = await _dbContext.Users.FindAsync(id);
            if (user == null)
                return false;

            _dbContext.Users.Remove(user);
            await _dbContext.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// Busca usuarios locais por nome ou matricula.
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

        private static void NormalizeIdentity(UserLocal user)
        {
            user.Name = user.Name.Trim();
            user.Username = user.Username.Trim();
            user.Registration = string.IsNullOrWhiteSpace(user.Registration)
                ? user.Username
                : user.Registration.Trim();
            user.Email = user.Email.Trim();
            user.Phone = user.Phone.Trim();
            user.NormalizedUsername = LocalIdentityPolicy.NormalizeIdentifier(user.Username);
            user.NormalizedEmail = LocalIdentityPolicy.NormalizeIdentifier(user.Email);
        }
    }
}
