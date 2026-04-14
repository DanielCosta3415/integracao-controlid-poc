using System;

namespace Integracao.ControlID.PoC.Models.ControlIDApi
{
    /// <summary>
    /// Representa um cartão cadastrado (RFID, QRCode, etc.) no equipamento Control iD.
    /// </summary>
    public class Card
    {
        /// <summary>
        /// Identificador único do cartão (ID da API ou banco).
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// Identificador do usuário ao qual o cartão pertence.
        /// </summary>
        public long UserId { get; set; }

        /// <summary>
        /// Valor do cartão (código hexadecimal, decimal, QRCode, etc.).
        /// </summary>
        public string Value { get; set; } = string.Empty;

        /// <summary>
        /// Tipo do cartão (ex: "RFID", "QRCode", etc.).
        /// Recomenda-se usar enum em vez de string no futuro.
        /// </summary>
        public string Type { get; set; } = string.Empty;

        /// <summary>
        /// Início da validade do cartão (Unix time ou DateTime, conforme padrão da API).
        /// Opcional.
        /// </summary>
        public long? BeginTime { get; set; }

        /// <summary>
        /// Fim da validade do cartão (Unix time ou DateTime, conforme padrão da API).
        /// Opcional.
        /// </summary>
        public long? EndTime { get; set; }

        /// <summary>
        /// Data/hora de criação local (opcional).
        /// </summary>
        public DateTime? CreatedAt { get; set; }

        /// <summary>
        /// Data/hora de última atualização local (opcional).
        /// </summary>
        public DateTime? UpdatedAt { get; set; }

        /// <summary>
        /// Status do cartão (ex: "ativo", "inativo", "bloqueado", etc.).
        /// Recomenda-se usar enum em vez de string no futuro.
        /// Opcional.
        /// </summary>
        public string Status { get; set; } = string.Empty;

        // Adicione outros campos conforme resposta da API ou necessidades do projeto.
    }
}