# Privacidade, LGPD e retencao local

Revisao tecnica de privacidade da PoC. Este documento nao e parecer juridico e nao declara conformidade total com a LGPD. Bases legais, papeis dos agentes de tratamento, contratos com terceiros e RIPD precisam de validacao formal do DPO/juridico antes de uso real.

## Escopo funcional

Esta PoC ASP.NET Core MVC integra com equipamentos Control iD para autenticacao, catalogo de endpoints oficiais, cadastros, callbacks, monitoramento, fila Push e persistencia local em SQLite. A aplicacao pode tratar dados pessoais comuns, dados tecnicos identificaveis, credenciais e dados sensiveis como foto, biometria, cartoes, QR codes e logs de acesso.

## Inventario e classificacao de dados

| Dado | Origem | Classificacao | Necessidade na PoC | Observacoes |
| --- | --- | --- | --- | --- |
| Nome, matricula/registration, status de usuario | UI/API Control iD | Pessoal comum | Necessario para cadastro e consulta operacional | Pode identificar titular. |
| E-mail e telefone | UI/API Control iD | Pessoal comum | Condicional | Deve ser coletado apenas quando o fluxo exigir. |
| Senha de usuario local, hash e salt | UI local/SQLite | Credencial/confidencial | Necessario para login local | Senha em claro nao deve ser persistida nem logada. |
| Sessao oficial Control iD | API Control iD/sessao ASP.NET | Credencial/confidencial | Necessario para chamadas oficiais autenticadas | Exibida apenas mascarada; nao logar. |
| Shared key/HMAC/certificados/VPN | Configuracao local | Secret/confidencial | Necessario para seguranca de callbacks/ambiente | Usar User Secrets, env vars ou cofre. |
| IP remoto, IP do equipamento, host, serial, device_id | HTTP/equipamento/API | Tecnico identificavel | Necessario para seguranca, diagnostico e roteamento | Logs devem usar referencias pseudonimizadas. |
| Fotos, imagens faciais e logos com pessoas | Upload/API/SQLite | Sensivel quando identifica pessoa | Condicional a fluxos de midia | Evitar dados reais em PoC. |
| Templates biometricos, fingerprint, face template | API/SQLite/payloads | Sensivel | Condicional a fluxos biometricos | Alto risco; requer base legal e RIPD. |
| Cartoes, tags, QR codes, PINs | UI/API/SQLite | Pessoal/credencial de acesso | Condicional a controle de acesso | Tratar como credenciais de acesso fisico. |
| Logs de acesso, monitoramento, callbacks e Push | Equipamento/API local | Pessoal, tecnico e possivelmente sensivel | Necessario para QA/diagnostico | Payload bruto pode conter dados pessoais. |
| Cookies de autenticacao, antiforgery e sessao | ASP.NET Core | Tecnico identificavel/seguranca | Necessario para UI segura | Sem evidencia de cookies de analytics. |
| Dados financeiros, saude, geolocalizacao, scores | Nao encontrado | N/A | Nao aplicavel | Nao introduzir sem requisito e avaliacao. |
| Criancas/adolescentes | Nao ha campo explicito de idade | Necessita validacao | Ambiguo | A base de usuarios do equipamento pode incluir menores; DPO deve validar contexto. |
| Decisao automatizada/perfis | Nao encontrado | N/A | Nao aplicavel | Nao ha score ou decisao automatizada propria da PoC. |

## Mapa de tratamento

