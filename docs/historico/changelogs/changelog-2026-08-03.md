# Registro técnico de alterações - 2026-08-03

> **Registro histórico** · Público: manutenção e auditoria · Responsável: Engenharia · Referência temporal: 2026-08-03.

## Escopo

Fechamento dos 14 riscos encontrados na investigação da solução completa, preservando
rotas, payloads públicos e regras funcionais da PoC.

## Correções

1. Cadeia de suprimentos: removida dependência direta sem uso, fixada versão segura do
   SQLite nativo, lockfiles atualizados, auditoria semanal e Dependabot.
2. Callbacks binários: HMAC calculado sobre os bytes exatos recebidos e
   canonicalizador compartilhado com o proxy assinador.
3. Bootstrap local: primeiro administrador criado em transação SQLite imediata,
   com índices únicos para username/e-mail normalizados.
4. Teste integrado: as compilações agora validam o código de saída e o relatório padrão fica em
   `artifacts/smoke/`, fora da documentação versionada; cada execução usa banco
   temporário isolado, falha cedo sem login e limpa o estado ao encerrar.
5. Identidade: limites consistentes para nome, username, e-mail, telefone e
   senha, com username em allowlist e senha entre 12 e 128 caracteres.
6. Sessão do equipamento: tentativa de login falha sem destruir sessão valida;
   uma nova sessão só substitui a anterior depois do sucesso.
7. Transporte de saída: tempo limite cancelável, `ResponseHeadersRead`, leitura
   limitada em fluxo e conjunto de caracteres tratado explicitamente.
8. Push: falha SQLite não recebe falso ACK; persistência propaga erro e métricas
   só registram sucesso depois do commit.
9. CSP: confirmações destrutivas usam `data-confirm`, sem manipuladores embutidos.
10. Dispositivos móveis: o cabeçalho deixa de ser fixo em telas estreitas, ações
    ganham rolagem horizontal sem trilho invasivo, alvos do topo chegam a 44 px e
    as substituições da estrutura ficam em folha de estilos separada.
11. Esquema: removida criação ad hoc de tabelas na inicialização; migrações são
    explícitas, a prontidão detecta pendências e existe modo somente de migração.
12. Mojibake: catálogo oficial usa literais Unicode determinísticos e não
    depende mais de reparo heurístico em runtime; o glifo de sucesso usa escape
    CSS determinístico em vez do caractere de substituição `?`.
13. Frontend: testes reais com `WebApplicationFactory` cobrem renderização,
    cabeçalhos, validação de senha, prontidão e respostas sem cache.
14. Manutenibilidade: leitura de resposta, canonicalização HMAC e resultado de
    registro foram extraídos; payload bruto duplicado foi removido; repositórios
    não convertem falha de infraestrutura em `false`; 52 blocos genéricos que
    apenas repetiam registro e relançamento foram removidos; a exceção HTTP é
    registrada uma única vez no fluxo.

## Fortalecimento complementar

- Nonces HMAC são globais entre caminhos, possuem capacidade configurável e falham
  de modo seguro quando o limite é atingido.
- Respostas dinâmicas usam `Cache-Control: no-store`; recursos estáticos mantêm a
  política de cache versionado.
- Erros HTML e JSON não expõem rastreamento de pilha nem detalhes internos e incluem IDs
  de diagnóstico.
- `RawJson` só é persistido quando representa um envelope distinto de `Payload`.
- Compose propaga os limites de resposta de saída, nonces e modo somente de migração
  documentados em `.env.example`.

## Validação executada

- restauração bloqueada e compilação da solução, simulador e proxy: zero avisos/erros;
- verificação de formatação da solução/proxy, espaços em branco e segredos: passaram;
- suíte xUnit e coletor de cobertura: 208/208 testes passaram;
- auditoria da cadeia de suprimentos: zero vulnerabilidades, pacotes preteridos ou atualizações
  pendentes; SBOM com 115 pacotes NuGet e 4 dependências vendorizadas;
- teste integrado local: 388 PASS, 0 FAIL e 55 SKIP intencionais;
- contrato com simulador, observabilidade off-line, prontidão operacional e FinOps:
  passaram;
- Compose validado e imagem Docker `Release` construída; o modo somente de migração
  saiu com código zero e o contêiner final respondeu `Healthy` em atividade/prontidão como
  usuário não-root;
- cadastro/login reais e estrutura verificados no navegador em computador e 390x844:
  sem erro de console ou transbordamento; o cabeçalho móvel caiu de 281 px para 206 px.

## Referência de implementação

| Item | Referência |
| --- | --- |
| Commit da correção dos 14 riscos | `03fb80f` (`fix: harden full-stack PoC reliability`) |
| Validação documental posterior | `eae6493` (`docs: revisar documentação em português`) |
| Contratos públicos | Preservados conforme testes e documentação da rodada |
| Migrações | Inclui `20260803192319_HardenLocalIdentity` |
| Reversão | Exige atenção ao esquema; código anterior não desfaz migração automaticamente |

