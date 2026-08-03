# ADR 0002 - Ingress e egress Control iD seguros por padrão fora de Development

Status: Aceita

Data: 2026-05-01

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
- Testes e runbooks devem cobrir callback security e contrato de equipamento.
- O proxy assinador adiciona uma opção operacional para equipamentos sem HMAC.

## Evidências

- `Program.cs`
- `Options/CallbackSecurityOptions.cs`
- `Services/Callbacks/CallbackSecurityEvaluator.cs`
- `Services/Callbacks/CallbackSignatureValidator.cs`
- `tools/ControlIdCallbackSigningProxy/`
- `docs/security-hardening.md`