| Tratamento | Finalidade | Origem | Destino | Tela/API/servico/banco | Acesso/alteracao/exclusao | Retencao | Base legal provavel |
| --- | --- | --- | --- | --- | --- | --- | --- |
| Cadastro e edicao de usuarios | Gerir identidades no equipamento | UI admin | Access API Control iD | `UsersController`, `users` | Admin altera/exclui | Minimo necessario | Necessita validacao juridica/DPO; possiveis execucao de contrato, obrigacao legal/regulatoria ou legitimo interesse conforme contexto. |
| Login local | Proteger acesso a PoC | UI local | Cookie/SQLite `Users` | `AuthController`, cookie auth | Usuario/admin | Enquanto conta existir | Necessita validacao; seguranca da aplicacao e legitimo interesse podem ser aplicaveis. |
| Login no equipamento | Criar sessao oficial Control iD | UI admin | Equipamento/sessao ASP.NET | `AuthController`, `SessionController` | Admin inicia/encerra | Curto prazo de sessao | Necessita validacao; execucao operacional/contratual. |
| Fotos e midia de usuario | Sincronizar imagem facial | UI/API | Equipamento/SQLite `Photos` | `MediaController`, `OfficialCallbacksController` | Admin cria/remove | Minimo necessario | Dados sensiveis quando biometria/facial: necessita base especifica e RIPD. |
| Templates biometricos | Cadastro/consulta biometrica | UI/API | Equipamento/SQLite `BiometricTemplates` | `BiometricTemplatesController` | Admin cria/remove | Minimo necessario | Dados sensiveis: necessita validacao juridica/DPO e provavelmente RIPD. |
| Cartoes, QR codes e tags | Credenciais de acesso fisico | UI/API | Equipamento/SQLite | `CardsController`, `QRCodesController`, callbacks | Admin cria/remove | Minimo necessario | Necessita validacao; controle de acesso/seguranca. |
| Callbacks e monitoramento | Receber eventos do equipamento | Equipamento | SQLite `MonitorEvents` | `CallbackIngressService`, callbacks `.fcgi` | Sistema grava; admin expurga | Curto prazo para QA | Necessita validacao; seguranca, auditoria e operacao. |
| Push e resultados | Enfileirar comandos e receber status | UI/equipamento | SQLite `PushCommands` | `PushCommandWorkflowService`, `/push`, `/result` | Admin cria/expurga; sistema atualiza | Curto prazo para QA | Necessita validacao; operacao tecnica. |
| Logs tecnicos | Diagnostico, seguranca e rastreabilidade | App/middleware | `Logs/`/Serilog | middlewares, controllers, services | Operador do host | Curto prazo | Necessita validacao; seguranca/prevencao. |
| Backups SQLite | Recuperacao local | SQLite | `artifacts/backups/` | scripts `backup-sqlite`, `restore-smoke` | Operador do host | Apenas enquanto necessario | Necessita validacao; recuperabilidade e continuidade. |

Todas as bases acima sao hipoteses tecnicas. A definicao final depende do controlador real, finalidade concreta, titulares afetados, contratos, setor, legislacao trabalhista/regulatoria e politica interna.

## Principios LGPD avaliados

| Principio | Estado tecnico | Lacunas |
| --- | --- | --- |
| Finalidade | Fluxos estao ligados a operacao Control iD e QA local | Formalizar finalidade por ambiente/projeto real. |
| Adequacao | Dados se relacionam a controle de acesso e integracao | Confirmar adequacao com politica do controlador. |
| Necessidade | Logs foram reduzidos para referencias pseudonimizadas; payloads brutos permanecem onde a PoC precisa depurar callbacks/Push | Definir se cada campo opcional e indispensavel em producao. |
| Livre acesso | Nao ha portal DSAR/self-service | Criar procedimento manual ou automatizado. |
| Qualidade | Dados refletem equipamento/API | Sem processo de correcao pelo titular. |
| Transparencia | Documentacao tecnica existe | Aviso de privacidade e informativos ao titular nao estao versionados. |
| Seguranca | Auth local, RBAC, HMAC, rate limit, headers, backups DPAPI e logs pseudonimizados | Validar configuracao real, cofre de segredos e hardening do host. |
| Prevencao | Limites de payload, expurgo guiado e mascaramento reduzem risco | Falta runbook formal de incidente e exercicio periodico. |
| Nao discriminacao | Nao ha score/decisao automatizada propria | Uso de biometria no contexto real precisa avaliacao. |
| Responsabilizacao | Baselines, docs e checks existem | DPA/contratos, RIPD e evidencias juridicas ainda pendentes. |

