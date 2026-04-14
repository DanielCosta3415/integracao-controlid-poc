using System.Linq;

namespace Integracao.ControlID.PoC.Services.ControlIDApi
{
    public sealed class OfficialApiDocumentationSeedCatalog
    {
        private static readonly Dictionary<string, string> FieldGlossary = new(StringComparer.OrdinalIgnoreCase)
        {
            ["login"] = "Usuario utilizado para autenticar no equipamento.",
            ["password"] = "Senha enviada ao equipamento ou usada para gerar hash ou upgrade.",
            ["session"] = "Token de sessao ativo do equipamento.",
            ["object"] = "Nome tecnico do objeto oficial trabalhado pela Access API.",
            ["values"] = "Conjunto de valores enviados para criacao ou atualizacao.",
            ["where"] = "Filtro usado para localizar os registros alvo da operacao.",
            ["id"] = "Identificador interno do registro ou slot no equipamento.",
            ["user_id"] = "Identificador interno do usuario no equipamento.",
            ["user_name"] = "Nome apresentado pelo equipamento durante a operacao.",
            ["registration"] = "Matricula ou codigo externo do usuario.",
            ["portal_id"] = "Portal, porta ou passagem fisica alvo da operacao.",
            ["event"] = "Codigo do evento ou nome do evento reconhecido pela API.",
            ["events"] = "Conjunto de eventos enviados ao equipamento ou usados em relatorios.",
            ["actions"] = "Lista de acoes remotas que o equipamento deve executar.",
            ["action"] = "Nome da acao remota a ser executada.",
            ["parameters"] = "Parametros textuais especificos da acao escolhida.",
            ["type"] = "Tipo da operacao, credencial, midia ou coluna usada pelo endpoint.",
            ["host"] = "Host ou endereco alvo do teste de conectividade.",
            ["port"] = "Porta TCP ou UDP usada pelo teste de conectividade.",
            ["message"] = "Texto exibido na tela do equipamento.",
            ["timeout"] = "Tempo de duracao da acao ou mensagem, normalmente em milissegundos.",
            ["frequency"] = "Frequencia do buzzer em Hz.",
            ["duty_cycle"] = "Percentual de ciclo ativo aplicado ao buzzer.",
            ["gpio"] = "Identificador do GPIO consultado ou alterado.",
            ["day"] = "Dia do mes usado para ajustar o relogio do equipamento.",
            ["month"] = "Mes usado para ajustar o relogio do equipamento.",
            ["year"] = "Ano usado para ajustar o relogio do equipamento.",
            ["hour"] = "Hora usada para ajustar o relogio do equipamento.",
            ["minute"] = "Minuto usado para ajustar o relogio do equipamento.",
            ["second"] = "Segundo usado para ajustar o relogio do equipamento.",
            ["keep_network_info"] = "Indica se as configuracoes de rede devem ser preservadas no reset.",
            ["custom_video_enabled"] = "Liga ou desliga o modo de propaganda com video personalizado.",
            ["frame_type"] = "Define o tipo de quadro capturado pela camera.",
            ["camera"] = "Seleciona a camera RGB ou IR quando a captura suportar multiplos sensores.",
            ["match"] = "Quando ativo, forca validacao adicional contra faces ja cadastradas.",
            ["user_images"] = "Lista de imagens faciais associadas a usuarios.",
            ["image"] = "Conteudo binario ou base64 da imagem enviada.",
            ["timestamp"] = "Momento da geracao da imagem ou do lote facial.",
            ["target"] = "Destino da chamada SIP ou do recurso acionado.",
            ["stop"] = "Se verdadeiro, interrompe o alarme atual em vez de apenas consultar status.",
            ["interlock_enabled"] = "Habilita ou desabilita o intertravamento em rede.",
            ["api_bypass_enabled"] = "Permite ou bloqueia bypass do intertravamento por API.",
            ["rex_bypass_enabled"] = "Permite ou bloqueia bypass do intertravamento por REX.",
            ["current"] = "Numero do fragmento atual ao enviar arquivos em multiplas partes.",
            ["total"] = "Quantidade total de fragmentos esperados pelo equipamento.",
            ["mode"] = "Modo adicional da operacao, normalmente enviado na query string.",
            ["match_duplicates"] = "Controla se rostos duplicados devem ser rejeitados."
        };

