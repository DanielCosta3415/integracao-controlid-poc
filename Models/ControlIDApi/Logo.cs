using System;

namespace Integracao.ControlID.PoC.Models.ControlIDApi
{
    /// <summary>
    /// Representa o logo (logomarca) armazenado no equipamento Control iD.
    /// </summary>
    [Serializable]
    public class Logo
    {
        /// <summary>
        /// Identificador único do logo (ID da API ou banco).
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// Logo codificado em base64, URL ou binário, conforme resposta da API.
        /// </summary>
        public string Base64Image { get; set; } = string.Empty;

        /// <summary>
        /// Data/hora do upload ou alteração do logo (Unix Time ou DateTime).
        /// </summary>
        public DateTime? Timestamp { get; set; }

        /// <summary>
        /// (Opcional) Nome do arquivo original (caso disponível).
        /// </summary>
        public string FileName { get; set; } = string.Empty;

        /// <summary>
        /// (Opcional) Formato do arquivo (ex: "png", "jpg").
        /// </summary>
        public string Format { get; set; } = string.Empty;

        /// <summary>
        /// (Opcional) Descrição ou referência do logo.
        /// </summary>
        public string Description { get; set; } = string.Empty;

        /// <summary>
        /// (Opcional) Tamanho do arquivo em bytes (caso a API forneça).
        /// </summary>
        public long? FileSize { get; set; }

        /// <summary>
        /// (Opcional) Data/hora de criação local.
        /// </summary>
        public DateTime? CreatedAt { get; set; }

        /// <summary>
        /// (Opcional) Data/hora de última atualização.
        /// </summary>
        public DateTime? UpdatedAt { get; set; }

        /// <summary>
        /// Retorna uma string resumida do logo para debug/log.
        /// </summary>
        public override string ToString()
        {
            return $"Logo Id={Id}, FileName={FileName}, Format={Format}, Size={FileSize}, Timestamp={Timestamp}, Description={Description}";
        }

        // Adicione outros campos conforme resposta da API ou necessidades do projeto.
    }
}
