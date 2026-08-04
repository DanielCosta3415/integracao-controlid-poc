# Estratégia de testes e regressão preventiva

> **Documento vivo** · Público: desenvolvimento e QA · Responsável: QA/SDET · Última validação: 2026-08-04.

Escopo: PoC ASP.NET Core MVC/Razor para integração com a Access API Control iD.
Esta estratégia complementa `docs/product-acceptance-criteria.md` e deve ser usada
para vincular cada mudança a requisito, critério de aceite, risco e teste.

## Objetivos

- Proteger fluxos críticos de conexão, sessão, catálogo oficial, objetos, operações de alto impacto, monitor, callbacks, push, segurança, privacidade e banco local.
- Priorizar testes deterministas que não dependam de equipamento físico, rede pública ou credenciais reais.
- Separar validação automatizada local de homologação manual com hardware real.
- Evitar mocks excessivos: usar SQLite em memória para repositórios e `HttpMessageHandler` gravável para contratos HTTP.

## Pirâmide aplicada ao projeto

| Nível | Uso no repositório | Evidência atual |
| --- | --- | --- |
| Unitário | Helpers, factories, sanitização, segurança, formatação e resolução de perfis | `tests/.../Helpers`, `tests/.../Services` |
| Controller/contrato leve | Fluxos MVC com sessão, TempData, validação local e chamadas oficiais simuladas | `tests/.../Controllers` |
| Integração local | Repositórios com SQLite em memória e workflows de persistência | `SqliteTestDatabase`, testes de `PushCommandRepository` e `MonitorEventRepository` |
| Frontend contract | Contratos Razor/CSS/JS sem dependência de navegador externo | `Frontend/AccessibilityAndResponsiveContractTests.cs` |
| E2E de navegador | Login local, sessão do stub, axe, responsividade, teclado e regressão visual | `tests/Integracao.ControlID.PoC.E2E` |
| Smoke local | Aplicação + stub local quando o fluxo exige processo ASP.NET completo | `tools/smoke-localhost.ps1` |
| Homologação física | Equipamento real, firmware, rede e callbacks públicos | Runbooks em `docs/reports/` e scripts `tools/contract-controlid-device.ps1` |

## Rastreabilidade principal

| Fluxo | Risco protegido | Testes automatizados principais | Lacuna consciente |
| --- | --- | --- | --- |
| F01 conexão/login/sessão | Criar sessão inválida, chamar endpoint autenticado sem contexto, logout por navegação cross-site | `AuthControllerTests`, `SessionControllerTests` | Smoke com equipamento real depende de ambiente |
| F02 catálogo/API oficial | Endpoint local ser invocado como outbound, resposta binária ser tratada como texto/Base64 intermediário, query sensível vazar ou paginação carregar volume sem limite | `OfficialApiContractDocumentationServiceTests`, `OfficialApiBinaryFileResultFactoryTests`, `OfficialApiInvokerServiceTests`, `OfficialObjectPagingTests` | Teste de contrato completo contra todas as respostas reais do fabricante |
| F03 objetos oficiais | JSON inválido ou confirmação incorreta chamar `create/modify/destroy` remoto | `OfficialObjectsControllerTests`, `HighImpactOperationGuardTests` | E2E com equipamento real para confirmar efeito remoto |
| F04 operações administrativas | Reboot, reset, recovery, remoção de admins e rede executarem sem frase correta | `SystemControllerTests`, `HighImpactOperationGuardTests` | Não executar ações destrutivas em smoke automático |
| F05 modos de operação | Resolver perfil incorreto ou montar payload divergente | `OperationModesPayloadFactoryTests`, `OperationModesProfileResolverTests` | Controller/stub completo e homologação com firmware real |
| F06 monitor/callbacks | Persistir payload não autorizado, corpo grande ou chave compartilhada inválida | `CallbackSecurityEvaluatorTests`, `CallbackRequestBodyReaderTests`, `CallbackIngressServiceTests`, `OfficialEventsControllerTests` | URL pública e origem real do equipamento |
| F07 push | Duplicidade, fila apagada sem confirmação, resultado sem idempotência | `PushControllerTests`, `PushCenterControllerTests`, `PushCommandRepositoryTests`, `PushCommandWorkflowServiceTests`, `PushIdempotencyKeyResolverTests` | Homologação com equipamento real e múltiplos dispositivos em rede |
| F08 segurança/privacidade/runtime | Exposição fora de development, segredo em log, headers fracos, dados pessoais em payload | `SecurityHeadersMiddlewareTests`, `PrivacyLogHelperTests`, `SecurityTextHelperTests`, testes de callbacks/push | Ferramenta externa dedicada de DAST/a11y/security scan |
| F09 banco local/schema | Índices ausentes, schema parcial, purge indevido | `OperationalIndexMigrationTests`, testes de repositório com SQLite | Restore com banco legado real só em ambiente controlado |

