# Catálogo de diagramas

> **Referência** · Público: todos os papéis · Responsável: Engenharia · Última validação: 2026-08-12.

Este catálogo localiza as representações visuais vigentes da PoC, esclarece sua
notação e define como mantê-las coerentes com o código. O diagrama permanece no
documento do domínio que explica seu comportamento; esta página é o índice
canônico, não uma cópia dos desenhos.

## Notação adotada

- Mermaid é o formato versionado porque o GitHub o renderiza junto do Markdown.
- `sequenceDiagram`, `stateDiagram-v2` e `classDiagram` representam UML de
  sequência, estados e classes.
- `erDiagram` representa o modelo entidade-relacionamento, não UML.
- `flowchart` representa arquitetura, topologia, atividade, casos de uso ou fluxo
  operacional em notação visual adaptada. O título informa a finalidade para não
  apresentar um fluxograma como UML formal.
- Setas tracejadas indicam dependência ou consulta; setas contínuas indicam fluxo,
  chamada ou alteração conforme a legenda do documento.
- Associações do modelo de dados são lógicas, salvo indicação explícita de chave
  estrangeira no schema.

## Inventário vigente

Linha de base de 2026-08-12: 41 blocos Mermaid em 20 documentos, compostos por
nove sequências UML, quatro máquinas de estados UML, sete diagramas de classes
UML, três entidade-relacionamento e 18 fluxogramas. Os 34 IDs abaixo agregam
desenhos do mesmo escopo quando isso melhora a navegação.

