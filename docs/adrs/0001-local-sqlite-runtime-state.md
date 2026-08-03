# ADR 0001 - SQLite local como estado de execução da PoC

Status: Aceita

Data: 2026-05-01

## Contexto

A PoC precisa persistir usuários locais, sessões, callbacks, eventos de monitor,
Push, logs operacionais e artefatos técnicos sem exigir infraestrutura externa.
O repositório também precisa ser executável localmente por desenvolvedores e em
container.

## Decisão

Usar SQLite local via Entity Framework Core como estado runtime da PoC. O arquivo
`integracao_controlid.db*` é estado local, não um artefato versionado.

## Alternativas consideradas

- Banco relacional externo: melhor para produção multi-instancia, mas criaria
  dependência de infraestrutura e segredos para uma PoC.
- Banco em memória: simples, mas perderia histórico de callbacks, Push e testes de
  recuperação.
- NoSQL/cache externo: não há necessidade comprovada no escopo atual.

## Consequências

- Setup local fica simples e reproduzível.
- Backup/restore precisa tratar `.db`, `-wal` e `-shm` juntos.
- Escala horizontal e concorrência ficam limitadas.
- Dados locais podem conter dados pessoais/sensíveis e devem ficar fora do Git.
- Mudanças de schema exigem migrations, backup e testes.

## Evidências

- `Data/IntegracaoControlIDContext.cs`
- `Data/Migrations/`
- `docs/data-model-and-recovery.md`
- `tools/backup-sqlite-operational.ps1`
