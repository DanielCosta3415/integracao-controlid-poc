# FinOps, capacidade e sustentabilidade operacional

Escopo: PoC ASP.NET Core MVC/Razor com SQLite local, logs em arquivo, Docker/Compose,
metricas internas e integracao com equipamento Control iD. Este documento nao
altera provedor, plano, DNS ou dados reais; ele define controles de custo,
capacidade e desperdicio para operacao segura e reproduzivel.

## Inventario de custos

| Fonte | Estado no repositorio | Direcionador de custo | Controle atual |
| --- | --- | --- | --- |
| Hospedagem web | Dockerfile/Compose, sem provedor cloud versionado | CPU, memoria, uptime e porta exposta | Usuario nao root, healthcheck e shutdown gracioso. |
| Banco | SQLite local em volume ou arquivo | Tamanho de `integracao_controlid.db*`, WAL/SHM e I/O local | Indices operacionais, limites de listagem e backup local. |
| Storage local | `/app/data`, `/app/Logs`, `artifacts/`, `docs/reports/` | Crescimento de banco, logs, backups, reports e restore-smoke | `.gitignore`, `.dockerignore`, rotacao de logs e retencao confirmada de backups. |
| Logs | Serilog console e arquivo rolling | Volume por nivel, request logging e retencao | `retainedFileCountLimit=14` e `fileSizeLimitBytes=10000000` por padrao. |
| Observabilidade | `/metrics`, health checks e JSONs versionados | Cardinalidade de labels, memoria, storage e coleta externa futura | Labels allowlist, metricas runtime de capacidade, sem usuario/IP/payload, sem fornecedor externo. |
| Analytics | Product analytics privacy-aware em metricas internas | Numero de series por fluxo/evento/status | Eventos finitos por allowlist, sem tracking pessoal. |
| APIs externas | Equipamento Control iD na rede local | Chamadas oficiais, timeouts, retentativas humanas, falhas repetidas | Timeout, rate limit e circuit breaker por endpoint/equipamento. |
| Filas/jobs | Fila Push persistida em SQLite, sem broker externo | Volume de `PushCommands`, polling e resultados | Idempotencia local, indices e expurgo manual confirmado. |
| Backups | Scripts locais em `artifacts/backups` com espelhamento opcional | Copias `.db`, `.wal`, `.shm`, protecao DPAPI e mirror off-host | Restore-smoke, DPAPI por padrao e dry-run de retencao. |
| Build minutes | GitHub Actions e builds locais | Restore, build, testes, smoke, scanners e Docker build | Lockfiles, CI separado e gates opt-in para checks caros. |
| Ambientes preview | Nao ha provedor/manifesto dedicado | Servicos esquecidos e volumes orfaos | Governanca humana exigida antes de criar preview. |
| CDN/cache externo | Nao encontrado | Nao aplicavel | Nao introduzir sem justificativa. |
| E-mail/SMS/push externo | Nao encontrado | Nao aplicavel | Push atual e polling do equipamento, nao servico pago. |

## Riscos de custo

| Severidade | Risco | Evidencia | Mitigacao aplicada ou recomendada |
| --- | --- | --- | --- |
| Alta | SQLite crescer com callbacks, Push, fotos, biometria e payloads brutos | `MonitorEvents`, `PushCommands`, `Photos`, `BiometricTemplates` | Limites de listagem, indices, expurgo confirmado e check `sqlite-runtime-size`. |
| Alta | Backups locais/mirror sem retencao definida | `backup-sqlite-operational.ps1` permite retencao opt-in | Retencao seca por padrao; `ops.local.json` deve definir politica e owner. |
| Media | Logs ruidosos elevarem storage e custo de coleta externa | Serilog em console/arquivo e request logging | Rotacao configurada, logs seguros, alertas `FIN-002` e revisao de nivel. |
| Media | DAST/scanners/smoke elevarem minutos de build quando rodados sempre | Gates externos sao opt-in e release gate e estrito | CI mantem checks essenciais; release gate roda validacoes caras sob decisao. |
| Media | Retentativas manuais contra equipamento indisponivel gerarem ruido e tempo operacional | Chamadas oficiais com timeout/circuit breaker | Circuit breaker, timeout e alertas de timeouts. |
| Media | Ambientes preview esquecidos com volumes persistentes | Preview nao existe hoje | Exigir dono, TTL e budget antes de criar preview. |
| Baixa | `docs/reports` acumular relatorios historicos grandes | Reports versionados sao docs de auditoria | Manter apenas reports sem dados reais; revisar tamanho periodicamente. |

