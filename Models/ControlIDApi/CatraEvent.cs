using System;

namespace Integracao.ControlID.PoC.Models.ControlIDApi
{
    /// <summary>
    /// Representa um evento de catraca (passagem, acionamento, etc.) no equipamento Control iD.
    /// </summary>
    public class CatraEvent
    {
        /// <summary>
        /// Identificador único do evento da catraca (ID da API ou banco).
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// Direção do evento (ex: 1 = entrada, 2 = saída).
        /// Recomenda-se, se possível, usar enum para maior clareza.
        /// </summary>
        public int Direction { get; set; }

        /// <summary>
        /// Data e hora do evento (DateTime ou Unix Time convertido).
        /// </summary>
        public DateTime? Time { get; set; }

        /// <summary>
        /// Informações adicionais do evento (mensagem, status, erro, etc.).
        /// </summary>
        public string Info { get; set; } = string.Empty;

        /// <summary>
        /// (Opcional) Identificador do usuário envolvido (se aplicável).
        /// </summary>
        public long? UserId { get; set; }

        /// <summary>
        /// (Opcional) Identificador do dispositivo/catraca.
        /// </summary>
        public long? DeviceId { get; set; }

        /// <summary>
        /// (Opcional) Data/hora de criação local.
        /// </summary>
        public DateTime? CreatedAt { get; set; }

        /// <summary>
        /// (Opcional) Data/hora de última atualização local.
        /// </summary>
        public DateTime? UpdatedAt { get; set; }

        // Adicione outros campos conforme resposta da API ou necessidades do projeto.
    }

    /*
    // Sugestão opcional para maior legibilidade:
    public enum CatraEventDirection
    {
        Entrada = 1,
        Saida = 2
        // Adicione outros tipos, se necessário
    }
    */
}