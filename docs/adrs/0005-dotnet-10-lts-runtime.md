# ADR 0005 - Adoção coordenada do .NET 10 LTS

> **Decisão aceita** · Público: arquitetura, desenvolvimento e plataforma · Responsável: liderança técnica · Última validação: 2026-08-03.

Estado: aceita

- Data da decisão: 2026-08-03
- Substitui: linha de runtime .NET 8 descrita na configuração anterior
- Substituída por: nenhuma decisão

## Direcionadores

- manter o runtime em uma linha LTS ativa até 14 de novembro de 2028;
- alinhar SDK, projetos, ASP.NET Core, Entity Framework Core, testes e contêiner;
- evitar atualizações major fragmentadas geradas automaticamente;
- preservar rotas, payloads, esquema SQLite e comportamento funcional;
- validar a mudança com testes, smoke, auditoria e imagem Linux.

## Contexto

A aplicação, os testes, o simulador e o proxy assinador usavam `net8.0`. PRs
automatizados propunham versões major incompatíveis entre si para pacotes do
Entity Framework Core e `Microsoft.Extensions`, sem migrar o framework-alvo nem
o contêiner. Mesclar essas alterações isoladamente criaria uma matriz de runtime
inconsistente e aumentaria o risco de regressão.

## Decisão

- Fixar o SDK `10.0.302` em `global.json`, com avanço limitado ao patch da mesma
  banda de recursos.
- Compilar os quatro projetos como `net10.0`.
- Alinhar Entity Framework Core, provedor SQLite, ferramentas e
  `Microsoft.AspNetCore.Mvc.Testing` em `10.0.10`.
- Pinar `dotnet-ef` `10.0.10` no manifesto local para que scripts não dependam
  de uma ferramenta global antiga.
- Atualizar as integrações de infraestrutura compatíveis na mesma janela de
  regressão: Serilog, Swashbuckle e SQLitePCLRaw.
- Usar imagens `mcr.microsoft.com/dotnet/sdk:10.0-alpine` e
  `mcr.microsoft.com/dotnet/aspnet:10.0-alpine`.
- Manter o Dependabot restrito a patch/minor; nova atualização major exige ADR,
  lockfiles regenerados e os gates completos.

## Contratos preservados

- rotas MVC, callbacks, `/push`, `/result` e endpoints de saúde/métricas;
- DTOs, ViewModels, cargas úteis e nomes de campos da Access API;
- migrações e esquema SQLite existentes;
- porta `8080`, usuário não root, volumes e healthcheck do contêiner;
- comandos públicos de restore, build, teste, auditoria e smoke.

## Alternativas consideradas

- Manter .NET 8 até o fim do suporte: rejeitado porque apenas adia a migração e
  mantém abertos os PRs major fragmentados.
- Adotar EF Core 9 sobre .NET 8: rejeitado por criar uma etapa intermediária sem
  benefício funcional para a PoC.
- Mesclar individualmente os PRs major: rejeitado porque versões de pacotes EF,
  SDK, framework-alvo e runtime do contêiner devem evoluir juntas.
- Atualizar somente o framework-alvo: rejeitado porque deixaria pacotes de
  infraestrutura em linhas antigas sem validar suas mudanças compatíveis.

## Consequências

- Desenvolvimento e CI precisam do SDK `10.0.302`; a CI o resolve pelo
  `global.json`.
- O primeiro restore baixa uma nova árvore de dependências e regenera todos os
  lockfiles.
- Swashbuckle 10 usa `Microsoft.OpenApi.OpenApiInfo`, sem o namespace antigo
  `.Models`.
- O documento OpenAPI inclui somente ações com método HTTP explícito; páginas
  MVC convencionais continuam roteáveis, mas não são anunciadas como contrato
  ambíguo.
- Referências diretas redundantes a `Microsoft.Extensions.Configuration` e
  `Microsoft.Extensions.Logging` foram removidas porque o framework compartilhado
  as fornece e o SDK 10 emite `NU1510`.
- A decisão não cria migração de dados e não altera o contrato público.

## Reversão

Reverter em conjunto `global.json`, os quatro `TargetFramework`, versões de
pacotes, lockfiles, import do OpenAPI e `DOTNET_VERSION` do Dockerfile. Como não
há alteração de esquema, a reversão de código não exige apagar ou transformar o
SQLite. Depois, repetir restore bloqueado, build, testes, smoke e construção do
contêiner; não reverta parcialmente apenas EF ou a imagem de runtime.

## Evidências

- `global.json`
- `.config/dotnet-tools.json`
- `Integracao.ControlID.PoC.csproj`
- `tests/Integracao.ControlID.PoC.Tests/Integracao.ControlID.PoC.Tests.csproj`
- `tools/ControlIdDeviceStub/ControlIdDeviceStub.csproj`
- `tools/ControlIdCallbackSigningProxy/ControlIdCallbackSigningProxy.csproj`
- `Dockerfile`
- `packages.lock.json` e lockfiles dos projetos auxiliares
- `.github/workflows/ci.yml`
- `tools/test-readiness-gates.ps1`

## Critério de revisão

Reavalie antes do fim do suporte do .NET 10, diante de vulnerabilidade sem
correção compatível ou se o provedor de implantação não suportar a linha LTS.
Qualquer substituição deve manter os pacotes Microsoft de framework na mesma
linha major e registrar compatibilidade de contêiner, dados e hardware.
