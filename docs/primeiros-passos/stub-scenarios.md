# Cenários do simulador Control iD

> **Referência** · Público: desenvolvimento, QA e segurança · Responsável: Engenharia · Última validação: 2026-08-12.

O projeto `tools/ControlIdDeviceStub` é um servidor HTTP exclusivo de loopback.
Ele mantém estado em memória, usa dados fictícios e expõe administração em
`/__stub/*`. Essas rotas não pertencem à Access API e nunca devem ser publicadas
como contrato do fabricante.

## Administração

| Método e rota | Finalidade |
| --- | --- |
| `GET /__stub/status` | Estado, perfil, massa, cenário e métricas por rota |
| `GET /__stub/catalog` | Cenários, perfis e massas aceitos |
| `POST /__stub/scenario` | Aplicar falha global ou direcionada |
| `POST /__stub/reset` | Recriar estado determinístico e limpar métricas |

Schemas de entrada: `tools/ControlIdDeviceStub/contracts/`. Exemplos:
`tools/ControlIdDeviceStub/fixtures/`. A tela `/Development/Simulator` oferece o
mesmo controle somente em `Development` e para administrador local.

## Cenários disponíveis

| Cenário | Comportamento | Uso principal |
| --- | --- | --- |
| `normal` | Contrato nominal | Happy path e smoke |
| `slow` | Atraso e resposta normal | Loading, latência e cancelamento |
| `timeout` | Atraso e `504` | Tempo limite e circuito |
| `bad-request` | `400` | Entrada rejeitada |
| `unauthorized` | `401` | Sessão ausente ou inválida |
| `forbidden` | `403` | Permissão insuficiente |
| `not-found` | `404` | Recurso ausente |
| `conflict` | `409` | Conflito de estado |
| `rate-limited` | `429` com `Retry-After` | Limite de chamadas |
| `server-error` | `503` | Falha transitória e circuito |
| `invalid-json` | Conteúdo não JSON com tipo JSON | Parsing defensivo |
| `truncated-json` | Documento JSON incompleto | Resposta interrompida |
| `unexpected-json` | Estrutura válida inesperada | Contrato implícito |
| `wrong-content-type` | Corpo e tipo incompatíveis | Negociação de conteúdo |
| `oversized-response` | Fluxo acima do limite | Proteção de memória |
| `session-expired` | Sessão inválida ou `401` | Reautenticação |
| `feature-unavailable` | `404` controlado | Firmware/licença sem recurso |
| `network-drop` | Conexão encerrada | Falha de transporte |

`endpoint` restringe o cenário a uma rota, `delayMs` aceita de 0 a 60.000 e
`responseBytes` controla respostas grandes entre 1 MiB e 64 MiB. O cenário
`normal` ignora os demais parâmetros.

## Perfis e massas

- perfis: `idface`, `idflex`, `idbox` e `legacy`;
- massas: 1, 100, 1.000, 10.000 e 100.000 registros;
- usuários, cartões e QR Codes são sintéticos e determinísticos;
- e-mails usam o domínio reservado `.invalid`;
- nenhuma foto, biometria ou credencial real é aceita como fixture.

## Exemplos

```powershell
$body = @{ name = "rate-limited"; endpoint = "/load_objects.fcgi" } | ConvertTo-Json
Invoke-RestMethod http://127.0.0.1:6600/__stub/scenario -Method Post -ContentType application/json -Body $body

$reset = @{ profile = "idface"; datasetSize = 10000 } | ConvertTo-Json
Invoke-RestMethod http://127.0.0.1:6600/__stub/reset -Method Post -ContentType application/json -Body $reset
```

Sempre finalize uma sessão manual com `POST /__stub/reset`. O script
`tools/contract-controlid-stub.ps1` já verifica catálogo, perfil, massa, falha,
latência e restauração do estado.

## Navegação documental

- [Voltar ao índice deste domínio](README.md).
- [Abrir a central de documentação](../README.md).
