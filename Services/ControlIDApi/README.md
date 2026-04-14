# Services/ControlIDApi

This folder now keeps only the official layer used by the PoC:

- `OfficialApiCatalogService`
- `OfficialApiInvokerService`
- `OfficialControlIdApiService`

The legacy wrappers from the old integration model were removed after the controllers migrated to the official Control iD API, avoiding architectural duplication and dead flows.
