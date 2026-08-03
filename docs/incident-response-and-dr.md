# Resposta a incidentes, continuidade e DR

Escopo: operação da PoC ASP.NET Core MVC/Razor de integração Control iD, com
SQLite local, callbacks/push, observabilidade local e execução containerizada.
Este runbook complementa `docs/observability-runbook.md`,
`docs/deployment-runbook.md`, `docs/data-model-and-recovery.md` e
`docs/privacy-governance-runbook.md`. Para uso real, copie `ops.example.json`
para `ops.local.json` fora do Git e preencha donos, canais, evidências, backup
externo, RTO/RPO e contingência física.

Este documento não autoriza rollback real, alteração de produção, deleção de
evidências, comunicação externa oficial ou restauração destrutiva sem decisão
humana responsável.

## Princípios de comando

- Proteger pessoas, dados e continuidade antes de corrigir código.
- Preservar evidências: logs, timestamps, correlation IDs, artefatos de deploy,
  versão, configuração efetiva e manifests de backup.
- Não copiar payloads pessoais, biometria, fotos, cartões, QR codes, senhas,
  session strings, shared keys ou headers de auth para tickets e chats.
- Registrar hipoteses como hipoteses até existir evidência.
- Fazer contenção reversível sempre que possível.
- Validar normalização com `/health/live`, `/health/ready`, logs, métricas e
  smoke/check relevante antes de encerrar.

## Matriz de severidade

| Severidade | Critérios | Impacto | Urgência | Responsável inicial |
| --- | --- | --- | --- | --- |
| SEV1 | Aplicação indisponível, perda ou corrupção de dados, vazamento confirmado, segredo comprometido, falha ampla de autenticação ou autorização, ou implantação que impede operação crítica | Operação parada, risco alto para titulares ou integridade comprometida | Resposta imediata | Comandante do incidente + Tech Lead/SRE |
| SEV2 | Degradação relevante, 5xx recorrente, banco instável, callbacks ou Push críticos falhando, integração externa indisponível em fluxo essencial | Operação parcial, risco de acúmulo ou atendimento manual | Resposta prioritária | SRE/Engenheiro de Release |
| SEV3 | Falha localizada, latência alta sem indisponibilidade, erro funcional com workaround, alerta recorrente sem impacto amplo | Impacto limitado e controlável | Resposta em horário operacional | Dono técnico da área |
| SEV4 | Dúvida, aviso, melhoria preventiva, documentação incompleta ou falso positivo confirmado | Sem impacto imediato | Planejamento normal | Mantenedor |

## Funções e escalonamento

| Papel | Responsabilidade |
| --- | --- |
| Incident Commander | Classifica SEV, coordena sala/canal, controla linha do tempo, aprova encerramento. |
| SRE/Operação | Diagnostica saúde, logs, métricas, container, host, rede, banco e rollback técnico. |
| Tech Lead/Maintainer | Avalia código, contrato, migrações, hotfix e risco de regressão. |
| Release Engineer | Controla versões, artefatos, tags, deploy/rollback e gates de readiness. |
| DPO/Jurídico | Decide comunicação externa, ANPD/titulares, base legal e preservação de evidências de dados. |
| Negócio/Produto | Define impacto funcional, usuários afetados e comunicação interna. |

Escalonar para SEV1 quando houver qualquer indicio de dado pessoal exposto,
secret real comprometido, corrupção de SQLite, indisponibilidade total ou falha
de autorização que permita acesso indevido.

## Plano geral de resposta

1. Declarar incidente com horário, severidade inicial, sistema, versão e impacto.
2. Designar Incident Commander e responsáveis técnicos.
3. Congelar mudanças não essenciais; não fazer deploy paralelo sem autorização.
4. Coletar evidências minimizadas: `X-Correlation-ID`, horários UTC/local, rota,
   status, duração, evento operacional, commit/tag, imagem container, host e
   alerta acionado.
