# Integração.ControlID.PoC

> **Guia** · Público: novos usuários, desenvolvimento e operação · Responsável: Engenharia · Última validação: 2026-08-12.

PoC web em ASP.NET Core 10 MVC/Razor para exploração operacional e técnica da
Access API da Control iD. A aplicação ajuda um time técnico a conectar um
equipamento, autenticar, navegar pelo catálogo oficial, testar fluxos de
hardware, receber callbacks, operar Push, persistir estado local em SQLite e
validar a prontidão antes de evoluir a integração.

Trate este repositório como uma PoC operacional: ele pode lidar com dados
pessoais, credenciais, sessões, fotos, biometria, cartões, QR Codes, logs de
acesso e cargas úteis de callbacks. Use dados fictícios em desenvolvimento e mantenha
segredos fora do Git.

## Estado, escopo e não objetivos

| Item | Estado atual |
| --- | --- |
| Maturidade | PoC operacional, adequada para desenvolvimento, demonstração e homologação controlada |
| Equipamentos | Contrato simulado pelo simulador local; compatibilidade física depende de modelo, firmware e licença |
| Persistência | SQLite local para uma instância; não representa arquitetura distribuída |
| Implantação | Contêiner reproduzível, sem provedor de produção escolhido |
| Privacidade | Controles técnicos presentes; decisões jurídicas e do controlador permanecem externas |

Não são objetivos desta PoC substituir o software oficial do fabricante, operar
controle de acesso crítico sem homologação, oferecer alta disponibilidade ou
declarar conformidade jurídica. O gate estrito impede tratar essas dependências
como concluídas sem evidência humana e ambiental.

## Comece aqui

Escolha o percurso mais curto para o seu objetivo:

1. Para entender produto e limites, consulte a
   [FAQ](docs/primeiros-passos/faq.md) e os
   [percursos por perfil](docs/primeiros-passos/persona-guides.md).
2. Para executar localmente, siga o
   [onboarding de desenvolvimento](docs/primeiros-passos/developer-onboarding.md).
3. Para navegar por todo o conhecimento, use a
   [central de documentação](docs/README.md).
4. Para contribuir, leia [CONTRIBUTING.md](CONTRIBUTING.md) e
   [AGENTS.md](AGENTS.md).

O primeiro fluxo funcional com o simulador está descrito abaixo; detalhes de
arquitetura, integração, segurança, testes e operação ficam nos respectivos
domínios da central.

## Tecnologias

| Área | Tecnologia |
| --- | --- |
| Linguagens | C#, Razor, HTML, CSS, JavaScript e PowerShell |
| Runtime/SDK | .NET 10 LTS, SDK `10.0.302` pinado em `global.json` |
| Web | ASP.NET Core MVC/Razor |
| Banco | SQLite com Entity Framework Core |
| Logs | Serilog em console e arquivo rolling |
| Observabilidade | Verificações de saúde, `/metrics` em texto Prometheus e `System.Diagnostics.Metrics` |
| Testes | xUnit, Playwright e axe |
| Teste integrado/contrato | PowerShell com simulador determinístico em `tools/ControlIdDeviceStub` |
| CI | GitHub Actions em `.github/workflows/ci.yml` |
| Container | `Dockerfile` e `docker-compose.yml` |
| Dependências | NuGet com `packages.lock.json` |

Não há gerenciador de pacotes do frontend configurado. `npm`, `pnpm` e `yarn` não fazem
parte do fluxo do projeto.

## Estrutura

| Caminho | Papel |
| --- | --- |
| `Program.cs` | Inicialização da aplicação, DI, middlewares, rotas, verificações de saúde, SQLite e validações de execução |
| `Controllers/` | Rotas MVC, callbacks, Push, catálogo oficial e fluxos operacionais |
| `Services/` | Integrações Control iD, regras reutilizáveis, repositórios, observabilidade, segurança e factories |
| `Data/` | `IntegracaoControlIDContext` e migrations EF Core |
| `Models/` | Modelos da API Control iD e entidades locais |
| `ViewModels/` | DTOs/ViewModels usados pelas views Razor |
| `Views/` | Telas Razor e parciais compartilhadas |
| `Middlewares/` | Correlation ID, tratamento de erro, headers, sessão e request logging |
| `Options/` | Opções de configuração tipadas |
| `tests/` | Testes xUnit |
| `tools/` | Scripts de teste integrado, prontidão, auditoria, cópia de segurança, analisadores e simuladores |
| `docs/` | Documentação técnica, guias operacionais, ADRs, relatórios e registros de alterações |
| `wwwroot/` | CSS/JS globais, assets e bibliotecas vendorizadas |

