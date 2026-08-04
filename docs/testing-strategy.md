# Estratégia de testes e regressão preventiva

> **Documento vivo** · Público: desenvolvimento e QA · Responsável: QA/SDET · Última validação: 2026-08-03.

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
dotnet test .\Integracao.ControlID.PoC.sln --no-build -v:minimal
dotnet format .\Integracao.ControlID.PoC.sln --verify-no-changes --no-restore -v:minimal
git diff --check
powershell -ExecutionPolicy Bypass -File .\tools\validate-documentation.ps1
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

O repositório usa o coletor `Code Coverage` disponível por meio de `Microsoft.NET.Test.Sdk`. O artefato é gerado fora do Git em `artifacts/test-readiness/coverage` pelo gate:

```powershell
powershell -ExecutionPolicy Bypass -File .\tools\test-readiness-gates.ps1 -RunCoverage
```

Um limite numérico ainda exige ferramenta de leitura ou relatório compatível com `.coverage`; caso seja necessário bloquear por percentual, a adição dessa ferramenta deve ser uma mudança separada, justificada e validada no arquivo de bloqueio.

## Critérios de validação externa

- Contrato simulado com stub local roda por padrão via `tools/contract-controlid-stub.ps1`.
- A homologação com equipamento real, firmware e rede pública depende do ambiente e é bloqueada por `-RequireHardwareContract` quando exigida.
- A auditoria formal externa de WCAG, DAST e SAST usa `tools/external-security-scans.ps1` e é bloqueada por `-RequireExternalScanners` quando exigida.
- A validação on-line de métricas depende da aplicação em execução e de uma credencial local de administrador; use `-RunObservabilityOnline -RequireObservabilityMetrics`.
- A cobertura numérica por percentual depende de um analisador ou relatório compatível; `-RunCoverage` bloqueia a ausência de artefato, e qualquer limite formal deve ser definido com uma ferramenta versionada antes de uma release regulada.
- Para uma release sem exceções, `-ReleaseGate` agrega smoke, cobertura, cadeia de suprimentos, construção do contêiner, observabilidade on-line, FinOps/capacidade em modo estrito, contrato físico e scanners externos; se o ambiente ou a ferramenta estiver ausente, ou se houver aviso de capacidade, o gate falha.

## Linha de base e comandos direcionados

Na linha de base documental de 2026-08-03, a solução possuía 209 testes aprovados
e 49 arquivos Markdown. A ampliação do primeiro contato elevou o inventário para
58 arquivos e atualizou o mesmo contrato automatizado de governança. A decisão
de runtime .NET 10 elevou o estado atual a 212 testes e 59 arquivos Markdown.
As contagens são referências, não metas fixas; novos comportamentos devem aumentar ou
substituir cobertura relevante sem remover cenários válidos.

```powershell
dotnet test .\Integracao.ControlID.PoC.sln --no-build --filter FullyQualifiedName~Controllers
dotnet test .\Integracao.ControlID.PoC.sln --no-build --filter FullyQualifiedName~Services.Callbacks
dotnet test .\Integracao.ControlID.PoC.sln --no-build --filter FullyQualifiedName~Services.Database
dotnet test .\Integracao.ControlID.PoC.sln --no-build --filter FullyQualifiedName~Frontend
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
- Cobertura percentual só vira gate após baseline e ferramenta de leitura
  versionada; até lá, cobertura de risco e rastreabilidade prevalecem.

## Evolução da cobertura

1. Versionar leitor compatível com o artefato `.coverage` sem atualização ampla
   de dependências.
2. Medir linha de base por linhas e ramificações, excluindo somente código gerado
   com justificativa.
3. Definir limite inicial que não incentive testes superficiais e impedir queda
   não justificada.
4. Adicionar navegação real de navegador para login, catálogo, erro, teclado e
   dois viewports, usando somente stub e dados fictícios.
5. Executar axe como evidência complementar; zero violações automáticas não
   substitui inspeção manual de foco, leitura e compreensão.

Até essas etapas, os 209 testes e a matriz de riscos são a linha de base
verificável, não uma garantia de cobertura completa.
