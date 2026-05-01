# Resposta a incidentes, continuidade e DR

Escopo: operacao da PoC ASP.NET Core MVC/Razor de integracao Control iD, com
SQLite local, callbacks/push, observabilidade local e execucao containerizada.
Este runbook complementa `docs/observability-runbook.md`,
`docs/deployment-runbook.md`, `docs/data-model-and-recovery.md` e
`docs/privacy-governance-runbook.md`. Para uso real, copie `ops.example.json`
para `ops.local.json` fora do Git e preencha donos, canais, evidencias, backup
externo, RTO/RPO e contingencia fisica.

Este documento nao autoriza rollback real, alteracao de producao, delecao de
evidencias, comunicacao externa oficial ou restauracao destrutiva sem decisao
humana responsavel.

## Principios de comando

- Proteger pessoas, dados e continuidade antes de corrigir codigo.
- Preservar evidencias: logs, timestamps, correlation IDs, artefatos de deploy,
  versao, configuracao efetiva e manifests de backup.
- Nao copiar payloads pessoais, biometria, fotos, cartoes, QR codes, senhas,
  session strings, shared keys ou headers de auth para tickets e chats.
- Registrar hipoteses como hipoteses ate existir evidencia.
- Fazer contencao reversivel sempre que possivel.
- Validar normalizacao com `/health/live`, `/health/ready`, logs, metricas e
  smoke/check relevante antes de encerrar.

## Matriz de severidade

| Severidade | Criterios | Impacto | Urgencia | Responsavel inicial |
| --- | --- | --- | --- | --- |
| SEV1 | Aplicacao indisponivel, perda/corrupcao de dados, vazamento confirmado, segredo comprometido, falha ampla de auth/autorizacao ou deploy que impede operacao critica | Operacao parada, risco alto a titulares ou integridade comprometida | Resposta imediata | Incident Commander + Tech Lead/SRE |
| SEV2 | Degradacao relevante, 5xx recorrente, banco instavel, callbacks/push criticos falhando, integracao externa indisponivel em fluxo essencial | Operacao parcial, risco de backlog ou atendimento manual | Resposta prioritaria | SRE/Release Engineer |
| SEV3 | Falha localizada, latencia alta sem indisponibilidade, erro funcional com workaround, alerta recorrente sem impacto amplo | Impacto limitado e controlavel | Resposta em horario operacional | Dono tecnico da area |
| SEV4 | Duvida, warning, melhoria preventiva, documentacao incompleta ou falso positivo confirmado | Sem impacto imediato | Planejamento normal | Maintainer |

## Funcoes e escalonamento

| Papel | Responsabilidade |
| --- | --- |
| Incident Commander | Classifica SEV, coordena sala/canal, controla linha do tempo, aprova encerramento. |
| SRE/Operacao | Diagnostica saude, logs, metricas, container, host, rede, banco e rollback tecnico. |
| Tech Lead/Maintainer | Avalia codigo, contrato, migracoes, hotfix e risco de regressao. |
| Release Engineer | Controla versoes, artefatos, tags, deploy/rollback e gates de readiness. |
| DPO/Juridico | Decide comunicacao externa, ANPD/titulares, base legal e preservacao de evidencias de dados. |
| Negocio/Produto | Define impacto funcional, usuarios afetados e comunicacao interna. |

Escalonar para SEV1 quando houver qualquer indicio de dado pessoal exposto,
secret real comprometido, corrupcao de SQLite, indisponibilidade total ou falha
de autorizacao que permita acesso indevido.

## Plano geral de resposta

1. Declarar incidente com horario, severidade inicial, sistema, versao e impacto.
2. Designar Incident Commander e responsaveis tecnicos.
3. Congelar mudancas nao essenciais; nao fazer deploy paralelo sem autorizacao.
4. Coletar evidencias minimizadas: `X-Correlation-ID`, horarios UTC/local, rota,
   status, duracao, evento operacional, commit/tag, imagem container, host e
   alerta acionado.
