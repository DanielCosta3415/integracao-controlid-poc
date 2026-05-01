# ADR 0003 - Observabilidade in-process e gates de readiness locais

Status: Aceita

Data: 2026-05-01

## Contexto

A PoC precisa ser diagnosticavel sem exigir fornecedor externo de APM, metrics ou
logs. Ao mesmo tempo, releases precisam validar build, testes, secrets, readiness,
observabilidade, FinOps e contratos.

## Decisao

Publicar health checks e metricas in-process em `/health/live`, `/health/ready` e
`/metrics`, com dashboards/alertas versionados e scripts PowerShell de validacao.
O endpoint `/metrics` fica protegido por administrador por padrao.

## Alternativas consideradas

- APM externo obrigatorio: melhor em producao, mas criaria custo e configuracao
  antes da escolha de provedor.
- Apenas logs em arquivo: insuficiente para alertas e dashboards.
- Apenas testes unitarios: insuficiente para readiness operacional.

## Consequencias

- O repositorio consegue validar observabilidade sem fornecedor externo.
- Ferramentas externas podem consumir Prometheus text quando houver ambiente.
- Labels precisam permanecer allowlist para evitar cardinalidade e dados
  sensiveis.
- CPU/saturacao ainda dependem de monitoramento do host/provedor.

## Evidencias

- `Services/Observability/OperationalMetrics.cs`
- `Services/Observability/PrometheusMetricsWriter.cs`
- `Services/Observability/RuntimeCapacityMetricsProvider.cs`
- `docs/observability-runbook.md`
- `docs/observability/alert-rules.json`
- `tools/observability-check.ps1`
- `tools/test-readiness-gates.ps1`
