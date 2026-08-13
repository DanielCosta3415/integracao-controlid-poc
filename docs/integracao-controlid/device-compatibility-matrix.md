# Matriz de compatibilidade dos equipamentos Control iD

> **Referência** · Público: avaliação, integração, produto e QA · Responsável: Engenharia · Última validação: 2026-08-12.

Esta matriz separa o que a PoC implementa do que foi comprovado em equipamento
físico. A Access API é comum à linha de acesso, mas ações, capacidades, limites,
modos e licenças variam por produto e firmware. A existência de uma rota no
catálogo não comprova que todo equipamento aceite a operação.

## Como interpretar o estado

| Estado | Significado |
| --- | --- |
| Implementado na PoC | Há controller, serviço ou entrada no catálogo para compor a chamada. |
| Validado com simulador | O contrato HTTP usado pela PoC foi exercitado sem hardware real. |
| Pendente de homologação | Falta comprovar efeito físico no modelo, firmware e licença indicados. |
| Não aplicável | A própria documentação oficial restringe o recurso a outra linha. |
| Necessita validação | A documentação consultada não é suficiente para uma afirmação segura. |

Nenhum item pendente deve ser promovido apenas porque a chamada retornou HTTP
2xx. Para ações físicas e configurações, releia o estado, observe o equipamento e
registre a evidência.

## Compatibilidade funcional por linha

| Linha/produto | Ação física indicada pela documentação oficial | Modos e particularidades relevantes | Cobertura da PoC | Estado atual |
| --- | --- | --- | --- | --- |
| iDAccess e iDFit | `door` | API comum; confirmar variante e firmware. | Catálogo, hardware, usuários, regras e ações remotas. | Implementado; efeito físico pendente de matriz por unidade. |
| iDBlock e iDBlock V2 | `catra` | Eventos de giro em `catra_event`; recursos próprios de catraca. | Catálogo, `CatraController`, callbacks e ações. | Implementado; sentidos e temporizações exigem homologação. |
| iDBlock Next | Configuração e sinais dependem da composição principal/secundários | Possui particularidades próprias de papéis e SecBox. | Catálogo genérico e recursos de catraca. | Necessita validação específica; não assumir equivalência integral com iDBlock V2. |
| iDBox | `door` | Quatro portais segundo a documentação oficial. | Abertura, estado de portas, objetos e configurações. | Implementado; numeração e cabeamento devem ser conferidos em bancada. |
| iDFlex | `sec_box` | Enterprise pode exigir upgrade/licença. | Modos, hardware, SecBox, ações e endpoint de upgrade. | Implementado; licença e efeito físico pendentes. |
| iDAccess Pro | `sec_box` | Configurações da SecBox ficam em objetos/configuração conforme firmware. | Hardware, objetos, modos e ações. | Implementado; validar variante e firmware. |
| iDAccess Nano | `sec_box` | Enterprise pode exigir upgrade/licença. | Modos, hardware, ações e endpoint de upgrade. | Implementado; licença e efeito físico pendentes. |
| iDUHF | `door` para relé interno e `sec_box` para relé externo | Recursos de UHF e relés diferem dos terminais faciais. | Catálogo genérico, hardware e ações. | Implementado em contrato; validar cada relé fisicamente. |
| iDFace | `sec_box` | Documentação do produto enfatiza Standalone/Pro; recursos faciais e de mídia dependem do firmware. | Fotos, face, mídia, SIP, áudio, modos e ações específicas. | Parcial por capacidade; não declarar Enterprise sem evidência do modelo. |
| iDFace Max | SecBox, relé interno e GPIOs conforme configuração | Recursos de energia, captura, sinais configuráveis e mensagens podem ser exclusivos. | Catálogo e telas de recursos específicos. | Implementado no catálogo; cada recurso exige firmware-alvo e homologação. |

