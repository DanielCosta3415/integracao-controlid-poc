using System;

namespace Integracao.ControlID.PoC.Models.ControlIDApi
{
    /// <summary>
    /// Representa um grupo de usuários/regras no equipamento Control iD.
    /// </summary>
    [Serializable]
    public class Group
    {
        /// <summary>
        /// Identificador único do grupo (ID da API ou banco).
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
        /// (Opcional) Data/hora de criação local.
        /// </summary>
        public DateTime? CreatedAt { get; set; }

        /// <summary>
        /// (Opcional) Data/hora de última atualização.
        /// </summary>
        public DateTime? UpdatedAt { get; set; }

        ///// <summary>
        ///// (Opcional) Id do grupo pai (caso grupos sejam hierárquicos).
        ///// </summary>
        //public long? ParentGroupId { get; set; }

        /// <summary>
        /// Retorna uma representação textual resumida do grupo (para debug/log).
        /// </summary>
        public override string ToString()
        {
            return $"Id={Id}, Name={Name}, Status={Status}, CreatedAt={CreatedAt}, UpdatedAt={UpdatedAt}";
        }

        // Adicione outros campos conforme resposta da API ou necessidade do projeto.
    }
}
