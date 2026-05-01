# Modelo de dados, integridade e recuperacao

Este documento descreve o estado de dados local da PoC, as regras de evolucao de schema e os procedimentos seguros de backup/restore. Ele deve ser revisado antes de mudancas em `Models/Database/`, `Data/Migrations/`, `Services/Database/`, callbacks, push ou retencao de payloads.

## Inventario de persistencias

| Persistencia | Evidencia | Uso | Observacao |
| --- | --- | --- | --- |
| SQLite local | `ConnectionStrings:DefaultConnection`, `IntegracaoControlIDContext`, `Data/Migrations/` | Estado runtime da PoC, eventos, push, cadastros e auditoria local | Arquivos `integracao_controlid.db*` nao devem ser versionados. |
| Arquivos de log | Serilog, `Logs/app_log.txt` | Diagnostico operacional | Pode conter metadados sensiveis; manter retencao curta. |
| Artefatos locais | `artifacts/`, `docs/reports/` | Relatorios de smoke, QA e backups locais | `artifacts/` fica fora do Git; reports versionados devem usar dados ficticios. |
| Fila persistente local | Tabela `PushCommands` | Comandos e resultados Push | Nao ha broker externo; consistencia e idempotencia sao locais. |
| Cache distribuido | Nao encontrado | N/A | Nao aplicavel no estado atual. |
| NoSQL/search/storage externo | Nao encontrado | N/A | Nao aplicavel no estado atual. |

## Tabelas e dados

| Tabela | Chave | Campos principais | Dados sensiveis | Retencao |
| --- | --- | --- | --- | --- |
| `AccessLogs` | `Id` | `Time`, `Event`, `DeviceId`, `UserId`, `PortalId`, `Info` | Eventos de acesso, usuario/dispositivo | Minimo necessario para QA. |
| `AccessRules` | `Id` | `Name`, `Type`, `Priority`, `BeginTime`, `EndTime`, `Status` | Regras de acesso | Enquanto necessaria para homologacao. |
| `BiometricTemplates` | `Id` | `UserId`, `Template`, `Type`, `FingerPosition`, `FingerType` | Biometria/template | Alto cuidado; evitar dados reais. |
| `Cards` | `Id` | `UserId`, `Value`, `Type`, `BeginTime`, `EndTime`, `Status` | Cartoes/tags | Minimo necessario. |
| `ChangeLogs` | `Id` | `OperationType`, `TableName`, `TableId`, `Timestamp`, `PerformedBy`, `Description` | Auditoria operacional | Curto prazo local. |
| `Configs` | `Id` | `Group`, `Key`, `Value`, `Description` | Pode conter configuracao sensivel | Nao armazenar secrets reais. |
| `Devices` | `Id` | `Name`, `Ip`, `IpAddress`, `SerialNumber`, `Firmware`, `Status`, `LastSeenAt` | IP/serial/rede | Enquanto ambiente existir. |
| `Groups` | `Id` | `Name`, `Description`, `Status` | Baixo a moderado | Conforme homologacao. |
| `Logos` | `Id` | `Base64Image`, `Timestamp`, `FileName`, `Format`, `Description` | Imagem/base64 | Evitar imagens reais. |
| `Logs` | `Id` | `Level`, `Message`, `Timestamp`, `StackTrace`, `User`, `EventCode`, `Source`, `AdditionalData` | Logs podem conter metadados sensiveis | Curto prazo local. |
| `MonitorEvents` | `EventId` | `ReceivedAt`, `RawJson`, `EventType`, `DeviceId`, `UserId`, `Payload`, `Status` | Payload bruto de callback | Limpar manualmente apos QA. |
| `Photos` | `Id` | `UserId`, `Base64Image`, `Timestamp`, `FileName`, `Format` | Foto/base64 | Alto cuidado; evitar dados reais. |
| `PushCommands` | `CommandId` | `ReceivedAt`, `CommandType`, `RawJson`, `Status`, `Payload`, `DeviceId`, `UserId` | Comando/resultado/payload bruto | Limpar manualmente apos analise. |
| `QRCodes` | `Id` | `UserId`, `Value`, `BeginTime`, `EndTime`, `Status` | QR code/token | Minimo necessario. |
| `Sessions` | `Id` | `DeviceAddress`, `SessionString`, `DeviceName`, `DeviceSerial`, `Username`, `ExpiresAt`, `IsActive` | Sessao e usuario | Curto prazo; nao compartilhar DB. |
| `Syncs` | `Id` | `SyncType`, `Status`, `Message`, `StartedAt`, `FinishedAt`, `ErrorCode`, `AdditionalData` | Diagnostico operacional | Curto prazo local. |
| `Users` | `Id` | `Name`, `Registration`, `Username`, `PasswordHash`, `Salt`, `Email`, `Phone`, `Status` | Dados pessoais e credenciais derivadas | Evitar dados reais; minimo necessario. |