Mapa de módulos: [responsabilidades da solução](docs/arquitetura/project-file-responsibilities.md).

## Requisitos

- .NET SDK 10 compatível com `global.json`.
- Windows PowerShell 5+ ou PowerShell 7+.
- Git.
- Docker opcional para validar container.
- Ferramentas externas opcionais para release estrito: Semgrep, OSV Scanner,
  OWASP ZAP, axe e Docker.

## Configuração local

Restaure dependências a partir da raiz:

```powershell
dotnet restore .\Integracao.ControlID.PoC.sln --locked-mode
dotnet restore .\tools\ControlIdDeviceStub\ControlIdDeviceStub.csproj --locked-mode
dotnet restore .\tools\ControlIdCallbackSigningProxy\ControlIdCallbackSigningProxy.csproj --locked-mode
dotnet tool restore
```

Configure segredos fora do repositório. Para desenvolvimento local, prefira User
Secrets ou variáveis de ambiente:

```powershell
dotnet user-secrets set "ControlIDApi:DefaultDeviceUrl" "http://<equipamento-ou-host>:8080"
dotnet user-secrets set "ControlIDApi:DefaultUsername" "<usuario>"
dotnet user-secrets set "ControlIDApi:DefaultPassword" "<senha>"
dotnet user-secrets set "CallbackSecurity:SharedKey" "<segredo-local>"
dotnet user-secrets set "CallbackSecurity:RequireSharedKey" "true"
dotnet user-secrets set "CallbackSecurity:RequireSignedRequests" "true"
dotnet user-secrets set "ControlIDApi:RequireAllowedDeviceHosts" "true"
dotnet user-secrets set "ControlIDApi:AllowedDeviceHosts:0" "<equipamento-ou-host>"
```

Para equipamentos sem assinatura HMAC nativa, use o proxy assinador local:

```powershell
dotnet user-secrets set --project .\tools\ControlIdCallbackSigningProxy\ControlIdCallbackSigningProxy.csproj "Proxy:SharedKey" "<mesmo-segredo-da-poc>"
dotnet user-secrets set --project .\tools\ControlIdCallbackSigningProxy\ControlIdCallbackSigningProxy.csproj "Proxy:AllowedRemoteIps:0" "<ip-do-equipamento>"
dotnet run --project .\tools\ControlIdCallbackSigningProxy\ControlIdCallbackSigningProxy.csproj --urls http://localhost:6700
```

## Execução local

Aplicação principal:

```powershell
dotnet run --project .\Integracao.ControlID.PoC.csproj
```

Simulador local do equipamento:

```powershell
dotnet run --project .\tools\ControlIdDeviceStub\ControlIdDeviceStub.csproj --no-launch-profile
```

Teste integrado local com aplicação e simulador:

```powershell
powershell -ExecutionPolicy Bypass -File .\tools\smoke-localhost.ps1
```

O relatório mais recente é gravado em [artifacts/smoke/localhost-smoke-latest.md](artifacts/smoke/localhost-smoke-latest.md);
o script interrompe imediatamente se a compilação da aplicação ou do simulador falhar.

Em `Development`, a especificação OpenAPI fica disponível em
`/swagger/v1/swagger.json` e a UI em `/swagger` quando `OpenApi:Enabled=true`.

## Primeira experiência com o simulador

Este roteiro leva do clone ao primeiro fluxo autenticado sem equipamento físico:

1. Execute o simulador; ele escuta em `http://127.0.0.1:6600`.
2. Configure `ControlIDApi:DefaultDeviceUrl` com essa URL. O simulador aceita as
   credenciais fictícias `stub-admin` e `stub-password`.
3. Execute a aplicação e abra `http://localhost:5000` ou
   `https://localhost:5001`, conforme `Properties/launchSettings.json`.
4. No primeiro acesso, abra `/Auth/Register`, cadastre dados fictícios e uma
   senha de 12 a 128 caracteres. O primeiro usuário local recebe o papel
   `Administrator`; cadastros seguintes exigem um administrador autenticado.