## Direitos dos titulares

| Direito | Cobertura tecnica atual | Lacuna |
| --- | --- | --- |
| Confirmacao e acesso | Admin consegue gerar relatorio minimizado em `Privacy/Index` e consultar usuarios/eventos no equipamento/local | Canal formal e SLA dependem de DPO/juridico. |
| Correcao | Admin pode editar usuarios/credenciais no equipamento | Necessita procedimento de solicitacao e registro. |
| Anonimizacao, bloqueio e eliminacao | Existem exclusoes por entidade e expurgo de MonitorEvents/PushCommands | Nao ha workflow consolidado por titular em todos os dados. |
| Portabilidade | `Privacy/Export` gera JSON minimizado, sem payload bruto | Definir formato final, escopo e seguranca para exportacao bruta. |
| Informacao sobre compartilhamento | Documentacao lista Control iD/equipamento e artefatos locais | DPA/contratos e terceiros reais precisam validacao. |
| Revogacao | Nao ha consentimento modelado no sistema | Se consentimento for usado, criar registro e revogacao. |
| Revisao de decisao automatizada | Nao ha decisao automatizada propria | Validar uso real do equipamento. |
| Canal e prazo | Nao implementado | DPO/juridico devem definir canal, prazos e responsabilidades. |

## Terceiros e transferencias

- Equipamento/firmware Control iD: recebe e retorna dados de usuarios, credenciais de acesso, fotos, biometria, eventos e configuracoes. Papel do terceiro, contrato, DPA e transferencia internacional: necessita validacao juridica/DPO.
- GitHub Actions/NuGet: evidenciados para codigo, CI e dependencias; nao devem receber dados reais da PoC. Nao enviar logs, bancos ou artefatos com dados pessoais.
- Sem evidencia de analytics, e-mail/SMS/push externo, gateway de pagamento, cache externo ou storage cloud de runtime.
- Callback signing proxy local e stub de equipamento sao ferramentas tecnicas; nao devem receber dados reais fora de ambiente controlado.

## Retencao, descarte e anonimizacao

| Dado local | Retencao recomendada | Descarte/controle |
| --- | --- | --- |
| `MonitorEvents` | Minimo necessario para QA/homologacao | `OfficialEvents/Purge` com frase `EXPURGAR EVENTOS`; payload bruto pode conter dados pessoais/sensiveis. |
| `PushCommands` | Ate concluir analise do ciclo Push | `PushCenter/Purge` com frase `EXPURGAR PUSH`; payload/resultados podem conter ids e comandos. |
| `Logs/` | Curto prazo local | Logs novos usam referencias pseudonimizadas para IP, usuario, equipamento e ids sensiveis; manter fora do Git. |
| `integracao_controlid.db*` | Ambiente local controlado | Nao versionar nem compartilhar; tratar como base com dados pessoais/sensiveis. |
| `artifacts/backups/` | Apenas enquanto necessario para rollback local | DPAPI por padrao; nao versionar; restringir permissoes com `tools/harden-local-state.ps1`. |
| Fotos/templates/cartoes/QRs | Minimo necessario | Preferir dados ficticios; exclusao real exige confirmacao humana e base juridica. |

Nao apagar dados reais sem confirmacao humana, registro da finalidade e decisao do controlador/DPO. Para dados em producao real, documentar politica de retencao, descarte seguro e evidencias.

## RIPD e incidentes

RIPD e recomendado e pode ser necessario antes de uso real porque a PoC pode tratar biometria, fotos, credenciais de acesso fisico, monitoramento de acesso e payloads brutos de eventos. A necessidade final depende de escala, finalidade, titulares, ambiente e papel do controlador.

Procedimento minimo recomendado para incidente:

