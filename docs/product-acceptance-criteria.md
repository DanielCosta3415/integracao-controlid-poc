# Critérios de aceite do produto

Este documento transforma a analise de produto em critérios verificáveis para evoluções futuras da PoC.

## Conexao e sessao

- Dado host/protocolo/porta invalidos, quando o usuario tenta conectar, a PoC rejeita a entrada antes de chamar o equipamento.
- Dado equipamento conectado, quando a API de informacoes retorna serial/firmware, a dashboard mostra o contexto do dispositivo ativo.
- Dado endpoint autenticado, quando nao ha sessao Control iD ativa, a PoC bloqueia a chamada com mensagem segura.
- Dado login bem-sucedido, quando a resposta contem sessao, a PoC persiste a sessao no contexto ASP.NET.

## API oficial e objetos

- Dado endpoint catalogado como callback, quando o usuario tenta invocar pela tela tecnica, a PoC informa que o endpoint e servido pela aplicacao.
- Dado resposta binaria, quando a API oficial retorna conteudo nao textual, a PoC preserva o retorno em Base64/download.
- Dado JSON invalido em create/modify/destroy, quando o usuario envia o formulario, a PoC nao deve chamar o equipamento.
- Dado `destroy-objects`, quando a confirmacao textual nao corresponde a `DESTROY <objeto>`, a PoC nao deve chamar o equipamento.

## Operacoes administrativas

- Dado acao de rede, reboot, modo update, remocao de admins ou reset de fabrica, quando a confirmacao textual estiver ausente/incorreta, a PoC nao deve chamar o equipamento.
- Dado alteracao de rede aplicada com sucesso, a mensagem deve orientar reconexao quando IP ou porta puderem mudar.
- Dado reset de fabrica sem preservar rede, a UI deve evidenciar alto impacto antes do envio.

## Monitor e callbacks

- Dado callback aceito, a PoC persiste `EventType`, `DeviceId`, `UserId`, payload bruto e status `received`.
- Dado shared key obrigatoria ausente/invalida, a PoC rejeita o callback e nao persiste evento.
- Dado payload acima do limite configurado, a PoC responde `413` e nao persiste evento.
- Dado limpeza de eventos oficiais, a PoC exige confirmacao textual antes de apagar historico local.

## Push

- Dado payload JSON valido na fila Push, a PoC cria comando `pending`.
- Dado payload JSON invalido na fila Push, a PoC rejeita antes de persistir.
- Dado comando pendente elegivel, quando o equipamento chama `GET /push`, a PoC retorna o payload e marca `delivered`.
- Dado `POST /result` sem status, a PoC registra `completed`.
- Dado evento legado com JSON invalido, a PoC persiste corpo bruto como `legacy_push_event` e status `received`.
- Dado limpeza da fila Push, a PoC exige confirmacao textual antes de apagar historico local.

## Privacidade e operacao

- Dado ambiente nao `Development`, a aplicacao deve falhar sem `AllowedHosts` explicito, `RequireSharedKey=true` e `SharedKey`.
- Dado payload com dado pessoal/sensivel, a PoC deve evitar exposicao em mensagens publicas, docs e exemplos versionados.
- Dado nova integracao com equipamento real/licenca, a validacao deve declarar firmware, modelo, modo operacional, URL publica e limitacoes observadas.
