using System;

namespace Integracao.ControlID.PoC.Models.ControlIDApi
{
    /// <summary>
    /// Representa um template biométrico (digital, facial, etc.) de um usuário no equipamento Control iD.
    /// </summary>
    public class BiometricTemplate
    {
        /// <summary>
        /// Identificador único do template biométrico (ID da API ou banco).
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// Identificador do usuário ao qual o template pertence.
        /// </summary>
        public long UserId { get; set; }

        /// <summary>
        /// Template biométrico em formato Base64/string.
        /// </summary>
        public string Template { get; set; } = string.Empty;

        /// <summary>
        /// Tipo do template (ex: 0 = digital, 1 = facial, etc.). Sugere-se o uso de enum futuramente.
        /// </summary>
        public int Type { get; set; }

        /// <summary>
        /// Posição do dedo (para digitais). Nulo para templates que não são digitais.
        /// </summary>
        public int? FingerPosition { get; set; }

        /// <summary>
        /// Tipo do dedo (para digitais). Nulo para templates que não são digitais.
        /// </summary>
        public int? FingerType { get; set; }

        /// <summary>
        /// Início da validade do template (Unix time ou DateTime). Opcional.
        /// </summary>
        public long? BeginTime { get; set; }

        /// <summary>
        /// Fim da validade do template (Unix time ou DateTime). Opcional.
        /// </summary>
        public long? EndTime { get; set; }

        /// <summary>
        /// Data/hora de criação local. Opcional.
        /// </summary>
        public DateTime? CreatedAt { get; set; }

        /// <summary>
        /// Data/hora de última atualização local. Opcional.
        /// </summary>
        public DateTime? UpdatedAt { get; set; }

        // Adicione outros campos conforme a resposta da API ou necessidades do projeto.
    }
}
