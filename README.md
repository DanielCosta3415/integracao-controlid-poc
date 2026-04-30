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

2. Configure segredos e dados sensíveis fora do repositório. Para desenvolvimento local, prefira variáveis de ambiente ou User Secrets:

```powershell
dotnet user-secrets set "ControlIDApi:DefaultDeviceUrl" "http://<equipamento-ou-host>:8080"
dotnet user-secrets set "ControlIDApi:DefaultUsername" "<usuário>"
dotnet user-secrets set "ControlIDApi:DefaultPassword" "<senha>"
dotnet user-secrets set "CallbackSecurity:SharedKey" "<segredo-local>"
```

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
- em `Development`, a especificação OpenAPI fica disponível em `/swagger/v1/swagger.json` e a UI em `/swagger`.

## Variáveis de ambiente úteis

A configuração segue o padrão nativo do ASP.NET Core (`Secao__Chave`). Use as variáveis abaixo para preparar execução local, integração com equipamento real e observabilidade.

| Variável | Exemplo | Uso |
| --- | --- | --- |
| `ASPNETCORE_ENVIRONMENT` | `Development` | Define o ambiente de execução. |
| `ASPNETCORE_URLS` | `https://localhost:5001` | Define a URL pública/local usada pela aplicação. |
| `ConnectionStrings__DefaultConnection` | `Data Source=integracao_controlid.db` | Caminho do SQLite local. |
| `ControlIDApi__DefaultDeviceUrl` | `http://<equipamento-ou-host>:8080` | Endereço padrão do equipamento Control iD. |
| `ControlIDApi__DefaultUsername` | `<usuário>` | Usuário sugerido para autenticação local. |
| `ControlIDApi__DefaultPassword` | `<senha>` | Senha sugerida para autenticação local. Não versione este valor. |
| `ControlIDApi__ConnectionTimeoutSeconds` | `30` | Timeout das chamadas oficiais. A aplicação normaliza o valor entre 5 e 300 segundos. |
| `ControlIDApi__CircuitBreaker__Enabled` | `true` | Habilita proteção contra falhas transitórias repetidas no equipamento. |
| `ControlIDApi__CircuitBreaker__FailureThreshold` | `5` | Quantidade de falhas transitórias consecutivas antes de abrir o circuito por endpoint/equipamento. |
| `ControlIDApi__CircuitBreaker__BreakDurationSeconds` | `30` | Tempo de bloqueio temporário após abertura do circuito. |
| `OpenApi__Enabled` | `false` | Habilita Swagger/OpenAPI fora de Development quando explicitamente configurado. |
| `Session__IdleTimeout` | `30` | Tempo de expiração da sessão ASP.NET Core em minutos. |
| `Session__CookieName` | `.IntegracaoControlID.Session` | Nome do cookie de sessão. |
| `CallbackSecurity__MaxBodyBytes` | `1048576` | Limite máximo de payload aceito em callbacks/monitor. |
| `CallbackSecurity__RequireSharedKey` | `true` | Exige chave compartilhada para entrada de callbacks. |
| `CallbackSecurity__SharedKeyHeaderName` | `X-ControlID-Callback-Key` | Header esperado para a chave compartilhada. |
| `CallbackSecurity__SharedKey` | `<segredo>` | Segredo usado para validar callbacks. Deve ficar fora do repositório. |
| `CallbackSecurity__AllowLoopback` | `true` | Permite callbacks locais mesmo com restrição de IP. |
| `CallbackSecurity__AllowedRemoteIps__0` | `192.168.0.10` | Primeiro IP remoto permitido para callbacks. Use índices adicionais para mais IPs. |
| `Logging__LogLevel__Default` | `Information` | Nível mínimo do logging padrão. |
| `Serilog__MinimumLevel__Default` | `Information` | Nível mínimo do Serilog. |
| `Logging__File__Path` | `Logs/app_log.txt` | Caminho esperado para logs em arquivo. |
| `Logging__File__RetainedFileCountLimit` | `14` | Quantidade de arquivos de log mantidos. |

