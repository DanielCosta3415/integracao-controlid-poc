# Changelog técnico - 2026-04-14

## O que mudou

- O shell, a navegação e o mapa funcional da PoC foram reorganizados por domínio operacional.
- Foi criada uma camada visual mais robusta para o catálogo oficial da API, com documentação de contrato por endpoint.
- A UI recebeu uma rodada completa de QA visual, acessibilidade, hardening e smoke tests locais.
- A camada de navegação e a busca global foram otimizadas para reduzir o custo de renderização e listeners redundantes.
- Integrações oficiais, callbacks e fila push passaram a emitir logs mais estratégicos para troubleshooting e monitoramento.
- A documentação principal do projeto foi criada ou atualizada com setup, variáveis de ambiente, testes e observabilidade.
- O centro unificado de modos `Standalone`, `Pro` e `Enterprise` foi incorporado à PoC, junto de matriz de homologação e runbook E2E.
- O pipeline ganhou compressão de resposta, carregamento tipográfico menos bloqueante e ajustes de renderização para superfícies técnicas longas.

## Por que mudou

- Reduzir o custo de manutenção e facilitar a descoberta de funcionalidades dentro da PoC.
- Tornar a Access API mais compreensível para desenvolvedores que usam o projeto como console de integração e documentação viva.
- Melhorar a confiabilidade operacional, com feedback mais claro para timeout, falha de persistência, endpoint inválido e problemas de sessão.
- Dar mais previsibilidade ao QA local com smoke test reproduzível e relatórios versionados.
- Preparar a PoC para debug e monitoramento sem depender de conhecimento informal do time.
- Facilitar a homologação de modos de operação por linha de produto sem espalhar regra de negócio pela interface.
- Reduzir latência percebida e custo de rede em páginas mais densas, sem sacrificar legibilidade ou manutenibilidade.

## Impacto esperado

- Menos ambiguidade no uso da PoC por novos desenvolvedores.
- Menor tempo para diagnosticar falhas em chamadas oficiais, callbacks e push.
- Menor risco de regressão visual, de performance e de segurança nas próximas iterações.
- Melhor cobertura operacional para cenários `Standalone`, `Pro` e `Enterprise`.
- Menor custo de carregamento nas superfícies técnicas mais extensas.
