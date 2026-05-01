# Validacao externa de seguranca, dependencias, DAST e acessibilidade

Escopo: padronizar execucao de SAST, OSV, DAST baseline e acessibilidade sem
versionar credenciais, sem enviar payloads pessoais a terceiros e sem depender de
comandos informais. Este runbook complementa `tools/test-readiness-gates.ps1`.

## Ferramentas esperadas

| Frente | Comando | Uso | Observacao |
| --- | --- | --- | --- |
| SAST | `semgrep` | Executa regras locais em `.semgrep.yml` | Nao usa ruleset remoto por padrao. |
| Dependencias OSV | `osv-scanner` | Avalia lockfiles e manifests por vulnerabilidades conhecidas | Complementa `dotnet list package --vulnerable`. |
| DAST baseline | `zap-baseline.py` ou `zap.bat` | Varre app local/staging controlado | Exige `EXTERNAL_SCAN_BASE_URL`; no Windows, o pacote ZAP pode expor apenas `zap.bat`, usado em quick scan headless. |
| Acessibilidade | `axe` | Varre pagina inicial do app em execucao | Exige `EXTERNAL_SCAN_BASE_URL`; nao coleta dado pessoal. |

Instale as ferramentas por canais oficiais ou imagem/container aprovados pelo
ambiente. O repositorio nao instala scanners automaticamente porque isso altera o
host do operador e pode exigir rede, licencas, trust store ou proxy corporativo.

## Comandos

Inventario local das ferramentas:

```powershell
powershell -ExecutionPolicy Bypass -File .\tools\external-security-scans.ps1 -InventoryOnly
```

Executar scanners disponiveis sem exigir todos:

```powershell
# Defina antes de iniciar a app local usada pelo scan para evitar 429 durante DAST/a11y.
$env:Auth__RateLimit__PermitLimit = "1000"
$env:Security__InteractiveRateLimit__PermitLimit = "5000"
dotnet run --project .\Integracao.ControlID.PoC.csproj --no-build --launch-profile Integracao.ControlID.PoC
```

Em outro terminal:

```powershell
powershell -ExecutionPolicy Bypass -File .\tools\external-security-scans.ps1 -BaseUrl http://127.0.0.1:5000/Auth/LocalLogin
```

Bloquear release quando qualquer ferramenta ou URL obrigatoria faltar:

```powershell
$env:EXTERNAL_SCAN_BASE_URL = "http://127.0.0.1:5000/Auth/LocalLogin"
powershell -ExecutionPolicy Bypass -File .\tools\test-readiness-gates.ps1 -RunExternalScanners -RequireExternalScanners
```

O `-ReleaseGate` tambem ativa `-RunExternalScanners` e `-RequireExternalScanners`.

## Contrato de equipamento

O gate executa sempre `tools/contract-controlid-stub.ps1`, que sobe o stub local
Control iD e valida `login.fcgi`, `session_is_valid.fcgi` e
`system_information.fcgi` sem credenciais reais.

Contrato com equipamento fisico real continua opt-in e deve falhar quando exigido
sem configuracao:

```powershell
$env:CONTROLID_DEVICE_URL = "http://<ip-ou-host-do-equipamento>:8080"
$env:CONTROLID_USERNAME = "<usuario-autorizado>"
$env:CONTROLID_PASSWORD = "<senha-autorizada>"
powershell -ExecutionPolicy Bypass -File .\tools\test-readiness-gates.ps1 -RequireHardwareContract
```

Nao use credenciais reais em docs, commits, reports versionados ou screenshots.

## Artefatos

Todos os relatórios ficam fora do Git:

- `artifacts/external-scans/external-security-scans-latest.md`
- `artifacts/external-scans/semgrep.json`
- `artifacts/external-scans/osv-scanner.json`
- `artifacts/external-scans/zap-baseline.html`
- `artifacts/external-scans/zap-baseline.json`
- `artifacts/external-scans/axe.console.txt`
- `artifacts/reports/controlid-stub-contract-latest.md`
- `artifacts/reports/controlid-device-contract-latest.md`

## Regras de privacidade

- Nao enviar banco SQLite, logs, backups, payloads brutos ou imagens para scanners
  remotos sem aprovacao humana e avaliacao DPO.
- Rodar DAST/a11y contra ambiente local, preview isolado ou staging controlado.
- Usar dados ficticios nos fluxos varridos.
- Revisar qualquer achado antes de publicar relatorio fora do time tecnico.
