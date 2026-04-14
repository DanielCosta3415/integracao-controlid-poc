using System;

namespace Integracao.ControlID.PoC.Models.Database
{
    /// <summary>
    /// Representa um evento de monitoramento recebido via Webhook ou Push, armazenado localmente no banco da aplicação.
    /// </summary>
    public class MonitorEventLocal
    {
        /// <summary>
        /// Identificador único do evento local (GUID ou ID do banco).
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
        /// (Opcional) Tipo ou nome do evento (ex: access_granted, alarm, heartbeat, etc).
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
        /// (Opcional) Status resumido do evento.
        /// </summary>
        public string Status { get; set; } = string.Empty;

        /// <summary>
        /// Data/hora de criação local.
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// (Opcional) Data/hora de última atualização.
        /// </summary>
        public DateTime? UpdatedAt { get; set; }

        // Adicione outros campos conforme necessidade local.
    }
}