5. Conter o impacto com acao reversivel: remover trafego, pausar chamada ao
   equipamento, bloquear origem, voltar versao, isolar host ou rotacionar segredo.
6. Mitigar causa provavel e validar saude.
7. Comunicar status interno de forma objetiva, sem dados sensiveis.
8. Encerrar somente apos normalizacao validada e criacao de postmortem.

## Runbooks por cenario

### IR-01 API fora do ar

| Campo | Procedimento |
| --- | --- |
| Sintoma | `/health/live` falha, porta nao responde, container/processo encerrado ou erro de startup. |
| Severidade | SEV1 se indisponibilidade total; SEV2 se ha instancia alternativa funcional. |
| Impacto | UI, callbacks, push, metricas e operacao local indisponiveis. |
| Como detectar | Alerta `OBS-001`, probe de `/health/live`, logs de supervisor/container. |
| Metricas/logs | `controlid_http_requests_total`, logs de startup, stdout/stderr, eventos de deploy. |
| Diagnostico | Confirmar processo, porta 8080/porta configurada, variaveis obrigatorias, ultimo commit/tag, `AllowedHosts`, shared key e paths de volume. |
| Contencao | Pausar novo trafego, remover instancia quebrada do balanceador ou voltar para imagem anterior se existir. |
| Mitigacao | Corrigir configuracao ausente, reiniciar processo/container ou aplicar rollback tecnico documentado. |
| Recuperacao | Validar `/health/live`, `/health/ready`, login local e uma rota segura de leitura. |
| Comunicacao | Informar indisponibilidade, impacto funcional, workaround e proxima atualizacao interna. |
| Escalonamento | SRE, Release Engineer e Tech Lead; SEV1 aciona Incident Commander. |
| Validacao | Dois probes saudaveis consecutivos e ausencia de novo erro de startup. |
| Pos-incidente | Registrar causa, gate ausente, diferenca entre config local/staging/producao e acao preventiva. |

### IR-02 Banco SQLite indisponivel

| Campo | Procedimento |
| --- | --- |
| Sintoma | `/health/ready` falha, erros EF/SQLite, lock persistente, permissao negada ou disco cheio. |
| Severidade | SEV1 se impede escrita/leitura critica ou ha risco de perda; SEV2 se parcial. |
| Impacto | Monitor, push, sessoes, cadastros e auditoria local degradados. |
| Como detectar | Alerta `OBS-002`, `OBS-008`, `OBS-009`, logs de persistencia. |
| Metricas/logs | `/health/ready`, logs `persistence_failed`, paths SQLite, espaco em disco. |
| Diagnostico | Conferir volume `/app/data`, arquivo `.db`, `-wal`, `-shm`, permissao, processo concorrente e migrations recentes. |
| Contencao | Parar novas escritas quando possivel, preservar arquivos atuais e gerar backup antes de qualquer tentativa destrutiva. |
| Mitigacao | Corrigir permissao/volume/disco; se for schema, validar em copia com `tools/restore-smoke-sqlite.ps1`. |
| Recuperacao | Subir app em ambiente controlado e validar readiness, listagens e persistencia de evento ficticio. |
| Comunicacao | Informar risco de indisponibilidade de historico local e janela de recuperacao estimada. |
| Escalonamento | SRE + Data/Backend; DPO se dados pessoais puderem ter sido expostos ou perdidos. |
| Validacao | `/health/ready` Healthy e logs sem novas falhas de persistencia. |
| Pos-incidente | Registrar backup usado, copia restaurada, checksum se disponivel e gaps de RTO/RPO. |

### IR-03 Latencia alta

