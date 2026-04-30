using System;

namespace Integracao.ControlID.PoC.Models.Database
{
    /// <summary>
    /// Representa um parâmetro de configuração armazenado localmente no banco da aplicação.
    /// </summary>
    public class ConfigLocal
    {
        /// <summary>
        /// Identificador único do parâmetro de configuração local (ID do banco).
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// Nome do grupo de configuração (ex: network, system, security).
        /// </summary>
        public string Group { get; set; } = string.Empty;

        /// <summary>
        /// Nome da chave de configuração.
        /// </summary>
        public string Key { get; set; } = string.Empty;

        /// <summary>
        /// Valor atual da configuração.
        /// </summary>
        public string Value { get; set; } = string.Empty;

        /// <summary>
        /// (Opcional) Descrição do parâmetro de configuração.
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Data/hora de criação do parâmetro local.
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// (Opcional) Data/hora de última atualização.
        /// </summary>
        public DateTime? UpdatedAt { get; set; }

        // Adicione outros campos conforme necessidade local.
    }
}