5. Conter o impacto com ação reversível: remover tráfego, pausar chamada ao
   equipamento, bloquear origem, voltar versão, isolar host ou rotacionar segredo.
6. Mitigar causa provável e validar saúde.
7. Comunicar status interno de forma objetiva, sem dados sensíveis.
8. Encerrar somente após normalização validada e criação de postmortem.

## Guias operacionais por cenário

### IR-01 API fora do ar

| Campo | Procedimento |
| --- | --- |
| Sintoma | `/health/live` falha, porta não responde, container/processo encerrado ou erro de startup. |
| Severidade | SEV1 se indisponibilidade total; SEV2 se há instancia alternativa funcional. |
| Impacto | UI, callbacks, push, métricas e operação local indisponíveis. |
| Como detectar | Alerta `OBS-001`, probe de `/health/live`, logs de supervisor/container. |
| Métricas/logs | `controlid_http_requests_total`, logs de startup, stdout/stderr, eventos de deploy. |
| Diagnóstico | Confirmar processo, porta 8080/porta configurada, variáveis obrigatórias, último commit/tag, `AllowedHosts`, shared key e paths de volume. |
| Contenção | Pausar novo tráfego, remover instancia quebrada do balanceador ou voltar para imagem anterior se existir. |
| Mitigação | Corrigir configuração ausente, reiniciar processo/container ou aplicar rollback técnico documentado. |
| Recuperação | Validar `/health/live`, `/health/ready`, login local e uma rota segura de leitura. |
| Comunicação | Informar indisponibilidade, impacto funcional, workaround e próxima atualização interna. |
| Escalonamento | SRE, Release Engineer e Tech Lead; SEV1 aciona Incident Commander. |
| Validação | Dois probes saudáveis consecutivos e ausência de novo erro de startup. |
| Pós-incidente | Registrar causa, gate ausente, diferença entre config local/staging/produção e ação preventiva. |

### IR-02 Banco SQLite indisponível

| Campo | Procedimento |
| --- | --- |
| Sintoma | `/health/ready` falha, erros EF/SQLite, lock persistente, permissão negada ou disco cheio. |
| Severidade | SEV1 se impede escrita ou leitura crítica, ou se há risco de perda; SEV2 se o impacto for parcial. |
| Impacto | Monitor, push, sessões, cadastros e auditoria local degradados. |
| Como detectar | Alerta `OBS-002`, `OBS-008`, `OBS-009`, logs de persistência. |
| Métricas/logs | `/health/ready`, logs `persistence_failed`, paths SQLite, espaço em disco. |
| Diagnóstico | Conferir volume `/app/data`, arquivo `.db`, `-wal`, `-shm`, permissão, processo concorrente e migrations recentes. |
| Contenção | Parar novas escritas quando possível, preservar arquivos atuais e gerar backup antes de qualquer tentativa destrutiva. |
| Mitigação | Corrigir permissão/volume/disco; se for schema, validar em cópia com `tools/restore-smoke-sqlite.ps1`. |
| Recuperação | Subir app em ambiente controlado e validar readiness, listagens e persistência de evento fictício. |
| Comunicação | Informar risco de indisponibilidade de histórico local e janela de recuperação estimada. |
| Escalonamento | SRE + Data/Backend; DPO se dados pessoais puderem ter sido expostos ou perdidos. |
| Validação | `/health/ready` Healthy e logs sem novas falhas de persistência. |
| Pós-incidente | Registrar backup usado, cópia restaurada, checksum se disponível e gaps de RTO/RPO. |

### IR-03 Latência alta

