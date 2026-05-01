# Estrategia de testes e regressao preventiva

Escopo: PoC ASP.NET Core MVC/Razor para integracao com a Access API Control iD.
Esta estrategia complementa `docs/product-acceptance-criteria.md` e deve ser usada
para vincular cada mudanca a requisito, criterio de aceite, risco e teste.

## Objetivos

- Proteger fluxos criticos de conexao, sessao, catalogo oficial, objetos, operacoes de alto impacto, monitor, callbacks, push, seguranca, privacidade e banco local.
- Priorizar testes deterministas que nao dependam de equipamento fisico, rede publica ou credenciais reais.
- Separar validacao automatizada local de homologacao manual com hardware real.
- Evitar mocks excessivos: usar SQLite em memoria para repositorios e `HttpMessageHandler` gravavel para contratos HTTP.

## Piramide aplicada ao projeto

| Nivel | Uso no repositorio | Evidencia atual |
| --- | --- | --- |
| Unitario | Helpers, factories, sanitizacao, seguranca, formatacao e resolucao de perfis | `tests/.../Helpers`, `tests/.../Services` |
| Controller/contrato leve | Fluxos MVC com sessao, TempData, validacao local e chamadas oficiais simuladas | `tests/.../Controllers` |
| Integracao local | Repositorios com SQLite em memoria e workflows de persistencia | `SqliteTestDatabase`, testes de `PushCommandRepository` e `MonitorEventRepository` |
| Frontend contract | Contratos Razor/CSS/JS sem dependencia de navegador externo | `Frontend/AccessibilityAndResponsiveContractTests.cs` |
| Smoke local | Aplicacao + stub local quando o fluxo exige processo ASP.NET completo | `tools/smoke-localhost.ps1` |
| Homologacao fisica | Equipamento real, firmware, rede e callbacks publicos | Runbooks em `docs/reports/` e scripts `tools/contract-controlid-device.ps1` |

## Rastreabilidade principal

| Fluxo | Risco protegido | Testes automatizados principais | Lacuna consciente |
| --- | --- | --- | --- |
| F01 conexao/login/sessao | Criar sessao invalida, chamar endpoint autenticado sem contexto, logout por navegacao cross-site | `AuthControllerTests`, `SessionControllerTests` | Smoke com equipamento real depende de ambiente |
| F02 catalogo/API oficial | Endpoint local ser invocado como outbound, resposta binaria ser tratada como texto, query sensivel vazar | `OfficialApiContractDocumentationServiceTests`, `OfficialApiBinaryFileResultFactoryTests`, `OfficialApiInvokerServiceTests` | Teste de contrato completo contra todas as respostas reais do fabricante |
| F03 objetos oficiais | JSON invalido ou confirmacao incorreta chamar `create/modify/destroy` remoto | `OfficialObjectsControllerTests`, `HighImpactOperationGuardTests` | E2E com equipamento real para confirmar efeito remoto |
| F04 operacoes administrativas | Reboot, reset, recovery, remocao de admins e rede executarem sem frase correta | `SystemControllerTests`, `HighImpactOperationGuardTests` | Nao executar acoes destrutivas em smoke automatico |
| F05 modos de operacao | Resolver perfil incorreto ou montar payload divergente | `OperationModesPayloadFactoryTests`, `OperationModesProfileResolverTests` | Controller/stub completo e homologacao com firmware real |
| F06 monitor/callbacks | Persistir payload nao autorizado, body grande ou shared key invalida | `CallbackSecurityEvaluatorTests`, `CallbackRequestBodyReaderTests`, `CallbackIngressServiceTests`, `OfficialEventsControllerTests` | URL publica e origem real do equipamento |
| F07 push | Duplicidade, fila apagada sem confirmacao, resultado sem idempotencia | `PushControllerTests`, `PushCenterControllerTests`, `PushCommandRepositoryTests`, `PushCommandWorkflowServiceTests`, `PushIdempotencyKeyResolverTests` | Homologacao com equipamento real e multiplos dispositivos em rede |
| F08 seguranca/privacidade/runtime | Exposicao fora de development, segredo em log, headers fracos, dados pessoais em payload | `SecurityHeadersMiddlewareTests`, `PrivacyLogHelperTests`, `SecurityTextHelperTests`, testes de callbacks/push | Ferramenta externa dedicada de DAST/a11y/security scan |
| F09 banco local/schema | Indices ausentes, schema parcial, purge indevido | `OperationalIndexMigrationTests`, testes de repositorio com SQLite | Restore com banco legado real so em ambiente controlado |

