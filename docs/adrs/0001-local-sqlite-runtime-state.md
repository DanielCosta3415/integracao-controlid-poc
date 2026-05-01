# ADR 0001 - SQLite local como estado runtime da PoC

Status: Aceita

Data: 2026-05-01

## Contexto

A PoC precisa persistir usuarios locais, sessoes, callbacks, eventos de monitor,
Push, logs operacionais e artefatos tecnicos sem exigir infraestrutura externa.
O repositorio tambem precisa ser executavel localmente por desenvolvedores e em
container.

## Decisao

Usar SQLite local via Entity Framework Core como estado runtime da PoC. O arquivo
`integracao_controlid.db*` e estado local, nao artefato versionado.

## Alternativas consideradas

- Banco relacional externo: melhor para producao multi-instancia, mas criaria
  dependencia de infraestrutura e segredos para uma PoC.
- Banco em memoria: simples, mas perderia historico de callbacks, Push e testes de
  recuperacao.
- NoSQL/cache externo: nao ha necessidade comprovada no escopo atual.

## Consequencias

- Setup local fica simples e reproduzivel.
- Backup/restore precisa tratar `.db`, `-wal` e `-shm` juntos.
- Escala horizontal e concorrencia ficam limitadas.
- Dados locais podem conter dados pessoais/sensiveis e devem ficar fora do Git.
- Mudancas de schema exigem migrations, backup e testes.

## Evidencias

- `Data/IntegracaoControlIDContext.cs`
- `Data/Migrations/`
- `docs/data-model-and-recovery.md`
- `tools/backup-sqlite-operational.ps1`
