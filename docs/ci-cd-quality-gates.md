# CI/CD e critérios de qualidade

Este documento descreve a automação versionada para impedir regressão antes de
merge ou publicação. Ele complementa `AGENTS.md`, `docs/testing-strategy.md`,
`docs/deployment-runbook.md` e `docs/residual-risk-closure.md`.

## Provedor detectado

| Item | Estado |
| --- | --- |
| Provedor de repositório | GitHub (`origin` aponta para GitHub). |
| CI | GitHub Actions em `.github/workflows/ci.yml`. |
| CD/deploy automático | Não existe e não deve ser criado sem autorização humana. |
| Container | `Dockerfile` e `docker-compose.yml`, validados pela CI. |
| Scripts internos | `tools/*.ps1`, especialmente `test-readiness-gates.ps1`. |
| Makefile/Jenkins/GitLab/Azure/Bitbucket | Não detectados. |

## Fluxos de trabalho

### `CI`

Arquivo: `.github/workflows/ci.yml`

Disparos:

- `push` em `main`;
- `pull_request`;
- agenda semanal, segunda-feira;
- disparo manual por `workflow_dispatch`.

O Dependabot em `.github/dependabot.yml` abre PRs semanais para NuGet e GitHub
Actions, agrupando apenas atualizações patch/minor compatíveis para revisão humana.

Permissões:

- `contents: read` apenas.

Jobs:

| Job | Runner | Objetivo |
| --- | --- | --- |
| `build-test-audit` | `windows-latest` | Restore locked, build, testes, smoke, format, whitespace, secrets, readiness, auditorias e artefatos. |
| `container-build` | `ubuntu-latest` | Validar `docker compose config` com placeholders seguros e build do Dockerfile. |

Não há job de deploy, release, publicação, tag ou push de imagem.

## Critérios de qualidade obrigatórios em PR e `main`

| Gate | Comando/step | Falha quando |
| --- | --- | --- |
| Checkout reprodutível | `actions/checkout@v4` | Repositório não pode ser lido. |
| SDK pinado | `actions/setup-dotnet@v4` com `global.json` | SDK .NET correto não resolve. |
| Cache seguro | `cache: true` usando `packages.lock.json` | Lockfiles mudam sem restore consistente. |
| Restauração bloqueada | `dotnet restore ... --locked-mode` | O arquivo de bloqueio está ausente ou desatualizado. |
| Build/typecheck | `dotnet build ... --no-restore` | Erro de compilação ou warning tratado como erro. |
| Testes | `dotnet test ... --no-build` | Qualquer teste xUnit falha. |
| Smoke local | `tools/smoke-localhost.ps1` | App/stub/fluxos locais não respondem. |
| Contrato simulado Control iD | `tools/contract-controlid-stub.ps1` | Stub não cumpre login, sessão ou system information. |
| Format/lint | `dotnet format --verify-no-changes` | Código fora do padrão. |
| Whitespace | `git diff --check` | Espaço em branco inválido ou conflito de whitespace. |
| Secrets | `tools/scan-secrets.ps1` | Secret de alta confiança encontrado. |
| Observabilidade | `tools/observability-check.ps1 -OfflineValidateOnly` | Alertas/dashboards/métricas documentadas quebram. |
| Operabilidade | `tools/operational-readiness-check.ps1` | Runbooks ou `ops.example.json` ficam inconsistentes. |
| FinOps/capacidade | `tools/finops-capacity-check.ps1` | Documentos, alertas ou limites locais quebram. |
| Supply chain/SBOM | `tools/audit-supply-chain.ps1` | Vulnerabilidade, pacote deprecated, patch pendente, vendor inconsistente ou SBOM inválido. |
| Inventário SAST/DAST/a11y | `tools/external-security-scans.ps1 -InventoryOnly` | Estado das ferramentas externas não fica rastreável em artefato. |
| Vulnerabilidades NuGet | `dotnet list package --vulnerable --include-transitive` | Pacote vulnerável aparece. |
| Compose | `docker compose config` com placeholders seguros | Compose não interpola ou fica inconsistente. |
| Docker build | `docker build --pull` | Imagem não compila. |

## Artefatos

A CI publica artefatos diagnósticos não sensíveis por 14 dias:

- `artifacts/smoke/**/*.md`;
- `artifacts/observability/**/*.md`;
- `artifacts/operational-readiness/**/*.md`;
- `artifacts/finops-capacity/**/*.md`;
- `artifacts/reports/**/*.md`;
- `artifacts/sbom/**/*.json`.

