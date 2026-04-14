using System;

namespace Integracao.ControlID.PoC.Models.Database
{
    /// <summary>
    /// Representa um registro de sincronização entre o banco local e o dispositivo Control iD.
    /// </summary>
    public class SyncLocal
    {
        /// <summary>
        /// Identificador único da sincronização local (ID do banco).
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// Tipo da sincronização (ex: upload, download, full, incremental, etc).
        /// </summary>
        public string SyncType { get; set; } = string.Empty;

        /// <summary>
        /// Status da sincronização (ex: success, fail, pending).
        /// </summary>
        public string Status { get; set; } = string.Empty;

        /// <summary>
        /// Mensagem de resultado ou log detalhado da sincronização.
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// Data/hora de início da sincronização.
        /// </summary>
        public DateTime StartedAt { get; set; }

        /// <summary>
        /// (Opcional) Data/hora de término da sincronização.
        /// </summary>
        public DateTime? FinishedAt { get; set; }

        /// <summary>
        /// (Opcional) Código de erro, se falhou.
        /// </summary>
        public string ErrorCode { get; set; } = string.Empty;

        /// <summary>
        /// (Opcional) Dados adicionais (payload, diff, parâmetros, etc).
        /// </summary>
        public string AdditionalData { get; set; } = string.Empty;

        /// <summary>
        /// Data/hora de criação local do registro de sincronização.
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// (Opcional) Data/hora de última atualização.
        /// </summary>
        public DateTime? UpdatedAt { get; set; }

        // Adicione outros campos conforme necessidade local.
    }
}
