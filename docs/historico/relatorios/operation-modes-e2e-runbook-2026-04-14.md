# Guia de ponta a ponta dos modos de operação

> **Registro histórico** · Público: QA e integração · Responsável: QA · Referência temporal: 2026-04-14.

Data: 2026-04-14  
Escopo: PoC `Integracao.ControlID.PoC`  
Objetivo: validar em bancada real os modos `Standalone`, `Pro` e `Enterprise` por linha de produto, separando implementação da PoC de homologação física.

## Objetivos desta validação

- confirmar que a PoC aplica corretamente o perfil operacional escolhido;
- validar o comportamento do equipamento após a troca de modo;
- confirmar callbacks, eventos e sinais persistidos pela PoC;
- registrar evidências para concluir a homologação por linha de produto.

## Referências oficiais

- [Introduction to Operating Modes](https://www.controlid.com.br/docs/access-api-en/operating-modes/introduction-to-operating-modes/)
- [Online Identification Events](https://www.controlid.com.br/docs/access-api-en/operating-modes/online-identification-events/)
- [Upgrade iDFace](https://www.controlid.com.br/docs/access-api-en/particularities-of-the-products/update-idface/)
- [Upgrade iDFlex and iDAccess Nano](https://www.controlid.com.br/docs/access-api-en/particularities-of-the-products/update-idflex-idaccess-nano/)
- [Particularities of Control iD's Terminals](https://www.controlid.com.br/docs/access-api-en/particularities-of-the-products/particularities-control-id-terminals/)

## Superfícies da PoC envolvidas

- Centro de modos: `Controllers/OperationModesController.cs` e `Views/OperationModes/Index.cshtml`.
- Modo on-line detalhado: `Controllers/DocumentedFeaturesController.cs`.
- Recursos específicos de produto: `Controllers/ProductSpecificController.cs`.
- Eventos oficiais: `Controllers/OfficialEventsController.cs`.
- Callbacks oficiais: `Controllers/OfficialCallbacksController.cs`.
- Persistência dos sinais: `Services/Callbacks/CallbackIngressService.cs`.

## Pré-requisitos gerais

### Ambiente

- PoC executando em `Development`;
- equipamento acessível por IP e porta;
- sessão oficial válida antes de aplicar `Pro` ou `Enterprise`;
- relógio do equipamento sincronizado;
- coleta de evidências habilitada via print, vídeo curto ou exportação de logs.

### Dados que precisam ser registrados por execução

- data e hora do teste;
- modelo exato do equipamento;
- número de série;
- firmware;
- tipo de licença aplicada;
- IP e porta do equipamento;
- operador responsável.

### Evidências mínimas

- print do centro de modos antes da alteração;
- print ou vídeo da aplicação do perfil;
- print de `OfficialEvents` após a alteração;
- print ou exportação dos callbacks persistidos;
- print do comportamento funcional do equipamento no modo homologado.

## Sequência base antes de cada modo

1. Abrir o painel de conexão e configurar IP, porta e protocolo.
2. Validar que o equipamento responde.
3. Abrir a sessão oficial.
4. Navegar até `Modos de operação`.
5. Registrar `Modo atual`, `Server ID atual`, `Modelo detectado` e `Sessão oficial`.
6. Abrir `Eventos oficiais` em outra aba da PoC para acompanhar o comportamento após a mudança.

## Homologação do modo Standalone

### Quando usar

- contingência local;
- operação sem dependência do servidor online;
- validação de comportamento offline do terminal.

### Passos na PoC

1. No centro de modos, conferir se não existe bloqueio de sessão ou licença.
2. Acionar `Aplicar Standalone`.
3. Confirmar a mensagem de retorno da PoC.
4. Registrar o JSON bruto da última resposta oficial.
5. Recarregar a tela e confirmar que o `Modo atual` passou a `Standalone`.

### Validações no equipamento

- o terminal permanece operando localmente;
- a identificação não depende do servidor online;
- o comportamento de contingência está coerente com a linha do produto;
- reboot e reconexão não reativam indevidamente o modo online.

### Evidências esperadas

- `online = false` ou equivalente refletido na leitura do perfil;
- nenhum callback obrigatório de identificação online disparado como fluxo normal;
- logs da PoC sem falha de persistência ou timeout na alteração.

## Homologação do modo Pro

### Quando usar

- modo online com identificação local habilitada;
- necessidade de enriquecer o fluxo com callbacks oficiais;
- cenários em que o servidor acompanha, mas o terminal ainda identifica localmente.

### Passos na PoC

1. Validar sessão no centro de modos.
2. Se aplicável, solicitar `Upgrade Pro` com a senha ou licença oficial.
3. Preencher `ServerName`, `ServerUrl`, `PublicKey` e `MaxRequestAttempts`.
4. Decidir se `ReuseExistingDevice` e `ExtractTemplate` ficam ativos.
5. Acionar `Aplicar Pro`.
6. Conferir a resposta oficial e o `Modo atual` após refresh.

### Validações no equipamento

- identificação local funcionando em modo online;
- callbacks de identificação online chegando na PoC;
- `server_id` reaproveitado ou criado corretamente;
- comportamento estável após reboot.

### Evidências esperadas

- eventos em `OfficialEvents` relacionados a identificação online;
- registros persistidos em `OfficialCallbacks`;
- nenhum erro de licença, sessão ou `server_id`;
- prova de que o equipamento continua identificando localmente.

## Homologação do modo Enterprise

### Quando usar

- decisão de identificação centralizada no servidor;
- topologias nas quais a linha compatível exige licença `Enterprise`;
- fluxos em que `local_identification` deve permanecer desativado.

### Passos na PoC

1. Validar sessão no centro de modos.
2. Se aplicável, solicitar `Upgrade Enterprise`.
3. Preencher `ServerName`, `ServerUrl`, `PublicKey` e `MaxRequestAttempts`.
4. Confirmar se o `server_id` atual pode ser reutilizado.
5. Acionar `Aplicar Enterprise`.
6. Registrar retorno, callbacks e sinais recentes da tela.

### Validações no equipamento

- identificação centralizada funcionando conforme a documentação da linha;
- callbacks relevantes chegando na PoC;
- reconexão do equipamento sem perda da configuração aplicada;
- comportamento coerente após reboot e nova autenticação.

### Evidências esperadas

- `online = true` com `local_identification = false` ou equivalente refletido na PoC;
- sinais recentes coerentes com o modo aplicado;
- logs sem erro de licença, timeout ou incompatibilidade de produto.

## Lista de verificação de regressão após cada homologação

- conexão continua funcional;
- sessão continua válida;
- `OfficialApi` ainda invoca endpoints autenticados;
- `PushCenter` e `OfficialEvents` seguem operando;
- callbacks continuam persistindo;
- nenhuma tela da PoC apresenta regressão visual ou de linguagem.

## Critérios para declarar homologação concluída

Uma linha de produto só pode ser marcada como homologada quando houver:

1. evidências do modo aplicado antes e depois;
2. sucesso funcional observado no equipamento real;
3. callbacks e eventos coerentes com o modo;
4. reboot e reconexão validados sem regressão;
5. registro do firmware, número de série e licença usada no teste.

## Modelo de registro por execução

Use o formato abaixo para cada execução real:

```md
## Linha testada
- Produto:
- Número de série:
- Firmware:
- Licença:
- Operador:
- Data:

## Modo validado
- Standalone | Pro | Enterprise

## Resultado
- Sucesso | Falha | Parcial

## Evidências
- Links/prints:

## Observações
- Comportamento após reboot:
- Callbacks recebidos:
- Ajustes necessários:
```

## Conclusão

Com este guia operacional, a PoC passa a ter:

- implementação funcional dos modos dentro da própria interface;
- matriz explícita de cobertura por linha de produto;
- roteiro E2E para transformar implementação em homologação real.

O que ainda depende do ambiente externo não é código adicional da PoC, e sim execução em hardware real com licença e firmware compatíveis.

## Uso como protocolo

Este arquivo preserva o roteiro de 2026-04-14. Antes de reutilizá-lo:

1. compare rotas e campos com [docs/integracao-controlid/operation-modes-implementation.md](../../integracao-controlid/operation-modes-implementation.md);
2. registre commit, modelo, firmware, licença, rede e operador;
3. execute somente em bancada autorizada e com plano de retorno;
4. grave o resultado em artefato separado, sem sobrescrever o protocolo;
5. vincule falhas à matriz de homologação e aos critérios F05/REQ-005.

O roteiro não é evidência de execução. Cada resultado deve declarar modo inicial,
modo solicitado, configuração relida, callbacks, reinicialização e risco residual.

## Relação com o protocolo vivo

Os passos invariantes foram consolidados em
[docs/integracao-controlid/operation-modes-implementation.md](../../integracao-controlid/operation-modes-implementation.md). Em conflito, prevalecem código, testes e
esse documento vivo; este arquivo preserva detalhes históricos e o modelo de
registro da bancada de 2026-04-14.

## Navegação documental

- [Voltar ao índice deste domínio](README.md).
- [Abrir a central de documentação](../../README.md).
