# ADR 0003 - Observabilidade no processo e critérios locais de prontidão

> **Decisão** · Público: arquitetura e operação · Responsável: Engenharia · Última validação: 2026-08-12.

Estado: aceita

- Data da decisão: 2026-05-01
- Substitui: nenhuma decisão
- Substituída por: nenhuma decisão

## Direcionadores

- diagnóstico local sem dependência obrigatória de fornecedor;
- sinais com baixa cardinalidade e sem dados pessoais;
- critérios reproduzíveis em desenvolvimento e CI;
- adoção futura de coletor externo sem acoplar regras de negócio.

## Contexto

A PoC precisa ser diagnosticável sem exigir fornecedor externo de APM, métricas ou
registros. Ao mesmo tempo, liberações precisam validar compilação, testes, segredos, prontidão,
observabilidade, FinOps e contratos.

## Decisão

Publicar verificações de saúde e métricas no processo em `/health/live`, `/health/ready` e
`/metrics`, com painéis/alertas versionados e scripts PowerShell de validação.
O endpoint `/metrics` fica protegido por administrador por padrão.

## Alternativas consideradas

- APM externo obrigatório: melhor em produção, mas criaria custo e configuração
  antes da escolha de provedor.
- Apenas registros em arquivo: insuficiente para alertas e painéis.
- Apenas testes unitários: insuficiente para prontidão operacional.

## Consequências

- O repositório consegue validar observabilidade sem fornecedor externo.
- Ferramentas externas podem consumir texto Prometheus quando houver ambiente.
- Rótulos precisam permanecer em lista de permissões para evitar cardinalidade e dados
  sensíveis.
- CPU/saturação ainda dependem de monitoramento do host/provedor.

## Evidências

- `Services/Observability/OperationalMetrics.cs`
- `Services/Observability/PrometheusMetricsWriter.cs`
- `Services/Observability/RuntimeCapacityMetricsProvider.cs`
- [docs/operacao/observability-runbook.md](../operacao/observability-runbook.md)
- `docs/observability/alert-rules.json`
- `tools/observability-check.ps1`
- `tools/test-readiness-gates.ps1`
- `tests/Integracao.ControlID.PoC.Tests/Services/Observability/ObservabilityEndpointContractTests.cs`
- `tests/Integracao.ControlID.PoC.Tests/Services/Observability/OperationalMetricsTests.cs`

## Critério de revisão

Reavalie ao escolher APM, coletor OpenTelemetry ou provedor de métricas. A troca
do backend não deve remover verificações de saúde, correlação nem o contrato local de
prontidão.

## Evolução da decisão

- Substitui: nenhuma decisão anterior.
- Substituída por: nenhuma até esta validação.
- Gatilhos objetivos: múltiplas instâncias, coleta distribuída ou provedor APM
  escolhido.
- Evidência para mudança: propagação de correlação, custo, retenção, proteção de
  dados, exportação indisponível sem bloquear a aplicação e equivalência das
  verificações de saúde atuais.

## Navegação documental

- [Voltar ao índice deste domínio](README.md).
- [Abrir a central de documentação](../README.md).
