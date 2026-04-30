using System;

namespace Integracao.ControlID.PoC.Models.Database
{
    /// <summary>
    /// Representa um QR Code armazenado localmente no banco da aplicação.
    /// </summary>
    public class QRCodeLocal
    {
        /// <summary>
        /// Identificador único do QR Code local (ID do banco).
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// Identificador do usuário ao qual o QR Code pertence (UserLocalId ou UserId).
        /// </summary>
        public long UserId { get; set; }

        /// <summary>
        /// Valor do QR Code (string representando o código).
        /// </summary>
        public string Value { get; set; } = string.Empty;

        /// <summary>
        /// Data/hora de criação local.
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// (Opcional) Data/hora de última atualização.
        /// </summary>
        public DateTime? UpdatedAt { get; set; }

        /// <summary>
        /// (Opcional) Início da validade do QR Code.
        /// </summary>
        public DateTime? BeginTime { get; set; }

        /// <summary>
        /// (Opcional) Fim da validade do QR Code.
        /// </summary>
        public DateTime? EndTime { get; set; }

        /// <summary>
        /// (Opcional) Status do QR Code (ativo, expirado, bloqueado, etc).
        /// </summary>
        public string Status { get; set; } = string.Empty;

        // Adicione outros campos conforme necessidade local.
    }
}
