# Revisão da cadeia de suprimentos

> **Referência** · Público: manutenção, AppSec e release · Responsável: Segurança/Privacidade · Última validação: 2026-08-13.

Baseline para revisões de dependências, licenças e SBOM deste repositório.

## Escopo

- Gerenciador principal: NuGet.
- Runtime: .NET 10 LTS, SDK `10.0.302` definido em `global.json`.
- Lockfiles: `packages.lock.json` na raiz, nos dois projetos em `tests/` e nos
  dois projetos em `tools/`.
- Ferramenta local: `dotnet-ef` `10.0.11` pinado em
  `.config/dotnet-tools.json`; não dependa de instalação global para migração ou
  teste de restauração.
- Frontend: bibliotecas estáticas vendorizadas em `wwwroot/lib`; não há `package.json`, `npm`, `pnpm` ou `yarn`.
- Outras stacks auditadas: não há arquivos de dependências Python, Cargo ou Node no repositório.

## Auditorias oficiais

Execute a partir da raiz:

```powershell
dotnet restore .\Integracao.ControlID.PoC.sln --locked-mode
dotnet tool restore
.\tools\audit-supply-chain.ps1
dotnet list .\Integracao.ControlID.PoC.sln package --vulnerable --include-transitive
dotnet list .\Integracao.ControlID.PoC.sln package --deprecated
dotnet list .\Integracao.ControlID.PoC.sln package --outdated --include-transitive
dotnet list .\tools\ControlIdDeviceStub\ControlIdDeviceStub.csproj package --vulnerable --include-transitive
dotnet list .\tools\ControlIdCallbackSigningProxy\ControlIdCallbackSigningProxy.csproj package --vulnerable --include-transitive
powershell -ExecutionPolicy Bypass -File .\tools\scan-secrets.ps1
```

## Política de atualização

- Preferir patch/minor compatível com `net10.0`.
- Não atualizar major automaticamente.
- O Dependabot limita cada ecossistema a dois PRs abertos, agrupa patch/minor e
  ignora major; migrações major são executadas como mudanças coordenadas.
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

O diretório `artifacts/` é ignorado pelo Git para evitar versionar artefatos locais. Publique o SBOM apenas em canal controlado de release/auditoria.

O SBOM cobre pacotes NuGet lockados, o `dotnet-ef` do manifesto local e
dependências vendorizadas declaradas em `wwwroot/lib/vendor-dependencies.json`.

## Dependências vendorizadas da interface

As bibliotecas em `wwwroot/lib` devem ser tratadas como dependências de terceiros mesmo sem package manager JS.

Inventário atual:

- Bootstrap `5.1.0`, licença MIT, em `wwwroot/lib/bootstrap`.
- jQuery `3.6.0`, licença MIT, em `wwwroot/lib/jquery`.
- jquery-validation `1.22.1`, licença MIT, em `wwwroot/lib/jquery-validation`.
- jquery-validation-unobtrusive `4.0.0`, licença Apache-2.0, em `wwwroot/lib/jquery-validation-unobtrusive`.

`jquery-validation` foi atualizado para `1.22.1`, versão oficial revisada em
2026-08-13. O piso `1.20.0` continua cobrindo o advisory moderado
`GHSA-rrj2-ph5q-jxw2` / `CVE-2025-3573`; a atualização também reduz a defasagem
do código vendorizado analisado pelo CodeQL.

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

## Matriz de licenças vendorizadas

| Dependência | Versão | Licença | Evidência |
| --- | --- | --- | --- |
| Bootstrap | 5.1.0 | MIT | `wwwroot/lib/bootstrap/LICENSE` |
| jQuery | 3.6.0 | MIT | `wwwroot/lib/jquery/LICENSE.txt` |
| jQuery Validation | 1.22.1 | MIT | [wwwroot/lib/jquery-validation/LICENSE.md](../../wwwroot/lib/jquery-validation/LICENSE.md) |
| jQuery Validation Unobtrusive | 4.0.0 | Apache-2.0 | `wwwroot/lib/jquery-validation-unobtrusive/LICENSE.txt` |

O manifesto `wwwroot/lib/vendor-dependencies.json` é a fonte canônica de versão,
origem, hash e versão mínima segura. Licença permitida tecnicamente não substitui
revisão jurídica de distribuição.

## Cadência e exceções

- Dependabot e auditoria completa: semanal.
- SBOM e vendors: a cada release e atualização de dependência.
- Pacote crítico: triagem imediata; alto: próximo ciclo prioritário; médio/baixo:
  registrar decisão e janela.
- Exceção exige pacote, aviso de segurança, exposição, mitigação, responsável, prazo e aprovação.
- Major version nunca é automática; requer análise de contrato e regressão.

## Proveniência e integridade

| Artefato | Controle atual | Evolução recomendada antes de distribuição formal |
| --- | --- | --- |
| NuGet | Lockfiles e restore bloqueado | Verificar assinatura e origem no ambiente corporativo |
| Ações GitHub | Versões declaradas no workflow | Fixar SHA revisado quando a política exigir imutabilidade |
| Imagem de contêiner | Build local reproduzível | Publicar digest, SBOM e atestado no registro aprovado |
| Bibliotecas vendorizadas | Manifesto, versão, hash e licença | Automatizar comparação com origem oficial |
| Ferramentas externas | Inventário por execução | Registrar versão, origem e checksum aprovados |

Não declare proveniência assinada sem registro, identidade e verificação reais.
SBOM descreve componentes; não prova sozinho origem confiável ou ausência de risco.

As imagens de construção e execução são fixadas, respectivamente, em
`10.0.302-noble` e `10.0.11-noble`. Não substitua essas versões por uma etiqueta
flutuante de linha (`10.0`): ela pode mudar de banda do SDK e deixar de atender o
`global.json`, mesmo sem alteração no repositório.

## Navegação documental

- [Voltar ao índice deste domínio](README.md).
- [Abrir a central de documentação](../README.md).