1. Conter acesso ao host, equipamento, banco, logs e backups.
2. Preservar evidencias sem copiar dados pessoais para canais inseguros.
3. Identificar titulares, categorias de dados, periodo e sistemas afetados.
4. Rotacionar secrets, shared keys, sessoes e credenciais impactadas.
5. Acionar DPO/juridico para avaliar notificacao a ANPD e titulares.
6. Registrar causa raiz, mitigacoes, risco residual e decisao formal.

## Controles tecnicos aplicados

- Logs HTTP agora registram `IPRef` e `UserRef`, sem IP remoto ou usuario bruto.
- Logs de autenticacao local e login/logout de equipamento usam referencias pseudonimizadas.
- Logs de sessao, callbacks, Push, usuarios, fotos, biometria, cartoes e QR codes usam `PrivacyLogHelper` para ids sensiveis.
- Alvo de observabilidade da Access API usa referencia pseudonimizada de endpoint, sem host, caminho, query ou sessao.
- Mensagem de sucesso do teste de conectividade deixou de exibir o endpoint bruto informado.
- `Privacy/Index` gera relatorio minimizado de atendimento a direitos do titular por ID, matricula, usuario, e-mail ou telefone.
- `Privacy/Export` exporta JSON minimizado sem foto Base64, biometria bruta, hashes, sessoes, payloads, cartoes ou QR codes.
- Testes unitarios cobrem estabilidade e nao exposicao de usuario, IP, endpoint e identificador pseudonimizados.
- `docs/privacy-governance-runbook.md` define RACI, DSAR, RIPD, DPA, retencao e incidente como artefatos verificaveis para decisao humana.

## Regras obrigatorias

- Nao versionar dados reais, secrets, bancos SQLite locais, logs ou artefatos de runtime.
- Nao copiar payload bruto para docs, issues ou commits quando houver dado pessoal/sensivel.
- Mascarar segredos e identificadores em exemplos, screenshots e mensagens de erro.
- Usar User Secrets, variaveis de ambiente ou cofre externo para credenciais e `CallbackSecurity:SharedKey`.
- Validar `AllowedHosts`, shared key, assinatura HMAC e IPs permitidos antes de expor a PoC fora de localhost.
- Limpar `MonitorEvents` e `PushCommands` apenas por acao manual confirmada na UI.
- Preferir expurgo por retencao (`EXPURGAR EVENTOS` ou `EXPURGAR PUSH`) a limpeza total quando o objetivo for reduzir historico.
- Tratar backups SQLite como dados sensiveis; backups novos sao protegidos por DPAPI por padrao.
- Rodar `tools/harden-local-state.ps1` no host local para restringir permissoes de SQLite, logs, backups e copias temporarias de restore.
- Nao usar dados pessoais reais em testes, docs, smoke, fixtures ou screenshots.

## Criterios de aceite de privacidade

- Fluxo que grava payload bruto documenta tabela, finalidade e forma de limpeza local.
- Tela que apaga historico local exige confirmacao textual.
- Mensagem ao usuario nao expoe stack trace, secret, sessao, IP interno sensivel ou payload completo.
- Exemplo versionado usa valores ficticios e placeholders.
- Ambiente nao `Development` falha no startup sem `AllowedHosts` explicito, `RequireSharedKey=true`, `SharedKey` configurado e assinatura HMAC quando exigida.
- Log novo que envolva titular, IP, host, device id, user id, biometria, cartao ou QR code usa mascaramento ou pseudonimizacao.

## Lacunas para DPO/juridico

- Definir controlador, operador, encarregado e matriz RACI.
- Validar bases legais por tratamento e por ambiente.
- Validar necessidade de consentimento ou outra base especifica para biometria/foto.
- Formalizar aviso de privacidade, canal de direitos e prazos.
- Validar contratos/DPA com Control iD, fornecedores de infraestrutura e qualquer terceiro real.
- Confirmar transferencia internacional, se houver.
- Aprovar RIPD, politica de retencao, descarte, backup e resposta a incidente.
