using System;

namespace Integracao.ControlID.PoC.Models.ControlIDApi
{
    /// <summary>
    /// Representa uma sessão autenticada com um equipamento Control iD.
    /// </summary>
    public class SessionInfo
    {
        /// <summary>
        /// Endereço IP ou domínio (mais porta) do equipamento Control iD.
        /// </summary>
        public string DeviceAddress { get; set; } = string.Empty;

        /// <summary>
        /// String de sessão autenticada (token/session retornado pelo login.fcgi).
        /// </summary>
        public string SessionString { get; set; } = string.Empty;

        /// <summary>
        /// Data e hora em que a sessão foi criada/autenticada.
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// (Opcional) Data e hora de expiração da sessão, se houver.
        /// </summary>
        public DateTime? ExpiresAt { get; set; }

        /// <summary>
        /// (Opcional) Nome/modelo do equipamento autenticado.
        /// </summary>
        public string? DeviceName { get; set; }

        /// <summary>
        /// (Opcional) Número de série do equipamento autenticado.
        /// </summary>
        public string? DeviceSerial { get; set; }

        /// <summary>
        /// (Opcional) Nome do usuário autenticado.
        /// </summary>
        public string? Username { get; set; }

        /// <summary>
        /// (Calculado) Indica se a sessão está ativa (token não nulo e não expirado).
        /// </summary>
        public bool IsActive
        {
            get
            {
                if (string.IsNullOrWhiteSpace(SessionString))
                    return false;
                if (ExpiresAt.HasValue && ExpiresAt.Value < DateTime.UtcNow)
                    return false;
                return true;
            }
        }

        // Adicione aqui outros campos conforme evoluir a resposta da API (ex: perfil, permissões, etc).
    }
}