## Riscos de capacidade

| Recurso | Risco | Controle atual | Limite inicial sugerido |
| --- | --- | --- | --- |
| CPU | Picos em endpoints oficiais, smoke, scanners e compressao | App MVC simples, timeout e circuit breaker | 1 vCPU para PoC; revisar se P95 subir ou smoke ficar lento. |
| Memoria | Payloads grandes, fotos/base64 e exportacoes | `CallbackSecurity:MaxBodyBytes`, leitores com limite | 512 MB a 1 GB para PoC; validar com fluxos de midia. |
| SQLite | Lock de arquivo, WAL grande, I/O em volume lento | Health `/health/ready`, indices e backups | Alertar acima de 512 MB locais ou 80% do volume. |
| Storage | Logs, backups, artifacts e banco no mesmo host | Volumes separados em Compose | Orçar `/app/data` e `/app/Logs` separadamente; revisar mensalmente. |
| Conexoes | Chamadas ao equipamento e browser local | HttpClient com timeout, rate limits | Evitar loops de chamada sem delay/backoff. |
| Throughput | Callbacks/push em rajadas | Rate limit de callbacks e persistencia local | Validar com bancada antes de expor ambiente real. |
| Terceiros | Sem SLA/custo de API cloud; equipamento local e ponto unico | Runbooks de contingencia fisica | Definir suporte e fallback manual antes de producao. |

## Otimizacoes aplicadas

- `tools/finops-capacity-check.ps1` mede, sem apagar nada, tamanho de SQLite local,
  logs, artifacts e reports versionados; tambem valida runbook, alertas, dashboard,
  limites de log, limites de consulta, retencao de backup e governanca em
  `ops.example.json`.
- `/metrics` publica gauges locais de capacidade para memoria de processo, heap
  gerenciado, tamanho de SQLite/logs/artifacts/reports e espaco livre de disco,
  usando apenas labels fixas e sem paths locais.
- `tools/test-readiness-gates.ps1` passa a executar o gate `finops-capacity` por
  padrao; em `-ReleaseGate`, warnings de capacidade bloqueiam a liberacao.
- `.github/workflows/ci.yml` valida os artefatos FinOps sem exigir provedor cloud.
- `docker-compose.yml` expoe limites de arquivo Serilog por variavel de ambiente,
  preservando os defaults seguros atuais.
- `ops.example.json` inclui ownership, budget, alertas de billing, revisao de
  capacidade, retencao e regra de limpeza de preview.
- `docs/observability/alert-rules.json` e `docs/observability/dashboard.json`
  incluem sinais de custo/capacidade para uso em ferramenta externa.

## Governanca FinOps

Campos obrigatorios para release operacional real devem ser copiados de
`ops.example.json` para `ops.local.json`, fora do Git:

- `finops.costOwner`: pessoa ou time dono do custo.
- `finops.monthlyBudget`: budget ou teto aprovado para o ambiente.
- `finops.billingDashboard`: origem real de billing, ou `not-applicable` quando
  nao houver provedor pago.
- `finops.actualSpendReviewSource`: export, relatorio ou local de revisao manual
  de gasto real.
