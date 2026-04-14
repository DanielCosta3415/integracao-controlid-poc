using System;

namespace Integracao.ControlID.PoC.Models.ControlIDApi
{
    /// <summary>
    /// Representa um registro de log de alteração no equipamento Control iD.
    /// </summary>
    public class ChangeLog
    {
        /// <summary>
        /// Identificador único do log de alteração (ID da API ou banco).
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// Tipo da operação realizada (ex: INSERT, UPDATE, DELETE).
        /// Recomenda-se o uso de enum, se valores forem fixos.
        /// </summary>
        public string OperationType { get; set; } = string.Empty;

        /// <summary>
        /// Nome da tabela afetada pela alteração.
        /// </summary>
        public string TableName { get; set; } = string.Empty;

        /// <summary>
        /// Identificador do registro/tabela afetada (pode ser nulo).
        /// </summary>
        public long? TableId { get; set; }

        /// <summary>
        /// Data e hora da operação (DateTime ou Unix Timestamp convertido para DateTime).
        /// </summary>
        public DateTime? Timestamp { get; set; }

        /// <summary>
        /// (Opcional) Usuário responsável pela alteração.
        /// </summary>
        public string PerformedBy { get; set; } = string.Empty;

        /// <summary>
        /// (Opcional) Descrição detalhada da alteração.
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// (Opcional) Data/hora de criação local do registro.
        /// </summary>
        public DateTime? CreatedAt { get; set; }

        /// <summary>
        /// (Opcional) Data/hora de última atualização local do registro.
        /// </summary>
        public DateTime? UpdatedAt { get; set; }

        // Adicione outros campos conforme resposta da API ou necessidades do projeto.
    }

    /*
    // Sugestão opcional: Enum para tipos de operação
    public enum OperationType
    {
        Insert,
        Update,
        Delete
        // ... outros se aplicável
    }
    */
}