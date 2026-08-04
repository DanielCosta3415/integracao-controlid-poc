# Perguntas frequentes sobre a PoC e a Access API

> **Documento vivo** · Público: primeiro contato, usuários, integração e operação · Responsável: produto técnico e engenharia de integração · Última validação: 2026-08-03.

Este documento responde às dúvidas mais comuns sobre o propósito, o acesso, a
rede e o funcionamento da PoC. As respostas distinguem quatro níveis de
evidência: comportamento atual do código, contrato validado com simulador,
documentação oficial e homologação pendente em equipamento físico.

Para um percurso guiado por função, use `persona-guides.md`. Para navegar por
toda a base técnica, use `README.md`.

## Produto e escopo

### 1. O que é esta PoC?

É uma aplicação ASP.NET Core 10 MVC/Razor que demonstra integração com a Access
API dos equipamentos de controle de acesso da Control iD. Ela reúne conexão,
sessão, catálogo de endpoints, objetos, ações, callbacks, Monitor, Push,
persistência SQLite e recursos de diagnóstico em uma interface única.

### 2. Qual problema ela resolve?

Ela reduz o esforço para estudar e ensaiar os contratos da API, visualizar a
direção das comunicações e validar fluxos antes de desenvolver uma integração
produtiva própria. Não elimina a necessidade de projetar regras de negócio,
segurança e operação para o ambiente final.

### 3. Quem é o público-alvo?

Desenvolvedores, integradores, administradores de controle de acesso, QA,
infraestrutura, segurança, privacidade, suporte, SRE e responsáveis por avaliar
produtos Control iD. Os percursos por perfil estão em `persona-guides.md`.

### 4. O que a PoC demonstra atualmente?

Demonstra autenticação local, conexão e login no equipamento, 96 entradas de
catálogo auditadas, operações de objetos, configurações, hardware, modos,
callbacks, Monitor, fila Push, observabilidade e estado local. Parte é exercitada
por testes/simulador; efeitos físicos continuam condicionados ao equipamento.

### 5. O que ela não implementa?

Não fornece provedor de nuvem, implantação automática, identidade corporativa,
MFA, motor biométrico Enterprise produtivo, sincronização bidirecional completa,
billing real nem política jurídica pronta. Também não substitui o procedimento
físico e o software oficial do cliente.

### 6. A PoC está pronta para produção?

Não por padrão. Produção exige equipamento e firmware homologados, rede/TLS,
cofre de segredos, backup externo, RTO/RPO, monitoramento, DPO/jurídico, capacidade
e aprovação humana. O gate estrito está descrito em
`residual-risk-closure.md`.

### 7. É possível usá-la sem equipamento físico?

Sim. O simulador em `tools/ControlIdDeviceStub` permite integração inicial e contrato local em
`http://127.0.0.1:6600`. Ele não simula com fidelidade hardware, câmera,
biometria, relés, licenças, limites ou variações de firmware.

### 8. Ela substitui o iDSecure ou uma integração direta?

Não. A PoC é uma referência técnica e exploratória. Uma comparação comercial ou
funcional com iDSecure depende do produto e do projeto; uma integração direta
continua responsável por sua arquitetura, sincronização, autorização e operação.

## Acesso local e autenticação

### 9. É necessário ter uma conta local?

Sim, para o acesso humano normal às telas. Cadastro inicial, login, páginas de
erro, verificações de saúde e ingressos do equipamento possuem exceções específicas; elas
não equivalem a acesso completo à interface.

### 10. Qual é o propósito da conta local?

Identificar a pessoa que usa a PoC e aplicar menor privilégio por papel. Ela
protege dados, configurações e ações que podem afetar o equipamento ou o estado
local.

### 11. Qual a diferença entre conta local e login Control iD?

A conta local cria um cookie para a PoC. O login Control iD envia credenciais a
`login.fcgi` e recebe uma `session` do equipamento. É necessário passar pelas duas
camadas para operar a integração completa.

### 12. Quem é o primeiro administrador?

O primeiro usuário cadastrado quando o SQLite não possui contas recebe
`Administrator`. O bootstrap é transacional; depois dele, o cadastro exige um
administrador autenticado.

### 13. O que um operador pode fazer?

Pode navegar, consultar superfícies gerais, testar/conectar um equipamento e
fazer login/logout oficial. Não pode invocar endpoints por POST, gerenciar dados,
ver detalhes sensíveis nem executar ações administrativas. A matriz exata está em
`local-account-administration.md`.

