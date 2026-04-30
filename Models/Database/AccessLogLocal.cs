using System;

namespace Integracao.ControlID.PoC.Models.Database
{
    /// <summary>
    /// Representa um registro de log de acesso armazenado localmente no banco da aplicação.
    /// </summary>
    public class AccessLogLocal
    {
        /// <summary>
        /// Identificador único do log local (ID do banco).
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// Data e hora do evento de acesso.
        /// </summary>
        public DateTime Time { get; set; }

        /// <summary>
        /// Código do evento (ex: entrada, saída, acesso negado, etc).
        /// </summary>
        public int Event { get; set; }

        /// <summary>
        /// Identificador do dispositivo que registrou o evento (DeviceLocalId ou DeviceId).
        /// </summary>
        public long? DeviceId { get; set; }

        /// <summary>
        /// Identificador do usuário envolvido no evento (UserLocalId ou UserId).
        /// </summary>
        public long? UserId { get; set; }

        /// <summary>
        /// Identificador do portal/acesso (porta/catraca), se aplicável.
        /// </summary>
        public int? PortalId { get; set; }

        /// <summary>
        /// Campo livre para informações adicionais (ex: descrição, erro, status).
        /// </summary>
        public string Info { get; set; } = string.Empty;

        /// <summary>
        /// Data/hora de criação do registro local.
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// (Opcional) Data/hora de última atualização.
        /// </summary>
        public DateTime? UpdatedAt { get; set; }

        // Adicione outros campos conforme necessidade local.
    }
}
