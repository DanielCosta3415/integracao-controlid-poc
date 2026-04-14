using System;

namespace Integracao.ControlID.PoC.Models.Database
{
    /// <summary>
    /// Representa um cartão (RFID, QRCode, etc) armazenado localmente no banco da aplicação.
    /// </summary>
    public class CardLocal
    {
        /// <summary>
        /// Identificador único do cartão local (ID do banco).
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// Identificador do usuário ao qual o cartão pertence (UserLocalId ou UserId).
        /// </summary>
        public long UserId { get; set; }

        /// <summary>
        /// Valor do cartão (código hexadecimal, decimal, QRCode, etc).
        /// </summary>
        public string Value { get; set; } = string.Empty;

        /// <summary>
        /// Tipo do cartão (RFID, QRCode, Barcode, etc).
        /// </summary>
        public string Type { get; set; } = string.Empty;

        /// <summary>
        /// Data/hora de criação local.
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// (Opcional) Data/hora de última atualização.
        /// </summary>
        public DateTime? UpdatedAt { get; set; }

        /// <summary>
        /// (Opcional) Início da validade do cartão.
        /// </summary>
        public DateTime? BeginTime { get; set; }

        /// <summary>
        /// (Opcional) Fim da validade do cartão.
        /// </summary>
        public DateTime? EndTime { get; set; }

        /// <summary>
        /// (Opcional) Status do cartão (ativo, inativo, bloqueado, etc).
        /// </summary>
        public string Status { get; set; } = string.Empty;

        // Adicione outros campos conforme necessidade local.
    }
}