| ID | Visão | Notação | Fonte canônica |
| --- | --- | --- | --- |
| VIS-001 | Arquitetura resumida para primeiro contato | Fluxograma | [README raiz](../../README.md#arquitetura-resumida) |
| VIS-002 | Componentes e fronteiras do monólito | Componentes em Mermaid | [Visão de arquitetura](architecture-overview.md#estilo-arquitetural) |
| VIS-003 | Autenticação local e no equipamento | Sequência UML | [Visão de arquitetura](architecture-overview.md#sequência-de-autenticação-e-integração) |
| VIS-004 | Entrada assíncrona de callbacks e Push | Sequência UML | [Visão de arquitetura](architecture-overview.md#sequência-de-entrada-assíncrona) |
| VIS-005 | Implantação de referência | Implantação em Mermaid | [Visão de arquitetura](architecture-overview.md#visão-de-implantação) |
| VIS-006 | Casos de uso e atores | Casos de uso em Mermaid | [Critérios de aceite](../produto/product-acceptance-criteria.md#atores-e-casos-de-uso) |
| VIS-007 | Operação administrativa de alto impacto | Atividade em Mermaid | [Critérios de aceite](../produto/product-acceptance-criteria.md#atividade-de-operação-de-alto-impacto) |
| VIS-008 | Navegação funcional da interface | Fluxograma | [Mapa de módulos](project-file-responsibilities.md#mapa-de-navegação-funcional) |
| VIS-009 | Pipeline de saída da Access API | Fluxograma | [Guia do cliente](../../Services/ControlIDApi/README.md#fluxo-de-uma-chamada) |
| VIS-010 | Classes do pipeline de saída | Classes UML | [Guia do cliente](../../Services/ControlIDApi/README.md#relações-entre-as-classes-principais) |
| VIS-011 | Estados do circuit breaker | Estados UML | [Guia do cliente](../../Services/ControlIDApi/README.md#estados-do-circuit-breaker) |
| VIS-012 | Classes dos ingressos externos | Classes UML | [Contratos](../integracao-controlid/integration-contracts.md#classes-dos-ingressos-externos) |
| VIS-013 | Invocação oficial com resiliência | Sequência UML | [Contratos](../integracao-controlid/integration-contracts.md#sequências-de-referência) |
| VIS-014 | Autenticação e persistência de ingresso | Sequência UML | [Contratos](../integracao-controlid/integration-contracts.md#sequências-de-referência) |
| VIS-015 | Classes do fluxo Push | Classes UML | [Push](../integracao-controlid/push-implementation.md#relações-entre-as-classes-do-push) |
| VIS-016 | Ciclo completo de um comando Push | Sequência UML | [Push](../integracao-controlid/push-implementation.md#sequência-ponta-a-ponta) |
| VIS-017 | Estados de um comando Push | Estados UML | [Push](../integracao-controlid/push-implementation.md#máquina-de-estados) |
| VIS-018 | Monitor ponta a ponta | Sequência UML | [Monitor](../integracao-controlid/monitor-implementation.md#fluxo-ponta-a-ponta) |
| VIS-019 | Modos Standalone, Pro e Enterprise | Estados UML | [Modos](../integracao-controlid/operation-modes-implementation.md#estados-e-transições) |
| VIS-020 | Aplicação e releitura de modo | Sequência UML | [Modos](../integracao-controlid/operation-modes-implementation.md#sequência-de-alteração-e-releitura) |
| VIS-021 | Classes da identidade local e sessão remota | Classes UML | [Contas locais](../seguranca-privacidade/local-account-administration.md#relações-entre-identidade-e-sessões) |
| VIS-022 | Ciclo das duas autenticações | Estados UML | [Contas locais](../seguranca-privacidade/local-account-administration.md#ciclo-de-vida-das-autenticações) |
| VIS-023 | Modelo local completo | Entidade-relacionamento | [Modelo de dados](../dados/data-model-and-recovery.md#diagrama-lógico-e-físico) |
| VIS-024 | Backup, restauração e reversão | Atividade em Mermaid | [Modelo de dados](../dados/data-model-and-recovery.md#atividade-de-backup-restauração-e-reversão) |
| VIS-025 | Inicialização do SQLite | Fluxograma | [Estado de execução](../dados/database-and-runtime-state.md#primeiro-início-e-arquivos-gerados) |
| VIS-026 | Três topologias de rede | Fluxogramas | [Topologias](../integracao-controlid/network-topologies.md#fluxos-de-comunicação) |
| VIS-027 | Fronteiras de confiança | Fluxo de dados | [Hardening](../seguranca-privacidade/security-hardening.md#fronteiras-de-confiança-e-fluxos-de-dados) |
| VIS-028 | Fluxo de tratamento de dados pessoais | Fluxograma | [Privacidade](../seguranca-privacidade/privacy-and-data-retention.md#fluxo-de-tratamento) |
| VIS-029 | Inicialização, migração e prontidão | Sequência UML | [Implantação](../operacao/deployment-runbook.md#sequência-de-inicialização-e-prontidão) |
| VIS-030 | Topologia de implantação independente de provedor | Fluxograma | [Implantação](../operacao/deployment-runbook.md#topologia-de-referência-independente-de-provedor) |
| VIS-031 | Correlação, logs, métricas e alertas | Sequência UML | [Observabilidade](../operacao/observability-runbook.md#fluxo-observável-de-uma-requisição) |
| VIS-032 | Diagnóstico operacional | Fluxograma | [Diagnóstico](../operacao/troubleshooting-controlid.md#triagem-em-cinco-minutos) |
| VIS-033 | Quality gates da CI | Fluxograma | [CI/CD](../qualidade/ci-cd-quality-gates.md#fluxo-da-automação) |
| VIS-034 | Proxy assinador | Fluxograma | [Guia do proxy](../../tools/ControlIdCallbackSigningProxy/README.md#topologia-e-fronteira-de-confiança) |

Alguns itens agregam mais de um desenho do mesmo escopo, como as três topologias
de rede, as duas sequências de contratos e as decomposições das classes de saída
e de ingresso. O inventário identifica a visão canônica; a quantidade mecânica
de blocos Mermaid é validada separadamente.

## Matriz de cobertura

| Pergunta do leitor | Visões principais |
| --- | --- |
| Quem usa a PoC e com quais permissões? | VIS-006, VIS-021 e VIS-022 |
| Como frontend, backend, banco e equipamento se conectam? | VIS-001, VIS-002 e VIS-005 |
| Como uma chamada sai para a Access API? | VIS-009, VIS-010, VIS-011 e VIS-013 |
| Como callbacks, Monitor e Push entram com segurança? | VIS-004, VIS-012, VIS-014, VIS-015 a VIS-018 e VIS-027 |
| Onde o estado fica e como é recuperado? | VIS-023 a VIS-025 |
| Como os modos de operação mudam? | VIS-019 e VIS-020 |
| Como implantar, observar e diagnosticar? | VIS-029 a VIS-033 |

## Regras de manutenção

1. Atualize o diagrama na mesma mudança que alterar componente, dependência,
   estado, rota, tabela ou fluxo representado.
2. Mantenha o desenho focado; divida-o quando ultrapassar aproximadamente 20
   participantes ou exigir zoom para leitura no GitHub.
3. Cite nomes reais de classes, rotas e tabelas quando a visão for técnica.
4. Não represente comportamento futuro como implementado. Use “externo”,
   “opcional”, “necessita validação” ou “não implementado” quando aplicável.
5. Não inclua IP, host, credencial, payload, identificador pessoal ou segredo real.
6. Ao criar, remover ou renomear uma visão, atualize este catálogo e o índice do
   domínio.
7. Execute `tools/validate-documentation.ps1` e revise visualmente o Mermaid no
   GitHub antes da liberação.

## Navegação documental

- [Voltar ao índice de arquitetura](README.md).
- [Abrir a central de documentação](../README.md).
