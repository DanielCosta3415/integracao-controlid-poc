using System;

namespace Integracao.ControlID.PoC.Models.ControlIDApi
{
    /// <summary>
    /// Representa o status geral de hardware do equipamento Control iD.
    /// </summary>
    [Serializable]
    public class HardwareStatus
    {
        /// <summary>
        /// Status geral do equipamento (ex: "online", "offline", "erro").
        /// </summary>
        public string Status { get; set; } = string.Empty;

        /// <summary>
        /// Versão do firmware instalada.
        /// </summary>
        public string Firmware { get; set; } = string.Empty;

        /// <summary>
        /// Temperatura interna do equipamento (graus Celsius), se disponível.
        /// </summary>
        public int? Temperature { get; set; }

        /// <summary>
        /// Status da porta (true = aberta, false = fechada, null = desconhecido).
        /// </summary>
        public bool? DoorOpen { get; set; }

        /// <summary>
        /// Status do tamper (alarme de violação).
        /// </summary>
        public bool? Tamper { get; set; }

        /// <summary>
        /// (Opcional) Tensão de alimentação (volts), se disponível.
        /// </summary>
        public double? PowerVoltage { get; set; }

        /// <summary>
        /// (Opcional) Estado dos sensores conectados (texto informativo da API).
        /// </summary>
        public string SensorsInfo { get; set; } = string.Empty;

        /// <summary>
        /// (Opcional) Data/hora de obtenção do status.
        /// </summary>
        public DateTime? RetrievedAt { get; set; }

        // Exemplos de campos para expansão futura:
        // public Dictionary<string, bool> GpioStates { get; set; }
        // public Dictionary<string, bool> RelayStates { get; set; }

        /// <summary>
        /// Retorna uma string resumida do status para logs/debug.
        /// </summary>
        public override string ToString()
        {
            return $"Status={Status}, Firmware={Firmware}, Temp={Temperature}°C, DoorOpen={DoorOpen}, Tamper={Tamper}, PowerVoltage={PowerVoltage}V, Sensors={SensorsInfo}";
        }

        // Adicione outros campos conforme resposta da API ou necessidade do projeto.
    }
}