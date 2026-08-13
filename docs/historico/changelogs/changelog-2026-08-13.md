# Fortalecimento técnico e de segurança de 2026-08-13

> **Registro histórico** · Público: manutenção, segurança e liberação · Responsável: Engenharia · Referência temporal: 2026-08-13.

## Alterações

- restringiu a invocação manual da API oficial a administradores e manteve o
  token de sessão exclusivamente no servidor;
- protegeu colunas sensíveis do SQLite por finalidade, com chaveiro persistente,
  health check e conversão legada confirmada após backup e ensaio de restauração;
- tornou HTTPS, allowlist de equipamento, certificado do chaveiro e atestado de
  volume criptografado obrigatórios fora de `Development`;
- reduziu respostas públicas de saúde e protegeu os detalhes por autorização;
- tipou conteúdo JSON de saída, restringiu anexos multipart e neutralizou
  separadores em valores destinados aos logs;
- atualizou `jquery-validation` de `1.20.0` para `1.22.1` com hash vendorizado
  recalculado e licença preservada;
- fixou GitHub Actions por SHA completo e habilitou Dependabot, secret scanning,
  push protection, CodeQL gerenciado e integridade de `main` no GitHub;
- removeu contato pessoal da interface e configurou o repositório local para
  usar o endereço `noreply` do GitHub em commits futuros.

## Validação

Os comandos e resultados desta rodada devem ser consultados no resumo da tarefa
e reproduzidos pelos gates descritos em
[CI/CD e critérios de qualidade](../../qualidade/ci-cd-quality-gates.md). Dados
legados reais não foram reescritos automaticamente, e nenhuma credencial real
foi usada ou registrada.

## Limitações

- a análise do comportamento físico e do TLS do equipamento continua dependendo
  de hardware, firmware e rede de homologação;
- `non-provider patterns` e `validity checks` do secret scanning não ficaram
  disponíveis no provedor nesta data;
- publicar as mudanças locais e obter uma nova execução remota é necessário para
  encerrar alertas CodeQL que apontam para linhas modificadas nesta rodada.

Voltar ao [índice de alterações](README.md).
