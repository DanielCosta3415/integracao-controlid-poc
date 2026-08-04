# Validação externa de segurança, dependências, DAST e acessibilidade

> **Guia operacional vivo** · Público: QA, AppSec e release · Responsável: Security/QA · Última validação: 2026-08-03.

Escopo: padronizar execução de SAST, OSV, DAST baseline e acessibilidade sem
versionar credenciais, sem enviar payloads pessoais a terceiros e sem depender de
comandos informais. Este guia operacional complementa `tools/test-readiness-gates.ps1`.

## Ferramentas esperadas

| Frente | Comando | Uso | Observação |
| --- | --- | --- | --- |
| SAST | `semgrep` | Executa regras locais em `.semgrep.yml` | Não usa conjunto de regras remoto por padrão. |
| Dependências OSV | `osv-scanner` | Avalia lockfiles e manifests por vulnerabilidades conhecidas | Complementa `dotnet list package --vulnerable`. |
| Linha de base DAST | `zap-baseline.py` ou `zap.bat` | Varre aplicação local/homologação controlada | Exige `EXTERNAL_SCAN_BASE_URL`; no Windows, o pacote ZAP pode expor apenas `zap.bat`, usado em varredura rápida sem interface gráfica. |
| Acessibilidade | `axe` | Varre página inicial do app em execução | Exige `EXTERNAL_SCAN_BASE_URL`; não coleta dado pessoal. |

Instale as ferramentas por canais oficiais ou imagem/container aprovados pelo
ambiente. O repositório não instala scanners automaticamente porque isso altera o
host do operador e pode exigir rede, licenças, trust store ou proxy corporativo.

## Comandos

Inventário local das ferramentas:

```powershell
powershell -ExecutionPolicy Bypass -File .\tools\external-security-scans.ps1 -InventoryOnly
```

Executar scanners disponíveis sem exigir todos:

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

Bloquear release quando qualquer ferramenta ou URL obrigatória faltar:

```powershell
$env:EXTERNAL_SCAN_BASE_URL = "http://127.0.0.1:5000/Auth/LocalLogin"
powershell -ExecutionPolicy Bypass -File .\tools\test-readiness-gates.ps1 -RunExternalScanners -RequireExternalScanners
```

O `-ReleaseGate` também ativa `-RunExternalScanners` e `-RequireExternalScanners`.
Para release operacional, registre ownership, data, status e local restrito dos
relatórios em `externalValidation.*` dentro de `ops.local.json`; o readiness
operacional falha se os status permanecerem pendentes.

## Contrato de equipamento

O gate executa sempre `tools/contract-controlid-stub.ps1`, que sobe o stub local
Control iD e valida `login.fcgi`, `session_is_valid.fcgi` e
`system_information.fcgi` sem credenciais reais.

Contrato com equipamento físico real continua opt-in e deve falhar quando exigido
sem configuração:

```powershell
$env:CONTROLID_DEVICE_URL = "http://<ip-ou-host-do-equipamento>:8080"
$env:CONTROLID_USERNAME = "<usuario-autorizado>"
$env:CONTROLID_PASSWORD = "<senha-autorizada>"
powershell -ExecutionPolicy Bypass -File .\tools\test-readiness-gates.ps1 -RequireHardwareContract
```

Não use credenciais reais em docs, commits, reports versionados ou screenshots.

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

- Não enviar banco SQLite, logs, backups, payloads brutos ou imagens para scanners
  remotos sem aprovação humana e avaliação DPO.
- Rodar DAST/a11y contra ambiente local, preview isolado ou staging controlado.
- Usar dados fictícios nos fluxos varridos.
- Revisar qualquer achado antes de publicar relatório fora do time técnico.

## Versões e reprodutibilidade

Antes da execução, registre versões sem instalar ou atualizar automaticamente:

```powershell
semgrep --version
osv-scanner --version
zap-baseline.py --help
axe --version
dotnet --info
```

Se um comando não existir, marque o scanner como bloqueado e use o canal oficial
aprovado pela organização para instalação. O relatório deve conter ferramenta,
versão, origem, hash da imagem quando aplicável, commit, URL sem credenciais,
horário, duração e parâmetros.

## Triagem de achados

| Estado | Significado | Evidência mínima |
| --- | --- | --- |
| Confirmado | Comportamento reproduzível e aplicável | Regra, local seguro e impacto |
| Falso positivo | Regra não se aplica ao fluxo real | Justificativa revisável |
| Aceito temporariamente | Risco real com mitigação compensatória | Dono, prazo e aprovação |
| Bloqueado | Ferramenta ou ambiente ausente | Dependência e próximo passo |

Achado crítico ou alto bloqueia release até correção ou aceite humano formal.
Não publique payload ofensivo, cookie, segredo ou dado pessoal como evidência.

## Controle de versões e artefatos

| Campo obrigatório | Exemplo seguro |
| --- | --- |
| Ferramenta e versão | `semgrep <versão-validada>` |
| Origem | Site oficial, pacote ou imagem aprovada |
| Configuração | Arquivo de regras e opções sem segredo |
| Alvo | URL local ou de homologação autorizada |
| Resultado | Código de saída, totais por severidade e falso positivo justificado |
| Artefato | Caminho restrito para SARIF, JSON, HTML ou log sanitizado |

Não fixe neste documento uma versão que ainda não tenha sido instalada e
validada. O relatório de cada execução é a fonte da versão efetiva; mudança de
versão exige nova linha de base para evitar comparar regras diferentes como se
fossem o mesmo controle.