5. Entre em `/Auth/LocalLogin`, conecte-se ao simulador em `/Auth/Login` e confirme a
   sessão em `/Auth/Status`.
6. Abra `OfficialApi`, consulte `system_information.fcgi` e confirme resposta de
   sucesso sem dados pessoais reais.
7. Abra `/Development/Simulator` para aplicar falhas, trocar o perfil de produto
   ou recriar uma massa sintética. A conexão passa a exibir a origem `Simulado`.

Resultado esperado: interface autenticada, sessão Control iD válida, prontidão
saudável e nenhuma credencial registrada em logs ou arquivos versionados. Para
uma validação automatizada equivalente, execute `tools/smoke-localhost.ps1`.

## Conta local e sessão Control iD

Sim, uma conta local é necessária para o acesso humano normal à interface. Ela
protege a própria PoC e atribui um dos papéis locais:

- `Administrator`: pode executar operações administrativas, escritas, ações
  físicas e consultas sensíveis;
- `Operator`: pode navegar, diagnosticar, conectar um equipamento e fazer login
  oficial, mas não pode executar as operações administrativas protegidas.

A conta local não substitui as credenciais do equipamento. Depois de entrar em
`/Auth/LocalLogin`, conecte o terminal e use `/Auth/Login` para criar uma segunda
sessão, emitida por `login.fcgi`. O primeiro cadastro local recebe
`Administrator`; os seguintes são criados por um administrador e recebem
`Operator`.

Não existe recuperação de senha sem a senha atual, promoção/desativação de conta,
SSO ou MFA. Consulte [docs/seguranca-privacidade/local-account-administration.md](docs/seguranca-privacidade/local-account-administration.md) para a matriz de
permissões e [docs/primeiros-passos/faq.md](docs/primeiros-passos/faq.md) para as perguntas de primeiro contato.

## Comandos oficiais

Compilação e testes:

```powershell
dotnet build .\Integracao.ControlID.PoC.sln --no-restore -v:minimal
dotnet test .\tests\Integracao.ControlID.PoC.Tests\Integracao.ControlID.PoC.Tests.csproj --no-build -v:minimal
pwsh .\tests\Integracao.ControlID.PoC.E2E\bin\Debug\net10.0\playwright.ps1 install chromium
dotnet test .\tests\Integracao.ControlID.PoC.E2E\Integracao.ControlID.PoC.E2E.csproj --no-build -v:minimal
```

Formatação, análise estática e verificação de tipos:

```powershell
dotnet format .\Integracao.ControlID.PoC.sln --verify-no-changes --no-restore -v:minimal
git diff --check
```

Observações:

- Não existe análise estática separada; `dotnet build` com avisos como erro e
  `dotnet format --verify-no-changes` são as verificações oficiais.
- Não existe verificação de tipos separada; essa verificação é feita pela própria compilação C#.
- Para corrigir formatação, use `dotnet format .\Integracao.ControlID.PoC.sln -v:minimal`
  e registre o efeito mecânico.

Auditorias e prontidão:

```powershell
powershell -ExecutionPolicy Bypass -File .\tools\validate-documentation.ps1
powershell -ExecutionPolicy Bypass -File .\tools\scan-secrets.ps1
powershell -ExecutionPolicy Bypass -File .\tools\audit-supply-chain.ps1
powershell -ExecutionPolicy Bypass -File .\tools\observability-check.ps1 -OfflineValidateOnly
powershell -ExecutionPolicy Bypass -File .\tools\operational-readiness-check.ps1
powershell -ExecutionPolicy Bypass -File .\tools\finops-capacity-check.ps1
powershell -ExecutionPolicy Bypass -File .\tools\contract-controlid-stub.ps1
powershell -ExecutionPolicy Bypass -File .\tools\maintainability-check.ps1
powershell -ExecutionPolicy Bypass -File .\tools\performance-baseline.ps1 -FailOnBudget
powershell -ExecutionPolicy Bypass -File .\tools\test-readiness-gates.ps1 -RunCoverage -RunPerformanceBaseline
powershell -ExecutionPolicy Bypass -File .\tools\test-readiness-gates.ps1
```