## Regras de qualidade dos testes

- Nomear testes por comportamento observável, não por detalhe interno.
- Usar dados fictícios e placeholders; nunca usar credenciais, fotos, biométricos ou payloads reais.
- Preferir asserts em status, payload, URL segura, persistência e ausência de chamadas indevidas.
- Quando a segurança depende de não chamar o equipamento, assertar que `RecordingHttpMessageHandler.Requests` ficou vazio.
- Para chamadas oficiais simuladas, validar método, caminho `.fcgi`, consulta de sessão fictícia e corpo JSON normalizado.
- Para dados, usar SQLite em memória via `SqliteTestDatabase`.
- Para UI/JS/CSS, manter testes de contrato textual apenas para invariantes estáveis; validação visual detalhada deve ser manual ou por ferramenta dedicada.

## Comandos oficiais

```powershell
dotnet build .\Integracao.ControlID.PoC.sln --no-restore -v:minimal
pwsh .\tests\Integracao.ControlID.PoC.E2E\bin\Debug\net10.0\playwright.ps1 install chromium
dotnet test .\tests\Integracao.ControlID.PoC.Tests\Integracao.ControlID.PoC.Tests.csproj --no-build -v:minimal
dotnet test .\tests\Integracao.ControlID.PoC.E2E\Integracao.ControlID.PoC.E2E.csproj --no-build -v:minimal
dotnet format .\Integracao.ControlID.PoC.sln --verify-no-changes --no-restore -v:minimal
git diff --check
powershell -ExecutionPolicy Bypass -File .\tools\validate-documentation.ps1
powershell -ExecutionPolicy Bypass -File .\tools\maintainability-check.ps1
powershell -ExecutionPolicy Bypass -File .\tools\performance-baseline.ps1 -FailOnBudget
```

Critério local completo para prontidão de liberação dos testes:

```powershell
powershell -ExecutionPolicy Bypass -File .\tools\test-readiness-gates.ps1 -RunCoverage
```

Use flags adicionais conforme o ambiente permitir:

```powershell
powershell -ExecutionPolicy Bypass -File .\tools\test-readiness-gates.ps1 -RunCoverage -RunSupplyChainAudit -RunSmoke
powershell -ExecutionPolicy Bypass -File .\tools\test-readiness-gates.ps1 -RunContainerBuild
powershell -ExecutionPolicy Bypass -File .\tools\test-readiness-gates.ps1 -RunExternalScanners
powershell -ExecutionPolicy Bypass -File .\tools\test-readiness-gates.ps1 -RunObservabilityOnline -RequireObservabilityMetrics
powershell -ExecutionPolicy Bypass -File .\tools\test-readiness-gates.ps1 -RequireFinOpsCapacity -RequireHardwareContract -RequireExternalScanners
powershell -ExecutionPolicy Bypass -File .\tools\test-readiness-gates.ps1 -ReleaseGate
```

Smoke local quando a mudança tocar callbacks, push, catálogo oficial, autenticação ou banco:

```powershell
powershell -ExecutionPolicy Bypass -File .\tools\smoke-localhost.ps1
```

## Cobertura

