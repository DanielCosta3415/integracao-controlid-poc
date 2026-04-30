# Banco de dados e estado de runtime

Este projeto usa SQLite local por padrao. A connection string `DefaultConnection` fica em `appsettings.json` e aponta para `integracao_controlid.db`.

## Estado criado em runtime

Os arquivos abaixo sao estado local e nao devem ser versionados:

- `integracao_controlid.db`
- `integracao_controlid.db-shm`
- `integracao_controlid.db-wal`
- `Logs/`
- `artifacts/`
- `bin/`
- `obj/`

Esses caminhos ja estao cobertos por `.gitignore`.

## Aplicacao de schema

Na inicializacao, `Program.cs` executa `Database.Migrate()` e cria, se necessario, as tabelas locais `MonitorEvents` e `PushCommands` por SQL idempotente.

Na linha de base atual nao ha pasta `Migrations/` nem arquivos `.sql` versionados. Por isso, iniciar a aplicacao pode criar ou atualizar o banco local, mesmo sem alterar arquivos rastreados pelo Git.

## Comandos seguros

Use estes comandos para validacao sem tocar em dados de producao:

```powershell
dotnet restore .\Integracao.ControlID.PoC.sln --locked-mode
dotnet build .\Integracao.ControlID.PoC.sln --no-restore -v:minimal
dotnet test .\Integracao.ControlID.PoC.sln --no-build -v:minimal
dotnet format .\Integracao.ControlID.PoC.sln --verify-no-changes --no-restore -v:minimal
dotnet list .\Integracao.ControlID.PoC.sln package --vulnerable --include-transitive
```

## Comandos que alteram estado local

Estes comandos sao esperados para criar arquivos locais, processos ou relatorios:

```powershell
dotnet run --project .\Integracao.ControlID.PoC.csproj
powershell -ExecutionPolicy Bypass -File .\tools\smoke-localhost.ps1
```

O smoke test sobe o stub local, executa fluxos HTTP e escreve relatorios em `docs/reports/`.

## Requisitos para ambientes nao Development

Ambientes fora de `Development` devem configurar:

- `AllowedHosts` sem wildcard `*`.
- `CallbackSecurity:RequireSharedKey=true`.
- `CallbackSecurity:SharedKey` com valor secreto via User Secrets, variavel de ambiente ou cofre externo.

Sem esses valores, a aplicacao falha na inicializacao para evitar exposicao acidental dos callbacks e endpoints push.
