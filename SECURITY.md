# Segurança

> **Política** · Público: usuários, mantenedores e pesquisadores · Responsável: Segurança/Privacidade · Última validação: 2026-08-12.

## Relato responsável

Não publique vulnerabilidades exploráveis, segredos ou dados pessoais em issues,
discussões, logs ou relatórios públicos. Use um canal privado definido pelos
mantenedores do repositório. Se nenhum canal privado estiver disponível, solicite
um meio de contato sem incluir detalhes técnicos sensíveis na mensagem inicial.

Inclua, de forma sanitizada:

- versão ou commit afetado;
- componente, rota ou fluxo;
- impacto observado;
- pré-condições e ambiente;
- evidência mínima sem credenciais, biometria, tokens ou dados reais.

## Escopo

São relevantes falhas de autenticação, autorização, callbacks, HMAC, SSRF,
upload, logs, banco local, dependências, exposição de dados e configurações
inseguras. A PoC não deve ser exposta diretamente à internet nem usada como
controle de acesso crítico sem homologação e hardening ambiental.

Os controles vigentes estão em [hardening](docs/seguranca-privacidade/security-hardening.md)
e [privacidade](docs/seguranca-privacidade/privacy-and-data-retention.md). A
ausência de uma falha conhecida não constitui garantia de segurança perfeita.
