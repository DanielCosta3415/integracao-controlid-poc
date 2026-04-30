using System;

namespace Integracao.ControlID.PoC.Models.ControlIDApi
{
    /// <summary>
    /// Representa uma foto de usuário armazenada no equipamento Control iD.
    /// </summary>
    [Serializable]
    public class Photo
    {
        /// <summary>
        /// Identificador único da foto (ID da API ou banco).
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// Identificador do usuário ao qual a foto pertence.
        /// </summary>
        public long UserId { get; set; }

        /// <summary>
        /// Foto em base64, URL, ou binário, conforme resposta da API.
        /// </summary>
        public string Base64Image { get; set; } = string.Empty;

        /// <summary>
        /// Data/hora do envio ou captura da foto (Unix Time ou DateTime).
        /// </summary>
        public DateTime? Timestamp { get; set; }

        /// <summary>
        /// (Opcional) Nome do arquivo original (caso disponível na API).
        /// </summary>
        public string FileName { get; set; } = string.Empty;

        /// <summary>
        /// (Opcional) Formato do arquivo (ex: "jpg", "png").
        /// </summary>
        public string Format { get; set; } = string.Empty;

        /// <summary>
        /// (Opcional) Status da foto (ex: "ativa", "inativa", "excluída", etc).
        /// </summary>
        public string Status { get; set; } = string.Empty;

        /// <summary>
        /// (Opcional) Descrição ou anotação associada à foto.
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// (Opcional) Tamanho do arquivo em bytes.
        /// </summary>
        public long? FileSize { get; set; }

        /// <summary>
        /// (Opcional) Data/hora de criação local no sistema.
        /// </summary>
        public DateTime? CreatedAt { get; set; }

        /// <summary>
        /// (Opcional) Data/hora de última atualização local.
        /// </summary>
        public DateTime? UpdatedAt { get; set; }

        /// <summary>
        /// ToString simplificado para debugging.
        /// </summary>
        public override string ToString()
        {
            return $"Photo: Id={Id}, UserId={UserId}, Format={Format}, FileName={FileName}, Timestamp={Timestamp}";
        }

        // Adicione outros campos conforme resposta da API ou necessidades do projeto.
    }
}