- `finops.billingAlertOwner`: responsavel por receber e agir em alertas.
- `finops.billingAlertThresholds`: marcos de alerta, por exemplo 50/80/100%.
- `finops.capacityReviewCadence`: frequencia de revisao de CPU, memoria e storage.
- `finops.logRetentionReview`: frequencia de revisao de logs e coleta externa.
- `finops.storageBudget`: limite aprovado para banco, logs, backups e artifacts.
- `finops.previewEnvironmentTtl`: TTL maximo para preview, quando existir.
- `finops.thirdPartyCostReview`: revisao de custos de scanner, CI, observabilidade
  e qualquer fornecedor futuro.

Cadencia recomendada:

- Revisao semanal durante homologacao ativa.
- Revisao mensal em ambiente estavel.
- Revisao imediata apos falha de storage, aumento de log, troca de provedor,
  criacao de preview ou ativacao de ferramenta externa.

## Alertas e limites sugeridos

| ID | Sinal | Threshold inicial | Acao |
| --- | --- | --- | --- |
| `FIN-001` | Espaco livre do host/volume | <= 20% por 15 min | Expurgar dados elegiveis com confirmacao, revisar backups e aumentar volume se justificado. |
| `FIN-002` | Volume de logs acima do budget | > 256 MB local ou crescimento anormal | Revisar nivel de log, coletor externo e eventos ruidosos. |
| `FIN-003` | SQLite acima do budget | > 512 MB local ou crescimento acelerado | Revisar retencao de callbacks, Push, fotos e biometria; executar backup antes de expurgo. |
| `FIN-004` | Timeouts/retries contra Control iD | >= 3 timeouts em 10 min por endpoint | Pausar chamadas repetidas, validar rede/equipamento e evitar desperdicio operacional. |
| `FIN-005` | Build/scanners caros fora de janela | Rodadas repetidas sem mudanca relevante | Usar checks caros em release gate ou sob demanda. |

Os thresholds sao ponto de partida para PoC. Ajuste somente com dados reais de
volume, objetivo de retencao e decisao de confiabilidade.

## Comando de validacao

Validacao padrao, sem apagar dados e sem falhar por warnings locais:

```powershell
powershell -ExecutionPolicy Bypass -File .\tools\finops-capacity-check.ps1
```

Validacao estrita para release local:

```powershell
powershell -ExecutionPolicy Bypass -File .\tools\finops-capacity-check.ps1 -FailOnWarnings
```

O relatorio gerado fica em `artifacts/finops-capacity/`, fora do Git.

## Trade-offs

- Manter SQLite local reduz custo e complexidade, mas limita concorrencia,
  escala horizontal e recuperacao automatizada.
- Logs em arquivo sao baratos para PoC, mas exigem retencao curta; coletor externo
  melhora busca e alertas, porem adiciona custo e governanca de dados.
- Scanners externos e smoke aumentam confianca, mas consomem tempo de build; por
  isso ficam opt-in e obrigatorios apenas no gate de release.
- Expurgo reduz custo e risco de dados, mas pode remover evidencias uteis; sempre
  exige confirmacao humana, backup e criterio de retencao.

## Riscos residuais

| Severidade | Risco residual | Proximo passo |
| --- | --- | --- |
| Alta | Sem provedor real ou billing real | Preencher `finops.billingDashboard` e `finops.actualSpendReviewSource` em `ops.local.json` antes de producao. |
| Alta | RTO/RPO e retencao ainda dependem de `ops.local.json` e validacao humana | Preencher `ops.local.json`, executar backup/restore-smoke e aprovar politica. |
| Media | Thresholds de storage sao estimativas iniciais de PoC | Ajustar depois de baseline de volume real. |
| Media | CPU continua dependente de monitoramento do host/provedor | Usar runtime metrics da app para memoria/storage e monitoramento externo para CPU/saturacao. |
| Baixa | Sem custo de terceiros hoje, mas scanners/observabilidade futuros podem cobrar por uso | Exigir revisao FinOps antes de ativar qualquer fornecedor externo. |
