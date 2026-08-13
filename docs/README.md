# Central de documentação

> **Guia** · Público: todos os papéis · Responsável: Engenharia · Última validação: 2026-08-12.

Este é o ponto de entrada canônico para o conhecimento da PoC. Os documentos
estão organizados por objetivo e por ciclo de vida: conteúdo vigente, decisões
arquiteturais e evidências históricas.

## Comece pelo seu objetivo

| Objetivo | Percurso recomendado | Evidência de conclusão |
| --- | --- | --- |
| Entender a PoC | [Visão geral](../README.md) → [FAQ](primeiros-passos/faq.md) → [percurso por perfil](primeiros-passos/persona-guides.md) | Escopo, limites e próximo passo identificados |
| Executar sem equipamento | [Onboarding](primeiros-passos/developer-onboarding.md) → [validação sem aparelho](primeiros-passos/validation-without-device.md) → [cenários do simulador](primeiros-passos/stub-scenarios.md) | Aplicação, simulador e smoke aprovados |
| Evoluir código | [Arquitetura](arquitetura/architecture-overview.md) → [contratos](integracao-controlid/integration-contracts.md) → [testes](qualidade/testing-strategy.md) | Contrato e testes afetados identificados |
| Integrar equipamento | [Compatibilidade](integracao-controlid/device-compatibility-matrix.md) → [rede](integracao-controlid/network-topologies.md) → [validação de endpoints](integracao-controlid/endpoint-validation-matrix.md) | Modelo, firmware, licença e topologia registrados |
| Operar ou diagnosticar | [Diagnóstico](operacao/troubleshooting-controlid.md) → [observabilidade](operacao/observability-runbook.md) → [incidentes](operacao/incident-response-and-dr.md) | Sintoma classificado e evidência segura coletada |
| Preparar liberação | [CI/CD](qualidade/ci-cd-quality-gates.md) → [implantação](operacao/deployment-runbook.md) → [riscos residuais](operacao/residual-risk-closure.md) | Gate estrito e aprovações humanas concluídos |

## Domínios

| Domínio | Conteúdo |
| --- | --- |
| [Primeiros passos](primeiros-passos/README.md) | FAQ, onboarding, personas, simulador e limites sem hardware |
| [Produto](produto/README.md) | Requisitos, critérios de aceite, analytics e identidade visual |
| [Arquitetura](arquitetura/README.md) | Camadas, fluxos, módulos e direção de dependências |
| [Decisões arquiteturais](adrs/README.md) | ADRs aceitos, substituições e consequências |
| [Integração Control iD](integracao-controlid/README.md) | Contratos, rede, compatibilidade, Monitor, Push e modos |
| [Dados](dados/README.md) | SQLite, modelo, migrações, retenção, backup e recuperação |
| [Segurança e privacidade](seguranca-privacidade/README.md) | Contas, hardening, LGPD e cadeia de suprimentos |
| [Qualidade](qualidade/README.md) | Testes, desempenho, validação externa e CI/CD |
| [Operação](operacao/README.md) | Deploy, observabilidade, incidentes, contingência e FinOps |
| [Histórico](historico/README.md) | Changelogs, auditorias e relatórios datados |

## Fontes canônicas

| Assunto | Fonte vigente | Complementos |
| --- | --- | --- |
| Escopo e primeiro uso | [README raiz](../README.md) | [FAQ](primeiros-passos/faq.md) |
| Setup e comandos | [Onboarding](primeiros-passos/developer-onboarding.md) | [AGENTS.md](../AGENTS.md) |
| Arquitetura | [Visão de arquitetura](arquitetura/architecture-overview.md) | [ADRs](adrs/README.md) |
| Access API | [Contratos de integração](integracao-controlid/integration-contracts.md) | [Catálogo de erros](integracao-controlid/api-error-catalog.md) |
| Compatibilidade física | [Matriz de compatibilidade](integracao-controlid/device-compatibility-matrix.md) | [Matriz de endpoints](integracao-controlid/endpoint-validation-matrix.md) |
| Dados e recuperação | [Modelo e recuperação](dados/data-model-and-recovery.md) | [Estado de execução](dados/database-and-runtime-state.md) |
| Segurança | [Hardening](seguranca-privacidade/security-hardening.md) | [Privacidade](seguranca-privacidade/privacy-and-data-retention.md) |
| Testes e release | [Estratégia de testes](qualidade/testing-strategy.md) | [CI/CD](qualidade/ci-cd-quality-gates.md) |
| Produção e incidentes | [Implantação](operacao/deployment-runbook.md) | [Resposta a incidentes](operacao/incident-response-and-dr.md) |

Em caso de conflito, prevalecem: contratos executáveis e testes, código atual,
ADR aceita, documento vigente e, por último, evidência histórica.

## Ciclo de vida

- **Guia:** orienta uma tarefa ou percurso.
- **Referência:** descreve contrato, estado ou modelo vigente.
- **Runbook:** prescreve diagnóstico e resposta operacional.
- **Decisão:** registra contexto e consequências de uma escolha arquitetural.
- **Política:** define regra permanente de governança.
- **Registro histórico:** preserva evidência datada e não representa o estado atual.

Documentos vigentes devem indicar público, responsável e última validação.
Registros históricos devem indicar data ou execução original e permanecer
imutáveis, salvo correção factual explicitamente registrada.

## Responsabilidade e cadência

| Tipo | Revisão mínima | Evento que antecipa revisão |
| --- | --- | --- |
| Guia | Trimestral | Mudança no percurso, comando ou público |
| Referência | Trimestral | Mudança no código, contrato, configuração ou modelo |
| Runbook | Trimestral | Incidente, exercício, alerta ou mudança de ambiente |
| Política | Semestral | Mudança de governança, segurança ou responsabilidade |
| Decisão | Imutável | Nova decisão substitui a anterior por outro ADR |
| Registro histórico | Sem revisão periódica | Apenas correção factual com adendo explícito |

Ownership normalizado: **Produto**, **Engenharia**, **QA**,
**Plataforma/SRE** e **Segurança/Privacidade**. O responsável pelo domínio deve
revisar também links, exemplos e riscos antes de atualizar a data de validação.

## Manutenção

1. Atualize o documento canônico quando o comportamento correspondente mudar.
2. Atualize o índice do domínio ao criar, mover ou remover documentos.
3. Use links Markdown relativos; caminhos em crases são reservados a arquivos
   de código, comandos e valores literais.
4. Não replique procedimentos extensos: vincule a fonte canônica.
5. Registre decisões estruturais em `docs/adrs/`.
6. Preserve relatórios datados em `docs/historico/`.
7. Execute:

```powershell
powershell -ExecutionPolicy Bypass -File .\tools\validate-documentation.ps1
git diff --check
```

Use `-CheckExternalUrls` em auditorias conectadas. A CI valida o acervo sem
depender da disponibilidade de terceiros.

## Governança do repositório

- [Contribuição](../CONTRIBUTING.md)
- [Segurança](../SECURITY.md)
- [Suporte](../SUPPORT.md)
- [Regras para agentes](../AGENTS.md)
