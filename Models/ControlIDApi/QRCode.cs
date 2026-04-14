using System;

namespace Integracao.ControlID.PoC.Models.ControlIDApi
{
    /// <summary>
    /// Representa um QR Code cadastrado no equipamento Control iD.
    /// </summary>
    [Serializable]
    public class QRCode
    {
        /// <summary>
        /// Identificador único do QR Code (ID da API ou banco).
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// Identificador do usuário ao qual o QR Code pertence.
        /// </summary>
        public long UserId { get; set; }

        /// <summary>
        /// Valor do QR Code (string representando o código).
        /// </summary>
        public string Value { get; set; } = string.Empty;

        /// <summary>
        /// Início da validade do QR Code (Unix time ou DateTime).
        /// </summary>
        public long? BeginTime { get; set; }

        /// <summary>
        /// Fim da validade do QR Code (Unix time ou DateTime).
        /// </summary>
        public long? EndTime { get; set; }

        /// <summary>
        /// (Opcional) Data/hora de criação local.
        /// </summary>
        public DateTime? CreatedAt { get; set; }

        /// <summary>
        /// (Opcional) Data/hora de última atualização.
        /// </summary>
        public DateTime? UpdatedAt { get; set; }

        /// <summary>
        /// (Opcional) Status do QR Code (ex: ativo, expirado, bloqueado).
        /// </summary>
        public string Status { get; set; } = string.Empty;

        /// <summary>
        /// (Opcional) Observações adicionais sobre o QR Code.
        /// </summary>
        public string Notes { get; set; } = string.Empty;

        /// <summary>
        /// ToString simplificado para debugging.
        /// </summary>
        public override string ToString()
        {
            return $"QRCode: Id={Id}, UserId={UserId}, Value={Value}, Status={Status}";
        }

        // Adicione outros campos conforme resposta da API ou necessidade do projeto.
    }
}