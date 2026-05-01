# ADR 0002 - Ingress e egress Control iD seguros por padrao fora de Development

Status: Aceita

Data: 2026-05-01

## Contexto

A PoC recebe callbacks/Push do equipamento e faz chamadas para a Access API
Control iD. Esses fluxos podem envolver credenciais, sessoes, payloads pessoais e
dados sensiveis. Fora de `Development`, configuracao permissiva criaria risco de
exposicao.

## Decisao

Fora de `Development`, exigir configuracao segura de host, callbacks assinados,
shared key, allowlist de equipamento e OpenAPI desabilitado. Equipamentos sem
HMAC nativo devem usar o proxy assinador local quando necessario.

## Alternativas consideradas

- Permitir callbacks sem assinatura fora de Development: rejeitado por risco de
  spoofing e tampering.
- Validar apenas por IP: insuficiente quando ha proxy, NAT ou rede compartilhada.
- Exigir ferramenta externa de API gateway desde a PoC: forte, mas adicionaria
  dependencia operacional sem evidencia suficiente.

## Consequencias

- Startup falha quando configuracoes inseguras sao detectadas.
- Setup real precisa preencher `.env`/User Secrets/secret manager corretamente.
- Testes e runbooks devem cobrir callback security e contrato de equipamento.
- O proxy assinador adiciona uma opcao operacional para equipamentos sem HMAC.

## Evidencias

- `Program.cs`
- `Options/CallbackSecurityOptions.cs`
- `Services/Callbacks/CallbackSecurityEvaluator.cs`
- `Services/Callbacks/CallbackSignatureValidator.cs`
- `tools/ControlIdCallbackSigningProxy/`
- `docs/security-hardening.md`
