# Supply Chain Review

Baseline para revisoes de dependencias, licencas e SBOM deste repositorio.

## Escopo

- Gerenciador principal: NuGet.
- Runtime: .NET 8 definido em `global.json`.
- Lockfiles: `packages.lock.json` na raiz, em `tests/Integracao.ControlID.PoC.Tests/` e nos dois projetos em `tools/`.
- Frontend: bibliotecas estaticas vendorizadas em `wwwroot/lib`; nao ha `package.json`, `npm`, `pnpm` ou `yarn`.
- Outras stacks auditadas: nao ha arquivos de dependencias Python, Cargo ou Node no repositorio.

## Auditorias oficiais

Execute a partir da raiz:

```powershell
dotnet restore .\Integracao.ControlID.PoC.sln --locked-mode
.\tools\audit-supply-chain.ps1
dotnet list .\Integracao.ControlID.PoC.sln package --vulnerable --include-transitive
dotnet list .\Integracao.ControlID.PoC.sln package --deprecated
dotnet list .\Integracao.ControlID.PoC.sln package --outdated --include-transitive
dotnet list .\tools\ControlIdDeviceStub\ControlIdDeviceStub.csproj package --vulnerable --include-transitive
dotnet list .\tools\ControlIdCallbackSigningProxy\ControlIdCallbackSigningProxy.csproj package --vulnerable --include-transitive
powershell -ExecutionPolicy Bypass -File .\tools\scan-secrets.ps1
```

## Politica de atualizacao

- Preferir patch/minor compativel com `net8.0`.
- Nao atualizar major automaticamente.
- Nao remover dependencia sem busca de uso, impacto e checks.
- Atualizacoes devem manter lockfile consistente.
- Pacotes de teste podem evoluir dentro da linha atual, mas migracoes de framework, como xUnit v3, exigem tarefa separada.

## SBOM

O repositorio nao depende de ferramenta externa instalada para gerar um SBOM basico. Use:

```powershell
powershell -ExecutionPolicy Bypass -File .\tools\generate-sbom.ps1
```

Saida padrao:

```text
artifacts/sbom/sbom.spdx.json
```

O diretorio `artifacts/` e ignorado pelo Git para evitar versionar artefatos locais. Publique o SBOM apenas em canal controlado de release/auditoria.

O SBOM cobre pacotes NuGet lockados e dependencias vendorizadas declaradas em `wwwroot/lib/vendor-dependencies.json`.

## Dependencias frontend vendorizadas

As bibliotecas em `wwwroot/lib` devem ser tratadas como dependencias de terceiros mesmo sem package manager JS.

Inventario atual:

- Bootstrap `5.1.0`, licenca MIT, em `wwwroot/lib/bootstrap`.
- jQuery `3.6.0`, licenca MIT, em `wwwroot/lib/jquery`.
- jquery-validation `1.20.0`, licenca MIT, em `wwwroot/lib/jquery-validation`.
- jquery-validation-unobtrusive `4.0.0`, licenca Apache-2.0, em `wwwroot/lib/jquery-validation-unobtrusive`.

`jquery-validation` foi atualizado de `1.19.5` para `1.20.0` porque a versao anterior e afetada pelo advisory moderado `GHSA-rrj2-ph5q-jxw2` / `CVE-2025-3573`, corrigido em `1.20.0`.

O arquivo `wwwroot/lib/vendor-dependencies.json` funciona como lockfile operacional dessas bibliotecas: registra versao, licenca, origem, versao minima segura e hash SHA-256 do diretorio. O hash normaliza finais de linha de arquivos texto e usa ordenacao ordinal de caminhos para ser reprodutivel entre Windows, Linux e runners de CI. Valide com:

```powershell
.\tools\audit-vendor-dependencies.ps1
```

Qualquer atualizacao futura deve:

- preservar arquivos de licenca;
- registrar origem e versao;
- validar telas e scripts Razor que as consomem;
- passar por revisao de vulnerabilidades conhecida para Bootstrap, jQuery, jquery-validation e jquery-validation-unobtrusive.

## Limites operacionais

- Nao ha ferramenta externa OSV, CycloneDX, Syft ou OWASP Dependency-Check instalada no repositorio; a auditoria local versionada cobre NuGet, patches disponiveis, pacotes preteridos, vendors frontend e SBOM SPDX.
- Licencas NuGet sao resolvidas dos `.nuspec` no cache local e vendors frontend sao resolvidos pelo manifesto. Para release formal, valide o SBOM com revisao juridica/licencas corporativa.