## Regras de qualidade dos testes

- Nomear testes por comportamento observavel, nao por detalhe interno.
- Usar dados ficticios e placeholders; nunca usar credenciais, fotos, biometricos ou payloads reais.
- Preferir asserts em status, payload, URL segura, persistencia e ausencia de chamadas indevidas.
- Quando a seguranca depende de nao chamar o equipamento, assertar que `RecordingHttpMessageHandler.Requests` ficou vazio.
- Para chamadas oficiais simuladas, validar metodo, path `.fcgi`, query de sessao ficticia e corpo JSON normalizado.
- Para dados, usar SQLite em memoria via `SqliteTestDatabase`.
- Para UI/JS/CSS, manter testes de contrato textual apenas para invariantes estaveis; validacao visual detalhada deve ser manual ou por ferramenta dedicada.

## Comandos oficiais

```powershell
dotnet build .\Integracao.ControlID.PoC.sln --no-restore -v:minimal
dotnet test .\Integracao.ControlID.PoC.sln --no-build -v:minimal
dotnet format .\Integracao.ControlID.PoC.sln --verify-no-changes --no-restore -v:minimal
git diff --check
```

Gate local completo para release readiness de testes:

```powershell
powershell -ExecutionPolicy Bypass -File .\tools\test-readiness-gates.ps1 -RunCoverage
```

Use flags adicionais conforme o ambiente permitir:

```powershell
powershell -ExecutionPolicy Bypass -File .\tools\test-readiness-gates.ps1 -RunCoverage -RunSupplyChainAudit -RunSmoke
powershell -ExecutionPolicy Bypass -File .\tools\test-readiness-gates.ps1 -RunContainerBuild
powershell -ExecutionPolicy Bypass -File .\tools\test-readiness-gates.ps1 -RunObservabilityOnline -RequireObservabilityMetrics
powershell -ExecutionPolicy Bypass -File .\tools\test-readiness-gates.ps1 -RequireHardwareContract -RequireExternalScanners
powershell -ExecutionPolicy Bypass -File .\tools\test-readiness-gates.ps1 -ReleaseGate
```

Smoke local quando a mudanca tocar callbacks, push, catalogo oficial, autenticacao ou banco:

```powershell
powershell -ExecutionPolicy Bypass -File .\tools\smoke-localhost.ps1
```

## Coverage

O repositorio usa o coletor `Code Coverage` disponivel via `Microsoft.NET.Test.Sdk`. O artefato e gerado fora do Git em `artifacts/test-readiness/coverage` pelo gate:

```powershell
powershell -ExecutionPolicy Bypass -File .\tools\test-readiness-gates.ps1 -RunCoverage
```

Threshold numerico ainda exige ferramenta de leitura/relatorio compativel com `.coverage`; caso seja necessario bloquear por percentual, adicionar essa ferramenta deve ser uma mudanca separada, justificada e validada no lockfile.

## Gates de validacao externa

- Homologacao com equipamento real, firmware e rede publica depende de ambiente e e bloqueada por `-RequireHardwareContract` quando exigida.
- Auditoria formal WCAG/DAST/SAST externa depende de ferramentas fora do repo e e bloqueada por `-RequireExternalScanners` quando exigida.
- Validacao online de metricas depende de app rodando e credencial local de administrador; use `-RunObservabilityOnline -RequireObservabilityMetrics`.
- Coverage numerico por percentual depende de parser/relatorio compativel; `-RunCoverage` bloqueia ausencia de artefato e qualquer threshold formal deve ser definido com ferramenta versionada antes de release regulado.
- Para release sem excecoes, `-ReleaseGate` agrega smoke, cobertura, supply chain, container build, observabilidade online, contrato fisico e scanners externos; se ambiente/ferramenta estiver ausente, o gate falha.