| Campo | Procedimento |
| --- | --- |
| Sintoma | UI lenta, timeouts ocasionais, P95/P99 alto ou requests acumulando. |
| Severidade | SEV2 se afeta fluxo essencial; SEV3 se localizada. |
| Impacto | Operadores podem repetir comandos, gerando risco de duplicidade ou backlog. |
| Como detectar | Dashboard de latencia HTTP e duracao de Access API. |
| Metricas/logs | `controlid_http_request_duration_milliseconds`, `controlid_official_api_duration_milliseconds`, logs de endpoint e duracao. |
| Diagnostico | Separar latencia local, SQLite, rede e equipamento; verificar endpoint oficial, payload grande e volume de callbacks/push. |
| Contencao | Reduzir trafego nao essencial, pausar operacoes repetitivas e orientar operadores a aguardar retorno. |
| Mitigacao | Validar equipamento/rede, circuito aberto, limite de payload e consultas locais. |
| Recuperacao | Confirmar P95/P99 normalizado e ausencia de timeouts novos. |
| Comunicacao | Informar degradacao e evitar reenvios manuais ate estabilizar. |
| Escalonamento | SRE + dono do fluxo afetado; fornecedor/equipe de rede se equipamento for causa provavel. |
| Validacao | Janela de metricas sem degradacao e smoke do fluxo impactado. |
| Pos-incidente | Registrar gargalo, estimativa de carga, necessidade de paginacao/cache/limite adicional. |

### IR-04 Erro 5xx elevado

| Campo | Procedimento |
| --- | --- |
| Sintoma | Erros 500/502/503, tela generica de erro ou alerta de 5xx. |
| Severidade | SEV2; SEV1 se amplo ou em auth/autorizacao/dados. |
| Impacto | Fluxos criticos falham e podem ocultar falha de integracao ou banco. |
| Como detectar | Alerta `OBS-003`, logs do `ExceptionHandlingMiddleware`, correlation ID. |
| Metricas/logs | `controlid_http_requests_total{status_group="5xx"}`, rota, trace id, stack no log interno. |
| Diagnostico | Agrupar por rota, commit, entrada, usuario role e dependencia. Verificar se ha release recente. |
| Contencao | Desabilitar operacao afetada por orientacao operacional ou rollback se novo deploy causou falha. |
| Mitigacao | Corrigir configuracao, dependencia indisponivel ou bug; nao expor stack trace ao usuario. |
| Recuperacao | Build/teste/smoke relacionado e queda sustentada de 5xx. |
| Comunicacao | Informar rotas afetadas e workaround. |
| Escalonamento | Tech Lead + Release Engineer se houver hotfix/rollback. |
| Validacao | Sem 5xx novos por janela definida e fluxo validado manualmente quando aplicavel. |
| Pos-incidente | Criar teste regressivo para a rota/entrada que quebrou. |

### IR-05 Falha de autenticacao

| Campo | Procedimento |
| --- | --- |
| Sintoma | Logins locais falham, login no equipamento falha ou sessoes expiram inesperadamente. |
| Severidade | SEV2 se bloqueia operadores; SEV3 se usuario isolado; SEV1 se bypass suspeito. |
| Impacto | Operacao fica bloqueada ou usuarios podem tentar repetidamente credenciais. |
| Como detectar | Alerta `OBS-006`, logs de `AuthController`, metricas `controlid_local_auth_attempts_total`. |
| Metricas/logs | Outcome de auth, role, device target pseudonimizado, status oficial sem senha/session. |
| Diagnostico | Confirmar usuario/role, credenciais locais, configuracao do equipamento, sessao oficial e clock quando assinatura estiver envolvida. |
| Contencao | Rate limit natural/operacional, bloquear origem abusiva se aplicavel, orientar reset controlado. |
| Mitigacao | Corrigir credencial/config local, reiniciar sessao oficial ou revalidar equipamento. |
| Recuperacao | Login local e login/logout do equipamento com credenciais autorizadas de teste. |
| Comunicacao | Nao compartilhar credenciais; informar canal seguro para reset. |
| Escalonamento | Tech Lead + responsavel de identidade; DPO se houver credencial exposta. |
| Validacao | Queda de falhas e login autorizado confirmado. |
| Pos-incidente | Revisar logs para ausencia de senha/session e necessidade de ajuste de rate limit. |

### IR-06 Falha de autorizacao