O projeto unitário usa `coverlet.collector` e gera Cobertura XML em
`artifacts/test-readiness/coverage`. `tools/validate-coverage.ps1` exige no
mínimo 28% de linhas e 16% de ramificações, pisos definidos a partir da medição
real de 28,07% e 16,03% em 2026-08-04.

```powershell
powershell -ExecutionPolicy Bypass -File .\tools\test-readiness-gates.ps1 -RunCoverage
```

Redução abaixo dos pisos falha localmente e na CI. Aumente os limites somente
após medir uma linha de base estável; não exclua código apenas para elevar o
percentual.

## Critérios de validação externa

- Contrato simulado com stub local roda por padrão via `tools/contract-controlid-stub.ps1`.
- A homologação com equipamento real, firmware e rede pública depende do ambiente e é bloqueada por `-RequireHardwareContract` quando exigida.
- A auditoria formal externa de WCAG, DAST e SAST usa `tools/external-security-scans.ps1` e é bloqueada por `-RequireExternalScanners` quando exigida.
- A validação on-line de métricas depende da aplicação em execução e de uma credencial local de administrador; use `-RunObservabilityOnline -RequireObservabilityMetrics`.
- A cobertura numérica é bloqueada por `coverlet.collector` e
  `tools/validate-coverage.ps1`.
- Para uma release sem exceções, `-ReleaseGate` agrega smoke, cobertura, cadeia de suprimentos, construção do contêiner, observabilidade on-line, FinOps/capacidade em modo estrito, contrato físico e scanners externos; se o ambiente ou a ferramenta estiver ausente, ou se houver aviso de capacidade, o gate falha.

## Linha de base e comandos direcionados

Na linha de base de 2026-08-04, o projeto unitário possui 242 testes e o E2E
possui uma jornada agregada que audita nove telas desktop e duas telas móveis.
O inventário documental possui 65 arquivos Markdown. As contagens são
referências, não metas: um teste novo precisa proteger comportamento ou risco.

```powershell
dotnet test .\tests\Integracao.ControlID.PoC.Tests\Integracao.ControlID.PoC.Tests.csproj --no-build --filter FullyQualifiedName~Controllers
dotnet test .\tests\Integracao.ControlID.PoC.Tests\Integracao.ControlID.PoC.Tests.csproj --no-build --filter FullyQualifiedName~Services.Callbacks
dotnet test .\tests\Integracao.ControlID.PoC.Tests\Integracao.ControlID.PoC.Tests.csproj --no-build --filter FullyQualifiedName~Services.Database
dotnet test .\tests\Integracao.ControlID.PoC.Tests\Integracao.ControlID.PoC.Tests.csproj --no-build --filter FullyQualifiedName~Frontend
```

| Camada | Objetivo | Critério de expansão |
| --- | --- | --- |
| Unitária/serviço | Regras, parsing, limites e estados | Toda nova regra ou bug reproduzível |
| Controller/componente | Binding, autorização, mensagens e resposta | Toda rota ou mudança de UX |
| Integração SQLite | Migração, transação, concorrência e falha | Toda alteração de schema/repositório |
| Contrato/stub | Compatibilidade HTTP sem hardware | Toda mudança de endpoint/payload |
| Smoke/E2E | Jornada integrada e configuração | Fluxo crítico, release e regressão ampla |

## Estabilidade dos testes

- Teste instável deve ter issue, responsável e prazo; não use repetição cega como cura.
- Quarentena ou `Skip` exige justificativa, risco protegido e condição de saída.
- Fixtures não usam relógio, rede ou banco compartilhado sem controle explícito.
- Falha deve preservar artefato útil sem segredo, cookie, biometria ou payload real.
- Cobertura percentual complementa, mas não substitui, cobertura de risco e
  rastreabilidade.

## Evolução da cobertura

1. Elevar gradualmente os pisos de linhas e ramificações com testes de risco.
2. Expandir o E2E somente para jornadas distintas, evitando uma suíte lenta e
   duplicada.
3. Manter referências visuais em desktop e mobile para superfícies críticas.
4. Executar axe como evidência complementar; zero violações automáticas não
   substitui inspeção manual de foco, leitura e compreensão.
