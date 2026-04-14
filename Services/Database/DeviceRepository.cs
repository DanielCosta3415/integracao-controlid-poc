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
    public class DeviceRepository
    {
        private readonly IntegracaoControlIDContext _dbContext;
        private readonly ILogger<DeviceRepository> _logger;

        public DeviceRepository(IntegracaoControlIDContext dbContext, ILogger<DeviceRepository> logger)
        {
            _dbContext = dbContext;
            _logger = logger;
        }

        /// <summary>
        /// Adiciona um novo dispositivo local.
        /// </summary>
        public async Task<DeviceLocal> AddDeviceAsync(DeviceLocal device)
        {
            try
            {
                device.CreatedAt = DateTime.UtcNow;
                _dbContext.Devices.Add(device);
                await _dbContext.SaveChangesAsync();
                return device;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao adicionar dispositivo local.");
                throw;
            }
        }

        /// <summary>
        /// Busca dispositivo local pelo Id.
        /// </summary>
        public async Task<DeviceLocal?> GetDeviceByIdAsync(long id)
        {
            return await _dbContext.Devices.FindAsync(id);
        }

        /// <summary>
        /// Busca todos os dispositivos locais.
        /// </summary>
        public async Task<List<DeviceLocal>> GetAllDevicesAsync()
        {
            return await _dbContext.Devices.OrderBy(d => d.Id).ToListAsync();
        }

        /// <summary>
        /// Atualiza dados de um dispositivo local.
        /// </summary>
        public async Task<bool> UpdateDeviceAsync(DeviceLocal device)
        {
            try
            {
                _dbContext.Devices.Update(device);
                await _dbContext.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Erro ao atualizar dispositivo local {device.Id}.");
                return false;
            }
        }

        /// <summary>
        /// Remove dispositivo local pelo Id.
        /// </summary>
        public async Task<bool> DeleteDeviceAsync(long id)
        {
            try
            {
                var device = await _dbContext.Devices.FindAsync(id);
                if (device == null)
                    return false;

                _dbContext.Devices.Remove(device);
                await _dbContext.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Erro ao remover dispositivo local {id}.");
                return false;
            }
        }

        /// <summary>
        /// Busca dispositivos locais por nome ou IP.
        /// </summary>
        public async Task<List<DeviceLocal>> SearchDevicesAsync(string? name = null, string? ip = null)
        {
            IQueryable<DeviceLocal> query = _dbContext.Devices;

            if (!string.IsNullOrWhiteSpace(name))
                query = query.Where(d => d.Name.Contains(name));

            if (!string.IsNullOrWhiteSpace(ip))
                query = query.Where(d => d.IpAddress == ip);

            return await query.OrderBy(d => d.Id).ToListAsync();
        }
    }
}