O validador documental padrão é determinístico e não usa rede. Em uma auditoria
conectada, acrescente `-CheckExternalUrls` para conferir também a disponibilidade
das referências externas, sem alterar o gate off-line da CI.

Critério estrito de liberação:

```powershell
powershell -ExecutionPolicy Bypass -File .\tools\test-readiness-gates.ps1 -ReleaseGate
```

`-ReleaseGate` exige teste integrado, cobertura, auditoria da cadeia de suprimentos,
construção do contêiner, observabilidade on-line, `ops.local.json` preenchido fora
do Git, FinOps/capacidade sem avisos, contrato com equipamento físico e analisadores externos. Se ambiente,
credencial ou ferramenta estiver ausente, o gate deve falhar.

## Variáveis de ambiente principais

Configuração segue o padrão nativo ASP.NET Core (`Secao__Chave`).

| Variável | Exemplo | Uso |
| --- | --- | --- |
| `ASPNETCORE_ENVIRONMENT` | `Development` | Ambiente de execução |
| `ASPNETCORE_URLS` | `https://localhost:5001` | URLs de binding da app |
| `ConnectionStrings__DefaultConnection` | `Data Source=integracao_controlid.db` | SQLite local |
| `Database__ApplyMigrationsOnStartup` | `false` | Aplica migrations apenas quando explicitamente habilitado; `Development` usa `true` |
| `Database__ExitAfterMigrations` | `false` | Encerra o processo após uma execução de migration controlada |
| `Database__Sqlite__BusyTimeoutMilliseconds` | `5000` | Espera por escrita concorrente antes de falhar |
| `Database__Sqlite__WriteAheadLoggingEnabled` | `true` | Solicita journal WAL no início |
| `AllowedHosts` | `poc.example.internal` | Hosts aceitos fora de `Development`; não use `*` |
| `DataProtection__KeyPath` | `/app/data/data-protection-keys` | Diretório persistente das chaves de cookies e antiforgery |
| `ControlIDApi__DefaultDeviceUrl` | `http://<equipamento-ou-host>:8080` | Equipamento Control iD |
| `ControlIDApi__ConnectionTimeoutSeconds` | `30` | Tempo limite das chamadas oficiais; normalizado entre 5 e 300 segundos |
| `ControlIDApi__MaxResponseBodyBytes` | `16777216` | Limite de resposta da API externa; normalizado entre 64 KiB e 64 MiB |
| `ControlIDApi__MaxStreamingResponseBytes` | `268435456` | Limite para download binário transferido em fluxo |
| `ControlIDApi__Concurrency__MaxConcurrentRequestsPerDevice` | `4` | Operações simultâneas por equipamento |
| `ControlIDApi__Concurrency__QueueLimitPerDevice` | `16` | Fila máxima por equipamento |
| `ControlIDApi__RequireAllowedDeviceHosts` | `true` | Exige lista de permissões de saída |
| `ControlIDApi__AllowedDeviceHosts__0` | `<equipamento-ou-host>` | Primeiro host permitido do equipamento |
| `CallbackSecurity__MaxBodyBytes` | `1048576` | Limite do corpo para callbacks/monitor |
| `CallbackSecurity__RequireSharedKey` | `true` | Exige chave compartilhada em ingressos externos |
| `CallbackSecurity__SharedKey` | `<segredo>` | Segredo fora do Git |
| `CallbackSecurity__RequireSignedRequests` | `true` | Exige assinatura HMAC com timestamp e nonce |
| `CallbackSecurity__MaxTrackedNonces` | `10000` | Limite em memória da proteção contra replay |
| `CallbackSecurity__AllowedRemoteIps__0` | `192.168.0.10` | Primeiro IP permitido para callbacks |
| `OpenApi__Enabled` | `false` | Swagger/OpenAPI fora de Development apenas com decisão explícita |
| `Observability__CapacitySnapshotIntervalSeconds` | `30` | Intervalo, entre 10 e 300 segundos, da coleta local de capacidade em segundo plano |
| `Observability__Metrics__Enabled` | `true` | Habilita `/metrics` |
| `Observability__Metrics__AllowAnonymous` | `false` | Deve ser `false` fora de Development |
| `Serilog__WriteTo__1__Args__retainedFileCountLimit` | `14` | Retenção de arquivos rolling |
| `Serilog__WriteTo__1__Args__fileSizeLimitBytes` | `10000000` | Limite por arquivo de log |
| `ForwardedHeaders__Enabled` | `false` | Suporte a proxy reverso confiável |
| `ForwardedHeaders__KnownProxies__0` | `10.0.0.10` | Proxy/load balancer confiável |