| Campo | Procedimento |
| --- | --- |
| Sintoma | 403 indevido, usuario acessa tela/acao sem permissao ou operacao administrativa exposta. |
| Severidade | SEV1 se houver acesso indevido; SEV2 se bloqueio indevido amplo. |
| Impacto | Risco de acao sensivel, privacidade, integridade ou indisponibilidade operacional. |
| Como detectar | Logs 401/403, relato de operador, testes de RBAC, dashboard de seguranca. |
| Metricas/logs | `controlid_http_requests_total` por 401/403, rota, role, correlation ID. |
| Diagnostico | Verificar policy/attribute, role do usuario, rota, metodo HTTP, antiforgery e ultimo deploy. |
| Contencao | Revogar sessao/usuario afetado, restringir acesso no proxy ou rollback se regressao recente. |
| Mitigacao | Corrigir policy/autorizacao em camada confiavel e adicionar teste de permissao. |
| Recuperacao | Validar usuario autorizado e nao autorizado no fluxo afetado. |
| Comunicacao | Informar impacto interno; se acesso indevido a dado pessoal for possivel, acionar DPO. |
| Escalonamento | Incident Commander + Security/AppSec + Tech Lead para qualquer bypass. |
| Validacao | Testes de autorizacao passando e sem novo acesso indevido em logs. |
| Pos-incidente | Revisar matriz de permissoes e criterios de aceite do fluxo. |

### IR-07 Integracao Control iD indisponivel

| Campo | Procedimento |
| --- | --- |
| Sintoma | Timeouts, circuito aberto, status nao 2xx ou equipamento sem resposta. |
| Severidade | SEV2 se fluxo essencial depende do equipamento; SEV3 se fluxo auxiliar. |
| Impacto | Operacoes oficiais, hardware, objetos, modos e validacoes podem falhar. |
| Como detectar | Alertas `OBS-004` e `OBS-005`, logs do `OfficialApiInvokerService`. |
| Metricas/logs | `controlid_official_api_invocations_total`, duracao, endpoint id, status group. |
| Diagnostico | Validar IP/porta/rede, firmware, sessao, allowlist `ControlIDApi__AllowedDeviceHosts__0` e credenciais fora do Git. |
| Contencao | Pausar operacoes repetitivas e nao criar retry manual em massa. |
| Mitigacao | Restaurar conectividade, renovar sessao, ajustar config segura ou acionar suporte/rede. |
| Recuperacao | Executar contrato fisico seguro com `tools/contract-controlid-device.ps1` quando ambiente permitir. |
| Comunicacao | Informar dependencia externa/equipamento e workaround manual se existir. |
| Escalonamento | SRE + responsavel pelo equipamento/rede + fornecedor quando aplicavel. |
| Validacao | Sem timeouts novos e endpoint de leitura seguro responde. |
| Pos-incidente | Registrar endpoint afetado, firmware/modelo e lacuna de contrato. |

### IR-08 Webhook/callback falhando

| Campo | Procedimento |
| --- | --- |
| Sintoma | Callbacks rejeitados, monitor sem eventos, push ingress falhando ou status 4xx/5xx. |
| Severidade | SEV2; SEV1 se bloqueio expuser dado ou causar perda de evento critico. |
| Impacto | Eventos de acesso, monitoramento e fila push podem ficar incompletos. |
| Como detectar | Alerta `OBS-007`, logs de `CallbackIngressService`, dashboards de ingressos externos. |
| Metricas/logs | `controlid_callback_ingress_total`, path, event family, outcome, status group. |
| Diagnostico | Validar shared key, assinatura HMAC, timestamp/nonce, IP permitido, tamanho de payload, URL publica e proxy assinador se usado. |
| Contencao | Bloquear origem suspeita, pausar equipamento afetado ou voltar configuracao anterior segura. |
| Mitigacao | Corrigir segredo/assinatura/allowlist/proxy; nao desabilitar controles fora de ambiente controlado. |
| Recuperacao | Enviar evento ficticio autorizado ou validar fluxo com equipamento em bancada. |
| Comunicacao | Informar possivel lacuna de eventos e janela afetada. |
| Escalonamento | Security/AppSec + SRE + responsavel do equipamento. |
| Validacao | Callback aceito com correlation ID e persistencia confirmada. |
| Pos-incidente | Revisar se houve perda de evento, replay ou payload acima do limite. |

