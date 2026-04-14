using System;

namespace Integracao.ControlID.PoC.Models.ControlIDApi
{
    /// <summary>
    /// Representa um registro de log de acesso (evento) no equipamento Control iD.
    /// </summary>
    public class AccessLog
    {
        /// <summary>
        /// Identificador único do log de acesso (ID da API ou banco).
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// Data e hora do evento no equipamento (DateTime local ou convertido de UnixTime).
        /// </summary>
        public DateTime? Time { get; set; }

        /// <summary>
        /// Código do tipo de evento (por exemplo: entrada, saída, acesso negado etc.).
        /// Recomendado: mapear para um enum se possível.
        /// </summary>
        public int Event { get; set; }

        /// <summary>
        /// Identificador do dispositivo que registrou o evento.
        /// </summary>
        public long? DeviceId { get; set; }

        /// <summary>
        /// Identificador do usuário envolvido no evento.
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
        /// (Opcional) Data/hora de criação local do registro na aplicação.
        /// </summary>
        public DateTime? CreatedAt { get; set; }

        /// <summary>
        /// (Opcional) Data/hora de última atualização local do registro.
        /// </summary>
        public DateTime? UpdatedAt { get; set; }

        /// <summary>
        /// (Opcional) Unix time recebido do equipamento (caso queira armazenar o valor bruto).
        /// </summary>
        public long? UnixTime { get; set; }

        /// <summary>
        /// Retorna o tempo do evento como UnixTime, se a propriedade Time estiver definida.
        /// </summary>
        public long? TimeAsUnix
        {
            get
            {
                if (Time.HasValue)
                {
                    return ((DateTimeOffset)Time.Value).ToUnixTimeSeconds();
                }
                return null;
            }
        }

        // Adicione outros campos conforme retorno da API ou necessidades do projeto.
    }
}