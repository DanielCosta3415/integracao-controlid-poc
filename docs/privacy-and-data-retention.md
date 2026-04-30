# Privacidade, dados sensiveis e retencao local

Esta PoC manipula dados operacionais da Access API da Control iD. Trate os dados abaixo como pessoais, sensiveis ou confidenciais:

- usuarios, matriculas, e-mails e telefones;
- fotos, biometrias, templates, PINs, cartoes, QR Codes e tags;
- logs de acesso, callbacks, resultados de Push e payloads brutos;
- IPs, seriais, identificadores de dispositivo e informacoes de rede;
- credenciais, sessoes, licencas, certificados e chaves compartilhadas.

## Regras obrigatorias

- Nao versionar dados reais, secrets, bancos SQLite locais, logs ou artefatos de runtime.
- Nao copiar payload bruto para docs, issues ou commits quando houver dado pessoal/sensivel.
- Mascarar segredos e identificadores em exemplos, screenshots e mensagens de erro.
- Usar User Secrets, variaveis de ambiente ou cofre externo para credenciais e `CallbackSecurity:SharedKey`.
- Validar `AllowedHosts`, shared key e IPs permitidos antes de expor a PoC fora de localhost.
- Limpar `MonitorEvents` e `PushCommands` apenas por acao manual confirmada na UI.
- Preferir expurgo por retencao (`EXPURGAR EVENTOS` ou `EXPURGAR PUSH`) a limpeza total quando o objetivo for reduzir historico.
- Tratar backups SQLite em `artifacts/backups/` como dados sensiveis; nao versionar nem compartilhar esses arquivos.

## Retencao recomendada

| Dado local | Retencao recomendada | Motivo |
| --- | --- | --- |
| `MonitorEvents` | minimo necessario para QA/homologacao | pode conter payload de identificacao, imagem, template ou evento operacional. |
| `PushCommands` | ate concluir analise do ciclo Push | pode conter comando, resultado, device_id, user_id e payload livre. |
| `Logs/` | curto prazo local | pode conter endpoints, status, IPs e metadados operacionais. |
| `integracao_controlid.db*` | ambiente local controlado | banco nao deve ser compartilhado como artefato de produto. |
| `artifacts/backups/` | apenas enquanto necessario para rollback local | copia completa do SQLite pode conter todos os dados sensiveis da PoC. |

## Critérios de aceite de privacidade

- Um fluxo que grava payload bruto deve documentar qual tabela recebe o dado e como limpar o historico local.
- Uma tela que apaga historico local deve exigir confirmacao textual.
- Uma mensagem ao usuario nao deve expor stack trace, secret, sessao, IP interno sensivel ou payload completo por acidente.
- Um exemplo versionado deve usar valores ficticios e placeholders.
- Um ambiente nao `Development` deve falhar no startup sem `AllowedHosts` explicito, `RequireSharedKey=true` e `SharedKey` configurado.