### IR-09 Job/fila push travado

| Campo | Procedimento |
| --- | --- |
| Sintoma | Comandos permanecem pendentes, polling vazio inesperado ou resultados nao atualizam. |
| Severidade | SEV2 se impacta operacao; SEV3 se fila nao critica. |
| Impacto | Equipamento pode nao receber comando ou operador pode reenfileirar manualmente. |
| Como detectar | Metricas de `controlid_push_operations_total`, tela `PushCenter`, logs de command id. |
| Metricas/logs | Operacoes enqueue/poll/result/persist, command id, device ref pseudonimizado. |
| Diagnostico | Verificar status, device id, idempotency key, permissao SQLite, resultado sem command id e clock. |
| Contencao | Pausar novos comandos para o device afetado; nao limpar fila sem confirmacao e backup quando houver historico importante. |
| Mitigacao | Corrigir persistencia/config do equipamento; reenfileirar somente com decisao operacional. |
| Recuperacao | Validar um comando ficticio em stub ou bancada e confirmar transicao pendente/entregue/concluido. |
| Comunicacao | Informar comandos possivelmente pendentes e evitar duplicidade manual. |
| Escalonamento | Backend/SRE + responsavel pelo equipamento. |
| Validacao | Fila sem pendencias anormais e resultados persistidos. |
| Pos-incidente | Registrar command ids afetados sem payload bruto e decidir retencao/expurgo. |

### IR-10 Deploy ruim

| Campo | Procedimento |
| --- | --- |
| Sintoma | Falha apos nova versao: health falha, 5xx, regressao de UI/API ou startup bloqueado por config. |
| Severidade | SEV1 se indisponivel; SEV2 se degradacao com rollback possivel. |
| Impacto | Pode afetar todos os fluxos e dados locais. |
| Como detectar | Falha em readiness, smoke, alertas apos deploy ou relato de operador. |
| Metricas/logs | Commit/tag, imagem, logs de startup, health, metricas antes/depois. |
| Diagnostico | Comparar versao anterior, diff de config, variaveis, migrations e resultado dos gates. |
| Contencao | Parar rollout, manter evidencia, acionar rollback tecnico para imagem anterior com mesmo volume. |
| Mitigacao | Corrigir config/hotfix em branch separado ou manter rollback ate validar. |
| Recuperacao | Reexecutar `tools/test-readiness-gates.ps1` e smoke do fluxo afetado. |
| Comunicacao | Informar versao afetada, rollback/hotfix e risco residual. |
| Escalonamento | Release Engineer + Tech Lead + Incident Commander em SEV1. |
| Validacao | Versao anterior ou hotfix saudavel e checks relevantes passando. |
| Pos-incidente | Revisar gate que nao capturou regressao e adicionar teste/alerta. |

### IR-11 Migracao problematica

| Campo | Procedimento |
| --- | --- |
| Sintoma | Startup falha em `Database.Migrate()`, schema inconsistente ou consulta quebra apos migration. |
| Severidade | SEV1 se bloqueia app ou corrompe dado; SEV2 se fluxo parcial. |
| Impacto | Banco local pode ficar indisponivel ou com dados incompletos. |
| Como detectar | Logs EF/SQLite, readiness falhando, erro apos deploy. |
| Metricas/logs | Logs de startup/migration, manifest de backup, migration id. |
| Diagnostico | Identificar migration, validar em copia restaurada, checar se houve operacao destrutiva ou campo obrigatorio novo. |
| Contencao | Parar app, preservar `.db`, `-wal`, `-shm`, gerar backup e nao executar novas tentativas destrutivas. |
| Mitigacao | Aplicar rollback de app se schema permitir; corrigir migration em ambiente controlado antes de tocar dado real. |
| Recuperacao | Restore somente com confirmacao humana e apos smoke em copia. |
| Comunicacao | Informar risco de dados e janela de indisponibilidade. |
| Escalonamento | Data/Backend + SRE + DPO se dados pessoais estiverem em risco. |
| Validacao | `/health/ready` Healthy e testes de repositorio/fluxo passam. |
| Pos-incidente | Documentar estrategia zero/low downtime e teste de migration faltante. |

