using Integracao.ControlID.PoC.Models.ControlIDApi;
using Integracao.ControlID.PoC.Models.Database;

namespace Integracao.ControlID.PoC.Monitor
{
    public static class MonitorEventMapper
    {
        /// <summary>
        /// Converte um MonitorEvent (API) em MonitorEventLocal (local/banco de dados).
        /// </summary>
        public static MonitorEventLocal ToLocal(this MonitorEvent evt)
        {
            return new MonitorEventLocal
            {
                EventId = evt.EventId == Guid.Empty ? Guid.NewGuid() : evt.EventId,
                EventType = evt.EventType,
                Payload = evt.Payload,
                Status = evt.Notes,
                DeviceId = evt.DeviceId,
                ReceivedAt = evt.ReceivedAt != default ? evt.ReceivedAt : System.DateTime.UtcNow,
                UserId = evt.UserId,
                RawJson = evt.RawJson
            };
        }

        /// <summary>
        /// Converte um MonitorEventLocal em MonitorEvent (para API/view).
        /// </summary>
        public static MonitorEvent ToApi(this MonitorEventLocal local)
        {
            return new MonitorEvent
            {
                EventId = local.EventId,
                EventType = local.EventType,
                Payload = local.Payload,
                Notes = local.Status,
                DeviceId = local.DeviceId,
                ReceivedAt = local.ReceivedAt,
                UserId = local.UserId,
                RawJson = local.RawJson
            };
        }
    }
}
