# ADR 0003 - Observabilidade no processo e gates locais de prontidão

Status: Aceita

Data: 2026-05-01

## Contexto

A PoC precisa ser diagnosticável sem exigir fornecedor externo de APM, metrics ou
logs. Ao mesmo tempo, releases precisam validar build, testes, secrets, readiness,
observabilidade, FinOps e contratos.

## Decisão

Publicar health checks e métricas in-process em `/health/live`, `/health/ready` e
`/metrics`, com dashboards/alertas versionados e scripts PowerShell de validação.
O endpoint `/metrics` fica protegido por administrador por padrão.

## Alternativas consideradas

- APM externo obrigatório: melhor em produção, mas criaria custo e configuração
  antes da escolha de provedor.
- Apenas logs em arquivo: insuficiente para alertas e dashboards.
- Apenas testes unitários: insuficiente para readiness operacional.

## Consequências

- O repositório consegue validar observabilidade sem fornecedor externo.
- Ferramentas externas podem consumir Prometheus text quando houver ambiente.
- Labels precisam permanecer allowlist para evitar cardinalidade e dados
  sensíveis.
- CPU/saturação ainda dependem de monitoramento do host/provedor.

## Evidências

- `Services/Observability/OperationalMetrics.cs`
- `Services/Observability/PrometheusMetricsWriter.cs`
- `Services/Observability/RuntimeCapacityMetricsProvider.cs`
- `docs/observability-runbook.md`
- `docs/observability/alert-rules.json`
- `tools/observability-check.ps1`
- `tools/test-readiness-gates.ps1`