Exemplo completo seguro: `.env.example`.

## Banco e estado local

- SQLite padrão: `integracao_controlid.db`.
- Arquivos `integracao_controlid.db*`, `Logs/`, `artifacts/`, `bin/` e `obj/`
  não devem ser versionados.
- Fora de `Development`, migrations não são aplicadas no startup sem
  `Database__ApplyMigrationsOnStartup=true`.
- `/health/ready` permanece unhealthy enquanto houver migration pendente.
- Dados locais podem conter informação pessoal ou sensível.

Comandos seguros:

```powershell
powershell -ExecutionPolicy Bypass -File .\tools\backup-sqlite.ps1
powershell -ExecutionPolicy Bypass -File .\tools\backup-sqlite-operational.ps1 -RunRestoreSmoke
powershell -ExecutionPolicy Bypass -File .\tools\restore-smoke-sqlite.ps1
powershell -ExecutionPolicy Bypass -File .\tools\harden-local-state.ps1
```

Detalhes: [docs/dados/data-model-and-recovery.md](docs/dados/data-model-and-recovery.md) e
[docs/dados/database-and-runtime-state.md](docs/dados/database-and-runtime-state.md).

## Observabilidade e operação

Endpoints operacionais:

| Endpoint | Finalidade | Exposição recomendada |
| --- | --- | --- |
| `GET /health/live` | Liveness do processo ASP.NET Core | Supervisor/load balancer |
| `GET /health/ready` | Prontidão do SQLite local | Verificação antes de receber tráfego |
| `GET /metrics` | Métricas Prometheus text | Administrador por padrão |

Sinais disponíveis:

- Correlation ID por request via `X-Correlation-ID`.
- Logs Serilog com dados sensíveis mascarados ou pseudonimizados.
- Métricas HTTP, Access API, callbacks, Push, auth local, analytics de produto e
  capacidade runtime/FinOps.
- Alertas e painéis versionados em `docs/observability/`.
- Guias operacionais em [docs/operacao/observability-runbook.md](docs/operacao/observability-runbook.md),
  [docs/operacao/incident-response-and-dr.md](docs/operacao/incident-response-and-dr.md) e
  [docs/operacao/equipment-contingency-runbook.md](docs/operacao/equipment-contingency-runbook.md).

## Contêiner e implantação

Artefatos versionados:

- `Dockerfile`: multi-stage .NET 10, runtime Alpine, usuário não root, porta 8080
  e healthcheck em `/health/ready`.
- `.dockerignore`: remove Git, logs, artefatos, SQLite local e `.env` do contexto.
- `docker-compose.yml`: volumes persistentes para `/app/data` e `/app/Logs`.

Comandos:

```powershell
docker build -t integracao-controlid-poc:local .
docker compose config
docker compose run --rm -e Database__ApplyMigrationsOnStartup=true -e Database__ExitAfterMigrations=true integracao-controlid-poc
docker compose up --build
```

Não há provedor cloud versionado. Qualquer Render, Azure, AWS, GCP, Fly.io, VPS
ou Kubernetes exige decisão humana, segredos fora do Git e atualização da
documentação operacional.

## Fluxos principais

- `Home`: painel inicial.
- `Workspace`: mapa funcional por domínio.
- `Auth`/`Session`: login local, login no equipamento, status e logout.
- `OfficialApi`: catálogo oficial e invocação assistida.
- `OfficialObjects`: exploração/CRUD técnico de objetos oficiais.
- `OperationModes`: Standalone, Pro e Enterprise.
- `ProductSpecific`: recursos por linha de equipamento.
- `AdvancedOfficial`: câmera, exportação, intertravamento e recursos avançados.
- `OfficialEvents`/`Monitor`: callbacks, monitoramento e eventos oficiais.
- `PushCenter`: fila Push, consultas periódicas e resultados.
- `Privacy`: relatório minimizado de atendimento a titular.

## Contrato com equipamento real

Opt-in, fora da CI e sem credenciais reais no Git:

