# Integracao.ControlID.PoC

PoC web em ASP.NET Core 8 para exploração operacional e técnica da Access API da Control iD. O projeto reúne:

- um console operacional para conexão, autenticação, hardware, cadastros e auditoria;
- um catálogo oficial de endpoints com documentação visual de contrato;
- persistência local em SQLite para eventos, push, usuários e artefatos da PoC;
- uma trilha de QA visual, smoke tests e hardening de segurança já aplicada.

## Stack e arquitetura

- ASP.NET Core MVC + Razor
- Entity Framework Core + SQLite
- Serilog para logs em console e arquivo
- xUnit para testes unitários
- Smoke test local em PowerShell com stub de equipamento

Pastas principais:

- `Controllers/`: fluxos MVC e endpoints auxiliares
- `Services/`: integrações oficiais, segurança, navegação e fábricas
- `Views/`: UI Razor da PoC
- `tests/`: testes unitários
- `tools/`: utilitários locais, incluindo smoke test e stub do equipamento
- `docs/reports/`: relatórios técnicos gerados durante as validações

Mapa detalhado de responsabilidades por pasta e arquivo:

- `docs/project-file-responsibilities.md`

Documentações funcionais de implementação:

- `docs/operation-modes-implementation.md`
- `docs/monitor-implementation.md`
- `docs/push-implementation.md`

## Requisitos

- .NET SDK 8
- Windows PowerShell 5+ ou PowerShell 7+
- Visual Studio 2022/2026 ou terminal com `dotnet`

## Configuração rápida

1. Restaure as dependências:

```powershell
dotnet restore .\Integracao.ControlID.PoC.sln
```

2. Ajuste as configurações locais, se necessário, por variáveis de ambiente, `appsettings.Development.json` ou User Secrets para segredos.

3. Compile a solução:

```powershell
dotnet build .\Integracao.ControlID.PoC.sln
```

4. Execute a PoC:

```powershell
dotnet run --project .\Integracao.ControlID.PoC.csproj
```

5. Acesse a interface:

- URL padrão local: `https://localhost:5001` ou a porta configurada pelo perfil de execução;
- o shell principal já expõe atalhos para `Workspace`, `OfficialApi`, `OperationModes`, `OfficialObjects` e `PushCenter`.

## Variáveis de ambiente úteis

A configuração segue o padrão nativo do ASP.NET Core (`Secao__Chave`). Exemplos:

- `ASPNETCORE_ENVIRONMENT=Development`
- `ConnectionStrings__DefaultConnection=Data Source=integracao_controlid.db`
- `ControlIDApi__DefaultDeviceUrl=http://<equipamento-ou-host>:8080`
- `ControlIDApi__DefaultUsername=<usuario-opcional>`
- `ControlIDApi__DefaultPassword=`
- `ControlIDApi__ConnectionTimeoutSeconds=30`
- `Session__IdleTimeout=30`
- `CallbackSecurity__RequireSharedKey=true`
- `CallbackSecurity__SharedKeyHeaderName=X-ControlID-Callback-Key`
- `CallbackSecurity__SharedKey=<segredo-configurado-fora-do-repositorio>`
- `CallbackSecurity__AllowLoopback=true`
- `Logging__LogLevel__Default=Information`
- `Serilog__MinimumLevel__Default=Information`
- `ASPNETCORE_URLS=https://localhost:5001`

Observações:

- nunca versione credenciais reais;
- prefira User Secrets ou variáveis de ambiente para valores sensíveis;
- `ControlIDApi__ConnectionTimeoutSeconds` controla o timeout real das chamadas oficiais.
- `CallbackSecurity__SharedKey` é obrigatória quando `CallbackSecurity__RequireSharedKey=true` fora de ambiente controlado.

## Banco local e dados da PoC

- O banco SQLite padrão fica em `integracao_controlid.db`.
- As migrações são aplicadas automaticamente no startup.
- A PoC também cria tabelas auxiliares de monitoramento local para callbacks e push.

## Testes automatizados

### Testes unitários

```powershell
dotnet test .\Integracao.ControlID.PoC.sln
```

Para acelerar uma rodada local depois de um `build` já concluído:

```powershell
dotnet test .\Integracao.ControlID.PoC.sln --no-build
```