## Integridade e evolucao

- O schema usa chaves primarias locais, mas nao declara foreign keys entre entidades. Isso evita quebrar dados importados ou simulados da Access API quando IDs remotos ainda nao possuem contrato relacional fechado no projeto.
- Indices adicionados sao nao unicos. Unicidade em `Registration`, `Username`, `SerialNumber` ou `Group/Key` deve exigir analise de duplicidade antes de virar constraint.
- `DeviceLocal` ainda possui `Ip` e `IpAddress`. O campo duplicado deve ser preservado ate existir plano de migracao e confirmacao de compatibilidade.
- `Program.cs` aplica `Database.Migrate()` no startup e mantem criacao idempotente de `MonitorEvents` e `PushCommands` para compatibilidade com bancos locais antigos.
- Mudancas futuras de schema devem entrar por migrations versionadas. Evite criar tabelas novas diretamente no startup, exceto compatibilidade temporaria documentada.

## Indices operacionais

A migration `AddOperationalIndexes` adiciona indices para filtros e ordenacoes ja usados pelos repositorios e telas operacionais:

- `AccessLogs`: data, usuario, dispositivo e evento.
- `MonitorEvents`: data de recebimento, tipo, status e dispositivo.
- `PushCommands`: status/dispositivo/data, tipo, usuario e recebimento.
- `Sessions`: sessoes ativas por data, dispositivo e usuario.
- `ChangeLogs`, `Logs`, `Syncs`: filtros por status/tipo/nivel e ordenacao temporal.
- `Users`, `Devices`, `Cards`, `QRCodes`, `BiometricTemplates`, `Photos`, `Configs`: lookup por identificadores funcionais frequentes. Em `Devices`, `Ip` e `IpAddress` ficam indexados para compatibilidade, mas novas consultas locais devem preferir `Ip`.

Justificativa: reduzir full scans provaveis em telas de historico, callbacks, push, logs e sessoes sem introduzir constraints novas.

## Limites de consulta e expurgo

Repositórios locais aplicam `LocalDataQueryLimits.DefaultListLimit` em listagens e buscas para evitar full scans e renderizacao de volumes excessivos por acidente. Fluxos de limpeza confirmados usam operacoes especificas de delete em banco, sem carregar toda a tabela em memoria.

Fluxos com expurgo guiado:

- `OfficialEvents/Purge`: remove `MonitorEvents` mais antigos que a janela de retencao informada, com frase `EXPURGAR EVENTOS`.
- `PushCenter/Purge`: remove `PushCommands` mais antigos que a janela de retencao informada, com frase `EXPURGAR PUSH`.

Os limites aceitos para retencao ficam entre 1 e 3650 dias.

## Migracoes

Ferramenta: EF Core migrations.

Historico atual:

- `InitialLocalSchema`: cria tabelas com `CREATE TABLE IF NOT EXISTS` para preservar bancos locais existentes.
- `AddOperationalIndexes`: cria indices com `CREATE INDEX IF NOT EXISTS` e remove com `DROP INDEX IF EXISTS`.

Regras de seguranca:

- Nao executar migration destrutiva sem backup e confirmacao humana.
- Nao remover coluna/tabela enquanto houver dados locais sem plano de migracao, rollback e retencao.
- Para mudancas de alta volumetria, preferir estrategia em etapas: adicionar campo nullable, preencher de forma controlada, validar, depois tornar obrigatorio se necessario.
- Scripts de rollback devem ser revisaveis e nao apagar dados por padrao.

## Seeds

Nao ha seeds versionados com dados reais ou ficticios permanentes. Os testes usam dados criados em memoria e o smoke local usa stub/fakes controlados. Mantenha essa regra: dados reais de usuarios, fotos, biometria, cartoes, QR codes, IPs de clientes ou secrets nao devem entrar no Git.

## Backup local seguro

Use o script abaixo antes de validar mudancas de schema em um SQLite local com dados importantes:

```powershell
powershell -ExecutionPolicy Bypass -File .\tools\backup-sqlite.ps1
```

