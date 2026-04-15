namespace Integracao.ControlID.PoC.Services.OperationModes
{
    public sealed class OperationModesPayloadFactory
    {
        /// <summary>
        /// Monta o payload oficial que desliga o modo online e preserva identificacao local.
        /// </summary>
        /// <returns>Objeto serializavel para `set-configuration` no perfil Standalone.</returns>
        public object BuildStandaloneSettings()
        {
            return new
            {
                general = new
                {
                    online = "0",
                    local_identification = "1"
                }
            };
        }

        /// <summary>
        /// Monta o payload oficial do modo Pro, com online ativo e identificacao local ligada.
        /// </summary>
        /// <param name="serverId">Identificador do servidor online cadastrado no equipamento.</param>
        /// <param name="extractTemplate">Indica se o equipamento deve extrair template durante o fluxo online.</param>
        /// <param name="maxRequestAttempts">Quantidade maxima de tentativas de comunicacao online.</param>
        /// <returns>Objeto serializavel para `set-configuration` no perfil Pro.</returns>
        public object BuildProSettings(long serverId, bool extractTemplate, int maxRequestAttempts)
        {
            return BuildOnlineProfileSettings(serverId, localIdentification: true, extractTemplate, maxRequestAttempts);
        }

        /// <summary>
        /// Monta o payload oficial do modo Enterprise, com online ativo e identificacao centralizada no servidor.
        /// </summary>
        /// <param name="serverId">Identificador do servidor online cadastrado no equipamento.</param>
        /// <param name="extractTemplate">Indica se o equipamento deve extrair template durante o fluxo online.</param>
        /// <param name="maxRequestAttempts">Quantidade maxima de tentativas de comunicacao online.</param>
        /// <returns>Objeto serializavel para `set-configuration` no perfil Enterprise.</returns>
        public object BuildEnterpriseSettings(long serverId, bool extractTemplate, int maxRequestAttempts)
        {
            return BuildOnlineProfileSettings(serverId, localIdentification: false, extractTemplate, maxRequestAttempts);
        }

        /// <summary>
        /// Monta a estrutura comum dos perfis online usada por Pro e Enterprise.
        /// </summary>
        /// <param name="serverId">Identificador do servidor online.</param>
        /// <param name="localIdentification">Define se a identificacao local permanece ativa.</param>
        /// <param name="extractTemplate">Define se o template deve ser extraido no fluxo online.</param>
        /// <param name="maxRequestAttempts">Numero maximo de tentativas online.</param>
        /// <returns>Objeto serializavel para `set-configuration`.</returns>
        public object BuildOnlineProfileSettings(
            long serverId,
            bool localIdentification,
            bool extractTemplate,
            int maxRequestAttempts)
        {
            return new
            {
                general = new
                {
                    online = "1",
                    local_identification = BoolString(localIdentification)
                },
                online_client = new
                {
                    server_id = serverId.ToString(),
                    extract_template = BoolString(extractTemplate),
                    max_request_attempts = maxRequestAttempts.ToString()
                }
            };
        }

        /// <summary>
        /// Monta o objeto `devices` usado para criar um servidor online e obter um server_id.
        /// </summary>
        /// <param name="name">Nome amigavel do servidor online.</param>
        /// <param name="address">URL publica ou endereco acessivel pelo equipamento.</param>
        /// <param name="publicKey">Chave publica opcional usada pela topologia online.</param>
        /// <returns>Objeto serializavel para `create-objects`.</returns>
        public object BuildOnlineServerDefinition(string name, string address, string publicKey)
        {
            return new
            {
                @object = "devices",
                values = new[]
                {
                    new
                    {
                        name,
                        ip = address,
                        public_key = publicKey
                    }
                }
            };
        }

        private static string BoolString(bool value) => value ? "1" : "0";
    }
}
