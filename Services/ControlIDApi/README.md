# Services/ControlIDApi

> **Guia de módulo vivo** · Público: desenvolvimento backend · Responsável: engenharia de integração · Última validação: 2026-08-03.

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

## Fluxo de uma chamada

```mermaid
flowchart LR
    Controller["Controller"] --> Facade["IOfficialControlIdApiService"]
    Facade --> Catalog["Catálogo e documentação"]
    Facade --> Invoker["OfficialApiInvokerService"]
    Invoker --> Breaker["OfficialApiCircuitBreaker"]
    Invoker --> Reader["OfficialApiResponseBodyReader"]
    Invoker --> Device["Access API Control iD"]
    Reader --> Presentation["Resultado seguro para a UI"]
```

1. O controller resolve um endpoint conhecido no catálogo.
2. Estratégias de consulta/corpo montam apenas parâmetros documentados.
3. O invocador valida destino, allowlist, sessão, timeout e cancelamento.
4. O circuit breaker evita cascata após falhas transitórias repetidas.
5. A resposta é lida em streaming com limite e charset explícito.
6. O resultado de apresentação remove detalhes internos antes da tela.

## Responsabilidades dos componentes

| Componente | Responsabilidade | Não deve fazer |
| --- | --- | --- |
| `OfficialApiCatalogService` | Expor metadados de endpoints oficiais | Executar HTTP |
| `OfficialApiInvokerService` | Transporte seguro e observável | Renderizar HTML |
| `OfficialControlIdApiService` | Orquestrar sessão e chamadas comuns | Duplicar políticas HTTP |
| Estratégias de parâmetros | Construir consulta/corpo conforme contrato | Aceitar campos arbitrários |
| `OfficialApiResponseBodyReader` | Aplicar limite e decodificação | Interpretar regra de negócio |
| Serviços de documentação | Gerar exemplos e descrições | Tornar exemplo em contrato implícito |

## Erros e extensibilidade

- Validação local deve falhar antes de abrir conexão.
- Timeout, cancelamento e circuito aberto são estados diferentes e devem
  permanecer distinguíveis em logs e métricas, mas seguros para o usuário.
- Não existe nova tentativa automática genérica: endpoints físicos podem não ser
  idempotentes.
- Novo endpoint exige catálogo, documentação de parâmetros, autorização adequada
  e testes de sucesso, entrada inválida, tempo limite e resposta inesperada.
- Mudança de payload público exige versão ou compatibilidade documentada em
  `docs/integration-contracts.md`.

## Testes relacionados

Execute os testes de `tests/Integracao.ControlID.PoC.Tests/Services/ControlIDApi/`
e os contratos de controllers afetados. O contrato simulado fica em
`tools/contract-controlid-stub.ps1`; equipamento físico é opt-in e nunca usa
credenciais versionadas.

## Roteiro para estender o módulo

1. Confirmar endpoint, método, parâmetros e compatibilidade na documentação do fabricante.
2. Registrar o contrato no catálogo sem duplicar regra de URL ou autenticação.
3. Reutilizar o invocador, timeout, limite de resposta e circuit breaker existentes.
4. Converter resposta binária ou JSON em resultado público seguro, sem corpo bruto de erro.
5. Criar testes de sucesso, sessão ausente, entrada inválida, timeout e resposta inesperada.
6. Atualizar `docs/integration-contracts.md` e a rastreabilidade do requisito.

Comando direcionado:

```powershell
dotnet test .\Integracao.ControlID.PoC.sln --no-build --filter FullyQualifiedName~Services.ControlIDApi
```