O script copia o arquivo `.db` e, se existirem, os arquivos `-wal` e `-shm` para `artifacts/backups/sqlite-<timestamp>/`, junto com um manifesto. Ele nao altera o banco de origem.

Para operacao real, prefira o wrapper operacional, que pode espelhar o backup
para destino fora do host, executar restore-smoke e aplicar retencao somente com
confirmacao textual:

```powershell
$env:CONTROLID_BACKUP_MIRROR_DIRECTORY = "\\servidor-seguro\backups\controlid-poc"
powershell -ExecutionPolicy Bypass -File .\tools\backup-sqlite-operational.ps1 -RunRestoreSmoke
```

Backups novos sao protegidos por DPAPI por padrao e recebem extensao `.protected`. Use `-Unprotected` apenas para interoperabilidade local temporaria e registre a justificativa. O manifesto informa `Protected`, `Protection` e `ProtectionScope`.

Recomendacoes:

- Pare a aplicacao antes do backup quando possivel, para reduzir risco de copia parcial.
- Se o banco estiver em WAL mode, preserve sempre `.db`, `-wal` e `-shm` juntos.
- Proteja backups como dados sensiveis; eles podem conter usuarios, sessoes, fotos, biometria, logs e payloads brutos.
- Restrinja permissoes locais de SQLite, logs e backups com `powershell -ExecutionPolicy Bypass -File .\tools\harden-local-state.ps1`.
- Nao versionar backups.
- Para revisar capacidade e custo local sem apagar dados, rode `powershell -ExecutionPolicy Bypass -File .\tools\finops-capacity-check.ps1`.

## Restore

Restore sobrescreve estado local e exige confirmacao humana. Procedimento recomendado:

1. Parar a aplicacao e qualquer processo que esteja usando o SQLite.
2. Fazer backup do estado atual com `tools/backup-sqlite.ps1`.
3. Validar o backup escolhido com `tools/restore-smoke-sqlite.ps1`.
4. Com confirmacao humana, copiar a copia restaurada validada para o caminho configurado em `ConnectionStrings:DefaultConnection`, preservando tambem `-wal` e `-shm` quando existirem.
5. Rodar `dotnet build .\Integracao.ControlID.PoC.sln --no-restore -v:minimal`.
6. Subir a aplicacao em ambiente local controlado e validar os fluxos afetados.

RTO/RPO nao estao garantidos para producao ate existir validacao em ambiente alvo. Para release operacional, `ops.local.json` deve registrar os valores aprovados e `tools/operational-readiness-check.ps1 -RequireConfig` bloqueia status pendente. O fechamento completo esta em `docs/residual-risk-closure.md`.

## Smoke de restore

Para validar que um backup pode ser aberto e receber migrations sem sobrescrever o banco real:

```powershell
powershell -ExecutionPolicy Bypass -File .\tools\restore-smoke-sqlite.ps1
```

Sem parametro, o script usa o backup mais recente em `artifacts/backups/`. Ele copia ou descriptografa o backup para `artifacts/restore-smoke/`, executa `dotnet ef database update --no-build --connection ...` sobre essa copia e grava um manifesto do teste.

## Riscos controlados e acompanhamento

| Severidade | Item | Controle implementado | Acompanhamento |
| --- | --- | --- | --- |
| Media | Dados sensiveis podem existir em SQLite, logs e backups locais | `.gitignore`, backups DPAPI por padrao, hardening de permissoes locais, docs de privacidade e expurgo confirmado de monitor/push | Executar `tools/harden-local-state.ps1` no host alvo e revisar mascaramento em logs a cada novo fluxo. |
| Baixa | Restore precisa ser exercitado de forma recorrente | Procedimento documentado e smoke de restore em copia temporaria, incluindo backups `.protected` | Executar smoke regularmente antes de mudancas de schema e em preparacoes de release. |
| Media | Sem foreign keys entre tabelas locais | Preserva compatibilidade com IDs remotos | Definir contrato relacional antes de constraints. |
| Media | Consultas de listagem ainda podem carregar muitos registros | Indices adicionados, limite padrao em repositorios e aviso de listagem truncada em monitor/push | Avaliar paginacao por tela quando houver volume real. |
| Media | `Ip` e `IpAddress` duplicam finalidade em `Devices` | Campo preservado para compatibilidade, consultas novas usam `Ip`, ambos ficam indexados | Planejar consolidacao versionada sem drop destrutivo. |
| Baixa | Sem seeds formais | Testes criam dados em memoria | Criar fixtures ficticias se surgirem testes de integracao mais amplos. |
