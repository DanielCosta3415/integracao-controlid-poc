# ADR 0006: simulador determinístico e validação real de navegador

> **Decisão aceita** · Público: arquitetura, QA e mantenedores · Responsável: liderança técnica · Última validação: 2026-08-04.

Estado: aceita

## Contexto

A evolução da PoC não pode depender da disponibilidade contínua de equipamento
físico. O stub anterior cobria apenas happy paths e testes textuais de frontend
não detectavam contraste, overflow, foco ou regressão visual em navegador real.

## Decisão

- manter um simulador exclusivo de loopback com estado, perfis, massas e falhas
  determinísticos;
- separar as rotas administrativas `__stub` do contrato oficial;
- usar schemas e fixtures versionados para a administração do simulador;
- executar Playwright com Chromium, axe e referências visuais em dados fictícios;
- exibir explicitamente se a conexão atual foi identificada como simulada;
- manter homologação física como nível de evidência separado e obrigatório para
  declarar compatibilidade de firmware ou efeito eletromecânico.

## Alternativas consideradas

- depender somente de mocks de `HttpMessageHandler`: rápido, mas não cobre rede,
  processo, Razor, JavaScript nem sessão;
- exigir hardware em toda mudança: maior fidelidade, porém baixa disponibilidade
  e pouca reprodutibilidade;
- usar serviço externo compartilhado: introduz custo, dados, rede e estado
  concorrente desnecessários para a PoC.

## Consequências

Positivas: falhas reproduzíveis, E2E autenticado, benchmark local, menor tempo de
diagnóstico e onboarding sem aparelho. Custos: manutenção do stub, Chromium na
CI, referências visuais e risco de divergência em relação ao firmware. Esse risco
é controlado pela matriz de evidência e pelo contrato físico, não por alegação de
equivalência.

## Validação

`tools/contract-controlid-stub.ps1`, o projeto
`tests/Integracao.ControlID.PoC.E2E`, `tools/performance-baseline.ps1` e
`docs/endpoint-validation-matrix.md` formam a evidência executável e documental
desta decisão.
