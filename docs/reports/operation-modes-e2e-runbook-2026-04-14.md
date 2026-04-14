# Runbook E2E dos modos de operacao

Data: 2026-04-14  
Escopo: PoC `Integracao.ControlID.PoC`  
Objetivo: validar em bancada real os modos `Standalone`, `Pro` e `Enterprise` por linha de produto, separando implementacao da PoC de homologacao fisica.

## Objetivos desta validacao

- confirmar que a PoC aplica corretamente o perfil operacional escolhido;
- validar o comportamento do equipamento apos a troca de modo;
- confirmar callbacks, eventos e sinais persistidos pela PoC;
- registrar evidencias para concluir a homologacao por linha de produto.

## Referencias oficiais

- [Introduction to Operating Modes](https://www.controlid.com.br/docs/access-api-en/operating-modes/introduction-to-operating-modes/)
- [Online Identification Events](https://www.controlid.com.br/docs/access-api-en/operating-modes/online-identification-events/)
- [Upgrade iDFace](https://www.controlid.com.br/docs/access-api-en/particularities-of-the-products/update-idface/)
- [Upgrade iDFlex and iDAccess Nano](https://www.controlid.com.br/docs/access-api-en/particularities-of-the-products/update-idflex-idaccess-nano/)
- [Particularities of Control iD's Terminals](https://www.controlid.com.br/docs/access-api-en/particularities-of-the-products/particularities-control-id-terminals/)

## Superficies da PoC envolvidas

- Centro de modos: [OperationModesController.cs](</C:/Users/danie/OneDrive/Desktop/Minhas Coisas/Projetos/Pessoais/Control iD/Integracao.ControlID.PoC/Controllers/OperationModesController.cs:1>) e [Views/OperationModes/Index.cshtml](</C:/Users/danie/OneDrive/Desktop/Minhas Coisas/Projetos/Pessoais/Control iD/Integracao.ControlID.PoC/Views/OperationModes/Index.cshtml:1>)
- Modo online detalhado: [DocumentedFeaturesController.cs](</C:/Users/danie/OneDrive/Desktop/Minhas Coisas/Projetos/Pessoais/Control iD/Integracao.ControlID.PoC/Controllers/DocumentedFeaturesController.cs:1>)
- Recursos especificos de produto: [ProductSpecificController.cs](</C:/Users/danie/OneDrive/Desktop/Minhas Coisas/Projetos/Pessoais/Control iD/Integracao.ControlID.PoC/Controllers/ProductSpecificController.cs:1>)
- Eventos oficiais: [OfficialEventsController.cs](</C:/Users/danie/OneDrive/Desktop/Minhas Coisas/Projetos/Pessoais/Control iD/Integracao.ControlID.PoC/Controllers/OfficialEventsController.cs:1>)
- Callbacks oficiais: [OfficialCallbacksController.cs](</C:/Users/danie/OneDrive/Desktop/Minhas Coisas/Projetos/Pessoais/Control iD/Integracao.ControlID.PoC/Controllers/OfficialCallbacksController.cs:1>)
- Persistencia dos sinais: [CallbackIngressService.cs](</C:/Users/danie/OneDrive/Desktop/Minhas Coisas/Projetos/Pessoais/Control iD/Integracao.ControlID.PoC/Services/Callbacks/CallbackIngressService.cs:1>)

## Pre-requisitos gerais

### Ambiente

- PoC executando em `Development`;
- equipamento acessivel por IP e porta;
- sessao oficial valida antes de aplicar `Pro` ou `Enterprise`;
- relogio do equipamento sincronizado;
- coleta de evidencias habilitada via print, video curto ou exportacao de logs.

### Dados que precisam ser registrados por execucao

- data e hora do teste;
- modelo exato do equipamento;
- numero de serie;
- firmware;
- tipo de licenca aplicada;
- IP e porta do equipamento;
- operador responsavel.

### Evidencias minimas

- print do centro de modos antes da alteracao;
- print ou video da aplicacao do perfil;
- print de `OfficialEvents` apos a alteracao;
- print ou exportacao dos callbacks persistidos;
- print do comportamento funcional do equipamento no modo homologado.

## Sequencia base antes de cada modo

1. Abrir o painel de conexao e configurar IP, porta e protocolo.
2. Validar que o equipamento responde.
3. Abrir a sessao oficial.
4. Navegar ate `Modos de operacao`.
5. Registrar `Modo atual`, `Server ID atual`, `Modelo detectado` e `Sessao oficial`.
6. Abrir `Eventos oficiais` em outra aba da PoC para acompanhar o comportamento apos a mudanca.

## Homologacao do modo Standalone

### Quando usar

- contingencia local;
- operacao sem dependencia do servidor online;
- validacao de comportamento offline do terminal.

### Passos na PoC

1. No centro de modos, conferir se nao existe bloqueio de sessao ou licenca.
2. Acionar `Aplicar Standalone`.
3. Confirmar a mensagem de retorno da PoC.
4. Registrar o JSON bruto da ultima resposta oficial.
5. Recarregar a tela e confirmar que o `Modo atual` passou a `Standalone`.

### Validacoes no equipamento

- o terminal permanece operando localmente;
- a identificacao nao depende do servidor online;
- o comportamento de contingencia esta coerente com a linha do produto;
- reboot e reconexao nao reativam indevidamente o modo online.

### Evidencias esperadas

- `online = false` ou equivalente refletido na leitura do perfil;
- nenhum callback obrigatorio de identificacao online disparado como fluxo normal;
- logs da PoC sem falha de persistencia ou timeout na alteracao.

## Homologacao do modo Pro

### Quando usar

- modo online com identificacao local habilitada;
- necessidade de enriquecer o fluxo com callbacks oficiais;
- cenarios em que o servidor acompanha, mas o terminal ainda identifica localmente.

### Passos na PoC

1. Validar sessao no centro de modos.
2. Se aplicavel, solicitar `Upgrade Pro` com a senha ou licenca oficial.
3. Preencher `ServerName`, `ServerUrl`, `PublicKey` e `MaxRequestAttempts`.
4. Decidir se `ReuseExistingDevice` e `ExtractTemplate` ficam ativos.
5. Acionar `Aplicar Pro`.
6. Conferir a resposta oficial e o `Modo atual` apos refresh.

### Validacoes no equipamento

- identificacao local funcionando em modo online;
- callbacks de identificacao online chegando na PoC;
- `server_id` reaproveitado ou criado corretamente;
- comportamento estavel apos reboot.

### Evidencias esperadas

- eventos em `OfficialEvents` relacionados a identificacao online;
- registros persistidos em `OfficialCallbacks`;
- nenhum erro de licenca, sessao ou `server_id`;
- prova de que o equipamento continua identificando localmente.

## Homologacao do modo Enterprise

### Quando usar

- decisao de identificacao centralizada no servidor;
- topologias nas quais a linha compativel exige licenca `Enterprise`;
- fluxos em que `local_identification` deve permanecer desativado.

### Passos na PoC

1. Validar sessao no centro de modos.
2. Se aplicavel, solicitar `Upgrade Enterprise`.
3. Preencher `ServerName`, `ServerUrl`, `PublicKey` e `MaxRequestAttempts`.
4. Confirmar se o `server_id` atual pode ser reutilizado.
5. Acionar `Aplicar Enterprise`.
6. Registrar retorno, callbacks e sinais recentes da tela.

### Validacoes no equipamento

- identificacao centralizada funcionando conforme a documentacao da linha;
- callbacks relevantes chegando na PoC;
- reconexao do equipamento sem perda da configuracao aplicada;
- comportamento coerente apos reboot e nova autenticacao.

### Evidencias esperadas

- `online = true` com `local_identification = false` ou equivalente refletido na PoC;
- sinais recentes coerentes com o modo aplicado;
- logs sem erro de licenca, timeout ou incompatibilidade de produto.

## Checklist de regressao apos cada homologacao

- conexao continua funcional;
- sessao continua valida;
- `OfficialApi` ainda invoca endpoints autenticados;
- `PushCenter` e `OfficialEvents` seguem operando;
- callbacks continuam persistindo;
- nenhuma tela da PoC apresenta regressao visual ou de linguagem.

## Criterios para declarar homologacao concluida

Uma linha de produto so pode ser marcada como homologada quando houver:

1. evidencias do modo aplicado antes e depois;
2. sucesso funcional observado no equipamento real;
3. callbacks e eventos coerentes com o modo;
4. reboot e reconexao validados sem regressao;
5. registro do firmware, numero de serie e licenca usada no teste.

## Modelo de registro por execucao

Use o formato abaixo para cada execucao real:

```md
## Linha testada
- Produto:
- Numero de serie:
- Firmware:
- Licenca:
- Operador:
- Data:

## Modo validado
- Standalone | Pro | Enterprise

## Resultado
- Sucesso | Falha | Parcial

## Evidencias
- Links/prints:

## Observacoes
- Comportamento apos reboot:
- Callbacks recebidos:
- Ajustes necessarios:
```

## Conclusao

Com este runbook, a PoC passa a ter:

- implementacao funcional dos modos dentro da propria interface;
- matriz explicita de cobertura por linha de produto;
- roteiro E2E para transformar implementacao em homologacao real.

O que ainda depende do ambiente externo nao e codigo adicional da PoC, e sim execucao em hardware real com licenca e firmware compativeis.
