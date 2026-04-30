using System;

namespace Integracao.ControlID.PoC.Models.ControlIDApi
{
    /// <summary>
    /// Representa um usuário cadastrado no equipamento Control iD.
    /// </summary>
    public class User
    {
        // Identidade e autenticação
        /// <summary>
        /// Identificador único do usuário (ID da API ou banco).
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// Matrícula/código do usuário.
        /// </summary>
        public string Registration { get; set; } = string.Empty;

        /// <summary>
        /// Nome completo do usuário.
        /// </summary>
        public string Name { get; set; } = string.Empty;

        // Segurança (hash, salt, tipo de usuário)
        /// <summary>
        /// Hash da senha do usuário.
        /// </summary>
        public string Password { get; set; } = string.Empty;

        /// <summary>
        /// Salt do hash de senha.
        /// </summary>
        public string Salt { get; set; } = string.Empty;

        /// <summary>
        /// Tipo de usuário (ex: admin, padrão, visitante, conforme mapeamento da API).
        /// </summary>
        public int UserTypeId { get; set; }

        // Validade de acesso
        /// <summary>
        /// Início da validade do usuário (Unix time ou DateTime).
        /// </summary>
        public long? BeginTime { get; set; }

        /// <summary>
        /// Fim da validade do usuário (Unix time ou DateTime).
        /// </summary>
        public long? EndTime { get; set; }

        // Metadados e informações opcionais
        /// <summary>
        /// (Opcional) Data/hora de criação local.
        /// </summary>
        public DateTime? CreatedAt { get; set; }

        /// <summary>
        /// (Opcional) Data/hora de última atualização.
        /// </summary>
        public DateTime? UpdatedAt { get; set; }

        /// <summary>
        /// (Opcional) E-mail do usuário.
        /// </summary>
        public string? Email { get; set; }

        /// <summary>
        /// (Opcional) Número de telefone.
        /// </summary>
        public string? Phone { get; set; }

        /// <summary>
        /// (Opcional) Status do usuário (ativo, inativo, bloqueado, etc).
        /// </summary>
        public string? Status { get; set; }

        // Ponto de extensão para campos customizados da API
        // public string? CustomField { get; set; }
    }
}