```powershell
$env:CONTROLID_DEVICE_URL = "http://<equipamento-ou-host>:8080"
$env:CONTROLID_USERNAME = "<usuario>"
$env:CONTROLID_PASSWORD = "<senha>"
powershell -ExecutionPolicy Bypass -File .\tools\contract-controlid-device.ps1
```

Use `tools/contract-controlid-stub.ps1` para validar o contrato sem hardware.

## Documentação principal

A [central de documentação](docs/README.md) oferece percursos por objetivo,
fontes canônicas e índices por domínio:

- [primeiros passos](docs/primeiros-passos/README.md);
- [produto](docs/produto/README.md);
- [arquitetura](docs/arquitetura/README.md) e [ADRs](docs/adrs/README.md);
- [integração Control iD](docs/integracao-controlid/README.md);
- [dados](docs/dados/README.md);
- [segurança e privacidade](docs/seguranca-privacidade/README.md);
- [qualidade](docs/qualidade/README.md);
- [operação](docs/operacao/README.md);
- [histórico](docs/historico/README.md).

Use [SUPPORT.md](SUPPORT.md) para solicitar ajuda e [SECURITY.md](SECURITY.md)
para relatar vulnerabilidades sem expor detalhes sensíveis.

## Diagnóstico rápido

### A PoC não conecta ao equipamento

- Confira esquema, IP e porta no painel de conexão.
- Valide `ControlIDApi__ConnectionTimeoutSeconds`.
- Confira a lista de permissões `ControlIDApi__AllowedDeviceHosts`.
- Veja os registros do `OfficialApiInvokerService` para tempo limite, status e destino
  pseudonimizado.

### Callbacks não aparecem

- Confira `CallbackSecurity__RequireSharedKey` e `CallbackSecurity__SharedKey`.
- Valide assinatura HMAC quando `RequireSignedRequests=true`.
- Valide IP remoto permitido.
- Acompanhe logs de `CallbackIngressService`.

### Push não entrega comandos

- Confira se o equipamento consulta `GET /push`.
- Valide se resultados chegam em `POST /result`.
- Consulte `PushCenter` e logs de persistência.

### `/metrics` não responde

- Confirme `Observability__Metrics__Enabled=true`.
- Por padrão, autentique como administrador.
- Fora de Development, `Observability__Metrics__AllowAnonymous=true` bloqueia startup.

### O shell parece lento

- Verifique se os ativos estáticos e a compressão estão funcionando.
- Use `OfficialApi` como referência para carga do catálogo.
- Valide tamanho de banco/logs com `tools/finops-capacity-check.ps1`.

## Mapa resumido do primeiro uso

```mermaid
flowchart LR
    Clone["Clonar e restaurar"] --> Stub["Iniciar o simulador fictício"]
    Stub --> LocalUser["Cadastrar usuário local fictício"]
    LocalUser --> DeviceLogin["Autenticar no simulador"]
    DeviceLogin --> OfficialApi["Consultar system_information.fcgi"]
    OfficialApi --> Gates["Executar verificações locais"]
```

O fluxo completo leva, em uma máquina já preparada, aproximadamente 10 a 20
minutos. O primeiro resultado confiável é a consulta ao simulador seguida da verificação
local aprovado; uma tela carregada isoladamente não comprova a integração.

| Ambiente | Cobertura documentada |
| --- | --- |
| Windows + Windows PowerShell 5.1 | Caminho principal e scripts operacionais |
| Windows + PowerShell 7 | Compatível com os comandos PowerShell documentados |
| Linux/macOS + PowerShell 7 | Compilação e aplicação são portáveis; scripts que usam DPAPI ou ACL do Windows exigem alternativa registrada |
| Contêiner Linux | Execução reproduzível da aplicação; operação real ainda depende de volumes, segredos e proxy configurados |

## Referências visuais

As capturas abaixo foram produzidas em `Development`, com banco efêmero e dados
fictícios. Elas ajudam a reconhecer a interface; testes e contratos continuam
sendo a evidência funcional.

![Tela de login local da PoC sem dados preenchidos](wwwroot/img/docs/local-login.png)

![Painel inicial autenticado com usuário fictício e sem equipamento conectado](wwwroot/img/docs/authenticated-home.png)

![Catálogo oficial da API com contagens e filtros visíveis](wwwroot/img/docs/official-api.png)
