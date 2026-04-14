# Matriz de homologação dos modos de operação

Data: 2026-04-14  
Escopo: PoC `Integracao.ControlID.PoC`  
Objetivo: registrar, por linha de produto e por modo, o que já está coberto dentro da PoC e o que ainda precisa de homologação em equipamento real.

## Premissas desta matriz

- Esta matriz separa claramente `cobertura implementada na PoC` de `homologação em hardware real`.
- Um item marcado como `Implementado na PoC` significa que a jornada existe no código, com UI, controller e integração oficial correspondente.
- Um item marcado como `Pendente de homologação real` significa que ainda falta validar o comportamento no equipamento físico, firmware e licença corretos.
- As referências oficiais usadas nesta matriz são:
  - [Introduction to Operating Modes](https://www.controlid.com.br/docs/access-api-en/operating-modes/introduction-to-operating-modes/)
  - [Online Identification Events](https://www.controlid.com.br/docs/access-api-en/operating-modes/online-identification-events/)
  - [Upgrade iDFace](https://www.controlid.com.br/docs/access-api-en/particularities-of-the-products/update-idface/)
  - [Upgrade iDFlex and iDAccess Nano](https://www.controlid.com.br/docs/access-api-en/particularities-of-the-products/update-idflex-idaccess-nano/)
  - [Particularities of Control iD's Terminals](https://www.controlid.com.br/docs/access-api-en/particularities-of-the-products/particularities-control-id-terminals/)
  - [iDFace - Getting Started](https://www.controlid.com.br/docs/idface-en/getting-started/)
  - [iDFlex Introduction](https://www.controlid.com.br/docs/idflex-en/)

## Cobertura implementada na PoC

Superfícies e serviços que sustentam os modos:

- Hub unificado: [Controllers/OperationModesController.cs](</C:/Users/danie/OneDrive/Desktop/Minhas Coisas/Projetos/Pessoais/Control iD/Integracao.ControlID.PoC/Controllers/OperationModesController.cs:1>)
- UI do hub: [Views/OperationModes/Index.cshtml](</C:/Users/danie/OneDrive/Desktop/Minhas Coisas/Projetos/Pessoais/Control iD/Integracao.ControlID.PoC/Views/OperationModes/Index.cshtml:1>)
- Payloads oficiais de modo: [Services/OperationModes/OperationModesPayloadFactory.cs](</C:/Users/danie/OneDrive/Desktop/Minhas Coisas/Projetos/Pessoais/Control iD/Integracao.ControlID.PoC/Services/OperationModes/OperationModesPayloadFactory.cs:1>)
- Resolução do perfil atual: [Services/OperationModes/OperationModesProfileResolver.cs](</C:/Users/danie/OneDrive/Desktop/Minhas Coisas/Projetos/Pessoais/Control iD/Integracao.ControlID.PoC/Services/OperationModes/OperationModesProfileResolver.cs:1>)
- Reaproveitamento no tópico documentado: [Services/DocumentedFeatures/DocumentedFeaturesPayloadFactory.cs](</C:/Users/danie/OneDrive/Desktop/Minhas Coisas/Projetos/Pessoais/Control iD/Integracao.ControlID.PoC/Services/DocumentedFeatures/DocumentedFeaturesPayloadFactory.cs:1>)
- Upgrade Pro: [Controllers/ProductSpecificController.cs](</C:/Users/danie/OneDrive/Desktop/Minhas Coisas/Projetos/Pessoais/Control iD/Integracao.ControlID.PoC/Controllers/ProductSpecificController.cs:31>)
- Upgrade Enterprise: [Controllers/ProductSpecificController.cs](</C:/Users/danie/OneDrive/Desktop/Minhas Coisas/Projetos/Pessoais/Control iD/Integracao.ControlID.PoC/Controllers/ProductSpecificController.cs:56>)
- Callbacks relevantes: [Controllers/OfficialCallbacksController.cs](</C:/Users/danie/OneDrive/Desktop/Minhas Coisas/Projetos/Pessoais/Control iD/Integracao.ControlID.PoC/Controllers/OfficialCallbacksController.cs:20>)
- Persistência dos sinais: [Services/Callbacks/CallbackIngressService.cs](</C:/Users/danie/OneDrive/Desktop/Minhas Coisas/Projetos/Pessoais/Control iD/Integracao.ControlID.PoC/Services/Callbacks/CallbackIngressService.cs:27>)

## Matriz por produto e por modo

Legenda:

- `Implementado na PoC`: a jornada existe e está operável dentro da PoC.
- `Não aplicável`: a documentação oficial consultada não posiciona esse modo como jornada principal para a linha analisada.
- `Pendente de homologação real`: a PoC implementa a trilha, mas ainda falta validação física em bancada.
- `Depende de licença`: a PoC cobre o fluxo, mas o sucesso depende de senha/licença oficial fornecida pela Control iD.

| Linha / produto | Standalone | Pro | Enterprise | Cobertura atual da PoC | Status de homologação real |
| --- | --- | --- | --- | --- | --- |
| `iDFace` | Implementado na PoC | Implementado na PoC | Não aplicável como fluxo principal segundo a documentação consultada | Hub de modos + upgrade Pro + callbacks online + tópicos documentados | Pendente de homologação real |
| `iDFace Max` | Implementado na PoC | Implementado na PoC | Não aplicável como fluxo principal segundo a documentação consultada | Mesmo conjunto do iDFace, somado às superfícies de produto com SIP, câmera e áudio | Pendente de homologação real |
| `iDFlex` | Implementado na PoC | Não aplicável como fluxo principal na documentação consultada | Implementado na PoC | Hub de modos + upgrade Enterprise + telas de produto | Pendente de homologação real |
| `iDAccess Nano` | Implementado na PoC | Não aplicável como fluxo principal na documentação consultada | Implementado na PoC | Hub de modos + upgrade Enterprise + leitura do contexto operacional | Pendente de homologação real |
| `iDAccess` | Implementado na PoC | Parcialmente coberto na PoC | Parcialmente coberto na PoC | A PoC suporta aplicação de perfis online/local, mas a compatibilidade por variante exige validação por modelo e licença | Pendente de homologação real |
| `Demais terminais Access API com online/local_identification` | Implementado na PoC | Implementado na PoC quando o firmware suportar identificação local online | Parcialmente coberto na PoC | O hub aplica os perfis via `set-configuration`, mas a compatibilidade precisa ser confirmada por linha | Pendente de homologação real |

## Leitura honesta por modo

### Standalone

Estado atual:

- A PoC aplica `online = 0` pelo hub de modos.
- A leitura do perfil atual está implementada e resolve `Standalone` quando `online = false`.
- O modo pode ser acompanhado na interface e nos sinais de monitor/eventos.

Conclusão:

- `Standalone` está implementado na PoC.
- Ainda precisa de homologação real por equipamento para confirmar comportamento, UX embarcada e efeitos colaterais de firmware.

### Pro

Estado atual:

- A PoC aplica `online = 1` e `local_identification = 1`.
- O fluxo já cobre `server_id`, `extract_template` e `max_request_attempts`.
- O upgrade Pro do iDFace está disponível.
- O hub também expõe os callbacks oficiais relevantes para o modo.

Conclusão:

- `Pro` está implementado na PoC para a trilha de integração.
- O fechamento absoluto depende de validar em hardware compatível e com licença válida.

### Enterprise

Estado atual:

- A PoC aplica `online = 1` e `local_identification = 0`.
- O upgrade Enterprise para `iDFlex` e `iDAccess Nano` está disponível.
- O hub concentra a aplicação do perfil e a checagem de callbacks/monitor.

Conclusão:

- `Enterprise` está implementado na PoC como jornada de integração.
- Ainda depende de homologação real por linha compatível e de licença oficial.

## Pendências para declarar cobertura absoluta

Para responder `100% homologado` sem ressalvas, ainda faltam:

1. Validar cada linha física em bancada com firmware real.
2. Registrar número de série, firmware, licença e data do teste.
3. Confirmar os callbacks efetivamente disparados em cada modo.
4. Validar o comportamento pós-upgrade em reboot e reconexão.
5. Confirmar limites e particularidades por variante de produto.

## Conclusão executiva

Hoje podemos afirmar:

- a PoC implementa a trilha unificada de `Standalone`, `Pro` e `Enterprise`;
- a navegação, aplicação de perfil, upgrades e observabilidade já existem no produto;
- o que ainda falta para a resposta absoluta é homologação em hardware real, por linha compatível.

Então, o status correto desta data é:

- `Implementação na PoC`: concluída
- `Homologação física por linha de produto`: pendente
