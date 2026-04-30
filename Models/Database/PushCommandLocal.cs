using System;

namespace Integracao.ControlID.PoC.Models.Database
{
    /// <summary>
    /// Representa um comando ou evento Push recebido do equipamento Control iD, armazenado localmente no banco da aplicação.
    /// </summary>
    public class PushCommandLocal
    {
        /// <summary>
        /// Identificador único do comando/evento Push local (GUID ou ID do banco).
        /// </summary>
        public Guid CommandId { get; set; }

        /// <summary>
        /// Data/hora do recebimento do comando/evento.
        /// </summary>
        public DateTime ReceivedAt { get; set; }

        /// <summary>
        /// Tipo do comando/evento (ex: access_granted, alarm, device_online, etc).
        /// </summary>
        public string CommandType { get; set; } = string.Empty;

        /// <summary>
        /// Conteúdo bruto do comando em JSON (para inspeção ou registro).
        /// </summary>
        public string RawJson { get; set; } = string.Empty;

        /// <summary>
        /// (Opcional) Status do comando (success, fail, pending, etc).
        /// </summary>
        public string Status { get; set; } = string.Empty;

        /// <summary>
        /// (Opcional) Informações adicionais ou payload relevante.
        /// </summary>
        public string Payload { get; set; } = string.Empty;

        /// <summary>
        /// (Opcional) Identificador do dispositivo de origem.
        /// </summary>
        public string DeviceId { get; set; } = string.Empty;

        /// <summary>
        /// (Opcional) Identificador do usuário relacionado ao comando.
        /// </summary>
        public string UserId { get; set; } = string.Empty;

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
