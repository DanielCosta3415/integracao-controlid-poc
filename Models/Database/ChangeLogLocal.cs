using System;

namespace Integracao.ControlID.PoC.Models.Database
{
    /// <summary>
    /// Representa um registro de log de alteração armazenado localmente no banco da aplicação.
    /// </summary>
    public class ChangeLogLocal
    {
        /// <summary>
        /// Identificador único do log de alteração local (ID do banco).
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// Tipo da operação realizada (ex: INSERT, UPDATE, DELETE).
        /// </summary>
        public string OperationType { get; set; } = string.Empty;

        /// <summary>
        /// Nome da tabela afetada.
        /// </summary>
        public string TableName { get; set; } = string.Empty;

        /// <summary>
        /// Identificador do registro afetado na tabela.
        /// </summary>
        public long? TableId { get; set; }

        /// <summary>
        /// Data e hora da operação.
        /// </summary>
        public DateTime Timestamp { get; set; }

        /// <summary>
        /// Usuário responsável pela alteração.
        /// </summary>
        public string PerformedBy { get; set; } = string.Empty;

        /// <summary>
        /// (Opcional) Descrição da alteração.
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Data/hora de criação do registro local.
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// (Opcional) Data/hora de última atualização.
        /// </summary>
        public DateTime? UpdatedAt { get; set; }

        // Adicione outros campos conforme necessidade local.
    }
}