### 14. Como criar, desativar ou excluir contas?

Um administrador cria operadores em `/Auth/Register`. Não existem telas para
promover, rebaixar, desativar ou excluir contas; essas capacidades devem ser
projetadas e testadas antes de uso operacional.

### 15. Como recuperar uma senha perdida?

Não existe redefinição por e-mail ou sem a senha atual. Preserve o SQLite e siga
o procedimento de recuperação de `local-account-administration.md`; não edite
hash ou papel diretamente para contornar o login.

### 16. Quando as sessões expiram?

O cookie local usa 60 minutos de inatividade com renovação deslizante por padrão;
a sessão ASP.NET usa 30 minutos. A validade da sessão Control iD é definida pelo
equipamento e deve ser conferida com `session_is_valid.fcgi`.

## Instalação e rede

### 17. Quais são os pré-requisitos?

.NET SDK 8 conforme `global.json`, PowerShell e acesso ao NuGet para a primeira
restauração. Docker é opcional. Não há `npm`, `pnpm` ou `yarn` configurados.

### 18. Como instalar e executar?

Execute `dotnet restore .\Integracao.ControlID.PoC.sln --locked-mode` e depois
`dotnet run --project .\Integracao.ControlID.PoC.csproj`. O roteiro completo está
em `developer-onboarding.md`.

### 19. Onde configurar a URL e as credenciais do equipamento?

Use o painel de conexão e o login oficial ou forneça defaults por User Secrets/
variáveis de ambiente. Nunca preencha credenciais reais em `appsettings.json`,
`.env.example`, documentação ou commit.

### 20. Quais portas são usadas?

Os perfis locais documentam 5000/5001 para a PoC, 6600 para o simulador, 6700 para o
proxy e 8080 no contêiner. A porta do equipamento é configurável; confirme no
terminal em vez de assumir um default.

### 21. A PoC precisa estar na mesma rede do equipamento?

Não logicamente, mas precisa haver rota segura entre os componentes. Chamadas da
PoC exigem alcance até o equipamento; Monitor, Push e modos on-line exigem alcance
do equipamento até a PoC/proxy.

### 22. Como funcionam DNS, NAT e URL pública?

O endereço configurado no equipamento deve ser resolvível e roteável por ele.
Atrás de NAT, use publicação controlada, VPN ou proxy; `localhost` no equipamento
aponta para o próprio equipamento, não para o host da PoC.

### 23. Como configurar TLS?

Use certificado válido para o nome publicado e confirme que o firmware confia na
cadeia. Fora de `Development`, a PoC deve ser servida por HTTPS; não desabilite
validação de certificado como correção permanente.

### 24. Quais regras de firewall e proxy são necessárias?

Libere somente origem, destino, protocolo, porta e caminho usados pelo fluxo. Proxy
reverso deve ser conhecido pela aplicação; a matriz e os cenários estão em
`network-topologies.md`.

## Compatibilidade

### 25. Quais modelos são suportados?

A PoC implementa contratos da linha de acesso, mas suporte físico precisa ser
qualificado por produto. iDAccess/iDFit, iDBlock, iDBox, iDFlex, iDAccess Pro/
Nano, iDUHF, iDFace e iDFace Max possuem particularidades registradas em
`device-compatibility-matrix.md`.

### 26. Qual firmware é suportado?

Não há uma única versão universal homologada. Registre a versão retornada pelo
equipamento e valide a célula funcional correspondente. Um firmware novo invalida
as evidências afetadas até nova regressão.

### 27. Quais recursos exigem licença?

Alguns modos e capacidades, como upgrades Pro/Enterprise em linhas específicas,
podem exigir senha/licença oficial. A PoC envia o pedido quando catalogado, mas
não emite nem garante a licença.

### 28. Como descobrir modelo, serial e firmware?

Conecte o equipamento e consulte `system_information.fcgi`. Serial real deve ser
pseudonimizado em relatórios públicos; modelo e firmware devem acompanhar a
evidência de homologação.

### 29. Qual ação abre cada produto?

Segundo as particularidades oficiais: `door` para iDAccess/iDFit/iDBox e relé
interno iDUHF; `catra` para iDBlock; `sec_box` para iDFlex, iDAccess Pro/Nano,
iDFace e relé externo iDUHF. Confirme a variante física.

