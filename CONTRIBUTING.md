# Contribuição

> **Política** · Público: contribuidores humanos e agentes · Responsável: Engenharia · Última validação: 2026-08-12.

Obrigado por contribuir com a PoC. Antes de alterar código, leia o
[README](README.md), as [regras para agentes](AGENTS.md) e a
[central de documentação](docs/README.md).

## Fluxo de contribuição

1. Confirme o comportamento atual e identifique o contrato público afetado.
2. Mantenha a mudança pequena, rastreável e alinhada à arquitetura existente.
3. Não use dados pessoais reais, credenciais ou payloads sensíveis.
4. Atualize testes e o documento canônico do domínio alterado.
5. Execute os checks mínimos descritos em [AGENTS.md](AGENTS.md#matriz-mínima-de-verificações).
6. Registre limitações, checks não executados e riscos residuais.

## Documentação

- Use português claro e UTF-8.
- Prefira links relativos clicáveis a caminhos em crases.
- Não duplique procedimentos: vincule a fonte canônica da
  [matriz documental](docs/README.md#fontes-canônicas).
- Classifique documentos como Guia, Referência, Runbook, Decisão, Política ou
  Registro histórico.
- Atualize o índice do domínio e execute `tools/validate-documentation.ps1`.

## Mudanças sensíveis

Alterações de contrato público, migrações destrutivas, exclusão de dados,
dependências centrais, configuração de produção, deploy, commit e push exigem a
confirmação humana definida em [AGENTS.md](AGENTS.md#ações-proibidas-sem-confirmação-humana).

## Licenciamento

Contribuições não alteram automaticamente o regime de direitos do projeto. O
repositório não possui licença de código aberto; leia o [aviso de
licenciamento](LICENSE) antes de usar ou redistribuir qualquer parte da solução.