        private static readonly Dictionary<string, OfficialApiEndpointDocumentationSeed> EndpointSeeds = new(StringComparer.OrdinalIgnoreCase)
        {
            ["login"] = new(
                RequestGuidance: "Envie credenciais validas do equipamento para abrir uma sessao nova.",
                ResponseGuidance: "O retorno esperado e um JSON com token ou informacao equivalente de sessao.",
                Body: [
                    new("login", "string", "Obrigatorio", "Usuario administrativo do equipamento.", "<usuario>", true),
                    new("password", "string", "Obrigatorio", "Senha correspondente ao usuario informado.", "<senha>", true)
                ]),
            ["remote-user-authorization"] = new(
                RequestGuidance: "Use este endpoint para responder a um evento online e decidir se o acesso sera autorizado.",
                ResponseGuidance: "O equipamento responde com o resultado da decisao aplicada e o status da acao remota.",
                Body: [
                    new("event", "integer", "Obrigatorio", "Codigo do evento online recebido pelo servidor.", "7", true),
                    new("user_id", "integer", "Obrigatorio", "ID do usuario reconhecido na logica do servidor.", "6", true),
                    new("user_name", "string", "Obrigatorio", "Nome exibido ao operador e ao equipamento.", "Ada Lovelace", true),
                    new("user_image", "boolean", "Opcional", "Informa se a interface do equipamento deve indicar presenca de imagem do usuario.", "false", true),
                    new("portal_id", "integer", "Obrigatorio", "Portal fisico onde a decisao de acesso sera aplicada.", "1", true),
                    new("actions", "array<object>", "Obrigatorio", "Lista de acoes que o equipamento deve executar ao concluir a autorizacao.", "[{...}]", true),
                    new("actions[].action", "string", "Obrigatorio", "Acao remota a ser disparada, como door, catra, collector ou sec_box.", "door", true),
                    new("actions[].parameters", "string", "Obrigatorio", "Parametros textuais exigidos pela acao escolhida.", "door=1", true)
                ]),
            ["execute-actions"] = new(
                RequestGuidance: "Monte uma lista simples de acoes remotas e parametros para disparo imediato.",
                ResponseGuidance: "O equipamento devolve o status de execucao de cada acao pedida.",
                Body: [
                    new("actions", "array<object>", "Obrigatorio", "Acoes remotas que a PoC deseja executar.", "[{...}]", true),
                    new("actions[].action", "string", "Obrigatorio", "Nome da acao remota suportada pelo equipamento.", "door", true),
                    new("actions[].parameters", "string", "Obrigatorio", "Parametros da acao no formato esperado pela Access API.", "door=1", true)
                ]),
            ["remote-enroll"] = new(
                RequestGuidance: "Use quando a PoC precisar iniciar cadastro remoto de face, biometria, cartao, PIN ou senha.",
                ResponseGuidance: "A confirmacao real costuma chegar por callbacks ou monitor, dependendo do modo configurado.",
                Body: [
                    new("type", "string", "Obrigatorio", "Tipo de credencial ou captura remota a ser iniciada.", "face", true),
                    new("user_id", "integer", "Obrigatorio", "Usuario que recebera o cadastro remoto.", "123", true),
                    new("save", "boolean", "Opcional", "Indica se o equipamento deve persistir o resultado capturado.", "true", true),
                    new("sync", "boolean", "Opcional", "Solicita sincronizacao imediata do cadastro apos a captura.", "true", true)
                ]),
            ["connection-test"] = new(
                RequestGuidance: "Execute um teste de conectividade do equipamento ate um host e porta especificos.",
                ResponseGuidance: "O retorno costuma incluir latencia, sucesso da conexao ou detalhes da falha.",
                Body: [
                    new("host", "string", "Obrigatorio", "Host ou IP de destino do teste.", "8.8.8.8", true),
                    new("port", "integer", "Obrigatorio", "Porta de destino usada pelo teste.", "53", true)
                ]),
            ["ping-test"] = new(
                RequestGuidance: "Use para validar se o equipamento consegue alcancar um host via rede.",
                ResponseGuidance: "O resultado normalmente mostra sucesso, tempo ou erro de resolucao.",
                Body: [
                    new("host", "string", "Obrigatorio", "Host ou IP alvo do ping.", "8.8.8.8", true)
                ]),
            ["nslookup-test"] = new(
                RequestGuidance: "Use para confirmar resolucao DNS a partir do equipamento.",
                ResponseGuidance: "O resultado costuma listar o IP resolvido ou a falha de DNS.",
                Body: [
                    new("host", "string", "Obrigatorio", "Dominio que deve ser resolvido pelo equipamento.", "www.controlid.com.br", true)
                ]),
            ["set-network-interlock"] = new(
                RequestGuidance: "Configure o intertravamento em rede e os bypasses permitidos pelo equipamento.",
                ResponseGuidance: "O retorno confirma a aplicacao da configuracao enviada.",
                Body: [
                    new("interlock_enabled", "integer", "Obrigatorio", "Use 1 para habilitar e 0 para desabilitar o intertravamento em rede.", "1", true),
                    new("api_bypass_enabled", "integer", "Obrigatorio", "Use 1 para permitir bypass por API e 0 para bloquear.", "0", true),
                    new("rex_bypass_enabled", "integer", "Obrigatorio", "Use 1 para permitir bypass por REX e 0 para bloquear.", "0", true)
                ]),
            ["change-login"] = new(
                RequestGuidance: "Atualize o login local do equipamento com cuidado, pois esse dado sera exigido em acessos futuros.",
                ResponseGuidance: "Depois da troca, a sessao atual pode precisar ser renovada com as novas credenciais.",
                Body: [
                    new("login", "string", "Obrigatorio", "Novo usuario de login do equipamento.", "<novo-usuario>", true),
                    new("password", "string", "Obrigatorio", "Nova senha de login do equipamento.", "<nova-senha>", true)
                ]),
            ["set-system-time"] = new(
                RequestGuidance: "Envie todos os componentes de data e hora na mesma chamada para evitar inconsistencia no relogio do equipamento.",
                ResponseGuidance: "O retorno confirma se a data e hora foram aplicadas.",
                Body: [
                    new("day", "integer", "Obrigatorio", "Dia do mes.", "13", true),
                    new("month", "integer", "Obrigatorio", "Mes.", "4", true),
                    new("year", "integer", "Obrigatorio", "Ano completo.", "2026", true),
                    new("hour", "integer", "Obrigatorio", "Hora.", "10", true),
                    new("minute", "integer", "Obrigatorio", "Minuto.", "30", true),
                    new("second", "integer", "Obrigatorio", "Segundo.", "0", true)
                ]),
            ["save-screenshot"] = new(
                RequestGuidance: "Use para capturar imagem RGB ou IR quando o equipamento suportar camera local.",
                ResponseGuidance: "O retorno tende a trazer referencia ao arquivo gerado ou conteudo binario ou base64.",
                Body: [
                    new("frame_type", "string", "Obrigatorio", "Tipo de frame solicitado ao endpoint.", "camera", true),
                    new("camera", "string", "Condicional", "Camera alvo da captura quando o dispositivo possui multiplos sensores.", "rgb", true)
                ]),
            ["alarm-status"] = new(
                RequestGuidance: "Consulte o alarme atual ou envie stop=true para interrompe-lo.",
                ResponseGuidance: "O retorno informa o estado atual do alarme e se a interrupcao foi aceita.",
                Body: [
                    new("stop", "boolean", "Opcional", "Use true para interromper o alarme; false ou ausencia para apenas consultar.", "false", true)
                ]),
            ["make-sip-call"] = new(
                RequestGuidance: "Inicie uma chamada SIP informando o ramal ou destino desejado.",
                ResponseGuidance: "O retorno confirma se a chamada foi aceita ou rejeitada pelo dispositivo.",
                Body: [
                    new("target", "string", "Obrigatorio", "Ramal ou destino SIP que recebera a chamada.", "503", true)
                ]),
            ["user-get-image"] = new(
                QueryGuidance: "Este endpoint depende de user_id na query adicional.",
                Query: [
                    new("user_id", "integer", "Obrigatorio", "ID do usuario cuja foto deve ser baixada.", "123", true)
                ]),
            ["logo-get"] = new(
                QueryGuidance: "Informe o slot do logo na query adicional.",
                Query: [
                    new("id", "integer", "Obrigatorio", "Slot do logo a ser consultado no equipamento.", "1", true)
                ]),
            ["logo-change"] = new(
                QueryGuidance: "Informe o slot alvo na query adicional e envie o PNG como corpo binario ou base64.",
                Query: [
                    new("id", "integer", "Obrigatorio", "Slot do logo que recebera o novo arquivo.", "1", true)
                ]),
            ["logo-destroy"] = new(
                QueryGuidance: "Informe o slot alvo na query adicional.",
                Query: [
                    new("id", "integer", "Obrigatorio", "Slot do logo a ser removido.", "1", true)
                ]),
            ["send-video"] = new(
                QueryGuidance: "Este envio trabalha em blocos e usa current e total na query adicional.",
                Query: [
                    new("current", "integer", "Obrigatorio", "Indice do fragmento atual do arquivo.", "1", true),
                    new("total", "integer", "Obrigatorio", "Quantidade total de fragmentos enviados.", "2", true)
                ]),
            ["set-pjsip-audio-message"] = new(
                QueryGuidance: "O envio do WAV usa fragmentacao na query adicional.",
                Query: [
                    new("current", "integer", "Obrigatorio", "Indice do bloco atual do WAV.", "1", true),
                    new("total", "integer", "Obrigatorio", "Quantidade total de blocos do WAV.", "1", true)
                ]),
            ["set-audio-access-message"] = new(
                QueryGuidance: "Informe qual evento recebera o audio e, se necessario, o fragmento enviado.",
                Query: [
                    new("event", "string", "Obrigatorio", "Evento do iDFace que recebera o audio customizado.", "authorized", true),
                    new("current", "integer", "Obrigatorio", "Indice do bloco atual do WAV.", "1", true),
                    new("total", "integer", "Obrigatorio", "Quantidade total de blocos do WAV.", "1", true)
                ])
        };