Fonte oficial principal: [particularidades dos terminais Control iD](https://www.controlid.com.br/docs/access-api-pt/particularidade-dos-produtos/particulariade-terminais-control-id/).

## Modos de operação

| Modo | Identificação | Autorização | Dependência principal | Situação na PoC |
| --- | --- | --- | --- | --- |
| Standalone | Equipamento | Equipamento | Base local completa e sincronizada. | Perfil implementado e recomendado como primeira homologação. |
| Pro | Equipamento | Servidor externo | Usuários/credenciais locais e servidor acessível. | Perfil implementado; exige callbacks e `server_id`. |
| Enterprise | Servidor externo | Servidor externo | Algoritmo e serviço de identificação externos, licença/modelo compatível. | Perfil configurável quando suportado; a PoC não fornece motor biométrico produtivo. |
| Contingência | Equipamento | Equipamento | Dados locais atualizados antes da falha. | Observável por callbacks; procedimento em [equipment-contingency-runbook.md](../operacao/equipment-contingency-runbook.md). |

A Control iD descreve Standalone como modo recomendado. Em Pro, o equipamento
identifica e envia o usuário ao servidor; em Enterprise, identificação e
autorização ficam no servidor. Consulte a
[introdução oficial aos modos](https://www.controlid.com.br/docs/access-api-pt/modos-de-operacao/introducao-aos-modos-de-operacao/)
e [operation-modes-implementation.md](operation-modes-implementation.md).

## Capacidades transversais da PoC

| Área | Cobertura de software | Limite de compatibilidade |
| --- | --- | --- |
| Sessão e sistema | Login, validação, logout e informações do sistema | Resposta e expiração podem variar por firmware. |
| Objetos | Carregar, criar, criar/modificar, modificar e destruir | Campos, relações e objetos disponíveis variam. |
| Identidades | Usuários, cartões, QR Codes, fotos e templates biométricos | Tipo de leitor, formato e capacidade dependem do produto. |
| Regras | Grupos, horários, portais e regras de acesso | Aplicação local é relevante sobretudo em Standalone/contingência. |
| Monitor | Recepção e persistência de eventos documentados | Tópicos emitidos dependem da configuração e do hardware. |
| Push | Fila, consultas periódicas e resultados | Comandos aceitos dependem da API presente no firmware. |
| Hardware | GPIO, portas, relés, catraca e validação biométrica | Numeração, ação e efeito são específicos do produto. |
| Mídia | Fotos, logotipo, vídeo, áudio e SIP | Muitos endpoints são exclusivos de iDFace/iDFace Max. |

O catálogo local possui 96 entradas na auditoria de 2026-04-13: 73 invocáveis e
23 callbacks/rotas de servidor. Essa contagem mede cobertura do catálogo, não
compatibilidade física universal. Consulte
[reports/controlid-api-audit-2026-04-13.md](../historico/relatorios/controlid-api-audit-2026-04-13.md).

## Licenças e upgrades

- A PoC cataloga `/upgrade_ten_thousand_face_templates.fcgi` para upgrade do
  iDFace quando aplicável.
- A PoC cataloga `/idflex_upgrade_enterprise.fcgi` para iDFlex/iDAccess Nano
  quando aplicável.
- A senha/licença deve ser fornecida pela Control iD e nunca versionada.
- Uma resposta de sucesso deve ser seguida de releitura de informações e teste
  funcional; a PoC não consegue emitir nem validar comercialmente uma licença.

## Capacidade e bases grandes

Não fixe um número universal de usuários, faces ou templates na documentação da
PoC. Os limites mudam por produto, variante, licença e firmware. Antes de uma
carga relevante:

1. consulte modelo, serial pseudonimizado, firmware e licença;
2. confirme a capacidade na documentação oficial vigente do produto;
3. use paginação em `load_objects.fcgi`;
4. para grande volume biométrico, valide `template_sync_init.fcgi` e
   `template_sync_end.fcgi` conforme a documentação oficial;
5. faça teste gradual, monitore memória/armazenamento e mantenha retorno seguro.

Referência: [carregamento de objetos](https://www.controlid.com.br/docs/access-api-en/objects/load-objects/).

## Registro obrigatório de homologação

| Campo | Exemplo seguro |
| --- | --- |
| Commit da PoC | Hash do commit sem credenciais. |
| Produto e variante | `iDFace <variante>` ou equivalente. |
| Firmware | Versão retornada por `system_information.fcgi`. |
| Licença/modo | Categoria, nunca a chave ou senha. |
| Rede | Topologia e portas; IP real fica em evidência restrita. |
| Operação | Endpoint, método e finalidade. |
| Resultado | Aprovado, reprovado, parcial ou não aplicável. |
| Evidência | Relatório sanitizado, foto sem pessoas ou registro restrito. |
| Retorno | Passos para restaurar configuração anterior. |
| Responsável/data | Dono da homologação e data UTC/local identificada. |

Use [reports/operation-modes-homologation-matrix-2026-04-14.md](../historico/relatorios/operation-modes-homologation-matrix-2026-04-14.md) como modelo
histórico, sem substituir seus resultados originais.

## Gatilhos de revalidação

Revalide somente as células afetadas quando ocorrer:

- atualização ou reversão de firmware;
- mudança de produto, variante, placa ou SecBox;
- alteração de licença ou modo;
- mudança de endpoint, carga útil ou configuração usada pela PoC;
- atualização relevante da documentação oficial;
- troca de rede, proxy, TLS ou URL de callback;
- divergência entre resposta HTTP e efeito físico;
- incidente ou regressão de integração.

A cadência e o processo de atualização estão em
[official-api-version-governance.md](official-api-version-governance.md).

## Fontes oficiais verificadas

- [Introdução à Access API](https://www.controlid.com.br/docs/access-api-pt/)
- [Particularidades dos terminais](https://www.controlid.com.br/docs/access-api-pt/particularidade-dos-produtos/particulariade-terminais-control-id/)
- [Parâmetros de configuração](https://www.controlid.com.br/docs/access-api-pt/configuracoes/parametros-configuracao/)
- [Abertura remota de porta e catraca](https://www.controlid.com.br/docs/access-api-pt/acoes/abertura-remota-porta-e-catraca/)
- [Notas de versão da linha de acesso](https://www.controlid.com.br/access_v2/changelog_pt-br.pdf)

## Navegação documental

- [Voltar ao índice deste domínio](README.md).
- [Abrir a central de documentação](../README.md).
