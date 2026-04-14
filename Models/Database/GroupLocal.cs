using System;

namespace Integracao.ControlID.PoC.Models.Database
{
    /// <summary>
    /// Representa um grupo de usuários armazenado localmente no banco da aplicação.
    /// </summary>
    public class GroupLocal
    {
        /// <summary>
        /// Identificador único do grupo local (ID do banco).
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// Nome do grupo.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// (Opcional) Descrição do grupo.
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// (Opcional) Status do grupo (ativo, inativo, etc).
        /// </summary>
        public string Status { get; set; } = string.Empty;

        /// <summary>
        /// Data/hora de criação do grupo local.
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// (Opcional) Data/hora de última atualização.
        /// </summary>
        public DateTime? UpdatedAt { get; set; }

        // Adicione outros campos conforme necessidade local.
    }
}