| Campo | Procedimento |
| --- | --- |
| Sintoma | UI lenta, timeouts ocasionais, P95/P99 alto ou requests acumulando. |
| Severidade | SEV2 se afeta fluxo essencial; SEV3 se localizada. |
| Impacto | Operadores podem repetir comandos, gerando risco de duplicidade ou backlog. |
| Como detectar | Dashboard de latência HTTP e duração de Access API. |
| Métricas/logs | `controlid_http_request_duration_milliseconds`, `controlid_official_api_duration_milliseconds`, logs de endpoint e duração. |
| Diagnóstico | Separar latência local, SQLite, rede e equipamento; verificar endpoint oficial, payload grande e volume de callbacks/push. |
| Contenção | Reduzir tráfego não essencial, pausar operações repetitivas e orientar operadores a aguardar retorno. |
| Mitigação | Validar equipamento/rede, circuito aberto, limite de payload e consultas locais. |
| Recuperação | Confirmar P95/P99 normalizado e ausência de timeouts novos. |
| Comunicação | Informar degradação e evitar reenvios manuais até estabilizar. |
| Escalonamento | SRE + dono do fluxo afetado; fornecedor/equipe de rede se equipamento for causa provável. |
| Validação | Janela de métricas sem degradação e smoke do fluxo impactado. |
| Pós-incidente | Registrar gargalo, estimativa de carga, necessidade de paginação/cache/limite adicional. |

### IR-04 Erro 5xx elevado

| Campo | Procedimento |
| --- | --- |
| Sintoma | Erros 500/502/503, tela genérica de erro ou alerta de 5xx. |
| Severidade | SEV2; SEV1 se amplo ou em auth/autorização/dados. |
| Impacto | Fluxos críticos falham e podem ocultar falha de integração ou banco. |
| Como detectar | Alerta `OBS-003`, logs do `ExceptionHandlingMiddleware`, correlation ID. |
| Métricas/logs | `controlid_http_requests_total{status_group="5xx"}`, rota, trace id, stack no log interno. |
| Diagnóstico | Agrupar por rota, commit, entrada, usuário role e dependência. Verificar se há release recente. |
| Contenção | Desabilitar operação afetada por orientação operacional ou rollback se novo deploy causou falha. |
| Mitigação | Corrigir configuração, dependência indisponível ou bug; não expor stack trace ao usuário. |
| Recuperação | Build/teste/smoke relacionado e queda sustentada de 5xx. |
| Comunicação | Informar rotas afetadas e workaround. |
| Escalonamento | Tech Lead + Release Engineer se houver hotfix/rollback. |
| Validação | Sem 5xx novos por janela definida e fluxo validado manualmente quando aplicável. |
| Pós-incidente | Criar teste regressivo para a rota/entrada que quebrou. |

### IR-05 Falha de autenticação

| Campo | Procedimento |
| --- | --- |
| Sintoma | Logins locais falham, login no equipamento falha ou sessões expiram inesperadamente. |
| Severidade | SEV2 se bloqueia operadores; SEV3 se usuário isolado; SEV1 se bypass suspeito. |
| Impacto | Operação fica bloqueada ou usuários podem tentar repetidamente credenciais. |
| Como detectar | Alerta `OBS-006`, logs de `AuthController`, métricas `controlid_local_auth_attempts_total`. |
| Métricas/logs | Outcome de auth, role, device target pseudonimizado, status oficial sem senha/session. |
| Diagnóstico | Confirmar usuário/role, credenciais locais, configuração do equipamento, sessão oficial e clock quando assinatura estiver envolvida. |
| Contenção | Rate limit natural/operacional, bloquear origem abusiva se aplicável, orientar reset controlado. |
| Mitigação | Corrigir credencial/config local, reiniciar sessão oficial ou revalidar equipamento. |
| Recuperação | Login local e login/logout do equipamento com credenciais autorizadas de teste. |
| Comunicação | Não compartilhar credenciais; informar canal seguro para reset. |
| Escalonamento | Tech Lead + responsável de identidade; DPO se houver credencial exposta. |
| Validação | Queda de falhas e login autorizado confirmado. |
| Pós-incidente | Revisar logs para ausência de senha/session e necessidade de ajuste de rate limit. |

### IR-06 Falha de autorização

