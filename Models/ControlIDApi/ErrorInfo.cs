using System;

namespace Integracao.ControlID.PoC.Models.ControlIDApi
{
    /// <summary>
    /// Representa informações detalhadas sobre um erro ocorrido na aplicação ou integração.
    /// </summary>
    [Serializable]
    public class ErrorInfo
    {
        /// <summary>
        /// Identificador único do erro (GUID ou ID incremental).
        /// </summary>
        public Guid ErrorId { get; set; }

        /// <summary>
        /// Mensagem principal do erro.
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// Stack trace do erro (detalhamento técnico).
        /// </summary>
        public string StackTrace { get; set; } = string.Empty;

        /// <summary>
        /// Data/hora UTC em que o erro ocorreu.
        /// </summary>
        public DateTime OccurredAt { get; set; }

        /// <summary>
        /// Caminho ou rota da requisição que gerou o erro.
        /// </summary>
        public string Path { get; set; } = string.Empty;

        /// <summary>
        /// Código de status HTTP, se aplicável.
        /// </summary>
        public int? StatusCode { get; set; }

        /// <summary>
        /// (Opcional) Mensagem da exceção interna, caso exista (InnerException).
        /// </summary>
        public string InnerException { get; set; } = string.Empty;

        /// <summary>
        /// (Opcional) Nome do usuário logado quando o erro ocorreu.
        /// </summary>
        public string User { get; set; } = string.Empty;

        /// <summary>
        /// (Opcional) Dados adicionais para troubleshooting.
        /// </summary>
        public string AdditionalData { get; set; } = string.Empty;

        /// <summary>
        /// Sobrescreve ToString para facilitar o log textual do erro.
        /// </summary>
        public override string ToString()
        {
            return $"[{OccurredAt:O}] ErrorId={ErrorId}, Status={StatusCode}, Path={Path}, User={User}, Message={Message}, Inner={InnerException}, StackTrace={StackTrace}";
        }

        // Adicione outros campos conforme necessidade do seu projeto.
    }
}
