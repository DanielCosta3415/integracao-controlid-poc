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
    public class SessionRepository
    {
        private readonly IntegracaoControlIDContext _dbContext;
        private readonly ILogger<SessionRepository> _logger;

        public SessionRepository(IntegracaoControlIDContext dbContext, ILogger<SessionRepository> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        /// <summary>
        /// Adiciona uma nova sessão local.
        /// </summary>
        public async Task<SessionLocal> AddSessionAsync(SessionLocal session)
        {
            session.CreatedAt = DateTime.UtcNow;
            _dbContext.Sessions.Add(session);
            await _dbContext.SaveChangesAsync();
            return session;
        }

        /// <summary>
        /// Busca sessão local pelo Id.
        /// </summary>
        public async Task<SessionLocal?> GetSessionByIdAsync(long id)
        {
            return await _dbContext.Sessions.FindAsync(id);
        }

        /// <summary>
        /// Busca todas as sessões locais.
        /// </summary>
        public async Task<List<SessionLocal>> GetAllSessionsAsync()
        {
            return await _dbContext.Sessions
                .AsNoTracking()
                .OrderByDescending(s => s.CreatedAt)
                .Take(LocalDataQueryLimits.DefaultListLimit)
                .ToListAsync();
        }

        /// <summary>
        /// Atualiza dados de uma sessão local.
        /// </summary>
        public async Task<bool> UpdateSessionAsync(SessionLocal session)
        {
            _dbContext.Sessions.Update(session);
            await _dbContext.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// Remove sessão local pelo Id.
        /// </summary>
        public async Task<bool> DeleteSessionAsync(long id)
        {
            var session = await _dbContext.Sessions.FindAsync(id);
            if (session == null)
                return false;

            _dbContext.Sessions.Remove(session);
            await _dbContext.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// Busca sessões ativas.
        /// </summary>
        public async Task<List<SessionLocal>> GetActiveSessionsAsync()
        {
            return await _dbContext.Sessions
                .AsNoTracking()
                .Where(s => s.IsActive)
                .OrderByDescending(s => s.CreatedAt)
                .Take(LocalDataQueryLimits.DefaultListLimit)
                .ToListAsync();
        }

        /// <summary>
        /// Busca sessão ativa para um determinado equipamento (DeviceAddress).
        /// </summary>
        public async Task<SessionLocal?> GetActiveSessionForDeviceAsync(string deviceAddress)
        {
            return await _dbContext.Sessions
                .AsNoTracking()
                .Where(s => s.DeviceAddress == deviceAddress && s.IsActive)
                .OrderByDescending(s => s.CreatedAt)
                .FirstOrDefaultAsync();
        }

        /// <summary>
        /// Busca sessões locais por usuário.
        /// </summary>
        public async Task<List<SessionLocal>> GetSessionsByUsernameAsync(string username)
        {
            return await _dbContext.Sessions
                .Where(s => s.Username == username)
                .OrderByDescending(s => s.CreatedAt)
                .Take(LocalDataQueryLimits.DefaultListLimit)
                .ToListAsync();
        }

        /// <summary>
        /// Marca uma sessão como inativa (logout local).
        /// </summary>
        public async Task<bool> DeactivateSessionAsync(long id)
        {
            var session = await _dbContext.Sessions.FindAsync(id);
            if (session == null)
                return false;

            session.IsActive = false;
            await _dbContext.SaveChangesAsync();
            return true;
        }

        /// <summary>
        /// Remove todas as sessões inativas/expiradas.
        /// </summary>
        public async Task<int> CleanupInactiveSessionsAsync()
        {
            return await _dbContext.Sessions
                .Where(s => !s.IsActive || (s.ExpiresAt != null && s.ExpiresAt < DateTime.UtcNow))
                .ExecuteDeleteAsync();
        }
    }
}
