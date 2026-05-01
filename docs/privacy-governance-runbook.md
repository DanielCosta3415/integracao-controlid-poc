# Runbook de governanca de privacidade

Este runbook transforma as lacunas residuais de privacidade em atividades verificaveis. Ele nao substitui validacao juridica, DPO/encarregado, contrato, DPA, RIPD aprovado ou decisao formal do controlador.

## Papeis e RACI

| Atividade | Responsavel tecnico | DPO/juridico | Controlador | Operacao |
| --- | --- | --- | --- | --- |
| Definir finalidade e base legal | Apoia com evidencias | Aprova | Decide | Informa contexto |
| Atender direito do titular | Gera relatorio minimizado | Valida resposta | Autoriza resposta | Executa comunicacao |
| Eliminar/bloquear/anonimizar dados | Executa apos aprovacao | Valida restricoes | Autoriza | Confirma impacto |
| Avaliar biometria/foto | Informa fluxos e dados | Conduz RIPD | Aprova risco | Aplica controles |
| Incidente de dados | Contem e coleta evidencias | Avalia notificacao | Decide notificacao | Comunica partes |

Campos pendentes para preencher antes de uso real:

- Controlador:
- Operador(es):
- Encarregado/DPO:
- Canal do titular:
- SLA interno:
- Repositorio de evidencias:

Para release operacional, esses responsaveis e canais devem estar tambem em
`ops.local.json`, criado a partir de `ops.example.json` fora do Git, e validados
por `tools/operational-readiness-check.ps1 -RequireConfig`. O mesmo gate bloqueia
release se `privacy.legalBasisApprovalStatus`, `privacy.dpaReviewStatus` ou
`privacy.ripdStatus` permanecerem pendentes.

## Registro de bases legais

Preencha e aprove antes de usar dados reais.

| Tratamento | Base legal proposta | Evidencia | Aprovador | Data | Status |
| --- | --- | --- | --- | --- | --- |
| Usuarios e credenciais | Necessita validacao | `docs/privacy-and-data-retention.md` | DPO/juridico | Pendente | Pendente |
| Biometria e fotos | Necessita validacao especifica | RIPD requerido/recomendado | DPO/juridico | Pendente | Pendente |
| Logs de acesso | Necessita validacao | Finalidade de seguranca/auditoria | DPO/juridico | Pendente | Pendente |
| Callbacks e Push | Necessita validacao | Operacao tecnica e QA | DPO/juridico | Pendente | Pendente |
| Backups SQLite | Necessita validacao | Continuidade/recuperacao | DPO/juridico | Pendente | Pendente |

## Atendimento a direitos do titular

1. Registrar solicitacao, data, canal, escopo e identidade do solicitante.
2. Confirmar titularidade ou representacao legal por meio aprovado pelo DPO.
3. No sistema, acessar `Privacidade e LGPD` e gerar relatorio por ID, matricula, usuario, e-mail ou telefone.
4. Usar o relatorio apenas como triagem minimizada: ele mostra categorias e contagens, nao payload bruto.
5. Validar com DPO/juridico se ha base para acesso, correcao, bloqueio, eliminacao, portabilidade ou negativa.
6. Executar alteracoes apenas nas telas administrativas especificas e com confirmacao humana quando houver impacto.
7. Registrar resposta, dados compartilhados, dados preservados, motivo da preservacao e risco residual.

## Matriz de decisao por direito

| Direito | Implementacao tecnica atual | Decisao humana obrigatoria |
| --- | --- | --- |
| Confirmacao | Relatorio minimizado por titular | Confirmar identidade e escopo. |
| Acesso | Categorias e contagens; dados brutos permanecem nas telas especificas | Definir quais campos podem ser entregues. |
| Correcao | Edicao nas telas de usuarios/credenciais | Confirmar fonte oficial e impacto no equipamento. |
| Anonimizacao/bloqueio | Nao automatizado | Verificar se preserva integridade de auditoria e seguranca. |
| Eliminacao | Exclusoes por entidade e expurgos confirmados | Confirmar retencao obrigatoria e risco operacional. |
| Portabilidade | Export JSON minimizado do relatorio | Definir formato final e conteudo bruto autorizado. |
| Compartilhamento | Documentacao lista terceiros provaveis | Validar contratos/DPA/transferencia. |
| Revogacao | Nao ha consentimento modelado | So aplicavel se a base legal aprovada for consentimento. |
| Revisao automatizada | Nao ha decisao automatizada propria | Validar uso real do equipamento. |

## RIPD

RIPD e recomendado antes de qualquer uso real com biometria, fotos, monitoramento de acesso, criancas/adolescentes ou larga escala.

Checklist minimo:

- Descrever finalidade, necessidade e proporcionalidade.
- Listar categorias de titulares e dados.
- Mapear fluxos, terceiros, transferencias e retencao.
- Avaliar risco a titulares: discriminacao, exposicao, acesso indevido, erro de exclusao, fraude e vigilancia excessiva.
- Registrar controles: RBAC, HMAC, rate limit, pseudonimizacao de logs, backup protegido, expurgo, minimizacao de arquivos e scan de secrets.
- Definir risco residual aceito, dono, data de revisao e evidencias.
- Aprovar formalmente com DPO/juridico/controlador.

## DPA, terceiros e transferencia internacional

Use esta matriz antes de incluir qualquer terceiro real:

| Terceiro | Papel | Dados recebidos | Finalidade | Pais/regiao | DPA/contrato | Status |
| --- | --- | --- | --- | --- | --- | --- |
| Control iD/equipamento/firmware | Necessita classificacao | Usuarios, credenciais, fotos, biometria, eventos | Controle de acesso/integracao | Necessita validacao | Pendente | Pendente |
| GitHub/NuGet | Ferramenta de codigo/dependencias | Nao deve receber dados reais | CI/dependencias | Necessita validacao | Pendente | Pendente |

Dados reais, bancos, logs e backups nao devem ser enviados para terceiros sem decisao formal.

## Retencao e descarte

Politica minima ate aprovacao formal:

- Usar apenas dados ficticios em desenvolvimento, testes, docs e smoke.
- Manter `MonitorEvents`, `PushCommands`, logs e backups pelo menor tempo operacional necessario.
- Usar expurgo por retencao quando autorizado, evitando limpeza total sem justificativa.
- Nao apagar dado real sem registro de solicitacao, aprovacao e impacto.
- Validar restore antes de descartar backup usado como evidencia.

## Incidente de dados

1. Conter acesso a aplicacao, host, equipamento, SQLite, logs e backups.
2. Preservar evidencias em local restrito sem copiar payloads para canais inseguros.
3. Identificar categorias de dados, titulares, periodo, causa provavel e terceiros envolvidos.
4. Rotacionar sessoes, shared keys, HMAC secrets, credenciais locais e credenciais de equipamento afetadas.
5. Executar `powershell -ExecutionPolicy Bypass -File .\tools\scan-secrets.ps1`.
6. Acionar DPO/juridico para avaliar notificacao a ANPD e titulares.
7. Registrar decisao, prazo, comunicacoes, mitigacoes e risco residual.

## Evidencias tecnicas disponiveis

- Relatorio minimizado em `PrivacyController`.
- Inventario de dados em `docs/privacy-and-data-retention.md`.
- Modelo de dados em `docs/data-model-and-recovery.md`.
- Hardening em `docs/security-hardening.md`.
- Scan de secrets em `tools/scan-secrets.ps1`.
- Expurgo confirmado em `OfficialEvents/Purge` e `PushCenter/Purge`.
