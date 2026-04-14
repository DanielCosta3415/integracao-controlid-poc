using System;

namespace Integracao.ControlID.PoC.Models.ControlIDApi
{
    /// <summary>
    /// Representa um dispositivo (controlador, terminal, etc.) cadastrado no ambiente Control iD.
    /// </summary>
    public class Device
    {
        /// <summary>
        /// Identificador único do dispositivo (ID da API ou banco).
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// Nome do dispositivo.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Endereço IP do dispositivo.
        /// </summary>
        public string IpAddress { get; set; } = string.Empty; // Alterado de "Ip" para maior clareza

        /// <summary>
        /// (Opcional) Porta TCP usada pelo dispositivo (caso aplicável).
        /// </summary>
        public int? Port { get; set; }

        /// <summary>
        /// (Opcional) Endereço MAC do dispositivo (caso aplicável).
        /// </summary>
        public string MacAddress { get; set; } = string.Empty;

        /// <summary>
        /// Número de série do dispositivo.
        /// </summary>
        public string SerialNumber { get; set; } = string.Empty;

        /// <summary>
        /// Modelo do dispositivo (ex: iDAccess, iDFace, etc).
        /// </summary>
        public string Model { get; set; } = string.Empty;

        /// <summary>
        /// Versão do firmware instalado.
        /// </summary>
        public string Firmware { get; set; } = string.Empty;

        /// <summary>
        /// (Opcional) Localização física ou setor onde o dispositivo está instalado.
        /// </summary>
        public string Location { get; set; } = string.Empty;

        /// <summary>
        /// Status do dispositivo (ex: ativo, inativo, online, offline, etc).
        /// </summary>
        public string Status { get; set; } = string.Empty;

        /// <summary>
        /// (Opcional) Data/hora de registro do dispositivo no sistema.
        /// </summary>
        public DateTime? RegisteredAt { get; set; }

        /// <summary>
        /// (Opcional) Data/hora da última comunicação do dispositivo.
        /// </summary>
        public DateTime? LastSeenAt { get; set; }

        /// <summary>
        /// (Opcional) Observações ou descrição adicional.
        /// </summary>
        public string Description { get; set; } = string.Empty;

        // Adicione outros campos conforme resposta da API ou necessidade do projeto.
    }
}
