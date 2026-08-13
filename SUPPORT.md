# Suporte

> **Guia** · Público: usuários, integradores e operação · Responsável: Engenharia · Última validação: 2026-08-12.

## Antes de solicitar ajuda

1. Consulte a [FAQ](docs/primeiros-passos/faq.md).
2. Siga o [diagnóstico Control iD](docs/operacao/troubleshooting-controlid.md).
3. Confirme [modelo, firmware, licença e evidência](docs/integracao-controlid/device-compatibility-matrix.md).
4. Reproduza com o [simulador](docs/primeiros-passos/stub-scenarios.md), quando possível.

## Evidências úteis

Informe commit, sistema operacional, SDK, modo de execução, endpoint, método,
status HTTP, duração e `X-Correlation-ID`. Para equipamento físico, inclua
modelo, firmware, licença, modo e topologia de rede.

Nunca envie senha, token, chave compartilhada, cabeçalho de autorização,
biometria, fotografia, documento, banco SQLite ou payload completo. Sanitize
logs seguindo o [catálogo de erros](docs/integracao-controlid/api-error-catalog.md).

Problemas de segurança devem seguir [SECURITY.md](SECURITY.md), não o canal
normal de suporte.