        public OfficialApiEndpointDocumentationSeed GetSeed(string endpointId)
        {
            return EndpointSeeds.TryGetValue(endpointId, out var seed)
                ? seed
                : OfficialApiEndpointDocumentationSeed.Empty;
        }

        public string LookupDescription(string fieldName)
        {
            if (FieldGlossary.TryGetValue(fieldName, out var description))
            {
                return description;
            }

            var normalized = fieldName.Split('.').Last().Replace("[]", string.Empty);
            if (FieldGlossary.TryGetValue(normalized, out description))
            {
                return description;
            }

            return "Campo identificado pela PoC a partir do contrato tecnico ou do payload de exemplo.";
        }
    }

    public sealed record OfficialApiEndpointDocumentationSeed(
        string RequestGuidance = "",
        string ResponseGuidance = "",
        string QueryGuidance = "",
        IList<OfficialApiParameterSeed>? Query = null,
        IList<OfficialApiParameterSeed>? Body = null)
    {
        public static OfficialApiEndpointDocumentationSeed Empty { get; } = new();
        public IList<OfficialApiParameterSeed> QueryFields => Query ?? [];
        public IList<OfficialApiParameterSeed> BodyFields => Body ?? [];
    }

    public sealed record OfficialApiParameterSeed(
        string Path,
        string TypeLabel,
        string RequirementLabel,
        string Description,
        string Example,
        bool Explicit);
}