| Campo | Procedimento |
| --- | --- |
| Sintoma | 403 indevido, usuário acessa tela/ação sem permissão ou operação administrativa exposta. |
| Severidade | SEV1 se houver acesso indevido; SEV2 se bloqueio indevido amplo. |
| Impacto | Risco de ação sensível, privacidade, integridade ou indisponibilidade operacional. |
| Como detectar | Logs 401/403, relato de operador, testes de RBAC, dashboard de segurança. |
| Métricas/logs | `controlid_http_requests_total` por 401/403, rota, role, correlation ID. |
| Diagnóstico | Verificar policy/attribute, role do usuário, rota, método HTTP, antiforgery e último deploy. |
| Contenção | Revogar sessão/usuário afetado, restringir acesso no proxy ou rollback se regressão recente. |
| Mitigação | Corrigir policy/autorização em camada confiável e adicionar teste de permissão. |
| Recuperação | Validar usuário autorizado e não autorizado no fluxo afetado. |
| Comunicação | Informar impacto interno; se acesso indevido a dado pessoal for possível, acionar DPO. |
| Escalonamento | Incident Commander + Security/AppSec + Tech Lead para qualquer bypass. |
| Validação | Testes de autorização passando e sem novo acesso indevido em logs. |
| Pós-incidente | Revisar matriz de permissões e critérios de aceite do fluxo. |

### IR-07 Integração Control iD indisponível

| Campo | Procedimento |
| --- | --- |
| Sintoma | Timeouts, circuito aberto, status não 2xx ou equipamento sem resposta. |
| Severidade | SEV2 se fluxo essencial depende do equipamento; SEV3 se fluxo auxiliar. |
| Impacto | Operações oficiais, hardware, objetos, modos e validações podem falhar. |
| Como detectar | Alertas `OBS-004` e `OBS-005`, logs do `OfficialApiInvokerService`. |
| Métricas/logs | `controlid_official_api_invocations_total`, duração, endpoint id, status group. |
| Diagnóstico | Validar IP/porta/rede, firmware, sessão, allowlist `ControlIDApi__AllowedDeviceHosts__0` e credenciais fora do Git. |
| Contenção | Pausar operações repetitivas e não criar retry manual em massa. |
| Mitigação | Restaurar conectividade, renovar sessão, ajustar config segura ou acionar suporte/rede. |
| Recuperação | Executar contrato físico seguro com `tools/contract-controlid-device.ps1` quando ambiente permitir. |
| Comunicação | Informar dependência externa/equipamento e workaround manual se existir. |
| Escalonamento | SRE + responsável pelo equipamento/rede + fornecedor quando aplicável. |
| Validação | Sem timeouts novos e endpoint de leitura seguro responde. |
| Pós-incidente | Registrar endpoint afetado, firmware/modelo e lacuna de contrato. |

### IR-08 Webhook/callback falhando

| Campo | Procedimento |
| --- | --- |
| Sintoma | Callbacks rejeitados, monitor sem eventos, push ingress falhando ou status 4xx/5xx. |
| Severidade | SEV2; SEV1 se bloqueio expuser dado ou causar perda de evento crítico. |
| Impacto | Eventos de acesso, monitoramento e fila push podem ficar incompletos. |
| Como detectar | Alerta `OBS-007`, logs de `CallbackIngressService`, dashboards de ingressos externos. |
| Métricas/logs | `controlid_callback_ingress_total`, path, event family, outcome, status group. |
| Diagnóstico | Validar chave compartilhada, assinatura HMAC, carimbo de data e hora, nonce, IP permitido, tamanho do payload, URL pública e proxy assinador, se utilizado. |
| Contenção | Bloquear origem suspeita, pausar equipamento afetado ou voltar configuração anterior segura. |
| Mitigação | Corrigir segredo/assinatura/allowlist/proxy; não desabilitar controles fora de ambiente controlado. |
| Recuperação | Enviar evento fictício autorizado ou validar fluxo com equipamento em bancada. |
| Comunicação | Informar possível lacuna de eventos e janela afetada. |
| Escalonamento | Security/AppSec + SRE + responsável do equipamento. |
| Validação | Callback aceito com correlation ID e persistência confirmada. |
| Pós-incidente | Revisar se houve perda de evento, replay ou payload acima do limite. |

