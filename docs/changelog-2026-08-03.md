# Changelog tecnico - 2026-08-03

## Escopo

Fechamento dos 14 riscos encontrados na investigacao full-stack, preservando
rotas, payloads publicos e regras funcionais da PoC.

## Correcoes

1. Supply chain: removida dependencia direta sem uso, fixada versao segura do
   SQLite nativo, lockfiles atualizados, auditoria semanal e Dependabot.
2. Callbacks binarios: HMAC calculado sobre os bytes exatos recebidos e
   canonicalizador compartilhado com o proxy assinador.
3. Bootstrap local: primeiro administrador criado em transacao SQLite imediata,
   com indices unicos para username/e-mail normalizados.
4. Smoke: builds agora validam exit code e o relatorio padrao fica em
   `artifacts/smoke/`, fora da documentacao versionada; cada execucao usa banco
   temporario isolado, falha cedo sem login e limpa o estado ao encerrar.
5. Identidade: limites consistentes para nome, username, e-mail, telefone e
   senha, com username em allowlist e senha entre 12 e 128 caracteres.
6. Sessao do equipamento: tentativa de login falha sem destruir sessao valida;
   uma nova sessao so substitui a anterior depois do sucesso.
7. Transporte outbound: timeout cancelavel, `ResponseHeadersRead`, leitura
   limitada em streaming e charset tratado explicitamente.
8. Push: falha SQLite nao recebe falso ACK; persistencia propaga erro e metricas
   so registram sucesso depois do commit.
9. CSP: confirmacoes destrutivas usam `data-confirm`, sem handlers inline.
10. Mobile: header deixa de ser sticky em telas estreitas, acoes ganham scroll
    horizontal sem trilho invasivo, alvos do topo chegam a 44 px e os overrides
    do shell ficam em stylesheet separado.
11. Schema: removida criacao ad hoc de tabelas no startup; migrations sao
    explicitas, readiness detecta pendencias e existe modo migrate-only.
12. Mojibake: catalogo oficial usa literais Unicode deterministicos e nao
    depende mais de reparo heuristico em runtime; o glifo de sucesso usa escape
    CSS deterministico em vez do caractere de substituicao `?`.
13. Frontend: testes reais com `WebApplicationFactory` cobrem renderizacao,
    headers, validacao de senha, readiness e respostas sem cache.
14. Manutenibilidade: leitura de resposta, canonicalizacao HMAC e resultado de
    registro foram extraidos; payload bruto duplicado foi removido; repositorios
    nao convertem falha de infraestrutura em `false`; 52 blocos genericos que
    apenas repetiam log e relancamento foram removidos; excecao HTTP e logada
    uma unica vez no pipeline.

## Hardening complementar

- Nonces HMAC sao globais entre paths, possuem capacidade configuravel e falham
  de modo seguro quando o limite e atingido.
- Respostas dinamicas usam `Cache-Control: no-store`; assets estaticos mantem a
  politica de cache versionado.
- Erros HTML e JSON nao expoem stack trace nem detalhes internos e incluem IDs
  de diagnostico.
- `RawJson` so e persistido quando representa envelope distinto de `Payload`.
- Compose propaga os limites de resposta outbound, nonces e modo migrate-only
  documentados em `.env.example`.

## Validacao executada

- restore locked e build da solucao, stub e proxy: zero warnings/erros;
- format check da solucao/proxy, whitespace check e scan de secrets: passaram;
- suite xUnit e coverage collector: 208/208 testes passaram;
- auditoria supply chain: zero vulnerabilidades, pacotes preteridos ou updates
  pendentes; SBOM com 115 pacotes NuGet e 4 dependencias vendorizadas;
- smoke localhost: 388 PASS, 0 FAIL e 55 SKIP intencionais;
- contrato com stub, observabilidade offline, prontidao operacional e FinOps:
  passaram;
- Compose validado e imagem Docker `Release` construida; migrate-only saiu com
  codigo zero e container final respondeu `Healthy` em live/readiness como
  usuario nao-root;
- cadastro/login reais e shell verificados no navegador em desktop e 390x844:
  sem erro de console ou overflow; header mobile caiu de 281 px para 206 px.
