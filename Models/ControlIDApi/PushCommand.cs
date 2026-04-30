using System;

namespace Integracao.ControlID.PoC.Models.ControlIDApi
{
    /// <summary>
    /// Representa um comando ou evento recebido via Push do equipamento Control iD.
    /// </summary>
    [Serializable]
    public class PushCommand
    {
        /// <summary>
        /// Identificador único do comando/evento Push (GUID ou ID incremental).
        /// </summary>
        public Guid CommandId { get; set; }

        /// <summary>
        /// Data/hora do recebimento do comando/evento.
        /// </summary>
        public DateTime ReceivedAt { get; set; }

        /// <summary>
        /// Tipo do comando/evento (ex: "access_granted", "alarm", "device_online", etc).
        /// </summary>
        public string CommandType { get; set; } = string.Empty;

        /// <summary>
        /// Conteúdo bruto do comando em JSON (para inspeção ou registro).
        /// </summary>
        public string RawJson { get; set; } = string.Empty;

        /// <summary>
        /// (Opcional) Status do comando ("success", "fail", "pending", etc).
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
        /// (Opcional) Endereço IP de origem do comando/evento.
        /// </summary>
        public string SourceIp { get; set; } = string.Empty;

        /// <summary>
        /// (Opcional) Código de erro, caso aplicável.
        /// </summary>
        public string ErrorCode { get; set; } = string.Empty;

        /// <summary>
        /// ToString simplificado para debugging.
        /// </summary>
        public override string ToString()
        {
            return $"PushCommand: Id={CommandId}, Type={CommandType}, Status={Status}, DeviceId={DeviceId}, UserId={UserId}, ReceivedAt={ReceivedAt}";
        }

        // Adicione outros campos conforme o tipo de comando/evento recebido da sua API.
    }
}
