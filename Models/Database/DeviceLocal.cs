using System;

namespace Integracao.ControlID.PoC.Models.Database
{
    /// <summary>
    /// Representa um dispositivo (terminal, controlador, etc) armazenado localmente no banco da aplicação.
    /// </summary>
    public class DeviceLocal
    {
        /// <summary>
        /// Identificador único do dispositivo local (ID do banco).
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// Nome do dispositivo.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Endereço IP do dispositivo.
        /// </summary>
        public string Ip { get; set; } = string.Empty;

        /// <summary>
        /// Alias para compatibilidade com os mapeamentos e repositórios existentes.
        /// </summary>
        public string IpAddress
        {
            get => Ip;
            set => Ip = value;
        }

        /// <summary>
        /// Número de série do dispositivo.
        /// </summary>
        public string SerialNumber { get; set; } = string.Empty;

        /// <summary>
        /// Versão do firmware instalada no dispositivo.
        /// </summary>
        public string Firmware { get; set; } = string.Empty;

        /// <summary>
        /// Status do dispositivo (ativo, inativo, online, offline, etc).
        /// </summary>
        public string Status { get; set; } = string.Empty;

        /// <summary>
        /// Data/hora de registro do dispositivo local.
        /// </summary>
        public DateTime RegisteredAt { get; set; }

        /// <summary>
        /// Alias de compatibilidade para o nome de campo usado em partes do projeto.
        /// </summary>
        public DateTime CreatedAt
        {
            get => RegisteredAt;
            set => RegisteredAt = value;
        }

        /// <summary>
        /// (Opcional) Data/hora da última comunicação.
        /// </summary>
        public DateTime? LastSeenAt { get; set; }

        /// <summary>
        /// Alias de compatibilidade para o nome de campo usado em partes do projeto.
        /// </summary>
        public DateTime? UpdatedAt
        {
            get => LastSeenAt;
            set => LastSeenAt = value;
        }

        /// <summary>
        /// (Opcional) Observações ou descrição adicional.
        /// </summary>
        public string Description { get; set; } = string.Empty;

        // Adicione outros campos conforme necessidade do projeto local.
    }
}
