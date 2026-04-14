using System;

namespace Integracao.ControlID.PoC.Models.Database
{
    /// <summary>
    /// Representa uma sessão autenticada armazenada localmente no banco da aplicação.
    /// </summary>
    public class SessionLocal
    {
        /// <summary>
        /// Identificador único da sessão local (ID do banco).
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// Endereço IP ou domínio + porta do equipamento Control iD.
        /// </summary>
        public string DeviceAddress { get; set; } = string.Empty;

        /// <summary>
        /// String de sessão autenticada (token/session retornado pelo login.fcgi).
        /// </summary>
        public string SessionString { get; set; } = string.Empty;

        /// <summary>
        /// Nome/modelo do equipamento autenticado.
        /// </summary>
        public string DeviceName { get; set; } = string.Empty;

        /// <summary>
        /// Número de série do equipamento.
        /// </summary>
        public string DeviceSerial { get; set; } = string.Empty;

        /// <summary>
        /// Usuário que autenticou a sessão.
        /// </summary>
        public string Username { get; set; } = string.Empty;

        /// <summary>
        /// Data/hora em que a sessão foi criada/autenticada.
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// (Opcional) Data/hora de expiração da sessão.
        /// </summary>
        public DateTime? ExpiresAt { get; set; }

        /// <summary>
        /// (Opcional) Flag de sessão ativa.
        /// </summary>
        public bool IsActive { get; set; }

        /// <summary>
        /// (Opcional) Data/hora de última atualização.
        /// </summary>
        public DateTime? UpdatedAt { get; set; }

        // Adicione outros campos conforme necessidade local.
    }
}