Os números de testes, smoke e SBOM são evidências da execução registrada, não
garantia permanente. Reexecute os critérios no commit candidato.

## Continuidade do histórico

Esta entrada encerra a rodada registrada, não o histórico do projeto. Mudanças
posteriores devem criar nova seção datada ou novo arquivo, citar o commit-base e
separar correções de documentação, código, dados e operação.

## Ampliação documental para primeiro contato

> **Estado:** alteração documental corrente, posterior à linha de base histórica
> descrita acima. Não altera contratos nem comportamento da aplicação.

- Foram acrescentados guias sobre perguntas frequentes, administração de contas
  locais, compatibilidade de equipamentos, topologias de rede, diagnóstico,
  sincronização e titularidade de dados, erros da API, percursos por perfil e
  governança das fontes oficiais.
- O inventário passou a registrar 58 arquivos Markdown: 57 documentos autorais e
  uma licença vendorizada. O índice técnico cataloga todos os documentos em
  `docs/` e o validador rejeita documentos novos que não sejam indexados.
- O [README.md](../../../README.md) passou a distinguir a conta local da sessão da Access API e a
  indicar percursos de leitura conforme a necessidade do leitor.
- Permissões, limites da PoC e afirmações de compatibilidade foram alinhados ao
  código, aos testes e à documentação oficial disponível da Control iD. Recursos
  físicos continuam condicionados ao modelo, ao firmware, à licença, aos módulos
  instalados e à homologação com equipamento real.
- Links, caminhos, cercas de código, codificação UTF-8, ortografia, terminologia e
  ausência de dados sensíveis devem ser revalidados pelos gates documentais antes
  de qualquer liberação.

### Validação do adendo

- inventário e estrutura: 58 arquivos Markdown, dos quais 57 são autorais e um é
  uma licença vendorizada;
- links externos: 36 URLs verificadas, incluindo as fontes oficiais da Control iD;
- compilação: zero avisos e zero erros;
- testes: 209 aprovados, zero reprovados e zero ignorados;
- formatação, espaços em branco, codificação, vínculos locais e varredura de
  segredos: aprovados.

Esses números representam esta execução e devem ser produzidos novamente no
commit candidato à liberação.

## Migração coordenada para .NET 10 LTS

> **Estado:** alteração técnica publicada no commit `fe0e6d2`. A execução CI
> `30875200484` aprovou os jobs `build-test-audit` e `container-build`.

- SDK pinado em `10.0.302` e os quatro projetos migrados para `net10.0`.
- ASP.NET Core, Entity Framework Core, provedor SQLite, ferramentas e projeto de
  testes alinhados em `10.0.10`.
- Serilog, Swashbuckle e SQLitePCLRaw atualizados dentro da mesma janela de
  regressão; o import de `OpenApiInfo` foi adaptado ao Microsoft.OpenApi 2.x.
- O OpenAPI passou a incluir apenas ações com verbo HTTP explícito, eliminando
  falha de geração causada por páginas MVC convencionais ambíguas.
- Referências diretas redundantes a `Microsoft.Extensions.Configuration` e
  `Microsoft.Extensions.Logging` foram removidas após o SDK emitir `NU1510`.
- `dotnet-ef` `10.0.10` foi pinado em manifesto local e restaurado na CI, sem
  dependência da versão global instalada na estação.
- Dockerfile atualizado para as imagens SDK/runtime .NET 10 Alpine, preservando
  usuário não root, porta, volumes e healthcheck.
- Dependabot passou a agrupar patch/minor, limitar PRs e ignorar major; mudanças
  major exigem migração coordenada e ADR.
- A decisão, os contratos preservados e o procedimento de reversão estão em
  [docs/adrs/0005-dotnet-10-lts-runtime.md](../../adrs/0005-dotnet-10-lts-runtime.md).

### Validação da migração

- restauração bloqueada e `dotnet tool restore`: aprovados;
- compilação da solução, simulador e proxy: zero avisos e zero erros;
- suíte xUnit: 212 aprovados, zero reprovados e zero ignorados, incluindo
  documento OpenAPI no pipeline real;
- smoke integrado: 388 aprovados, zero reprovados e 55 desvios intencionais por
  ausência de hardware/ambiente externo;
- contrato do simulador, cobertura, formatação, documentação, segredos,
  observabilidade, prontidão operacional e FinOps: aprovados;
- cadeia de suprimentos: zero vulnerabilidades ou pacotes preteridos; SBOM com
  102 pacotes NuGet, incluindo a ferramenta local, e quatro vendors;
- SQLite temporário: migrações, backup e restauração aprovados com
  `dotnet-ef` local `10.0.10`;
- Compose: configuração aprovada; a construção local foi bloqueada por `EOF` do
  registro MCR, mas a CI remota construiu a imagem Linux no commit `fe0e6d2`.
- PRs Dependabot `#1` a `#8`: encerrados automaticamente sem merge após a nova
  política e a atualização direta da `main`; nenhum conteúdo pendente permaneceu.

## Navegação documental

- [Voltar ao índice deste domínio](README.md).
- [Abrir a central de documentação](../../README.md).
