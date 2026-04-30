using System;
using System.Threading.Tasks;
using Integracao.ControlID.PoC.Models.ControlIDApi;
using Integracao.ControlID.PoC.Models.Database;
using Integracao.ControlID.PoC.Services.Database;
using Microsoft.Extensions.Logging;

namespace Integracao.ControlID.PoC.Monitor
{
    public class MonitorEventHandler
    {
        private readonly MonitorEventRepository _monitorEventRepository;
        private readonly ILogger<MonitorEventHandler> _logger;

        public MonitorEventHandler(MonitorEventRepository monitorEventRepository, ILogger<MonitorEventHandler> logger)
        {
            _monitorEventRepository = monitorEventRepository;
            _logger = logger;
        }

        /// <summary>
        /// Processa um evento de monitoramento recebido da API Control iD (Webhook/Push).
        /// Persiste o evento localmente e pode disparar outras ações (ex: log, notificação, auditoria).
        /// </summary>
        public async Task HandleMonitorEventAsync(MonitorEvent evt)
        {
            try
            {
                // Mapeia o evento recebido (API) para o modelo local.
                var monitorEventLocal = new MonitorEventLocal
                {
                    EventId = evt.EventId == Guid.Empty ? Guid.NewGuid() : evt.EventId,
                    EventType = evt.EventType,
                    Payload = evt.Payload,
                    Status = evt.Notes,
                    DeviceId = evt.DeviceId,
                    ReceivedAt = DateTime.UtcNow,
                    UserId = evt.UserId,
                    RawJson = evt.RawJson
                };

                await _monitorEventRepository.AddMonitorEventAsync(monitorEventLocal);

                _logger.LogInformation("Evento monitorado processado e salvo: {EventType} ({DeviceId})", evt.EventType, evt.DeviceId);

                // Outras ações: notificação, log externo, integração com SignalR, etc.
                // Exemplo: await _notifier.NotifyAsync(monitorEventLocal);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao processar evento monitorado do equipamento {DeviceId}.", evt.DeviceId);
                // Pode-se persistir log de erro localmente ou escalar para mecanismo de alerta.
            }
        }
    }
}
