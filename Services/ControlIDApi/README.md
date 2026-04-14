# Services/ControlIDApi

Camada oficial de integracao da PoC com a Access API da Control iD.

Servicos principais:

- `OfficialApiCatalogService`: catalogo local dos endpoints oficiais usados pela PoC.
- `OfficialApiInvokerService`: invocacao HTTP oficial com sanitizacao, timeout configuravel e logs estruturados.
- `OfficialControlIdApiService`: orquestracao de chamadas usando o contexto atual da sessao da PoC.
- `OfficialApiContractDocumentationService`: composicao da documentacao visual de contrato mostrada no modulo `OfficialApi`.

Pontos operacionais importantes:

- o timeout das chamadas oficiais usa `ControlIDApi__ConnectionTimeoutSeconds`;
- erros de validacao, timeout e falha inesperada sao registrados em log;
- a camada visual de `OfficialApi` usa esses servicos para documentar endpoint, query, body e exemplos.
