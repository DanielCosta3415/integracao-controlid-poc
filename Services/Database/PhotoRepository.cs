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
    public class PhotoRepository
    {
        private readonly IntegracaoControlIDContext _dbContext;
        private readonly ILogger<PhotoRepository> _logger;

        public PhotoRepository(IntegracaoControlIDContext dbContext, ILogger<PhotoRepository> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        /// <summary>
        /// Adiciona uma nova foto local.
        /// </summary>
        public async Task<PhotoLocal> AddPhotoAsync(PhotoLocal photo)
        {
            try
            {
                photo.CreatedAt = DateTime.UtcNow;
                _dbContext.Photos.Add(photo);
                await _dbContext.SaveChangesAsync();
                return photo;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao adicionar foto local.");
                throw;
            }
        }

        /// <summary>
        /// Busca foto local pelo Id.
        /// </summary>
        public async Task<PhotoLocal?> GetPhotoByIdAsync(long id)
        {
            return await _dbContext.Photos.FindAsync(id);
        }

        /// <summary>
        /// Busca todas as fotos locais.
        /// </summary>
        public async Task<List<PhotoLocal>> GetAllPhotosAsync()
        {
            return await _dbContext.Photos
                .OrderBy(p => p.Id)
                .Take(LocalDataQueryLimits.DefaultListLimit)
                .ToListAsync();
        }

        /// <summary>
        /// Atualiza dados de uma foto local.
        /// </summary>
        public async Task<bool> UpdatePhotoAsync(PhotoLocal photo)
        {
            try
            {
                _dbContext.Photos.Update(photo);
                await _dbContext.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Erro ao atualizar foto local {photo.Id}.");
                return false;
            }
        }

        /// <summary>
        /// Remove foto local pelo Id.
        /// </summary>
        public async Task<bool> DeletePhotoAsync(long id)
        {
            try
            {
                var photo = await _dbContext.Photos.FindAsync(id);
                if (photo == null)
                    return false;

                _dbContext.Photos.Remove(photo);
                await _dbContext.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Erro ao remover foto local {id}.");
                return false;
            }
        }

        /// <summary>
        /// Busca fotos locais por usuário, formato ou período.
        /// </summary>
        public async Task<List<PhotoLocal>> SearchPhotosAsync(
            long? userId = null,
            string? format = null,
            DateTime? startDate = null,
            DateTime? endDate = null)
        {
            IQueryable<PhotoLocal> query = _dbContext.Photos;

            if (userId.HasValue)
                query = query.Where(p => p.UserId == userId.Value);

            if (!string.IsNullOrWhiteSpace(format))
                query = query.Where(p => p.Format == format);

            if (startDate.HasValue)
                query = query.Where(p => p.CreatedAt >= startDate.Value);

            if (endDate.HasValue)
                query = query.Where(p => p.CreatedAt <= endDate.Value);

            return await query
                .OrderBy(p => p.Id)
                .Take(LocalDataQueryLimits.DefaultListLimit)
                .ToListAsync();
        }
    }
}
