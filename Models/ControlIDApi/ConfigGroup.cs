using System;

namespace Integracao.ControlID.PoC.Models.ControlIDApi
{
    /// <summary>
    /// Representa um parâmetro de configuração (dentro de um grupo) do equipamento Control iD.
    /// </summary>
    public class ConfigGroup
    {
        /// <summary>
        /// Identificador único do parâmetro de configuração (ID da API ou banco).
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// Nome do grupo de configuração (ex: network, access, system, etc).
        /// </summary>
        public string Group { get; set; } = string.Empty;

        /// <summary>
        /// Nome da chave do parâmetro de configuração.
        /// </summary>
        public string Key { get; set; } = string.Empty;

        /// <summary>
        /// Valor atual do parâmetro de configuração.
        /// </summary>
        public string Value { get; set; } = string.Empty;

        /// <summary>
        /// (Opcional) Descrição do parâmetro de configuração.
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// (Opcional) Data/hora de criação local do registro.
        /// </summary>
        public DateTime? CreatedAt { get; set; }

        /// <summary>
        /// (Opcional) Data/hora de última atualização local.
        /// </summary>
        public DateTime? UpdatedAt { get; set; }

        // (Opcional) Nome do usuário que realizou a última alteração
        public string ModifiedBy { get; set; } = string.Empty;

        /// <summary>
        /// (Opcional) Status do parâmetro (ex: ativo, inativo).
        /// </summary>
        public string Status { get; set; } = string.Empty;

        // Adicione outros campos conforme resposta da API ou necessidade do projeto.
    }
}