### Smoke test funcional local

O smoke test sobe o stub local, percorre os happy paths e os edge cases críticos e gera um relatório em `docs/reports/`.

```powershell
powershell -ExecutionPolicy Bypass -File .\tools\smoke-localhost.ps1
```

Relatório atual de referência:

- `docs/reports/localhost-smoke-test-2026-04-14.md`

## Observabilidade e monitoramento

A PoC já sai preparada para monitoramento básico local:

- log de requisições HTTP no middleware `RequestLoggingMiddleware`;
- log estruturado de invocação oficial da Access API, incluindo endpoint, target, status e duração;
- log de entrada de callbacks com aceite, bloqueio e falha de persistência;
- log de fila push para enfileiramento, entrega e recebimento de resultados.
- compressão de resposta habilitada para HTML, CSS, JS e JSON, reduzindo custo de rede em páginas e payloads técnicos.

Saídas de log:

- console da aplicação;
- arquivo rolling em `Logs/app_log.txt`.

Pontos recomendados para monitorar primeiro:

- falhas de `OfficialApiInvokerService` por timeout, validação ou status HTTP inesperado;
- eventos rejeitados em `CallbackIngressService`;
- erros de persistência e entrega em `PushCenterController`;
- exceções capturadas por `ExceptionHandlingMiddleware`.

Checklist recomendado para debug operacional:

1. Verifique se o equipamento configurado está acessível.
2. Valide se a sessão foi aberta antes de chamar endpoints autenticados.
3. Acompanhe `Logs/app_log.txt` durante callbacks, push e chamadas oficiais.
4. Abra `OfficialApi` para confirmar contrato visual, payload e query.
5. Rode o smoke test local antes de publicar mudanças sensíveis.

## Fluxos principais

- `Home`: painel executivo-operacional da PoC
- `Workspace`: mapa funcional por domínio
- `OperationModes`: centro unificado para `Standalone`, `Pro` e `Enterprise`
- `OfficialApi`: catálogo de endpoints oficiais e invocação assistida
- `OfficialObjects`: CRUD técnico de objetos oficiais
- `ProductSpecific`: particularidades por linha de equipamento, SIP, áudio, QR/TOTP e upgrades
- `AdvancedOfficial`: recursos oficiais avançados, exportação, câmera e intertravamento
- `OfficialEvents` e `PushCenter`: observabilidade, callbacks e fila push

## Documentação complementar

- `Services/ControlIDApi/README.md`: resumo da camada oficial de integração
- `docs/reports/heuristic-ui-audit-2026-04-14.md`: auditoria heurística de UX/UI
- `docs/reports/design-system-accessibility-audit-2026-04-14.md`: auditoria de design system e acessibilidade
- `docs/reports/visual-inventory-2026-04-14.md`: inventário visual consolidado
- `docs/reports/operation-modes-homologation-matrix-2026-04-14.md`: matriz de cobertura e homologação por linha de produto
- `docs/reports/operation-modes-e2e-runbook-2026-04-14.md`: roteiro E2E de bancada para validar `Standalone`, `Pro` e `Enterprise`
- `docs/changelog-2026-04-14.md`: resumo técnico do que mudou e por que mudou

## Troubleshooting rápido

### A PoC não conecta ao equipamento

- confira esquema, IP e porta no painel de conexão;
- valide `ControlIDApi__ConnectionTimeoutSeconds`;
- veja os logs do `OfficialApiInvokerService` para timeout, status HTTP e target.

### Callbacks não aparecem

- confira `CallbackSecurity__RequireSharedKey` e `CallbackSecurity__SharedKey`;
- valide o IP remoto permitido quando houver restrição;
- acompanhe os logs de `CallbackIngressService`.

### Push não entrega comandos

- confira se o dispositivo está consultando `GET /push`;
- valide se os resultados estão chegando em `POST /result`;
- acompanhe os logs de `PushCenterController`.

### O shell parece lento em páginas técnicas longas

- valide se os assets estáticos estão sendo servidos normalmente;
- confira se há compressão de resposta habilitada no ambiente;
- use a página `OfficialApi` como referência para verificar se o catálogo está sendo carregado com os filtros esperados;
- em ambiente local, evite extensões do navegador que injetem scripts pesados sobre o `localhost`.
