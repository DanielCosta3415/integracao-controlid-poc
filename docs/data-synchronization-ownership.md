# Sincronização e propriedade dos dados

> **Documento vivo** · Público: produto, integração, dados e operação · Responsável: arquitetura de dados · Última validação: 2026-08-03.

Este documento define qual componente é fonte de verdade para cada estado e como
evitar conflitos entre a PoC, o SQLite, o navegador, o equipamento e um servidor
externo. A aplicação atual não executa uma sincronização completa automática de
toda a base Control iD em segundo plano.

## Armazenamentos envolvidos

| Armazenamento | Conteúdo | Papel atual |
| --- | --- | --- |
| SQLite da PoC | Contas locais, inventários auxiliares, Monitor, Push, logs e sessões históricas | Estado operacional local; não é réplica completa garantida do equipamento. |
| Sessão ASP.NET | URL, identificação do equipamento e `session` oficial | Contexto temporário de um navegador. |
| Banco interno do equipamento | Usuários, credenciais, regras, portais, horários, logs e configurações suportadas | Fonte de verdade das chamadas Access API e do comportamento Standalone. |
| Servidor externo Pro/Enterprise | Decisões e, no Enterprise, identificação | Dependência externa; não está implementado como motor produtivo nesta PoC. |
| Arquivos/relatórios | Backups, logs, SBOM e evidências | Diagnóstico e recuperação, fora do Git quando contêm estado real. |

## Fonte de verdade por domínio

| Dado | Fonte de verdade | Cópia/visão local | Regra de reconciliação |
| --- | --- | --- | --- |
| Conta e papel da PoC | SQLite `Users` | Claims do cookie | Novo login recarrega o papel; não confundir com usuário do equipamento. |
| Equipamento selecionado | Sessão ASP.NET | Painel de conexão | Expira com a sessão; não representa inventário multiusuário compartilhado. |
| Sessão oficial | Equipamento | `ControlID_SessionString` na sessão ASP.NET | `session_is_valid.fcgi` decide validade. |
| Modo de operação | Configuração do equipamento | Tela de modos | Sempre reler após escrita; não há tabela local de modo atual. |
| Usuários e credenciais de acesso | Equipamento ou sistema mestre definido pelo projeto real | Entidades/telas locais podem ser auxiliares | O dono real precisa ser decidido antes de sincronização bidirecional. |
| Regras, grupos, horários e portais | Equipamento em Standalone; servidor em modo on-line conforme arquitetura | Telas e modelos da PoC | Aplicar em ordem e confirmar por leitura. |
| Eventos de Monitor/callback | Equipamento | SQLite `MonitorEvents` | Cada evento aceito vira evidência local; duplicidade depende do contrato de origem. |
| Fila Push | SQLite `PushCommands` | Equipamento faz consultas periódicas | Estado local controla `pending`, `delivered` e resultado. |
| Logs de acesso do terminal | Equipamento | Consulta/Monitor/relatórios | Paginar e deduplicar por identificador/contexto; não assumir entrega exatamente uma vez. |

## Responsabilidade conforme o modo

### Standalone

O equipamento identifica e autoriza. O sistema integrador precisa manter no
terminal usuários, credenciais, grupos, horários e regras suficientes para a
operação local. A Control iD recomenda esse modo como padrão.

### Pro

O equipamento identifica; o servidor autoriza. Usuários e credenciais necessários
à identificação continuam sincronizados no terminal, enquanto regras de decisão
ficam no servidor. Em contingência, a base local precisa permitir a política
aprovada.

### Enterprise

Identificação e autorização ficam no servidor. A PoC demonstra configuração e
contratos, mas não fornece um motor biométrico produtivo nem uma política completa
de autorização centralizada.

