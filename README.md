# Integracao.ControlID.PoC

PoC web em ASP.NET Core 8 para exploracao operacional e tecnica da Access API da Control iD. O projeto combina:

- console operacional para conexao, autenticacao, hardware, cadastro e auditoria;
- catalogo oficial de endpoints com documentacao visual de contrato;
- persistencia local em SQLite para eventos, push, usuarios e artefatos da PoC;
- trilha de QA visual, smoke tests e hardening de seguranca ja aplicada.

## Stack e arquitetura

- ASP.NET Core MVC + Razor Pages
- Entity Framework Core + SQLite
- Serilog para logs em console e arquivo
- xUnit para testes unitarios
- smoke test local em PowerShell com stub de equipamento

Pastas principais:

- `Controllers/`: fluxos MVC e endpoints auxiliares
- `Services/`: integracoes oficiais, seguranca, navegacao e fabricas
- `Views/`: UI Razor da PoC
- `tests/`: testes unitarios
- `tools/`: utilitarios locais, incluindo smoke test e stub do equipamento
- `docs/reports/`: relatorios tecnicos gerados durante as validacoes

## Requisitos

- .NET SDK 8
- Windows PowerShell 5+ ou PowerShell 7+
- Visual Studio 2022/2026 ou terminal com `dotnet`

## Setup rapido

1. Restaurar dependencias:

```powershell
dotnet restore .\Integracao.ControlID.PoC.sln
```

2. Ajustar configuracoes locais, se necessario, por variaveis de ambiente ou `appsettings.Development.json`.

3. Compilar a solucao:

```powershell
dotnet build .\Integracao.ControlID.PoC.sln
```

4. Executar a PoC:

```powershell
dotnet run --project .\Integracao.ControlID.PoC.csproj
```

## Variaveis de ambiente uteis

A configuracao segue o padrao nativo do ASP.NET Core (`Secao__Chave`). Exemplos:

- `ASPNETCORE_ENVIRONMENT=Development`
- `ConnectionStrings__DefaultConnection=Data Source=integracao_controlid.db`
- `ControlIDApi__DefaultDeviceUrl=http://192.168.1.100:8080`
- `ControlIDApi__DefaultUsername=admin`
- `ControlIDApi__DefaultPassword=`
- `ControlIDApi__ConnectionTimeoutSeconds=30`
- `Session__IdleTimeout=30`
- `CallbackSecurity__RequireSharedKey=true`
- `CallbackSecurity__SharedKeyHeaderName=X-ControlID-Callback-Key`
- `CallbackSecurity__SharedKey=segredo-local`
- `CallbackSecurity__AllowLoopback=true`
- `Logging__LogLevel__Default=Information`
- `Serilog__MinimumLevel__Default=Information`

Observacoes:

- nunca versione credenciais reais;
- prefira User Secrets ou variaveis de ambiente para valores sensiveis;
- `ControlIDApi__ConnectionTimeoutSeconds` agora controla o timeout real das chamadas oficiais.

## Banco local e dados da PoC

- O banco SQLite padrao fica em `integracao_controlid.db`.
- As migracoes sao aplicadas automaticamente no startup.
- A PoC tambem cria tabelas auxiliares de monitoramento local para callbacks e push.

## Testes automatizados

### Testes unitarios

```powershell
dotnet test .\Integracao.ControlID.PoC.sln
```

### Smoke test funcional local

O smoke test sobe o stub local, exercita happy paths e edge cases criticos e gera um relatorio em `docs/reports/`.

```powershell
powershell -ExecutionPolicy Bypass -File .\tools\smoke-localhost.ps1
```

Relatorio atual de referencia:

- `docs/reports/localhost-smoke-test-2026-04-14.md`

## Observabilidade e monitoramento

A PoC ja sai preparada para monitoramento basico local:

- log de requests HTTP no middleware `RequestLoggingMiddleware`;
- log estruturado de invocacao oficial da Access API, incluindo endpoint, target, status e duracao;
- log de callback ingress com aceite, bloqueio e falha de persistencia;
- log de fila push para enfileiramento, entrega e recebimento de resultados.

Saidas de log:

- console da aplicacao;
- arquivo rolling em `Logs/app_log.txt`.

Checklist recomendado para debug operacional:

1. verificar se o equipamento configurado esta acessivel;
2. validar se a sessao foi aberta antes de endpoints autenticados;
3. acompanhar `Logs/app_log.txt` durante callbacks, push e chamadas oficiais;
4. abrir `OfficialApi` para confirmar contrato visual, payload e query;
5. rodar o smoke test local antes de publicar mudancas sensiveis.

## Fluxos principais

- `Home`: painel executivo-operacional da PoC
- `Workspace`: mapa funcional por dominio
- `OfficialApi`: catalogo de endpoints oficiais e invocacao assistida
- `OfficialObjects`: CRUD tecnico de objetos oficiais
- `OfficialEvents` e `PushCenter`: observabilidade, callbacks e fila push

## Documentacao complementar

- `Services/ControlIDApi/README.md`: resumo da camada oficial de integracao
- `docs/reports/heuristic-ui-audit-2026-04-14.md`: auditoria heuristica de UX/UI
- `docs/reports/design-system-accessibility-audit-2026-04-14.md`: auditoria de design system e acessibilidade
- `docs/reports/visual-inventory-2026-04-14.md`: inventario visual consolidado
- `docs/changelog-2026-04-14.md`: resumo tecnico do que mudou e por que mudou

## Troubleshooting rapido

### A PoC nao conecta no equipamento

- confira esquema, IP e porta no painel de conexao;
- valide `ControlIDApi__ConnectionTimeoutSeconds`;
- veja os logs do `OfficialApiInvokerService` para timeout, status HTTP e target.

### Callbacks nao aparecem

- confira `CallbackSecurity__RequireSharedKey` e `CallbackSecurity__SharedKey`;
- valide IP remoto permitido quando houver restricao;
- acompanhe os logs de `CallbackIngressService`.

### Push nao entrega comandos

- confira se o dispositivo esta consultando `GET /push`;
- valide se os resultados estao chegando em `POST /result`;
- acompanhe os logs de `PushCenterController`.