### 30. Como funcionam as atualizações de produto/modo?

São chamadas específicas com licença fornecida pelo fabricante. Faça um instantâneo da
configuração, aplique somente em bancada autorizada, releia o estado e registre
retorno; nunca versione a chave.

### 31. Quantos usuários ou templates cabem?

Depende de produto, variante, firmware e licença. Consulte a documentação vigente
e carregue gradualmente; a PoC não declara um número universal.

### 32. Quando a compatibilidade deve ser revalidada?

Após firmware, produto, licença, SecBox, endpoint, carga útil, rede/TLS, incidente ou
mudança oficial. O processo está em `official-api-version-governance.md`.

## Sessão e chamadas da API

### 33. Como funciona `login.fcgi`?

É um POST com usuário e senha do equipamento. Em sucesso, retorna `session`, que
deve acompanhar as chamadas que a exigem. A PoC guarda o valor na sessão ASP.NET
e não deve registrá-lo.

### 34. A mesma sessão deve ser reutilizada?

Sim, enquanto for válida e pertencer ao mesmo equipamento/contexto. Não copie a
sessão entre navegadores, usuários ou ambientes.

### 35. Como validar ou renovar a sessão?

Use `session_is_valid.fcgi`. Se inválida, execute novo login; a PoC não possui
refresh token da Access API.

### 36. Qual codificação e tipo de conteúdo usar?

Corpos textuais devem usar UTF-8. A maioria dos endpoints JSON usa
`application/json`; mídia pode usar multipart ou binário conforme o contrato do
catálogo e a página oficial.

### 37. Como o tempo limite e o circuito funcionam?

O tempo limite de saída é configurável, com padrão de 10 segundos e limite normalizado
entre 5 e 300. O circuito abre após falhas transitórias repetidas. Não há retentativa
automático na PoC.

### 38. Como interpretar erros da API?

Comece por método, caminho, status, `Content-Type`, duração, firmware e resposta
sanitizada. Não existe envelope universal para todo endpoint; use
`api-error-catalog.md`.

### 39. Onde a sessão do equipamento é armazenada?

Na sessão ASP.NET sob `ControlID_SessionString`; a URL fica em
`ControlID_DeviceAddress`. O cookie de sessão é HttpOnly e SameSite Strict.

### 40. Quando usar o catálogo ou uma tela especializada?

Use telas especializadas para fluxos guiados e validações de domínio. Use o
catálogo para estudar/invocar contratos conhecidos. Callbacks marcados como não
invocáveis devem ser chamados pelo equipamento, não pela tela.

## Modos de operação

### 41. Qual a diferença entre Standalone, Pro e Enterprise?

Standalone identifica e autoriza no terminal; Pro identifica no terminal e
autoriza no servidor; Enterprise transfere ambos ao servidor. Contingência volta
à decisão local durante falhas de comunicação.

### 42. Qual modo é recomendado?

A documentação oficial recomenda Standalone como padrão. Outro modo deve ser
escolhido somente quando o caso de uso exigir decisão centralizada e houver
servidor, rede e contingência operáveis.

### 43. Como escolher o modo?

Considere latência, disponibilidade, volume, motor biométrico, política de
autorização, capacidade local, contingência, licença e suporte do produto. Não
escolha apenas pela existência do botão na PoC.

### 44. Quem mantém usuários e regras em cada modo?

Em Standalone, o equipamento precisa de toda a base local. Em Pro, credenciais
para identificação ficam locais e o servidor decide acesso. Em Enterprise, o
servidor assume identificação/autorização; a PoC não é esse motor produtivo.

### 45. Como ativar cada modo?

A PoC lê e altera `general.online` e `general.local_identification`; Pro/
Enterprise também exigem `online_client.server_id`. Consulte
`operation-modes-implementation.md` antes de escrever.

### 46. O que é `server_id`?

É o ID do objeto `devices` que representa o servidor/sistema no banco do terminal.
Ele deve ser criado uma vez ou reutilizado e associado à configuração on-line.

### 47. Como funciona a contingência?

Após falhas consecutivas de comunicação, um terminal on-line pode voltar à
identificação/autorização local. Isso só é seguro se os dados locais estiverem
atualizados. Siga `equipment-contingency-runbook.md`.

### 48. Por que `/device_is_alive.fcgi` é importante?

