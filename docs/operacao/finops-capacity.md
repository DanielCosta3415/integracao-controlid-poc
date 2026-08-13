# FinOps, capacidade e sustentabilidade operacional

> **Referência** · Público: FinOps, SRE e produto · Responsável: Plataforma/SRE · Última validação: 2026-08-12.

Escopo: PoC ASP.NET Core MVC/Razor com SQLite local, logs em arquivo, Docker/Compose,
métricas internas e integração com equipamento Control iD. Este documento não
altera provedor, plano, DNS ou dados reais; ele define controles de custo,
capacidade e desperdício para operação segura e reproduzível.

## Inventário de custos

| Fonte | Estado no repositório | Direcionador de custo | Controle atual |
| --- | --- | --- | --- |
| Hospedagem web | Dockerfile/Compose, sem provedor cloud versionado | CPU, memória, uptime e porta exposta | Usuário não root, healthcheck e shutdown gracioso. |
| Banco | SQLite local em volume ou arquivo | Tamanho de `integracao_controlid.db*`, WAL/SHM e I/O local | Índices operacionais, limites de listagem e backup local. |
| Storage local | `/app/data`, `/app/Logs`, `artifacts/`, `docs/historico/relatorios/` | Crescimento de banco, logs, backups, relatórios e restore-smoke | `.gitignore`, `.dockerignore`, rotação de logs e retenção confirmada de backups. |
| Logs | Serilog console e arquivo rolling | Volume por nível, request logging e retenção | `retainedFileCountLimit=14` e `fileSizeLimitBytes=10000000` por padrão. |
| Observabilidade | `/metrics`, health checks e JSONs versionados | Cardinalidade de labels, memória, storage e coleta externa futura | Labels allowlist, métricas runtime de capacidade, sem usuário/IP/payload, sem fornecedor externo. |
| Análise de produto | Métricas internas com privacidade | Número de séries por fluxo, evento e estado | Eventos finitos por lista de permissões, sem rastreamento pessoal. |
| APIs externas | Equipamento Control iD na rede local | Chamadas oficiais, timeouts, retentativas humanas, falhas repetidas | Timeout, rate limit e circuit breaker por endpoint/equipamento. |
| Filas/jobs | Fila Push persistida em SQLite, sem broker externo | Volume de `PushCommands`, polling e resultados | Idempotência local, índices e expurgo manual confirmado. |
| Cópias de segurança | Scripts locais em `artifacts/backups` com espelhamento opcional | Cópias `.db`, `.wal`, `.shm`, proteção DPAPI e espelhamento fora do host | Teste smoke de restauração, DPAPI por padrão e simulação de retenção. |
| Build minutes | GitHub Actions e builds locais | Restore, build, testes, smoke, scanners e Docker build | Lockfiles, CI separado e gates opt-in para checks caros. |
| Ambientes preview | Não há provedor/manifesto dedicado | Serviços esquecidos e volumes orfaos | Governança humana exigida antes de criar preview. |
| CDN/cache externo | Não encontrado | Não aplicável | Não introduzir sem justificativa. |
| E-mail/SMS/push externo | Não encontrado | Não aplicável | Push atual e polling do equipamento, não serviço pago. |

## Riscos de custo

| Severidade | Risco | Evidência | Mitigação aplicada ou recomendada |
| --- | --- | --- | --- |
| Alta | SQLite crescer com callbacks, Push, fotos, biometria e payloads brutos | `MonitorEvents`, `PushCommands`, `Photos`, `BiometricTemplates` | Limites de listagem, índices, expurgo confirmado e check `sqlite-runtime-size`. |
| Alta | Cópias locais/espelhadas sem retenção definida | `backup-sqlite-operational.ps1` permite retenção opcional | Retenção simulada por padrão; `ops.local.json` deve definir política e responsável. |
| Média | Logs ruidosos elevarem o armazenamento e o custo de coleta externa | Serilog em console e arquivo, com registro de requisições | Rotação configurada, logs seguros, alertas `FIN-002` e revisão de nível. |
| Média | DAST, scanners e smoke elevarem os minutos de compilação quando executados sempre | Gates externos são opcionais, e o gate de release é estrito | A CI mantém verificações essenciais; o gate de release executa validações caras mediante decisão. |
| Média | Novas tentativas manuais contra equipamento indisponível gerarem ruído e tempo operacional | Chamadas oficiais com tempo limite e circuit breaker | Circuit breaker, tempo limite e alertas de expiração. |
| Média | Ambientes de pré-visualização esquecidos com volumes persistentes | Não há ambiente de pré-visualização atualmente | Exigir responsável, TTL e orçamento antes de criar um ambiente de pré-visualização. |
| Baixa | `docs/historico/relatorios/` acumular evidências grandes | Relatórios versionados apoiam auditoria | Manter apenas evidências sanitizadas; revisar tamanho periodicamente. |

## Riscos de capacidade