### IR-09 Tarefa ou fila Push travada

| Campo | Procedimento |
| --- | --- |
| Sintoma | Comandos permanecem pendentes, polling vazio inesperado ou resultados não atualizam. |
| Severidade | SEV2 se afetar a operação; SEV3 se a fila não for crítica. |
| Impacto | Equipamento pode não receber comando ou operador pode reenfileirar manualmente. |
| Como detectar | Métricas de `controlid_push_operations_total`, tela `PushCenter`, logs de command id. |
| Métricas/logs | Operações enqueue/poll/result/persist, command id, device ref pseudonimizado. |
| Diagnóstico | Verificar status, device id, idempotency key, permissão SQLite, resultado sem command id e clock. |
| Contenção | Pausar novos comandos para o device afetado; não limpar fila sem confirmação e backup quando houver histórico importante. |
| Mitigação | Corrigir persistência/config do equipamento; reenfileirar somente com decisão operacional. |
| Recuperação | Validar um comando fictício em stub ou bancada e confirmar transição pendente/entregue/concluído. |
| Comunicação | Informar comandos possivelmente pendentes e evitar duplicidade manual. |
| Escalonamento | Backend/SRE + responsável pelo equipamento. |
| Validação | Fila sem pendências anormais e resultados persistidos. |
| Pós-incidente | Registrar command ids afetados sem payload bruto e decidir retenção/expurgo. |

### IR-10 Implantação malsucedida

| Campo | Procedimento |
| --- | --- |
| Sintoma | Falha após nova versão: health falha, 5xx, regressão de UI/API ou startup bloqueado por config. |
| Severidade | SEV1 se indisponível; SEV2 se degradação com rollback possível. |
| Impacto | Pode afetar todos os fluxos e dados locais. |
| Como detectar | Falha em readiness, smoke, alertas após deploy ou relato de operador. |
| Métricas/logs | Commit/tag, imagem, logs de startup, health, métricas antes/depois. |
| Diagnóstico | Comparar versão anterior, diff de config, variáveis, migrations e resultado dos gates. |
| Contenção | Parar rollout, manter evidência, acionar rollback técnico para imagem anterior com mesmo volume. |
| Mitigação | Corrigir config/hotfix em branch separado ou manter rollback até validar. |
| Recuperação | Reexecutar `tools/test-readiness-gates.ps1` e smoke do fluxo afetado. |
| Comunicação | Informar versão afetada, rollback/hotfix e risco residual. |
| Escalonamento | Release Engineer + Tech Lead + Incident Commander em SEV1. |
| Validação | Versão anterior ou hotfix saudável e checks relevantes passando. |
| Pós-incidente | Revisar gate que não capturou regressão e adicionar teste/alerta. |

### IR-11 Migração problemática

| Campo | Procedimento |
| --- | --- |
| Sintoma | Startup falha em `Database.Migrate()`, schema inconsistente ou consulta quebra após migration. |
| Severidade | SEV1 se bloqueia app ou corrompe dado; SEV2 se fluxo parcial. |
| Impacto | Banco local pode ficar indisponível ou com dados incompletos. |
| Como detectar | Logs EF/SQLite, readiness falhando, erro após deploy. |
| Métricas/logs | Logs de startup/migration, manifest de backup, migration id. |
| Diagnóstico | Identificar migration, validar em cópia restaurada, checar se houve operação destrutiva ou campo obrigatório novo. |
| Contenção | Parar app, preservar `.db`, `-wal`, `-shm`, gerar backup e não executar novas tentativas destrutivas. |
| Mitigação | Aplicar rollback de app se schema permitir; corrigir migration em ambiente controlado antes de tocar dado real. |
| Recuperação | Restore somente com confirmação humana e após smoke em cópia. |
| Comunicação | Informar risco de dados e janela de indisponibilidade. |
| Escalonamento | Data/Backend + SRE + DPO se dados pessoais estiverem em risco. |
| Validação | `/health/ready` Healthy e testes de repositório/fluxo passam. |
| Pós-incidente | Documentar estratégia zero/low downtime e teste de migration faltante. |