### IR-12 Dados corrompidos

| Campo | Procedimento |
| --- | --- |
| Sintoma | Registros duplicados inesperados, payload invalido persistido, erro de leitura ou divergencia entre UI e equipamento. |
| Severidade | SEV1 se compromete integridade ou titulares; SEV2 se escopo limitado. |
| Impacto | Decisoes operacionais podem usar informacao incorreta. |
| Como detectar | Relato, logs de validacao, erro EF/SQLite, comparacao com equipamento. |
| Metricas/logs | Change logs, monitor/push ids, timestamps, usuario/role pseudonimizado. |
| Diagnostico | Delimitar tabelas, periodo, origem e ultima escrita; nao editar diretamente sem backup. |
| Contencao | Suspender escrita no conjunto afetado e preservar copia forense local restrita. |
| Mitigacao | Corrigir via fluxo da aplicacao ou script revisado em ambiente controlado; restore se aprovado. |
| Recuperacao | Validar consistencia com consultas, telas e equipamento. |
| Comunicacao | Informar dados/periodo afetados sem expor conteudo pessoal. |
| Escalonamento | Data/Backend + DPO/Juridico quando houver dado pessoal. |
| Validacao | Amostra validada e logs sem nova corrupcao. |
| Pos-incidente | Adicionar validacao, constraint ou teste se contrato estiver claro. |

### IR-13 Vazamento de dados

| Campo | Procedimento |
| --- | --- |
| Sintoma | Dado pessoal/sensivel em log, repo, artefato, tela, response, backup compartilhado ou canal inseguro. |
| Severidade | SEV1 ate avaliacao DPO/Juridico. |
| Impacto | Risco a titulares, obrigacoes LGPD, perda de confianca e necessidade de notificacao. |
| Como detectar | Revisao, scan de secrets, relato, diff, logs, artefato publicado. |
| Metricas/logs | Evidencia minimizada, caminho/commit/artefato, periodo, categorias de dados. |
| Diagnostico | Classificar dado, escopo, quem acessou, origem e se ainda esta exposto. |
| Contencao | Remover acesso publico, revogar compartilhamento, preservar evidencia restrita e nao apagar trilha sem aprovacao. |
| Mitigacao | Corrigir fonte tecnica, rotacionar dados/credenciais se aplicavel, expurgar copias conforme decisao formal. |
| Recuperacao | Confirmar que novo log/response/artefato esta minimizado e scan nao encontra recorrencia. |
| Comunicacao | Acionar DPO/Juridico; comunicacao externa somente apos decisao humana formal. |
| Escalonamento | Incident Commander + DPO/Juridico + Security/AppSec + Tech Lead. |
| Validacao | Scan, revisao de diffs e teste de privacidade do fluxo afetado. |
| Pos-incidente | Postmortem com decisao de notificacao, categorias, titulares, causa e controles novos. |

### IR-14 Secret comprometido

| Campo | Procedimento |
| --- | --- |
| Sintoma | Shared key, senha, token, session string, certificado privado ou API key aparece em repo/log/artefato/canal. |
| Severidade | SEV1 ate rotacao e confirmacao de escopo. |
| Impacto | Bypass de callback, acesso indevido, fraude operacional ou movimento lateral. |
| Como detectar | `tools/scan-secrets.ps1`, revisao, alerta de provedor ou relato. |
| Metricas/logs | Caminho/commit/canal, horario, tipo de segredo, sistemas dependentes. |
| Diagnostico | Determinar se segredo era real, ativo, ambiente, privilegio e exposicao. |
| Contencao | Revogar/rotacionar imediatamente em ambiente dono; invalidar sessoes afetadas. |
| Mitigacao | Atualizar secrets fora do Git, remover valor de docs/logs/artefatos com preservacao de evidencia restrita. |
| Recuperacao | Confirmar que apps usam novo segredo e que callbacks assinados ainda funcionam. |
| Comunicacao | Notificar responsaveis internos; DPO/Juridico se segredo dava acesso a dados pessoais. |
| Escalonamento | Security/AppSec + SRE + dono do sistema/provedor. |
| Validacao | Scan limpo, probes saudaveis e ausencia de uso do segredo antigo. |
| Pos-incidente | Registrar origem, controle faltante, rotacao, impacto e decisao sobre historico Git/artefatos. |