| Recurso | Risco | Controle atual | Limite inicial sugerido |
| --- | --- | --- | --- |
| CPU | Picos em endpoints oficiais, smoke, scanners e compressão | App MVC simples, timeout e circuit breaker | 1 vCPU para PoC; revisar se P95 subir ou smoke ficar lento. |
| Memória | Payloads grandes, fotos e exportações | `CallbackSecurity:MaxBodyBytes`, leitores com limite, bytes binários sem Base64 intermediário e upload de vídeo fragmentado com buffer reutilizável | 512 MB a 1 GB para PoC; validar com fluxos de mídia. |
| SQLite | Lock de arquivo, WAL grande, I/O em volume lento | Health `/health/ready`, índices e backups | Alertar acima de 512 MB locais ou 80% do volume. |
| Storage | Logs, backups, artifacts e banco no mesmo host | Volumes separados em Compose | Orçar `/app/data` e `/app/Logs` separadamente; revisar mensalmente. |
| Conexoes | Chamadas ao equipamento e browser local | HttpClient com timeout, rate limits | Evitar loops de chamada sem delay/backoff. |
| Throughput | Callbacks/push em rajadas | Rate limit de callbacks e persistência local | Validar com bancada antes de expor ambiente real. |
| Terceiros | Sem SLA/custo de API cloud; equipamento local e ponto único | Runbooks de contingência física | Definir suporte e fallback manual antes de produção. |

## Otimizações aplicadas

- `tools/finops-capacity-check.ps1` mede, sem apagar nada, tamanho de SQLite local,
  registros, artefatos e relatórios versionados; também valida guia operacional, alertas, painel,
  limites de log, limites de consulta, retenção de backup e governança em
  `ops.example.json`.
- `/metrics` publica gauges locais de capacidade para memória de processo, heap
  gerenciado, tamanho de SQLite/logs/artifacts/reports e espaço livre de disco,
  usando apenas labels fixas e sem paths locais. O snapshot que consulta o
  sistema de arquivos é atualizado em segundo plano a cada 30 segundos por
  padrão; a requisição de `/metrics` apenas serializa o estado em memória.
- Downloads binários não passam por Base64 e o envio de vídeo reutiliza um
  buffer limitado por bloco, reduzindo alocações proporcionais ao arquivo.
- Listagens oficiais usam páginas de 100 itens com lookahead; galerias consultam
  o inventário uma vez e carregam cada miniatura somente quando necessária.
- `tools/test-readiness-gates.ps1` passa a executar o gate `finops-capacity` por
  padrão; em `-ReleaseGate`, warnings de capacidade bloqueiam a liberação.
- `.github/workflows/ci.yml` valida os artefatos FinOps sem exigir provedor cloud.
- `docker-compose.yml` expõe limites de arquivo Serilog por variável de ambiente,
  preservando os defaults seguros atuais.
- `ops.example.json` inclui ownership, budget, alertas de billing, revisão de
  capacidade, retenção e regra de limpeza de preview.
- `docs/observability/alert-rules.json` e `docs/observability/dashboard.json`
  incluem sinais de custo/capacidade para uso em ferramenta externa.

## Governança FinOps

Campos obrigatórios para release operacional real devem ser copiados de
`ops.example.json` para `ops.local.json`, fora do Git:

- `finops.costOwner`: pessoa ou time dono do custo.
- `finops.monthlyBudget`: budget ou teto aprovado para o ambiente.
- `finops.billingDashboard`: origem real de billing, ou `not-applicable` quando
  não houver provedor pago.
- `finops.actualSpendReviewSource`: export, relatório ou local de revisão manual
  de gasto real.
- `finops.billingAlertOwner`: responsável por receber e agir em alertas.
- `finops.billingAlertThresholds`: marcos de alerta, por exemplo 50/80/100%.
- `finops.capacityReviewCadence`: frequência de revisão de CPU, memória e storage.
- `finops.logRetentionReview`: frequência de revisão de logs e coleta externa.
- `finops.storageBudget`: limite aprovado para banco, logs, backups e artifacts.
- `finops.previewEnvironmentTtl`: TTL máximo para preview, quando existir.
- `finops.thirdPartyCostReview`: revisão de custos de scanner, CI, observabilidade
  e qualquer fornecedor futuro.

Cadência recomendada:

- Revisão semanal durante homologação ativa.
- Revisão mensal em ambiente estável.
- Revisão imediata após falha de storage, aumento de log, troca de provedor,
  criação de preview ou ativação de ferramenta externa.

## Alertas e limites sugeridos

