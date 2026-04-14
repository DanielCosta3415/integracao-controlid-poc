using System;

namespace Integracao.ControlID.PoC.Models.ControlIDApi
{
    /// <summary>
    /// Representa informações gerais do sistema retornadas pelo equipamento Control iD.
    /// </summary>
    public class SystemInfo
    {
        /// <summary>
        /// Número de série do equipamento.
        /// </summary>
        public string Serial { get; set; } = string.Empty;

        /// <summary>
        /// Versão do firmware do equipamento (ex: "1.2.3").
        /// </summary>
        public string Version { get; set; } = string.Empty;

        /// <summary>
        /// Modelo do equipamento (ex: "iDAccess Pro").
        /// </summary>
        public string Model { get; set; } = string.Empty;

        /// <summary>
        /// Hostname configurado no equipamento.
        /// </summary>
        public string Hostname { get; set; } = string.Empty;

        /// <summary>
        /// Informações detalhadas do firmware (ex: build, data/hora do build).
        /// </summary>
        public string Firmware { get; set; } = string.Empty;

        /// <summary>
        /// Uptime do equipamento (tempo desde o último boot, pode vir como string "10 days, 2:01:45").
        /// </summary>
        public string Uptime { get; set; } = string.Empty;

        /// <summary>
        /// (Opcional) Data/hora de obtenção dessas informações.
        /// </summary>
        public DateTime? RetrievedAt { get; set; }

        /// <summary>
        /// (Opcional) Informações adicionais recebidas da API.
        /// </summary>
        public string? AdditionalInfo { get; set; }

        // Se o uptime vier como número de segundos, pode expor também:
        // public long? UptimeSeconds { get; set; }
        // public TimeSpan? UptimeTimeSpan => UptimeSeconds.HasValue ? TimeSpan.FromSeconds(UptimeSeconds.Value) : (TimeSpan?)null;

        // Adicione outros campos conforme resposta da API ou necessidades do projeto.
    }
}