## Continuidade operacional

| Item | Estado atual | Procedimento |
| --- | --- | --- |
| Backup SQLite | Manual, DPAPI por padrao, em `tools/backup-sqlite.ps1` | Executar antes de mudanca de schema, deploy com risco de dados ou investigacao que possa exigir rollback. |
| Restore SQLite | Smoke em copia com `tools/restore-smoke-sqlite.ps1`; restore real exige confirmacao humana | Validar backup em copia, parar app, preservar estado atual e restaurar somente com autorizacao. |
| Volume container | `docker-compose.yml` usa `controlid-data:/app/data` e `controlid-logs:/app/Logs` | Nunca executar container de ambiente persistente sem volume duravel. |
| Rollback app | Documentado em `docs/deployment-runbook.md` | Manter imagem anterior tagueada e reusar mesmo `.env`/volumes quando rollback for aprovado. |
| Equipamento Control iD | Dependencia externa/fisica | Manter procedimento manual de contingencia do cliente/operacao fora do repo. |
| Observabilidade | Health, metricas, alertas JSON e monitor local | Usar `tools/observability-check.ps1` offline/online conforme ambiente. |
| Configuracao operacional | `ops.example.json` versionado e `ops.local.json` ignorado pelo Git | Validar com `tools/operational-readiness-check.ps1 -RequireConfig` antes de release real. |
| Backup operacional | `tools/backup-sqlite-operational.ps1` envolve backup DPAPI, mirror opcional, restore-smoke e retencao confirmada | Definir `CONTROLID_BACKUP_MIRROR_DIRECTORY` ou `-MirrorDirectory` para copia fora do host. |
| Contingencia fisica | `docs/equipment-contingency-runbook.md` | Testar manual fallback em bancada e registrar dono em `ops.local.json`. |

## RTO/RPO

As metas abaixo sao objetivos iniciais de planejamento, nao SLA homologado. A PoC
nao possui provedor produtivo, backup automatico, restore periodico obrigatorio ou
replicacao.

| Cenario | RTO alvo inicial | RPO alvo inicial | Status |
| --- | --- | --- | --- |
| Falha de processo/container sem corrupcao | Ate 30 minutos apos deteccao | 0 se volume SQLite intacto | Necessita validacao em ambiente alvo. |
| Deploy ruim com imagem anterior disponivel | Ate 60 minutos apos decisao de rollback | 0 se schema compativel e volume intacto | Depende de imagem anterior e gate de rollback. |
| SQLite corrompido com backup valido | Ate 4 horas apos decisao de restore | Desde o ultimo backup valido | Nao garantido; restore real nao homologado. |
| Vazamento/secret comprometido | Contencao inicial em ate 30 minutos | N/A | Depende de rotacao no provedor/equipamento. |
| Perda total de host sem backup externo | Indefinido | Indefinido | Lacuna critica ate haver backup fora do host. |

Lacunas para producao real:

- Definir provedor, storage, criptografia, retencao e local de backup em `ops.local.json`.
- Automatizar a chamada de `tools/backup-sqlite-operational.ps1` no host alvo e testar restore periodico.
- Definir RTO/RPO aprovados por negocio e DPO em `ops.local.json`.
- Validar `docs/equipment-contingency-runbook.md` com a operacao fisica.
- Definir canal oficial de incidentes, on-call e calendario de revisao.

