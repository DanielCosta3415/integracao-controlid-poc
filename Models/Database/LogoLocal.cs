using System;

namespace Integracao.ControlID.PoC.Models.Database
{
    /// <summary>
    /// Representa um logo (logomarca) armazenado localmente no banco da aplicação.
    /// </summary>
    public class LogoLocal
    {
        /// <summary>
        /// Identificador único do logo local (ID do banco).
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// Logo em base64, URL local ou binário, conforme o armazenamento local.
        /// </summary>
        public string Base64Image { get; set; } = string.Empty;

        /// <summary>
        /// Data/hora do upload ou alteração do logo.
        /// </summary>
        public DateTime Timestamp { get; set; }

        /// <summary>
        /// (Opcional) Nome do arquivo original.
        /// </summary>
        public string FileName { get; set; } = string.Empty;

        /// <summary>
        /// (Opcional) Formato do arquivo (png, jpg, etc).
        /// </summary>
        public string Format { get; set; } = string.Empty;

        /// <summary>
        /// (Opcional) Descrição ou referência do logo.
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// Data/hora de criação local.
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// (Opcional) Data/hora de última atualização.
        /// </summary>
        public DateTime? UpdatedAt { get; set; }

        // Adicione outros campos conforme necessidade local.
    }
}
