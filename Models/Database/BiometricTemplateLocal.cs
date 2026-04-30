using System;

namespace Integracao.ControlID.PoC.Models.Database
{
    /// <summary>
    /// Representa um template biométrico armazenado localmente no banco da aplicação.
    /// </summary>
    public class BiometricTemplateLocal
    {
        /// <summary>
        /// Identificador único do template biométrico local (ID do banco).
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// Identificador do usuário ao qual o template pertence (UserLocalId ou UserId).
        /// </summary>
        public long UserId { get; set; }

        /// <summary>
        /// Template biométrico em formato base64/string.
        /// </summary>
        public string Template { get; set; } = string.Empty;

        /// <summary>
        /// Tipo do template (ex: 0 = digital, 1 = facial, etc).
        /// </summary>
        public int Type { get; set; }

        /// <summary>
        /// Posição do dedo (para digitais), se aplicável.
        /// </summary>
        public int FingerPosition { get; set; }

        /// <summary>
        /// Tipo do dedo (para digitais), se aplicável.
        /// </summary>
        public int FingerType { get; set; }

        /// <summary>
        /// Data/hora de criação local.
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// (Opcional) Data/hora de última atualização.
        /// </summary>
        public DateTime? UpdatedAt { get; set; }

        /// <summary>
        /// (Opcional) Início da validade do template (DateTime).
        /// </summary>
        public DateTime? BeginTime { get; set; }

        /// <summary>
        /// (Opcional) Fim da validade do template (DateTime).
        /// </summary>
        public DateTime? EndTime { get; set; }

        // Adicione outros campos conforme necessidade local.
    }
}
