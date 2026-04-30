using System;

namespace Integracao.ControlID.PoC.Models.ControlIDApi
{
    /// <summary>
    /// Representa uma regra de acesso no equipamento Control iD.
    /// </summary>
    public class AccessRule
    {
        /// <summary>
        /// Identificador único da regra de acesso (ID da API ou banco).
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// Nome ou descrição da regra de acesso.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Tipo da regra (ex: horário, grupo, restrição, etc).
        /// Recomenda-se mapear para um enum, se possível.
        /// </summary>
        public int Type { get; set; }

        /// <summary>
        /// Prioridade da regra (menor valor = maior prioridade).
        /// </summary>
        public int Priority { get; set; }

        /// <summary>
        /// (Opcional) Data/hora de início da validade da regra.
        /// </summary>
        public DateTime? BeginTime { get; set; }

        /// <summary>
        /// (Opcional) Data/hora de fim da validade da regra.
        /// </summary>
        public DateTime? EndTime { get; set; }

        /// <summary>
        /// (Opcional) Status da regra (ex: ativa, inativa, expirada).
        /// Recomenda-se mapear para um enum, se possível.
        /// </summary>
        public string Status { get; set; } = string.Empty;

        /// <summary>
        /// (Opcional) Data/hora de criação local do registro.
        /// </summary>
        public DateTime? CreatedAt { get; set; }

        /// <summary>
        /// (Opcional) Data/hora da última atualização local do registro.
        /// </summary>
        public DateTime? UpdatedAt { get; set; }

        // Adicione outros campos conforme resposta da API ou necessidade do projeto.
    }
}
