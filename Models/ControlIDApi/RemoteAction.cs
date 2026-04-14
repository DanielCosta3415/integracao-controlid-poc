using System;

namespace Integracao.ControlID.PoC.Models.ControlIDApi
{
    /// <summary>
    /// Representa uma ação remota enviada/executada em um equipamento Control iD.
    /// </summary>
    [Serializable]
    public class RemoteAction
    {
        /// <summary>
        /// Identificador único da ação remota (ID da API ou banco).
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// Tipo/ação disparada (ex: "open_door", "buzzer_on", "display_message", etc).
        /// </summary>
        public string Action { get; set; } = string.Empty;

        /// <summary>
        /// Parâmetros enviados para a ação (formato JSON, string, etc).
        /// Exemplos: { "door": 1 }, { "message": "Porta liberada" }
        /// </summary>
        public string Parameters { get; set; } = string.Empty;

        /// <summary>
        /// Resultado da execução (ex: "success", "fail", mensagem de erro, etc).
        /// </summary>
        public string Result { get; set; } = string.Empty;

        /// <summary>
        /// Código de status retornado pela API/dispositivo (padrão HTTP ou próprio da API).
        /// </summary>
        public int? StatusCode { get; set; }

        /// <summary>
        /// Data/hora em que a ação foi executada.
        /// </summary>
        public DateTime? ExecutedAt { get; set; }

        /// <summary>
        /// (Opcional) Usuário ou sistema que disparou a ação.
        /// </summary>
        public string PerformedBy { get; set; } = string.Empty;

        /// <summary>
        /// (Opcional) Observações ou detalhes adicionais da ação remota.
        /// </summary>
        public string Notes { get; set; } = string.Empty;

        /// <summary>
        /// (Opcional) Data/hora de criação local (auditoria).
        /// </summary>
        public DateTime? CreatedAt { get; set; }

        /// <summary>
        /// (Opcional) Data/hora de última atualização.
        /// </summary>
        public DateTime? UpdatedAt { get; set; }

        /// <summary>
        /// ToString simplificado para debugging.
        /// </summary>
        public override string ToString()
        {
            return $"RemoteAction: Id={Id}, Action={Action}, PerformedBy={PerformedBy}, Status={StatusCode}, Result={Result}";
        }

        // Adicione outros campos conforme resposta da API ou necessidades do projeto.
    }
}