| ID | Sinal | Threshold inicial | Ação |
| --- | --- | --- | --- |
| `FIN-001` | Espaço livre do host/volume | <= 20% por 15 min | Expurgar dados elegíveis com confirmação, revisar backups e aumentar volume se justificado. |
| `FIN-002` | Volume de logs acima do budget | > 256 MB local ou crescimento anormal | Revisar nível de log, coletor externo e eventos ruidosos. |
| `FIN-003` | SQLite acima do budget | > 512 MB local ou crescimento acelerado | Revisar retenção de callbacks, Push, fotos e biometria; executar backup antes de expurgo. |
| `FIN-004` | Timeouts/retries contra Control iD | >= 3 timeouts em 10 min por endpoint | Pausar chamadas repetidas, validar rede/equipamento e evitar desperdicio operacional. |
| `FIN-005` | Build/scanners caros fora de janela | Rodadas repetidas sem mudança relevante | Usar checks caros em release gate ou sob demanda. |

Os thresholds são ponto de partida para PoC. Ajuste somente com dados reais de
volume, objetivo de retenção e decisão de confiabilidade.

## Comando de validação

Validação padrão, sem apagar dados e sem falhar por warnings locais:

```powershell
powershell -ExecutionPolicy Bypass -File .\tools\finops-capacity-check.ps1
```

Validação estrita para release local:

```powershell
powershell -ExecutionPolicy Bypass -File .\tools\finops-capacity-check.ps1 -FailOnWarnings
```

O relatório gerado fica em `artifacts/finops-capacity/`, fora do Git.

## Contrapartidas

- Manter SQLite local reduz custo e complexidade, mas limita concorrência,
  escala horizontal e recuperação automatizada.
- Logs em arquivo são baratos para PoC, mas exigem retenção curta; coletor externo
  melhora busca e alertas, porém adiciona custo e governança de dados.
- Scanners externos e smoke aumentam confiança, mas consomem tempo de build; por
  isso ficam opt-in e obrigatórios apenas no gate de release.
- Expurgo reduz custo e risco de dados, mas pode remover evidências úteis; sempre
  exige confirmação humana, backup e critério de retenção.

## Riscos residuais

| Severidade | Risco residual | Próximo passo |
| --- | --- | --- |
| Alta | Sem provedor real ou billing real | Preencher `deployment.*` e `finops.*` em `ops.local.json`; `operational-readiness-check.ps1 -RequireConfig` bloqueia placeholders/status pendente. |
| Alta | RTO/RPO e retenção ainda dependem de `ops.local.json` e validação humana | Preencher `rtoRpo.*`, executar backup/restore-smoke e aprovar política; ver [docs/operacao/residual-risk-closure.md](residual-risk-closure.md). |
| Média | Limites de armazenamento são estimativas iniciais da PoC | Ajustar depois de obter uma linha de base do volume real. |
| Média | A CPU continua dependente do monitoramento do host ou provedor | Usar métricas de execução da aplicação para memória e armazenamento, além de monitoramento externo para CPU e saturação. |
| Baixa | Sem custo de terceiros hoje, mas scanners/observabilidade futuros podem cobrar por uso | Exigir revisão FinOps antes de ativar qualquer fornecedor externo. |

## Modelo de dimensionamento

Sem carga real, use cenários e registre as premissas; não apresente estimativa como
medição.

| Cenário | Instância | Persistência | Retenção | Uso esperado |
| --- | --- | --- | --- | --- |
| Desenvolvimento | Um processo | SQLite local descartável | Curta | Uma pessoa e stub |
| Bancada compartilhada | Um processo | Volume persistente e backup externo | Conforme política aprovada | Poucos operadores e um equipamento por vez |
| Produção candidata | Não definido | Depende de RTO/RPO e volume | Aprovada por DPO/SRE | Exige teste de carga e provedor escolhido |

Fórmulas mínimas:

- armazenamento mensal = crescimento diário medido × dias de retenção × margem;
- custo por fluxo = custo mensal atribuível / fluxos concluídos válidos;
- margem livre = `(limite - pico observado) / limite`;
- chamadas externas = throughput × tentativas, incluindo somente retries seguros.

Revisão mensal deve registrar gasto real, previsão, variação, crescimento de
SQLite, logs e backups, picos de memória, chamadas por fornecedor e decisão de
dimensionamento. Alertas de custo nunca substituem limites técnicos de segurança.

## Registro da linha de base

| Medida | Unidade | Fonte | Valor atual |
| --- | --- | --- | --- |
| CPU e memória de pico | percentual/MiB | host ou contêiner | Não medido em provedor real |
| Crescimento do SQLite | MiB/dia | arquivo do banco | Medir no ambiente candidato |
| Logs e backups | MiB/dia | diretórios operacionais | Medir com retenção aprovada |
| Chamadas Control iD | requisições/minuto | métricas internas | Medir por fluxo homologado |
| Custo total | moeda/mês | faturamento do provedor | Indisponível sem provedor |

Substitua “não medido” somente com janela, ambiente e fonte registrados. Uma
estimativa sem data ou carga não deve ser usada para aprovar capacidade.

## Navegação documental

- [Voltar ao índice deste domínio](README.md).
- [Abrir a central de documentação](../README.md).