Não publicar logs completos, bancos SQLite, backups, payloads pessoais, fotos,
biometria, cartões, QR Codes, headers de auth ou secrets.

## Reprodução local

Use os comandos abaixo a partir da raiz:

```powershell
dotnet restore .\Integracao.ControlID.PoC.sln --locked-mode
dotnet restore .\tools\ControlIdDeviceStub\ControlIdDeviceStub.csproj --locked-mode
dotnet restore .\tools\ControlIdCallbackSigningProxy\ControlIdCallbackSigningProxy.csproj --locked-mode
dotnet build .\Integracao.ControlID.PoC.sln --no-restore -v:minimal
dotnet build .\tools\ControlIdDeviceStub\ControlIdDeviceStub.csproj --no-restore -v:minimal
dotnet build .\tools\ControlIdCallbackSigningProxy\ControlIdCallbackSigningProxy.csproj --no-restore -v:minimal
dotnet test .\Integracao.ControlID.PoC.sln --no-build -v:minimal
powershell -ExecutionPolicy Bypass -File .\tools\smoke-localhost.ps1
powershell -ExecutionPolicy Bypass -File .\tools\contract-controlid-stub.ps1
dotnet format .\Integracao.ControlID.PoC.sln --verify-no-changes --no-restore -v:minimal
dotnet format .\tools\ControlIdCallbackSigningProxy\ControlIdCallbackSigningProxy.csproj --verify-no-changes --no-restore -v:minimal
git diff --check
powershell -ExecutionPolicy Bypass -File .\tools\scan-secrets.ps1
powershell -ExecutionPolicy Bypass -File .\tools\observability-check.ps1 -OfflineValidateOnly
powershell -ExecutionPolicy Bypass -File .\tools\operational-readiness-check.ps1
powershell -ExecutionPolicy Bypass -File .\tools\finops-capacity-check.ps1
powershell -ExecutionPolicy Bypass -File .\tools\audit-supply-chain.ps1
powershell -ExecutionPolicy Bypass -File .\tools\external-security-scans.ps1 -InventoryOnly
```

Para Compose local sem secrets reais, use placeholders apenas para validar
interpolação:

```powershell
$env:AllowedHosts = "poc.example.internal"
$env:ControlIDApi__AllowedDeviceHosts__0 = "controlid-device.local"
$env:CallbackSecurity__SharedKey = "placeholder-shared-key-32-characters-minimum"
docker compose config
```

## Gate manual de release

Para release operacional real, use:

```powershell
powershell -ExecutionPolicy Bypass -File .\tools\test-readiness-gates.ps1 -ReleaseGate
```

Esse gate é intencionalmente mais estrito que a CI de PR. Ele exige ambiente
preparado, `ops.local.json` fora do Git, observabilidade online, contrato físico
Control iD, scanners externos e FinOps/capacidade sem warnings.

## Proteção de branch recomendada

Configurar no GitHub, fora do repositório:

- exigir PR antes de merge em `main`;
- exigir pelo menos uma revisão humana;
- exigir branch up to date antes do merge;
- exigir status checks `build-test-audit` e `container-build`;
- bloquear bypass por administradores, salvo emergência documentada;
- exigir resolução de conversas;
- bloquear force push e deleção de branch `main`;
- exigir assinatura de commit se a organização já usar essa política.

## Diagnóstico de falhas

| Falha | Primeiro arquivo/comando |
| --- | --- |
| Restore locked | Conferir `packages.lock.json` do projeto afetado. |
| Build/test | Rodar localmente o mesmo comando do step. |
| Smoke | Abrir artefato `artifacts/smoke/localhost-smoke-ci.md`. |
| Secret scan | Verificar achado sem colar segredo em issue/PR. |
| Supply chain | Conferir `docs/supply-chain-review.md` e SBOM em `artifacts/sbom/`. |
| Observabilidade/FinOps/readiness | Ler artefatos em `artifacts/*/*latest.md`. |
| Docker | Rodar `docker compose config` e `docker build --pull ...` localmente. |

## Limites

- A CI não executa deploy.
- A CI não usa credenciais reais nem equipamento físico.
- Scanners externos completos ficam no `-ReleaseGate` ou em ambiente preparado.
- Branch protection precisa ser aplicada nas configurações do GitHub.
- Coverage numérico por percentual ainda depende de ferramenta adicional
  versionada; hoje a suíte bloqueia falha de teste e o gate de coverage bloqueia
  ausência de artefato quando solicitado.