Referência: [modos de operação](https://www.controlid.com.br/docs/access-api-pt/modos-de-operacao/introducao-aos-modos-de-operacao/).

## Ordem recomendada para cadastros Standalone

Para regras indiretas, fluxo recomendado pela documentação oficial:

1. criar `users`;
2. criar `groups`;
3. vincular `user_groups`;
4. criar `access_rules`;
5. vincular `group_access_rules`;
6. criar `time_zones`;
7. criar `time_spans`;
8. vincular `access_rule_time_zones`;
9. vincular regras aos portais por `portal_access_rules`.

Regras diretas em `user_access_rules` são apropriadas apenas para exceções. Ao
remover dados, avalie os vínculos dependentes antes do objeto principal; a PoC não
deve inventar cascata que o contrato oficial não garanta.

Referência: [cadastrar usuários e regras](https://www.controlid.com.br/docs/access-api-pt/primeiros-passos/cadastrar-usuarios-e-suas-regras/).

## Paginação e grandes bases

`load_objects.fcgi` aceita `fields`, `where`, `limit`, `offset` e `order`. Para
paginação, `offset` só deve ser usado com `limit`. Não carregue toda a base para
uma tela quando o volume for desconhecido.

Para grande carga de biometria, a documentação oficial orienta:

1. chamar `template_sync_init.fcgi`;
2. carregar templates em lotes com `create_objects.fcgi`;
3. chamar `template_sync_end.fcgi` para reativar e executar a sincronização.

Esse procedimento altera o estado do equipamento e deve ser homologado no modelo
real. A PoC cataloga os endpoints, mas não transforma uma carga massiva em
operação automaticamente segura.

Referência: [carregar objetos](https://www.controlid.com.br/docs/access-api-en/objects/load-objects/).

## Identificadores e duplicidade

- Preserve IDs remotos como chaves do contrato do equipamento; IDs locais podem
  representar outro domínio.
- Usuário, cartão e vínculos possuem unicidades próprias descritas na lista
  oficial de objetos.
- Não deduplique pessoa apenas por nome.
- Cartão, matrícula e credenciais devem ser normalizados conforme contrato e
  contexto do cliente, sem registrar valor em log.
- Callbacks aceitos podem repetir eventos; use identificadores oficiais quando
  disponíveis e registre a estratégia antes de eliminar duplicidade.

## Conflitos de escrita

Antes de habilitar mais de um escritor para o mesmo equipamento, defina:

1. sistema mestre por objeto;
2. versão ou marcador de alteração;
3. precedência em conflito;
4. janela e direção de sincronização;
5. tratamento de exclusão;
6. idempotência;
7. trilha de auditoria;
8. procedimento de reconciliação.

No estado atual, a PoC não implementa controle distribuído de versão, fila de
sincronização bidirecional nem resolução automática de conflitos. Operar ao mesmo
tempo com outro gerenciador pode sobrescrever ou divergir dados.

## Exclusão e expiração

- `destroy_objects.fcgi` é destrutivo e exige confirmação textual na PoC.
- A exclusão no equipamento não elimina automaticamente cópias locais, eventos,
  logs ou backups.
- O expurgo de `MonitorEvents` e `PushCommands` é separado e exige confirmação.
- Visitantes e credenciais expiradas podem possuir comportamento de limpeza
  específico do firmware; valide antes de depender dele.
- Direitos de titulares exigem localizar dados em equipamento, SQLite, logs e
  cópias de segurança, conforme `privacy-governance-runbook.md`.

## Reconciliação operacional

| Verificação | Evidência esperada |
| --- | --- |
| Sessão | `session_is_valid.fcgi` válido para o equipamento correto. |
| Configuração | `get_configuration.fcgi` confirma valor após escrita. |
| Objeto | `load_objects.fcgi` com `where` confirma ID e campos necessários. |
| Regra | Vínculos de grupo, horário e portal existem e não são órfãos. |
| Evento | Monitor/log oficial confirma efeito esperado. |
| Push | Comando tem entrega e resultado correlacionados. |
| Exclusão | Objeto não aparece na leitura e retenções locais foram avaliadas separadamente. |

## Procedimento para sincronização nova

1. Definir escopo e fonte de verdade.
2. Inventariar modelo, firmware, licença, capacidade e objetos afetados.
3. Fazer cópia de segurança aprovada do estado relevante.
4. Ensaiar com o simulador (stub) e dados fictícios.
5. Ler estado remoto antes de escrever.
6. Aplicar lote pequeno e idempotente quando possível.
7. Reler e comparar sem expor dados pessoais.
8. Medir duração, erros e volume.
9. Definir retorno e tratamento de parciais antes de ampliar.
10. Atualizar a matriz de compatibilidade e os critérios de aceite.

Consulte também `data-model-and-recovery.md`, `privacy-and-data-retention.md` e
`integration-contracts.md`.