### IR-12 Dados corrompidos

| Campo | Procedimento |
| --- | --- |
| Sintoma | Registros duplicados inesperados, payload inválido persistido, erro de leitura ou divergência entre UI e equipamento. |
| Severidade | SEV1 se compromete integridade ou titulares; SEV2 se escopo limitado. |
| Impacto | Decisões operacionais podem usar informação incorreta. |
| Como detectar | Relato, logs de validação, erro EF/SQLite, comparação com equipamento. |
| Métricas/logs | Change logs, monitor/push ids, timestamps, usuário/role pseudonimizado. |
| Diagnóstico | Delimitar tabelas, período, origem e última escrita; não editar diretamente sem backup. |
| Contenção | Suspender escrita no conjunto afetado e preservar cópia forense local restrita. |
| Mitigação | Corrigir via fluxo da aplicação ou script revisado em ambiente controlado; restore se aprovado. |
| Recuperação | Validar consistência com consultas, telas e equipamento. |
| Comunicação | Informar dados/período afetados sem expor conteúdo pessoal. |
| Escalonamento | Data/Backend + DPO/Jurídico quando houver dado pessoal. |
| Validação | Amostra validada e logs sem nova corrupção. |
| Pós-incidente | Adicionar validação, constraint ou teste se contrato estiver claro. |

### IR-13 Vazamento de dados

| Campo | Procedimento |
| --- | --- |
| Sintoma | Dado pessoal/sensível em log, repo, artefato, tela, response, backup compartilhado ou canal inseguro. |
| Severidade | SEV1 até avaliação DPO/Jurídico. |
| Impacto | Risco a titulares, obrigações LGPD, perda de confiança e necessidade de notificação. |
| Como detectar | Revisão, scan de secrets, relato, diff, logs, artefato publicado. |
| Métricas/logs | Evidência minimizada, caminho/commit/artefato, período, categorias de dados. |
| Diagnóstico | Classificar o dado, o escopo, quem o acessou, a origem e se ele ainda está exposto. |
| Contenção | Remover o acesso público, revogar o compartilhamento, preservar a evidência de forma restrita e não apagar a trilha sem aprovação. |
| Mitigação | Corrigir a fonte técnica, rotacionar dados ou credenciais quando aplicável e expurgar cópias conforme decisão formal. |
| Recuperação | Confirmar que o novo log, a resposta ou o artefato está minimizado e que a varredura não encontra recorrência. |
| Comunicação | Acionar DPO/Jurídico; comunicação externa somente após decisão humana formal. |
| Escalonamento | Incident Commander + DPO/Jurídico + Security/AppSec + Tech Lead. |
| Validação | Scan, revisão de diffs e teste de privacidade do fluxo afetado. |
| Pós-incidente | Postmortem com decisão de notificação, categorias, titulares, causa e controles novos. |

### IR-14 Segredo comprometido

