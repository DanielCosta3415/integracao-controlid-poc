# ADR 0002 - Fluxos Control iD de entrada e saída seguros fora de Development

> **Decisão aceita** · Público: arquitetura e segurança · Responsável: liderança técnica · Última validação: 2026-08-03.

Estado: aceita

- Data da decisão: 2026-05-01
- Substitui: nenhuma decisão
- Substituída por: nenhuma decisão

## Direcionadores

- autenticar origens externas em uma camada confiável;
- reduzir falsificação, adulteração, replay e SSRF;
- falhar de modo seguro fora de desenvolvimento;
- manter compatibilidade com equipamentos sem HMAC por adaptador restrito.

## Contexto

A PoC recebe callbacks/Push do equipamento e faz chamadas para a Access API
Control iD. Esses fluxos podem envolver credenciais, sessões, payloads pessoais e
dados sensíveis. Fora de `Development`, configuração permissiva criaria risco de
exposição.

## Decisão

Fora de `Development`, exigir configuração segura de host, callbacks assinados,
shared key, allowlist de equipamento e OpenAPI desabilitado. Equipamentos sem
HMAC nativo devem usar o proxy assinador local quando necessário.

## Alternativas consideradas

- Permitir callbacks sem assinatura fora de Development: rejeitado por risco de
  spoofing e tampering.
- Validar apenas por IP: insuficiente quando há proxy, NAT ou rede compartilhada.
- Exigir ferramenta externa de API gateway desde a PoC: forte, mas adicionaria
  dependência operacional sem evidência suficiente.

## Consequências

- Startup falha quando configurações inseguras são detectadas.
- Setup real precisa preencher `.env`/User Secrets/secret manager corretamente.
- Testes e guias operacionais devem cobrir a segurança de callbacks e o contrato de equipamento.
- O proxy assinador adiciona uma opção operacional para equipamentos sem HMAC.

## Evidências

- `Program.cs`
- `Options/CallbackSecurityOptions.cs`
- `Services/Callbacks/CallbackSecurityEvaluator.cs`
- `Services/Callbacks/CallbackSignatureValidator.cs`
- `tools/ControlIdCallbackSigningProxy/`
- `docs/security-hardening.md`
- `tests/Integracao.ControlID.PoC.Tests/Services/Callbacks/CallbackSignatureValidatorTests.cs`
- `tests/Integracao.ControlID.PoC.Tests/Services/Security/ControlIdInputSanitizerTests.cs`

## Critério de revisão

Reavalie somente quando o fabricante oferecer contrato autenticado equivalente,
quando a topologia de rede mudar ou quando um gateway confiável assumir
formalmente os mesmos controles e evidências.

## Evolução da decisão

- Substitui: nenhuma decisão anterior.
- Substituída por: nenhuma até esta validação.
- Gatilhos objetivos: autenticação equivalente do fabricante, gateway aprovado
  ou mudança da topologia de confiança.
- Evidência para mudança: contrato do fornecedor, modelo de ameaça atualizado,
  teste de replay, limites, rotação de chave e validação de falha segura.
