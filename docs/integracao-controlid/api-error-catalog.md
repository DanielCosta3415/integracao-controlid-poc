# Catálogo de erros da integração

> **Referência** · Público: usuários técnicos, desenvolvimento, QA e suporte · Responsável: Engenharia · Última validação: 2026-08-12.

Este catálogo relaciona sintomas da PoC com a camada que pode ter falhado. A
Access API não possui um único envelope de erro universal para todos os produtos
e firmwares; preserve a resposta sanitizada e valide o contrato específico antes
de interpretar um campo como definitivo.

## Camadas de erro

| Camada | Exemplos | Responsável inicial |
| --- | --- | --- |
| Interface/entrada | Campo obrigatório, JSON inválido, confirmação ausente | Usuário/Produto |
| Conta local | Credencial inválida, papel insuficiente, cookie expirado | Segurança da PoC |
| Contexto do equipamento | URL ausente, equipamento não conectado, sessão ausente | Integração |
| Rede | DNS, conexão recusada, TLS, tempo limite | Infraestrutura |
| Access API | HTTP não 2xx, erro funcional, resposta inesperada | Integração/Fornecedor |
| Ingresso | IP, chave, HMAC, repetição, limite de corpo/taxa | Segurança/Plataforma |
| Persistência | SQLite indisponível, restrição de integridade, disco | Dados/SRE |
| Efeito físico | Relé, porta, catraca, sensor, licença | Operação física/Fornecedor |

## Respostas HTTP locais

| Status | Significado provável na PoC | Conduta |
| ---: | --- | --- |
| 200 | Chamada local aceita; não prova efeito físico | Validar corpo, releitura, evento e hardware. |
| 400 | Entrada inválida ou contrato não atendido | Corrigir campo, tipo, JSON ou confirmação. |
| 401 | Login local ausente ou chave compartilhada inválida no ingresso | Identificar a rota e usar a credencial apropriada. |
| 403 | Papel insuficiente, origem/IP ou assinatura não permitida | Não remover a proteção; corrigir identidade/topologia. |
| 404 | Rota, ID local ou endpoint não encontrado | Conferir método, caminho e versão. |
| 408 | Tempo limite representado pelo equipamento/proxy | Diagnosticar rede e disponibilidade. |
| 409 | Repetição de nonce ou conflito conhecido | Gerar nonce novo e investigar repetição; não reenviar escrita às cegas. |
| 413 | Corpo ou resposta acima do limite | Reduzir lote/mídia ou revisar limite com justificativa. |
| 429 | Limitação de taxa | Aguardar janela e investigar volume/origem. |
| 500 | Erro interno não tratado | Usar correlação, registros seguros e procedimento de incidente. |
| 502 | Resposta externa inválida ou acima do contrato do gateway/invocador | Preservar metadados e comparar firmware. |
| 503 | Dependência, SQLite ou circuito indisponível | Restabelecer dependência e validar prontidão. |

## Mensagens funcionais da PoC

| Mensagem/sintoma | Interpretação | Próxima ação |
| --- | --- | --- |
| “Nenhum dispositivo conectado” | Não existe URL de equipamento no contexto ASP.NET | Conectar pelo painel após teste de rede. |
| “É necessário conectar-se e autenticar...” | A operação exige equipamento e `session` oficial | Conectar, fazer login e validar sessão. |
| “A resposta do dispositivo não continha uma sessão válida” | `login.fcgi` não retornou `session` utilizável | Conferir credenciais, firmware e corpo sanitizado. |
| “Esta operação exige uma sessão ativa” | Endpoint catalogado exige `session` | Fazer novo login oficial. |
| “Tempo limite excedido...” | A chamada ultrapassou `ConnectionTimeoutSeconds` | Diagnosticar rede/dispositivo; não repetir escrita automaticamente. |
| “Circuito temporariamente aberto” ou equivalente | Limite de falhas transitórias atingido | Corrigir causa e aguardar `BreakDurationSeconds`. |
| “Este endpoint é um callback...” | Rota é servida pela PoC e não deve ser invocada contra o equipamento | Configurar o equipamento para chamá-la. |
| “Revise os dados...” | ModelState ou validação local falhou | Corrigir entrada antes de chamar o equipamento. |
| “Acesso negado” | Papel local não atende à ação | Usar conta administrativa autorizada. |

