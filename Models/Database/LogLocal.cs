using System;

namespace Integracao.ControlID.PoC.Models.Database
{
    /// <summary>
    /// Representa um registro de log genérico armazenado localmente no banco da aplicação.
    /// </summary>
    public class LogLocal
    {
        /// <summary>
        /// Identificador único do log local (ID do banco).
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// Nível do log (ex: Information, Warning, Error, Debug).
        /// </summary>
        public string Level { get; set; } = string.Empty;

        /// <summary>
        /// Mensagem principal do log.
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// Data/hora do registro do log.
        /// </summary>
        public DateTime Timestamp { get; set; }

        /// <summary>
        /// (Opcional) Stack trace ou detalhes técnicos do log, se aplicável.
        /// </summary>
        public string StackTrace { get; set; } = string.Empty;

        /// <summary>
        /// (Opcional) Nome do usuário logado quando o evento foi registrado.
        /// </summary>
        public string User { get; set; } = string.Empty;

        /// <summary>
        /// (Opcional) Código ou categoria do evento.
        /// </summary>
        public string EventCode { get; set; } = string.Empty;

        /// <summary>
        /// (Opcional) Origem do log (controller, serviço, webhook, etc).
        /// </summary>
        public string Source { get; set; } = string.Empty;

        /// <summary>
        /// (Opcional) Dados adicionais para troubleshooting.
        /// </summary>
        public string AdditionalData { get; set; } = string.Empty;

        /// <summary>
        /// Data/hora de criação local do log.
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// (Opcional) Data/hora de última atualização.
        /// </summary>
        public DateTime? UpdatedAt { get; set; }

        // Adicione outros campos conforme necessidade local.
    }
}