## Incidentes de seguranca e LGPD

Classificacao inicial:

- Suspeita de dado pessoal exposto: SEV1 ate triagem.
- Secret real exposto: SEV1 ate rotacao.
- Acesso indevido confirmado: SEV1.
- Log com dado pessoal minimizado insuficientemente: SEV2 ou SEV1 se publicado.

Procedimento:

1. Conter acesso ao sistema, host, logs, SQLite, backups e artefatos.
2. Preservar evidencias em local restrito; nao colar payload bruto em tickets.
3. Identificar categorias de dados, titulares possivelmente afetados, periodo,
   causa provavel, terceiros e ambiente.
4. Rotacionar credenciais, shared keys, sessoes e secrets afetados.
5. Executar `powershell -ExecutionPolicy Bypass -File .\tools\scan-secrets.ps1`.
6. Acionar DPO/Juridico para decidir ANPD/titulares e prazos.
7. Registrar decisao, mitigacao, comunicacao, risco residual e revisao preventiva.

## Comunicacao interna

Use mensagens curtas e verificaveis:

- Status: investigando, contido, mitigado, recuperado ou encerrado.
- Severidade atual e criterio.
- Impacto funcional sem expor dados pessoais.
- Inicio do incidente e proxima atualizacao.
- Acao esperada de operadores, se houver.
- Link para ticket/canal interno restrito.

Nao publicar:

- Senhas, tokens, shared keys, session strings, biometria, fotos, documentos,
  payloads completos, headers de auth ou banco/backups.
- Comunicacao externa oficial sem aprovacao DPO/Juridico/controlador.

## Template de postmortem

```markdown
# Postmortem: <titulo>

Data:
Severidade:
Incident Commander:
Sistemas/versoes afetados:
Status final:

## Resumo

<O que aconteceu, em linguagem objetiva e sem dados sensiveis.>

## Impacto

- Usuarios/operadores afetados:
- Fluxos afetados:
- Dados afetados:
- Duracao:
- RTO/RPO observado:

## Linha do tempo

| Horario | Evento | Evidencia |
| --- | --- | --- |
| | | |

## Causa raiz

- Causa tecnica:
- Causa operacional/processual:
- Fatores contribuintes:

## Deteccao

- Alerta/sinal:
- Tempo ate deteccao:
- O que deveria ter alertado:

## Resposta

- Contencao:
- Mitigacao:
- Recuperacao:
- Comunicacao:

## O que funcionou

-

## O que falhou

-

## Acoes corretivas

| Acao | Dono | Prioridade | Prazo | Evidencia esperada |
| --- | --- | --- | --- | --- |
| | | | | |

## Riscos residuais

-

## Decisoes DPO/Juridico

<Preencher somente por responsavel autorizado quando envolver dados pessoais.>
```

## Riscos residuais

| Risco | Severidade | Mitigacao atual | Proxima acao |
| --- | --- | --- | --- |
| Sem provedor produtivo/on-call formal | Alta | `ops.example.json`, `ops.local.json` ignorado e `operational-readiness-check.ps1 -RequireConfig` bloqueando release sem donos/canais reais | Preencher e aprovar `ops.local.json` antes de uso real. |
| Backup automatico fora do host ausente | Alta | `backup-sqlite-operational.ps1` com mirror opcional, restore-smoke e retencao confirmada | Agendar o script no host alvo com destino externo seguro. |
| RTO/RPO nao homologados | Alta | Gate exige `rtoRpo.validationStatus` aprovado/validado em `ops.local.json` | Executar exercicio real em staging/producao com dados ficticios. |
| Dependencia de equipamento fisico | Alta | `docs/equipment-contingency-runbook.md` e contrato fisico via gate | Testar fallback manual com operacao fisica e fornecedor. |
| Comunicacao externa LGPD depende de decisao humana | Alta | `ops.local.json` exige DPO/privacy owner, canal de escalonamento e repositorio de evidencias | DPO/Juridico devem aprovar canal, prazo e template externo. |