O terminal usa essa chamada para verificar o servidor e sair da contingência. O
servidor precisa responder HTTP de sucesso conforme o contrato on-line.

## Usuários, credenciais e regras

### 49. Como criar, editar e excluir usuários?

Use operações de objetos sobre `users`: criar, criar/modificar, modificar, carregar
e destruir. Valide IDs, campos obrigatórios, vínculos e confirmação antes de
excluir.

### 50. Quais credenciais podem ser associadas?

Dependendo do produto: cartão, face, template biométrico, PIN/senha, QR Code e
UHF. Cada tipo possui objeto, formato e capacidade específicos.

### 51. A senha de um usuário do equipamento é enviada em claro?

O objeto `users.password` deve receber o hash gerado por
`user_hash_password.fcgi`, não a senha em claro. Isso é diferente da senha local
da PoC e da credencial usada em `login.fcgi`.

### 52. Como grupos, regras e portais se relacionam?

O fluxo recomendado associa usuário ao grupo, grupo à regra, regra aos horários e
regra ao portal. Regra direta por usuário deve ser exceção. Veja
`data-synchronization-ownership.md`.

### 53. Como configurar horários e feriados?

Use `time_zones`, `time_spans` e os vínculos de regras. Confirme fuso/NTP e teste
limites de início/fim, dias da semana e feriados no firmware real.

### 54. Como funciona antipassback?

É uma configuração/regra que impede sequências indevidas de entrada/saída. Seus
modos e resets variam por produto/configuração; habilite somente após ensaio de
contingência e reconciliação.

### 55. Como carregar bases grandes?

Use filtros, `limit`, `offset` e ordem em `load_objects.fcgi`. Para muitos
templates, valide `template_sync_init.fcgi`/`template_sync_end.fcgi` e processe em
lotes.

### 56. Qual sistema é a fonte de verdade?

Precisa ser decidido por domínio. A PoC não garante sincronização bidirecional;
em Standalone, o equipamento decide acesso, e o SQLite local não deve ser tratado
como réplica completa. Veja `data-synchronization-ownership.md`.

## Operações físicas e cadastro remoto

### 57. Como abrir uma porta ou catraca remotamente?

Use `execute_actions.fcgi` com ação/parâmetros compatíveis, por uma conta local
administrativa e sessão oficial válida. Valide o efeito físico e o evento, não
somente o HTTP.

### 58. Quando usar `door`, `catra` ou `sec_box`?

Depende da linha e do relé. Consulte a resposta 29 e
`device-compatibility-matrix.md`; uma ação incorreta pode retornar erro ou não
produzir o efeito esperado.

### 59. Quais operações exigem confirmação adicional?

Exclusão de objetos, limpeza/expurgo, reinicialização, alteração de rede, reset de
fábrica, modo de recuperação e remoção de administradores usam frases explícitas
na PoC. Confirmação não substitui backup ou autorização.

### 60. Como funciona o cadastro remoto?

`remote_enroll.fcgi` admite cartão, face, biometria, PIN ou senha. Pode ser
síncrono ou assíncrono e salvar no dispositivo conforme `save`; no assíncrono, o
Monitor precisa estar configurado para receber o resultado.

### 61. Quais imagens faciais são aceitas?

Formato, qualidade e enquadramento dependem do endpoint e firmware. Use os testes
oficiais de imagem e dados fictícios/autorizados; não force sanitização que
corrompa a imagem.

### 62. Como tratar face duplicada e dedo de pânico?

Cadastro facial pode retornar `FACE_EXISTS` e usuário correspondente. Dedo de
pânico é um atributo sensível com impacto de alarme; configure e teste somente em
bancada e procedimento de segurança aprovados.

### 63. A PoC gerencia fotos, logotipo, vídeo e áudio?

Sim, há catálogo e telas para várias dessas operações, incluindo capacidades
específicas de iDFace/iDFace Max. Aplicabilidade, formato e limite devem ser
confirmados por produto.

### 64. Como recuperar uma operação perigosa?

Registre estado anterior e retorno antes de executar. Após falha, pare repetições,
releia o equipamento, aplique a configuração anterior quando aprovado e siga os
procedimentos operacionais; a restauração de fábrica não possui reversão automática.

## Callbacks, Monitor e Push

### 65. Qual a diferença entre callback, Monitor e Push?