| Campo | Procedimento |
| --- | --- |
| Sintoma | Shared key, senha, token, session string, certificado privado ou API key aparece em repo/log/artefato/canal. |
| Severidade | SEV1 até rotação e confirmação de escopo. |
| Impacto | Bypass de callback, acesso indevido, fraude operacional ou movimento lateral. |
| Como detectar | `tools/scan-secrets.ps1`, revisão, alerta de provedor ou relato. |
| Métricas/logs | Caminho/commit/canal, horário, tipo de segredo, sistemas dependentes. |
| Diagnóstico | Determinar se o segredo era real e ativo, além do ambiente, privilégio e nível de exposição. |
| Contenção | Revogar/rotacionar imediatamente em ambiente dono; invalidar sessões afetadas. |
| Mitigação | Atualizar secrets fora do Git, remover valor de docs/logs/artefatos com preservação de evidência restrita. |
| Recuperação | Confirmar que apps usam novo segredo e que callbacks assinados ainda funcionam. |
| Comunicação | Notificar responsáveis internos; DPO/Jurídico se segredo dava acesso a dados pessoais. |
| Escalonamento | Security/AppSec + SRE + dono do sistema/provedor. |
| Validação | Scan limpo, probes saudáveis e ausência de uso do segredo antigo. |
| Pós-incidente | Registrar origem, controle faltante, rotação, impacto e decisão sobre histórico Git/artefatos. |

## Continuidade operacional

| Item | Estado atual | Procedimento |
| --- | --- | --- |
| Backup SQLite | Manual, DPAPI por padrão, em `tools/backup-sqlite.ps1` | Executar antes de mudança de schema, deploy com risco de dados ou investigação que possa exigir rollback. |
| Restore SQLite | Smoke em cópia com `tools/restore-smoke-sqlite.ps1`; restore real exige confirmação humana | Validar backup em cópia, parar app, preservar estado atual e restaurar somente com autorização. |
| Volume container | `docker-compose.yml` usa `controlid-data:/app/data` e `controlid-logs:/app/Logs` | Nunca executar container de ambiente persistente sem volume durável. |
| Rollback app | Documentado em `docs/deployment-runbook.md` | Manter imagem anterior tagueada e reusar mesmo `.env`/volumes quando rollback for aprovado. |
| Equipamento Control iD | Dependência externa/física | Manter procedimento manual de contingência do cliente/operação fora do repo. |
| Observabilidade | Health, métricas, alertas JSON e monitor local | Usar `tools/observability-check.ps1` offline/online conforme ambiente. |
| Configuração operacional | `ops.example.json` versionado e `ops.local.json` ignorado pelo Git | Validar com `tools/operational-readiness-check.ps1 -RequireConfig` antes de release real. |
| Backup operacional | `tools/backup-sqlite-operational.ps1` envolve backup DPAPI, mirror opcional, restore-smoke e retenção confirmada | Definir `CONTROLID_BACKUP_MIRROR_DIRECTORY` ou `-MirrorDirectory` para cópia fora do host. |
| Contingência física | `docs/equipment-contingency-runbook.md` | Testar manual fallback em bancada e registrar dono em `ops.local.json`. |

## RTO/RPO

As metas abaixo são objetivos iniciais de planejamento, não SLA homologado. A PoC
não possui provedor produtivo, backup automático, restore periódico obrigatório ou
replicação.

| Cenário | RTO alvo inicial | RPO alvo inicial | Status |
| --- | --- | --- | --- |
| Falha de processo/container sem corrupção | Até 30 minutos após detecção | 0 se volume SQLite intacto | Necessita validação em ambiente alvo. |
| Deploy ruim com imagem anterior disponível | Até 60 minutos após decisão de rollback | 0 se schema compatível e volume intacto | Depende de imagem anterior e gate de rollback. |
| SQLite corrompido com backup válido | Até 4 horas após decisão de restore | Desde o último backup válido | Não garantido; restore real não homologado. |
| Vazamento/secret comprometido | Contenção inicial em até 30 minutos | N/A | Depende de rotação no provedor/equipamento. |
| Perda total do host sem backup externo | Indefinido | Indefinido | Lacuna crítica até haver backup fora do host. |

Lacunas para produção real:

- Definir provedor, storage, criptografia, retenção e local de backup em `ops.local.json`.
- Automatizar a chamada de `tools/backup-sqlite-operational.ps1` no host alvo e testar restore periódico.
- Definir RTO/RPO aprovados por negócio e DPO em `ops.local.json`.
- Validar `docs/equipment-contingency-runbook.md` com a operação física.
- Definir canal oficial de incidentes, on-call e calendario de revisão.

