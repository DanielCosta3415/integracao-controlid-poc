using System;

namespace Integracao.ControlID.PoC.Models.ControlIDApi
{
    /// <summary>
    /// Representa o resultado de uma ação remota executada no equipamento Control iD.
    /// </summary>
    public class RemoteActionResult
    {
        /// <summary>
        /// Indica se a ação foi executada com sucesso.
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// Mensagem detalhada sobre o resultado da ação.
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// Código de status retornado (pode ser HTTP, código interno da API, etc).
        /// </summary>
        public int? Code { get; set; }

        /// <summary>
        /// (Opcional) Dados adicionais retornados pela ação (payload, objeto, etc).
        /// </summary>
        public object? Data { get; set; }

        // Adicione aqui outros campos conforme resposta real da API Control iD
    }
}
