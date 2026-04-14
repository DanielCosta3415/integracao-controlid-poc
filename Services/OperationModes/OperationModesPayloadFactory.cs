namespace Integracao.ControlID.PoC.Services.OperationModes
{
    public sealed class OperationModesPayloadFactory
    {
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

        public object BuildProSettings(long serverId, bool extractTemplate, int maxRequestAttempts)
        {
            return BuildOnlineProfileSettings(serverId, localIdentification: true, extractTemplate, maxRequestAttempts);
        }

        public object BuildEnterpriseSettings(long serverId, bool extractTemplate, int maxRequestAttempts)
        {
            return BuildOnlineProfileSettings(serverId, localIdentification: false, extractTemplate, maxRequestAttempts);
        }

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
