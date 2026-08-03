# Changelog técnico - 2026-08-03

## Escopo

Fechamento dos 14 riscos encontrados na investigação full-stack, preservando
rotas, payloads públicos e regras funcionais da PoC.

## Correções

1. Supply chain: removida dependência direta sem uso, fixada versão segura do
   SQLite nativo, lockfiles atualizados, auditoria semanal e Dependabot.
2. Callbacks binários: HMAC calculado sobre os bytes exatos recebidos e
   canonicalizador compartilhado com o proxy assinador.
3. Bootstrap local: primeiro administrador criado em transação SQLite imediata,
   com índices únicos para username/e-mail normalizados.
4. Smoke: builds agora validam exit code e o relatório padrão fica em
   `artifacts/smoke/`, fora da documentação versionada; cada execução usa banco
   temporário isolado, falha cedo sem login e limpa o estado ao encerrar.
5. Identidade: limites consistentes para nome, username, e-mail, telefone e
   senha, com username em allowlist e senha entre 12 e 128 caracteres.
6. Sessão do equipamento: tentativa de login falha sem destruir sessão valida;
   uma nova sessão só substitui a anterior depois do sucesso.
7. Transporte outbound: timeout cancelável, `ResponseHeadersRead`, leitura
   limitada em streaming e charset tratado explicitamente.
8. Push: falha SQLite não recebe falso ACK; persistência propaga erro e métricas
   só registram sucesso depois do commit.
9. CSP: confirmações destrutivas usam `data-confirm`, sem handlers inline.
10. Mobile: header deixa de ser sticky em telas estreitas, ações ganham scroll
    horizontal sem trilho invasivo, alvos do topo chegam a 44 px e os overrides
    do shell ficam em stylesheet separado.
11. Schema: removida criação ad hoc de tabelas no startup; migrations são
    explícitas, readiness detecta pendências e existe modo migrate-only.
12. Mojibake: catálogo oficial usa literais Unicode determinísticos e não
    depende mais de reparo heurístico em runtime; o glifo de sucesso usa escape
    CSS determinístico em vez do caractere de substituição `?`.
13. Frontend: testes reais com `WebApplicationFactory` cobrem renderização,
    headers, validação de senha, readiness e respostas sem cache.
14. Manutenibilidade: leitura de resposta, canonicalização HMAC e resultado de
    registro foram extraídos; payload bruto duplicado foi removido; repositórios
    não convertem falha de infraestrutura em `false`; 52 blocos genéricos que
    apenas repetiam log e relançamento foram removidos; exceção HTTP é registrada
    uma única vez no pipeline.

## Hardening complementar

- Nonces HMAC são globais entre paths, possuem capacidade configurável e falham
  de modo seguro quando o limite é atingido.
- Respostas dinâmicas usam `Cache-Control: no-store`; recursos estáticos mantêm a
  política de cache versionado.
- Erros HTML e JSON não expõem stack trace nem detalhes internos e incluem IDs
  de diagnóstico.
- `RawJson` só é persistido quando representa um envelope distinto de `Payload`.
- Compose propaga os limites de resposta outbound, nonces e modo migrate-only
  documentados em `.env.example`.

## Validação executada

- restore locked e build da solução, stub e proxy: zero warnings/erros;
- format check da solução/proxy, whitespace check e scan de secrets: passaram;
- suíte xUnit e coverage collector: 208/208 testes passaram;
- auditoria supply chain: zero vulnerabilidades, pacotes preteridos ou updates
  pendentes; SBOM com 115 pacotes NuGet e 4 dependências vendorizadas;
- smoke localhost: 388 PASS, 0 FAIL e 55 SKIP intencionais;
- contrato com stub, observabilidade offline, prontidão operacional e FinOps:
  passaram;
- Compose validado e imagem Docker `Release` construída; migrate-only saiu com
  código zero e container final respondeu `Healthy` em live/readiness como
  usuário não-root;
- cadastro/login reais e shell verificados no navegador em desktop e 390x844:
  sem erro de console ou overflow; header mobile caiu de 281 px para 206 px.
