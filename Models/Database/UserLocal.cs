using System;

namespace Integracao.ControlID.PoC.Models.Database
{
    /// <summary>
    /// Representa um usuário armazenado localmente no banco da aplicação (SQLite).
    /// </summary>
    public class UserLocal
    {
        /// <summary>
        /// Identificador único do usuário local (ID do banco).
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// Nome completo do usuário.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Matrícula/registro principal do usuário no ecossistema Control iD.
        /// </summary>
        public string Registration { get; set; } = string.Empty;

        /// <summary>
        /// Login ou identificador único local (quando diferente da matrícula).
        /// </summary>
        public string Username { get; set; } = string.Empty;

        /// <summary>
        /// Hash da senha do usuário local.
        /// </summary>
        public string PasswordHash { get; set; } = string.Empty;

        /// <summary>
        /// Salt utilizado no hash da senha.
        /// </summary>
        public string Salt { get; set; } = string.Empty;

        /// <summary>
        /// E-mail do usuário.
        /// </summary>
        public string Email { get; set; } = string.Empty;

        /// <summary>
        /// Telefone do usuário.
        /// </summary>
        public string Phone { get; set; } = string.Empty;

        /// <summary>
        /// Status do usuário (ativo, inativo, bloqueado, etc).
        /// </summary>
        public string Status { get; set; } = string.Empty;

        /// <summary>
        /// Papel local da PoC usado para autorizar operações administrativas.
        /// </summary>
        public string Role { get; set; } = "Operator";

        /// <summary>
        /// Data/hora de criação do usuário no sistema.
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// (Opcional) Data/hora de última atualização.
        /// </summary>
        public DateTime? UpdatedAt { get; set; }

        // Adicione outros campos conforme necessidade da aplicação local.
    }
}