## Incidentes de segurança e LGPD

Classificação inicial:

- Suspeita de dado pessoal exposto: SEV1 até triagem.
- Secret real exposto: SEV1 até rotação.
- Acesso indevido confirmado: SEV1.
- Log com dado pessoal minimizado insuficientemente: SEV2 ou SEV1 se publicado.

Procedimento:

1. Conter acesso ao sistema, host, logs, SQLite, backups e artefatos.
2. Preservar evidências em local restrito; não colar payload bruto em tickets.
3. Identificar categorias de dados, titulares possivelmente afetados, período,
   causa provável, terceiros e ambiente.
4. Rotacionar credenciais, shared keys, sessões e secrets afetados.
5. Executar `powershell -ExecutionPolicy Bypass -File .\tools\scan-secrets.ps1`.
6. Acionar DPO/Jurídico para decidir ANPD/titulares e prazos.
7. Registrar decisão, mitigação, comunicação, risco residual e revisão preventiva.

## Comunicação interna

Use mensagens curtas e verificáveis:

- Status: investigando, contido, mitigado, recuperado ou encerrado.
- Severidade atual e criterio.
- Impacto funcional sem expor dados pessoais.
- Inicio do incidente e próxima atualização.
- Ação esperada de operadores, se houver.
- Link para ticket/canal interno restrito.

Não publicar:

- Senhas, tokens, shared keys, session strings, biometria, fotos, documentos,
  payloads completos, headers de auth ou banco/backups.
- Comunicação externa oficial sem aprovação DPO/Jurídico/controlador.

## Modelo de análise pós-incidente

```markdown
# Análise pós-incidente: <título>

Data:
Severidade:
Comandante do incidente:
Sistemas/versões afetados:
Status final:

## Resumo

<O que aconteceu, em linguagem objetiva e sem dados sensíveis.>

## Impacto

- Usuários/operadores afetados:
- Fluxos afetados:
- Dados afetados:
- Duração:
- RTO/RPO observado:

## Linha do tempo

| Horário | Evento | Evidência |
| --- | --- | --- |
| | | |

## Causa raiz

- Causa técnica:
- Causa operacional/processual:
- Fatores contribuintes:

## Detecção

- Alerta/sinal:
- Tempo até a detecção:
- O que deveria ter alertado:

## Resposta

- Contenção:
- Mitigação:
- Recuperação:
- Comunicação:

## O que funcionou

-

## O que falhou

-

## Ações corretivas

| Ação | Responsável | Prioridade | Prazo | Evidência esperada |
| --- | --- | --- | --- | --- |
| | | | | |

## Riscos residuais

-

## Decisões do DPO ou do departamento jurídico

<Preencher somente por responsável autorizado quando envolver dados pessoais.>
```

## Riscos residuais

| Risco | Severidade | Mitigação atual | Próxima ação |
| --- | --- | --- | --- |
| Sem provedor produtivo/on-call formal | Alta | `ops.example.json`, `ops.local.json` ignorado e `operational-readiness-check.ps1 -RequireConfig` bloqueando release sem donos/canais reais | Preencher e aprovar `ops.local.json` antes de uso real. |
| Backup automático fora do host ausente | Alta | `backup-sqlite-operational.ps1` com mirror opcional, restore-smoke e retenção confirmada | Agendar o script no host alvo com destino externo seguro. |
| RTO/RPO não homologados | Alta | Gate exige `rtoRpo.validationStatus` aprovado/validado em `ops.local.json` | Executar exercicio real em staging/produção com dados fictícios. |
| Dependência de equipamento físico | Alta | `docs/equipment-contingency-runbook.md` e contrato físico via gate | Testar fallback manual com operação física e fornecedor. |
| Comunicação externa LGPD depende de decisão humana | Alta | `ops.local.json` exige DPO/privacy owner, canal de escalonamento e repositório de evidências | DPO/Jurídico devem aprovar canal, prazo e template externo. |
