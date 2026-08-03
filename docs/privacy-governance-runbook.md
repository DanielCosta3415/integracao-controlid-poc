# Guia operacional de governança de privacidade

Este runbook transforma as lacunas residuais de privacidade em atividades verificáveis. Ele não substitui validação jurídica, DPO/encarregado, contrato, DPA, RIPD aprovado ou decisão formal do controlador.

## Papéis e RACI

| Atividade | Responsável técnico | DPO/jurídico | Controlador | Operação |
| --- | --- | --- | --- | --- |
| Definir finalidade e base legal | Apoia com evidências | Aprova | Decide | Informa contexto |
| Atender direito do titular | Gera relatório minimizado | Valida resposta | Autoriza resposta | Executa a comunicação |
| Eliminar, bloquear ou anonimizar dados | Executa após aprovação | Valida restrições | Autoriza | Confirma o impacto |
| Avaliar biometria/foto | Informa fluxos e dados | Conduz RIPD | Aprova risco | Aplica controles |
| Incidente de dados | Contém e coleta evidências | Avalia notificação | Decide notificação | Comunica partes |

Campos pendentes para preencher antes de uso real:

- Controlador:
- Operador(es):
- Encarregado/DPO:
- Canal do titular:
- SLA interno:
- Repositório de evidências:

Para release operacional, esses responsáveis e canais devem estar também em
`ops.local.json`, criado a partir de `ops.example.json` fora do Git, e validados
por `tools/operational-readiness-check.ps1 -RequireConfig`. O mesmo gate bloqueia
release se `privacy.legalBasisApprovalStatus`, `privacy.dpaReviewStatus` ou
`privacy.ripdStatus` permanecerem pendentes.

## Registro de bases legais

Preencha e aprove antes de usar dados reais.

| Tratamento | Base legal proposta | Evidência | Aprovador | Data | Status |
| --- | --- | --- | --- | --- | --- |
| Usuários e credenciais | Necessita validação | `docs/privacy-and-data-retention.md` | DPO/jurídico | Pendente | Pendente |
| Biometria e fotos | Necessita validação específica | RIPD requerido/recomendado | DPO/jurídico | Pendente | Pendente |
| Logs de acesso | Necessita validação | Finalidade de segurança/auditoria | DPO/jurídico | Pendente | Pendente |
| Callbacks e Push | Necessita validação | Operação técnica e QA | DPO/jurídico | Pendente | Pendente |
| Backups SQLite | Necessita validação | Continuidade/recuperação | DPO/jurídico | Pendente | Pendente |

## Atendimento a direitos do titular

1. Registrar solicitação, data, canal, escopo e identidade do solicitante.
2. Confirmar titularidade ou representação legal por meio aprovado pelo DPO.
3. No sistema, acessar `Privacidade e LGPD` e gerar relatório por ID, matricula, usuário, e-mail ou telefone.
4. Usar o relatório apenas como triagem minimizada: ele mostra categorias e contagens, não payload bruto.
5. Validar com DPO/jurídico se há base para acesso, correção, bloqueio, eliminação, portabilidade ou negativa.
6. Executar alterações apenas nas telas administrativas específicas e com confirmação humana quando houver impacto.
7. Registrar resposta, dados compartilhados, dados preservados, motivo da preservação e risco residual.

## Matriz de decisão por direito

| Direito | Implementação técnica atual | Decisão humana obrigatória |
| --- | --- | --- |
| Confirmação | Relatório minimizado por titular | Confirmar identidade e escopo. |
| Acesso | Categorias e contagens; dados brutos permanecem nas telas específicas | Definir quais campos podem ser entregues. |
| Correção | Edição nas telas de usuários/credenciais | Confirmar fonte oficial e impacto no equipamento. |
| Anonimização/bloqueio | Não automatizado | Verificar se preserva integridade de auditoria e segurança. |
| Eliminação | Exclusoes por entidade e expurgos confirmados | Confirmar retenção obrigatória e risco operacional. |
| Portabilidade | Export JSON minimizado do relatório | Definir formato final e conteúdo bruto autorizado. |
| Compartilhamento | Documentação lista terceiros prováveis | Validar contratos/DPA/transferência. |
| Revogação | Não há consentimento modelado | Só aplicável se a base legal aprovada for consentimento. |
| Revisão automatizada | Não há decisão automatizada própria | Validar uso real do equipamento. |

## RIPD

O RIPD é recomendado antes de qualquer uso real com biometria, fotos, monitoramento de acesso, crianças ou adolescentes, ou tratamento em larga escala.

Checklist mínimo:

- Descrever finalidade, necessidade e proporcionalidade.
- Listar categorias de titulares e dados.
- Mapear fluxos, terceiros, transferências e retenção.
- Avaliar risco a titulares: discriminação, exposição, acesso indevido, erro de exclusão, fraude e vigilancia excessiva.
- Registrar controles: RBAC, HMAC, rate limit, pseudonimização de logs, backup protegido, expurgo, minimização de arquivos e scan de secrets.
- Definir risco residual aceito, dono, data de revisão e evidências.
- Aprovar formalmente com DPO/jurídico/controlador.

## DPA, terceiros e transferência internacional

Use esta matriz antes de incluir qualquer terceiro real:

| Terceiro | Papel | Dados recebidos | Finalidade | Pais/regiao | DPA/contrato | Status |
| --- | --- | --- | --- | --- | --- | --- |
| Control iD/equipamento/firmware | Necessita classificação | Usuários, credenciais, fotos, biometria, eventos | Controle de acesso/integração | Necessita validação | Pendente | Pendente |
| GitHub/NuGet | Ferramenta de código/dependências | Não deve receber dados reais | CI/dependências | Necessita validação | Pendente | Pendente |

Dados reais, bancos, logs e backups não devem ser enviados para terceiros sem decisão formal.

## Retenção e descarte

Política mínima até aprovação formal:

- Usar apenas dados fictícios em desenvolvimento, testes, docs e smoke.
- Manter `MonitorEvents`, `PushCommands`, logs e backups pelo menor tempo operacional necessário.
- Usar expurgo por retenção quando autorizado, evitando limpeza total sem justificativa.
- Não apagar dado real sem registro de solicitação, aprovação e impacto.
- Validar restore antes de descartar backup usado como evidência.

## Incidente de dados

1. Conter acesso a aplicação, host, equipamento, SQLite, logs e backups.
2. Preservar evidências em local restrito sem copiar payloads para canais inseguros.
3. Identificar categorias de dados, titulares, período, causa provável e terceiros envolvidos.
4. Rotacionar sessões, shared keys, HMAC secrets, credenciais locais e credenciais de equipamento afetadas.
5. Executar `powershell -ExecutionPolicy Bypass -File .\tools\scan-secrets.ps1`.
6. Acionar DPO/jurídico para avaliar notificação a ANPD e titulares.
7. Registrar decisão, prazo, comunicações, mitigações e risco residual.

## Evidências técnicas disponíveis

- Relatório minimizado em `PrivacyController`.
- Inventário de dados em `docs/privacy-and-data-retention.md`.
- Modelo de dados em `docs/data-model-and-recovery.md`.
- Hardening em `docs/security-hardening.md`.
- Scan de secrets em `tools/scan-secrets.ps1`.
- Expurgo confirmado em `OfficialEvents/Purge` e `PushCenter/Purge`.
