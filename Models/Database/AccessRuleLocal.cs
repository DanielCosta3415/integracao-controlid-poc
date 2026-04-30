using System;

namespace Integracao.ControlID.PoC.Models.Database
{
    /// <summary>
    /// Representa uma regra de acesso armazenada localmente no banco da aplicação.
    /// </summary>
    public class AccessRuleLocal
    {
        /// <summary>
        /// Identificador único da regra de acesso local (ID do banco).
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// Nome ou descrição da regra de acesso.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Tipo de regra (ex: horário, grupo, restrição, etc).
        /// </summary>
        public int Type { get; set; }

        /// <summary>
        /// Prioridade da regra (menor valor = maior prioridade).
        /// </summary>
        public int Priority { get; set; }

        /// <summary>
        /// Data/hora de início da validade.
        /// </summary>
        public DateTime? BeginTime { get; set; }

        /// <summary>
        /// Data/hora de fim da validade.
        /// </summary>
        public DateTime? EndTime { get; set; }

        /// <summary>
        /// Status da regra (ativa, inativa, expirou, etc).
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
