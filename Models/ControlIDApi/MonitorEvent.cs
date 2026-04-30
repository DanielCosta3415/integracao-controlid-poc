using System;

namespace Integracao.ControlID.PoC.Models.ControlIDApi
{
    /// <summary>
    /// Representa um evento de monitoramento recebido via Webhook ou Push do equipamento Control iD.
    /// </summary>
    [Serializable]
    public class MonitorEvent
    {
        /// <summary>
        /// Identificador único do evento (geralmente um GUID ou ID incremental).
        /// </summary>
        public Guid EventId { get; set; }

        /// <summary>
        /// Data/hora do recebimento do evento.
        /// </summary>
        public DateTime ReceivedAt { get; set; }

        /// <summary>
        /// Conteúdo bruto do evento em JSON (para inspeção ou registro).
        /// </summary>
        public string RawJson { get; set; } = string.Empty;

        /// <summary>
        /// (Opcional) Tipo ou nome do evento (ex: "access_granted", "alarm", "heartbeat", etc).
        /// </summary>
        public string EventType { get; set; } = string.Empty;

        /// <summary>
        /// (Opcional) Identificador do dispositivo de origem.
        /// </summary>
        public string DeviceId { get; set; } = string.Empty;

        /// <summary>
        /// (Opcional) Identificador do usuário relacionado ao evento.
        /// </summary>
        public string UserId { get; set; } = string.Empty;

        /// <summary>
        /// (Opcional) Status, resultado ou payload principal do evento.
        /// </summary>
        public string Payload { get; set; } = string.Empty;

        /// <summary>
        /// (Opcional) IP de origem do evento, se disponível.
        /// </summary>
        public string SourceIp { get; set; } = string.Empty;

        /// <summary>
        /// (Opcional) Observações adicionais ou descrição do evento.
        /// </summary>
        public string Notes { get; set; } = string.Empty;

        /// <summary>
        /// Override para facilitar logs/debug.
        /// </summary>
        public override string ToString()
        {
            return $"[MonitorEvent] Id={EventId}, Type={EventType}, Device={DeviceId}, User={UserId}, At={ReceivedAt}, SourceIp={SourceIp}, Notes={Notes}";
        }

        // Adicione outros campos conforme a estrutura dos eventos recebidos da sua API.
    }
}
