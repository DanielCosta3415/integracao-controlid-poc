# Revisão da cadeia de suprimentos

Baseline para revisoes de dependências, licenças e SBOM deste repositório.

## Escopo

- Gerenciador principal: NuGet.
- Runtime: .NET 8 definido em `global.json`.
- Lockfiles: `packages.lock.json` na raiz, em `tests/Integracao.ControlID.PoC.Tests/` e nos dois projetos em `tools/`.
- Frontend: bibliotecas estáticas vendorizadas em `wwwroot/lib`; não há `package.json`, `npm`, `pnpm` ou `yarn`.
- Outras stacks auditadas: não há arquivos de dependências Python, Cargo ou Node no repositório.

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

## Política de atualização

- Preferir patch/minor compatível com `net8.0`.
- Não atualizar major automaticamente.
- Não remover dependência sem busca de uso, impacto e checks.
- Atualizações devem manter lockfile consistente.
- Pacotes de teste podem evoluir dentro da linha atual, mas migrações de framework, como xUnit v3, exigem tarefa separada.

## SBOM

O repositório não depende de ferramenta externa instalada para gerar um SBOM básico. Use:

```powershell
powershell -ExecutionPolicy Bypass -File .\tools\generate-sbom.ps1
```

Saída padrão:

```text
artifacts/sbom/sbom.spdx.json
```

O diretório `artifacts/` e ignorado pelo Git para evitar versionar artefatos locais. Publique o SBOM apenas em canal controlado de release/auditoria.

O SBOM cobre pacotes NuGet lockados e dependências vendorizadas declaradas em `wwwroot/lib/vendor-dependencies.json`.

## Dependências de frontend vendorizadas

As bibliotecas em `wwwroot/lib` devem ser tratadas como dependências de terceiros mesmo sem package manager JS.

Inventário atual:

- Bootstrap `5.1.0`, licença MIT, em `wwwroot/lib/bootstrap`.
- jQuery `3.6.0`, licença MIT, em `wwwroot/lib/jquery`.
- jquery-validation `1.20.0`, licença MIT, em `wwwroot/lib/jquery-validation`.
- jquery-validation-unobtrusive `4.0.0`, licença Apache-2.0, em `wwwroot/lib/jquery-validation-unobtrusive`.

`jquery-validation` foi atualizado de `1.19.5` para `1.20.0` porque a versão anterior e afetada pelo advisory moderado `GHSA-rrj2-ph5q-jxw2` / `CVE-2025-3573`, corrigido em `1.20.0`.

O arquivo `wwwroot/lib/vendor-dependencies.json` funciona como lockfile operacional dessas bibliotecas: registra versão, licença, origem, versão mínima segura e hash SHA-256 do diretório. O hash normaliza finais de linha de arquivos texto e usa ordenação ordinal de caminhos para ser reprodutível entre Windows, Linux e runners de CI. Valide com:

```powershell
.\tools\audit-vendor-dependencies.ps1
```

Qualquer atualização futura deve:

- preservar arquivos de licença;
- registrar origem e versão;
- validar telas e scripts Razor que as consomem;
- passar por revisão de vulnerabilidades conhecida para Bootstrap, jQuery, jquery-validation e jquery-validation-unobtrusive.

## Limites operacionais

- Não há ferramenta externa OSV, CycloneDX, Syft ou OWASP Dependency-Check instalada no repositório; a auditoria local versionada cobre NuGet, patches disponíveis, pacotes preteridos, vendors frontend e SBOM SPDX.
- Licenças NuGet são resolvidas dos `.nuspec` no cache local e vendors frontend são resolvidos pelo manifesto. Para release formal, valide o SBOM com revisão jurídica/licenças corporativa.