## Erros da Access API

Respostas oficiais podem usar status HTTP, JSON com `error`, campos de sucesso ou
corpo textual/binário. O tratamento deve considerar o endpoint e o firmware.

Exemplos documentados oficialmente:

- `login.fcgi` deve retornar `session`; ausência indica resposta incompatível;
- cadastro facial remoto pode retornar `FACE_EXISTS` e `match_user_id` quando a
  face corresponde a outro usuário;
- operações de objeto podem rejeitar campo, tipo, vínculo ou unicidade;
- ações de produto podem falhar por modelo, licença, porta ou parâmetro;
- em chamadas assíncronas, o resultado pode chegar depois pelo Monitor em vez de
  estar no corpo original.

Referência: [cadastro remoto](https://www.controlid.com.br/docs/access-api-pt/acoes/cadastro-remoto-biometria-facial-cartao/).

## Validação por tipo de operação

| Tipo | Antes | Depois | Retentativa automática |
| --- | --- | --- | --- |
| Leitura | Sessão, filtro, limite e endpoint | Validar esquema e paginação | Somente se contrato e política permitirem. A PoC não faz retentativa. |
| Criação | Unicidade, vínculos e lote | Reler IDs e campos | Não; pode duplicar. |
| Modificação | Estado anterior e carga útil mínima | Reler campos alterados | Não; pode sobrescrever estado. |
| Exclusão | Confirmação, dependências e backup | Confirmar ausência e avaliar retenções | Nunca automaticamente. |
| Ação física | Produto, porta, sentido e autorização | Observar evento/sensor | Nunca automaticamente. |
| Configuração | Instantâneo anterior e retorno | Reler configuração | Não automaticamente. |
| Push | Comando e dispositivo alvo | Correlacionar resultado | Somente com idempotência comprovada. |

## Resposta inesperada

Quando o fluxo espera JSON e recebe outro conteúdo:

1. registre status, `Content-Type`, tamanho, endpoint, firmware e correlação;
2. preserve o corpo somente em local restrito e minimizado;
3. verifique se a resposta é binária prevista para mídia/relatório;
4. compare a página oficial e a matriz de compatibilidade;
5. crie teste de contrato antes de ajustar o analisador ou DTO;
6. não exponha rastreamento de pilha ou corpo bruto ao usuário.

## Dados permitidos em registros e chamados

Permitidos quando necessários:

- ID interno da definição do endpoint;
- método e caminho sem consulta sensível;
- status HTTP, duração e tamanho;
- referência pseudonimizada de equipamento/usuário;
- modelo, firmware e classe de licença;
- `X-Correlation-ID`.

Proibidos:

- senha, `session`, chave compartilhada e assinatura;
- cabeçalho de autenticação completo;
- foto, biometria, cartão, QR Code e documento;
- carga útil integral de usuário ou evento;
- IP/serial real em canal público.

## Critério de escalonamento

Escalone para o fornecedor quando o contrato oficial e a rede estiverem
confirmados, a falha for reproduzível no firmware real e houver evidência
sanitizada. Escalone internamente para segurança em caso de 401/403 inesperado,
repetição, segredo suspeito ou exposição; para SRE em 5xx/prontidão; e para operação
física quando HTTP indicar sucesso sem efeito.

O roteiro detalhado está em [troubleshooting-controlid.md](../operacao/troubleshooting-controlid.md) e os contratos em
[integration-contracts.md](integration-contracts.md).

## Navegação documental

- [Voltar ao índice deste domínio](README.md).
- [Abrir a central de documentação](../README.md).
