# Services/ControlIDApi

Camada oficial de integração da PoC com a Access API da Control iD.

Serviços principais:

- `OfficialApiCatalogService`: catálogo local dos endpoints oficiais usados pela PoC.
- `OfficialApiInvokerService`: invocação HTTP oficial com sanitização, timeout configurável e logs estruturados.
- `OfficialControlIdApiService`: orquestração de chamadas usando o contexto atual da sessão da PoC.
- `OfficialApiContractDocumentationService`: composição da documentação visual de contrato exibida no módulo `OfficialApi`.

Pontos operacionais importantes:

- o timeout das chamadas oficiais usa `ControlIDApi__ConnectionTimeoutSeconds`;
- erros de validação, timeout e falhas inesperadas são registrados em log;
- resolução de endpoint inexistente e falha de parse JSON também geram log estruturado;
- a camada visual de `OfficialApi` usa esses serviços para documentar endpoint, query, body e exemplos.