Callbacks cobrem endpoints que o equipamento chama para identificação/cadastro;
Monitor envia notificações assíncronas; Push faz o equipamento buscar comandos e
devolver resultados. Todos são ingressos na PoC.

### 66. Quem inicia cada comunicação?

Na chamada Access API comum, a PoC inicia. Nos modos on-line, callbacks, Monitor e
Push, o equipamento inicia a conexão HTTP para o servidor.

### 67. A PoC precisa de URL pública?

Somente se o equipamento estiver fora da rede e precisar iniciar chamadas. Ainda
assim, prefira VPN/rede privada/proxy controlado em vez de exposição direta.

### 68. Quais endpoints e respostas são esperados?

Monitor usa `/api/notifications/{topic}` e normalmente espera 200 vazio. Push usa
`GET /push` e `POST /result`. Callbacks `.fcgi` podem esperar corpo de autorização
ou confirmação conforme `integration-contracts.md`.

### 69. Como autenticar chamadas recebidas?

Fora de Development, use IP permitido, chave compartilhada, HMAC-SHA256,
timestamp, nonce, limite de corpo e taxa. Se o equipamento não assina, use o proxy
assinador local.

### 70. Como tratar retentativa, repetição e ordem?

Não assuma entrega exatamente uma vez nem ordem global. Rejeite nonce repetido,
correlacione IDs e só aceite retentativa de escrita quando houver idempotência
comprovada.

### 71. Como a fila Push funciona?

Um administrador enfileira comando `pending`; a consulta periódica elegível o marca como
`delivered`; `/result` registra o retorno. Sem comando, a PoC responde `{}`.

### 72. O que acontece se servidor ou rede ficar indisponível?

Monitor/Push atrasam ou falham conforme firmware e tempo limite; modos on-line podem
entrar em contingência. Recupere a rede, responda ao keep-alive e reconcilie
eventos/comandos.

## Dados e privacidade

### 73. O que fica no SQLite?

Contas locais, entidades auxiliares, sessões históricas, eventos de Monitor,
comandos Push, logs e outros estados descritos em `data-model-and-recovery.md`.

### 74. O que permanece somente no equipamento?

O banco oficial de objetos/configurações e a decisão física permanecem no
terminal, salvo quando a PoC consulta ou persiste uma cópia explícita. A sessão
local não transforma o SQLite em réplica integral.

### 75. Quais dados são pessoais ou sensíveis?

Nome, e-mail, telefone, identificadores, IP/serial, cartões e logs são pessoais ou
técnicos identificáveis; foto e biometria podem ser sensíveis. A classificação
depende do contexto e requer DPO/jurídico.

### 76. O que pode aparecer nos logs?

Somente contexto operacional mínimo: endpoint, status, duração, correlação e
referências pseudonimizadas. Senhas, sessões, chaves, imagens, biometria e cargas úteis
integrais são proibidos.

### 77. Qual é a retenção?

A PoC define limites e expurgos locais, mas a retenção real precisa ser aprovada
por finalidade, segurança e LGPD. Logs padrão retêm 14 arquivos diários; eventos e
Push possuem expurgo confirmado.

### 78. Como fazer backup e restauração?

Use `tools/backup-sqlite-operational.ps1 -RunRestoreSmoke` e valide em cópia
temporária. Restauração real sobrescreve estado e exige aprovação humana.

### 79. Como atender direitos de titulares?

Localize o dado no equipamento, SQLite, logs e backups; confirme identidade e
aplique procedimento aprovado de acesso, correção, bloqueio ou eliminação. A base
legal e o prazo requerem DPO/jurídico.

### 80. Quais dados devem ser usados em testes?

Somente dados fictícios, minimizados e sem referência a pessoas reais. Não use
foto, biometria, cartão, documento, IP de cliente ou credencial real.

## Segurança

### 81. Quais papéis existem?

`Administrator` e `Operator`. O primeiro concentra escrita e ações críticas; o
segundo possui acesso autenticado limitado. A autorização confiável fica no
backend.

### 82. Posso manter credenciais padrão do equipamento?

Não em ambiente real. Troque defaults conforme política do produto e mantenha a
credencial em cofre; exemplos oficiais não são recomendação de segurança.

### 83. HTTPS é obrigatório?

Para ambiente exposto ou produção, sim. A comunicação com o equipamento também
deve usar HTTPS quando suportado e corretamente provisionado.

### 84. O SSH do equipamento deve ficar habilitado?

