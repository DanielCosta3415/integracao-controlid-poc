# ADR 0001 - SQLite local como estado de execução da PoC

> **Decisão aceita** · Público: arquitetura e dados · Responsável: liderança técnica · Última validação: 2026-08-03.

Estado: aceita

- Data da decisão: 2026-05-01
- Substitui: nenhuma decisão
- Substituída por: nenhuma decisão

## Direcionadores

- execução local sem infraestrutura obrigatória;
- estado persistente suficiente para demonstração, testes e diagnóstico;
- recuperação verificável sem versionar dados de execução;
- custo operacional proporcional ao escopo de uma PoC de instância única.

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
- `tests/Integracao.ControlID.PoC.Tests/Data/OperationalIndexMigrationTests.cs`
- `tests/Integracao.ControlID.PoC.Tests/Services/Database/RepositoryFailureContractTests.cs`

## Critério de revisão

Reavalie esta decisão antes de múltiplas réplicas com escrita, banco remoto,
requisitos de alta disponibilidade ou volume incompatível com os limites medidos
em `docs/finops-capacity.md`.

## Evolução da decisão

- Substitui: nenhuma decisão anterior.
- Substituída por: nenhuma até esta validação.
- Gatilhos objetivos: mais de uma réplica gravadora, requisito de alta
  disponibilidade ou SQLite acima do limite operacional de 512 MB.
- Evidência para mudança: perfil de carga, plano de migração, compatibilidade de
  reversão e teste de restauração no banco candidato.