Observações:

- nunca versione credenciais reais;
- prefira User Secrets ou variáveis de ambiente para valores sensíveis;
- `ControlIDApi__ConnectionTimeoutSeconds` controla o timeout real das chamadas oficiais;
- `CallbackSecurity__SharedKey` é obrigatória quando `CallbackSecurity__RequireSharedKey=true` fora de ambiente controlado.
- para validar callbacks reais, a URL da PoC precisa estar acessível pelo equipamento Control iD.

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

### Auditoria de secrets

```powershell
powershell -ExecutionPolicy Bypass -File .\tools\scan-secrets.ps1
```

### Smoke test funcional local

O smoke test sobe o stub local, percorre os happy paths e os edge cases críticos e gera um relatório em `docs/reports/`.

```powershell
powershell -ExecutionPolicy Bypass -File .\tools\smoke-localhost.ps1
```

Relatório atual de referência:

- `docs/reports/localhost-smoke-test-2026-04-14.md`

### Contrato com equipamento real

Este check é opt-in, não roda na CI e exige credenciais fora do Git:

```powershell
$env:CONTROLID_DEVICE_URL = "http://<equipamento-ou-host>:8080"
$env:CONTROLID_USERNAME = "<usuario>"
$env:CONTROLID_PASSWORD = "<senha>"
powershell -ExecutionPolicy Bypass -File .\tools\contract-controlid-device.ps1
```

## Observabilidade e monitoramento

A PoC já sai preparada para monitoramento básico local:

- log de requisições HTTP no middleware `RequestLoggingMiddleware`;
- log estruturado de invocação oficial da Access API, incluindo endpoint, target, status e duração;
- log de entrada de callbacks com aceite, bloqueio e falha de persistência;
- log de modos de operação para aplicação de `Standalone`, `Pro`, `Enterprise`, upgrades e validação de sessão;
- log de fila push para enfileiramento, entrega, polling e recebimento de resultados;
- compressão de resposta habilitada para HTML, CSS, JS e JSON, reduzindo custo de rede em páginas e payloads técnicos.

Saídas de log:

- console da aplicação;
- arquivo rolling em `Logs/app_log.txt`.

Pontos recomendados para monitorar primeiro:

- falhas de `OfficialApiInvokerService` por timeout, validação, endpoint inválido ou status HTTP inesperado;
- eventos rejeitados em `CallbackIngressService`;
- erros de persistência e entrega em `PushCenterController`;
- falhas ao aplicar modos em `OperationModesController`;
- alertas de segurança de `CallbackSecurity.RequireSharedKey` durante o startup;
- exceções capturadas por `ExceptionHandlingMiddleware`.

Sinais práticos para alertas locais ou ferramentas externas:

| Sinal | Onde aparece | Ação sugerida |
| --- | --- | --- |
| Timeout em chamada oficial | `OfficialApiInvokerService` | Verificar rede, IP, porta, sessão e disponibilidade do equipamento. |
| Callback bloqueado | `CallbackIngressService` ou `CallbackSecurityEvaluator` | Conferir chave compartilhada, IP permitido e tamanho do payload. |
| Falha ao persistir Push | `PushCenterController` ou `PushCommandRepository` | Conferir SQLite, lock de arquivo e permissão de escrita. |
| Resultado Push sem `command_id` | `PushCenterController` | Verificar se o equipamento está devolvendo o identificador correto. |
| Modo não detectado | `OperationModesController` | Conferir resposta de `get-configuration` e sessão oficial. |

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
- `docs/changelog-2026-04-15.md`: changelog das atualizações de documentação, comentários e observabilidade

- `docs/database-and-runtime-state.md`: estado local, comandos seguros e requisitos de runtime
- `docs/integration-contracts.md`: inventario de integracoes, contratos, payloads e riscos
- `docs/privacy-and-data-retention.md`: regras de privacidade, dados sensíveis e retenção local
- `docs/product-acceptance-criteria.md`: critérios de aceite funcionais para os fluxos críticos

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