A documentação oficial informa que pode vir habilitado para suporte. Avalie a
necessidade e desabilite/restrinja conforme política, modelo e suporte, sem perder
um canal operacional aprovado.

### 85. Como a lista de permissões protege a PoC?

`AllowedDeviceHosts` impede que URLs fornecidas à interface façam a aplicação
acessar destinos arbitrários, mitigando SSRF. `AllowedRemoteIps` limita origens de
callbacks. Ambas precisam refletir a topologia real.

### 86. Há limitação de taxa?

Sim: global/interativa, login local e ingressos possuem janelas configuráveis.
Ela reduz abuso e sobrecarga, mas não substitui autenticação, capacidade ou WAF.

### 87. Onde armazenar e como rotacionar segredos?

Use User Secrets no desenvolvimento e cofre/variáveis protegidas no ambiente.
Rotacione PoC, proxy e equipamento em janela controlada e nunca registre valores
antigos ou novos.

### 88. A PoC pode ser exposta diretamente à internet?

Não é o desenho recomendado. Antes de qualquer exposição, exija proxy/VPN, TLS,
hosts restritos, HMAC, IP permitido, OpenAPI desabilitado, métricas protegidas,
monitoramento e aprovação de produção.

## Testes e operação

### 89. O que o simulador (stub) comprova?

Comprova os contratos simulados usados pela suíte e pelo teste integrado. Não comprova
capacidade, temporização, licença, efeito físico, câmera, algoritmo biométrico nem
particularidades completas do firmware.

### 90. Quais verificações devem ser executadas?

Restauração de dependências em modo bloqueado, compilação, formatação, testes,
documentação, segredos, cadeia de suprimentos e contrato com simulador. O conjunto está em `AGENTS.md` e
`testing-strategy.md`.

### 91. O que exige homologação física?

Sessão real, escrita de objetos, licenças, modos, callbacks públicos, Push real,
relés, portas, catracas, sensores, mídia e contingência do modelo/firmware alvo.

### 92. Quais sinais operacionais existem?

`/health/live`, `/health/ready`, `/metrics`, logs Serilog, identificador de
correlação, métricas de latência/erro, estado de circuit breaker e históricos de
Monitor/Push.

### 93. Como diagnosticar sessão, tempo limite e callback?

Use `troubleshooting-controlid.md`: determine direção, valide health/session,
teste rede, revise allowlists/assinatura e capture status/duração/correlação sem
carga útil sensível.

### 94. A PoC suporta múltiplos equipamentos simultaneamente?

O SQLite pode inventariar vários dispositivos e o Push separa comandos por
`device_id`, mas cada sessão de navegador mantém um equipamento/sessão oficial em
contexto. A PoC é dimensionada para poucos operadores e um equipamento por vez
por contexto, não para orquestração multi-instância.

### 95. Como implantar e reverter?

Use Docker/Compose ou o ambiente de execução .NET conforme `deployment-runbook.md`. Preserve
volume SQLite/logs, configuração anterior e imagem/commit; não execute migração
destrutiva nem deploy automático sem aprovação.

### 96. Que evidências devem acompanhar um pedido de suporte?

Data/fuso, commit, modelo, firmware, classe de licença, endpoint, status, duração,
correlação, topologia, verificações de saúde e ações tentadas. Nunca envie senha, chave,
sessão, carga útil com dados pessoais, foto ou biometria.

## Fontes oficiais principais

- [Introdução à Access API](https://www.controlid.com.br/docs/access-api-pt/)
- [Fazer login](https://www.controlid.com.br/docs/access-api-pt/gerenciamento-secao/fazer-login/)
- [Introdução aos modos de operação](https://www.controlid.com.br/docs/access-api-pt/modos-de-operacao/introducao-aos-modos-de-operacao/)
- [Introdução ao Monitor](https://www.controlid.com.br/docs/access-api-pt/monitor/introducao-ao-monitor/)
- [Introdução ao Push](https://www.controlid.com.br/docs/access-api-pt/modo-push/introducao-ao-push/)
- [Lista de objetos](https://www.controlid.com.br/docs/access-api-pt/objetos/lista-de-objetos/)
- [Particularidades dos produtos](https://www.controlid.com.br/docs/access-api-pt/particularidade-dos-produtos/particulariade-terminais-control-id/)

As referências são vivas. A política de revisão e os gatilhos de revalidação
estão em `official-api-version-governance.md`.
