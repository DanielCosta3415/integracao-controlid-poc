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
    public class QRCodeRepository
    {
        private readonly IntegracaoControlIDContext _dbContext;
        private readonly ILogger<QRCodeRepository> _logger;

        public QRCodeRepository(IntegracaoControlIDContext dbContext, ILogger<QRCodeRepository> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        /// <summary>
        /// Adiciona um novo QR Code local.
        /// </summary>
        public async Task<QRCodeLocal> AddQRCodeAsync(QRCodeLocal qrCode)
        {
            try
            {
                qrCode.CreatedAt = DateTime.UtcNow;
                _dbContext.QRCodes.Add(qrCode);
                await _dbContext.SaveChangesAsync();
                return qrCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao adicionar QR Code local.");
                throw;
            }
        }

        /// <summary>
        /// Busca QR Code local pelo Id.
        /// </summary>
        public async Task<QRCodeLocal?> GetQRCodeByIdAsync(long id)
        {
            return await _dbContext.QRCodes.FindAsync(id);
        }

        /// <summary>
        /// Busca todos os QR Codes locais.
        /// </summary>
        public async Task<List<QRCodeLocal>> GetAllQRCodesAsync()
        {
            return await _dbContext.QRCodes
                .OrderBy(q => q.Id)
                .Take(LocalDataQueryLimits.DefaultListLimit)
                .ToListAsync();
        }

        /// <summary>
        /// Atualiza dados de um QR Code local.
        /// </summary>
        public async Task<bool> UpdateQRCodeAsync(QRCodeLocal qrCode)
        {
            try
            {
                _dbContext.QRCodes.Update(qrCode);
                await _dbContext.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Erro ao atualizar QR Code local {qrCode.Id}.");
                return false;
            }
        }

        /// <summary>
        /// Remove QR Code local pelo Id.
        /// </summary>
        public async Task<bool> DeleteQRCodeAsync(long id)
        {
            try
            {
                var qrCode = await _dbContext.QRCodes.FindAsync(id);
                if (qrCode == null)
                    return false;

                _dbContext.QRCodes.Remove(qrCode);
                await _dbContext.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Erro ao remover QR Code local {id}.");
                return false;
            }
        }

        /// <summary>
        /// Busca QR Codes locais por usuário ou status.
        /// </summary>
        public async Task<List<QRCodeLocal>> SearchQRCodesAsync(long? userId = null, string? status = null)
        {
            IQueryable<QRCodeLocal> query = _dbContext.QRCodes;

            if (userId.HasValue)
                query = query.Where(q => q.UserId == userId.Value);

            if (!string.IsNullOrWhiteSpace(status))
                query = query.Where(q => q.Status == status);

            return await query
                .OrderBy(q => q.Id)
                .Take(LocalDataQueryLimits.DefaultListLimit)
                .ToListAsync();
        }
    }
}
