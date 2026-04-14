using System;

namespace Integracao.ControlID.PoC.Models.Database
{
    /// <summary>
    /// Representa uma foto de usuário armazenada localmente no banco da aplicação.
    /// </summary>
    public class PhotoLocal
    {
        /// <summary>
        /// Identificador único da foto local (ID do banco).
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// Identificador do usuário ao qual a foto pertence (UserLocalId ou UserId).
        /// </summary>
        public long UserId { get; set; }

        /// <summary>
        /// Foto em base64, URL local ou binário, conforme o armazenamento local.
        /// </summary>
        public string Base64Image { get; set; } = string.Empty;

        /// <summary>
        /// Data/hora do upload ou captura da foto.
        /// </summary>
        public DateTime Timestamp { get; set; }

        /// <summary>
        /// (Opcional) Nome do arquivo original.
        /// </summary>
        public string FileName { get; set; } = string.Empty;

        /// <summary>
        /// (Opcional) Formato do arquivo (jpg, png, etc).
        /// </summary>
        public string Format { get; set; } = string.Empty;

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
