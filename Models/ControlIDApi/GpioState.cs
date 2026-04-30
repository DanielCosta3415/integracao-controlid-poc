using System;
using System.Collections.Generic;

namespace Integracao.ControlID.PoC.Models.ControlIDApi
{
    /// <summary>
    /// Representa o estado dos GPIOs (entradas e saídas digitais) do equipamento Control iD.
    /// </summary>
    [Serializable]
    public class GpioState
    {
        /// <summary>
        /// Dicionário de entradas digitais (nome/índice -> estado).
        /// </summary>
        public Dictionary<string, bool> Inputs { get; set; } = new();

        /// <summary>
        /// Dicionário de saídas digitais (nome/índice -> estado).
        /// </summary>
        public Dictionary<string, bool> Outputs { get; set; } = new();

        /// <summary>
        /// (Opcional) Data/hora de obtenção do estado.
        /// </summary>
        public DateTime? RetrievedAt { get; set; }

        /// <summary>
        /// (Opcional) Observações ou mensagem da API.
        /// </summary>
        public string Info { get; set; } = string.Empty;

        // Construtor para inicializar os dicionários e evitar nulls
        public GpioState()
        {
            Inputs = new Dictionary<string, bool>();
            Outputs = new Dictionary<string, bool>();
        }

        /// <summary>
        /// Facilita debug rápido mostrando o estado resumido dos GPIOs.
        /// </summary>
        public override string ToString()
        {
            return $"Inputs: [{string.Join(", ", Inputs)}], Outputs: [{string.Join(", ", Outputs)}], RetrievedAt: {RetrievedAt?.ToString("O") ?? "null"}, Info: {Info}";
        }

        // Adicione outros campos conforme resposta da API ou necessidade do projeto.
    }
}
