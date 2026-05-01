# ControlIdCallbackSigningProxy

Proxy local para equipamentos Control iD que nao conseguem gerar assinatura HMAC nativamente.

O proxy recebe chamadas do equipamento em uma interface de rede restrita, valida IP remoto e chave opcional de entrada, assina a requisicao com `X-ControlID-Signature`, `X-ControlID-Timestamp` e `X-ControlID-Nonce`, injeta a shared key esperada pela PoC e encaminha para a aplicacao.

Antes de encaminhar, o proxy remove headers sensiveis recebidos do cliente e insere uma assinatura nova. Assim a PoC continua exigindo `CallbackSecurity:RequireSignedRequests=true` mesmo quando o equipamento nao sabe assinar. Respostas acima de `Proxy:MaxResponseBytes` sao bloqueadas para reduzir risco de consumo excessivo de memoria.

## Execucao

Configure segredos fora do repositorio:

```powershell
dotnet restore .\tools\ControlIdCallbackSigningProxy\ControlIdCallbackSigningProxy.csproj --locked-mode
dotnet user-secrets set --project .\tools\ControlIdCallbackSigningProxy\ControlIdCallbackSigningProxy.csproj "Proxy:SharedKey" "<mesmo-segredo-da-poc>"
dotnet user-secrets set --project .\tools\ControlIdCallbackSigningProxy\ControlIdCallbackSigningProxy.csproj "Proxy:ForwardBaseUrl" "http://localhost:5000"
dotnet user-secrets set --project .\tools\ControlIdCallbackSigningProxy\ControlIdCallbackSigningProxy.csproj "Proxy:AllowedRemoteIps:0" "<ip-do-equipamento>"
```

Depois execute:

```powershell
dotnet run --project .\tools\ControlIdCallbackSigningProxy\ControlIdCallbackSigningProxy.csproj --urls http://localhost:6700
```

Configure o equipamento para chamar o proxy, nao a PoC diretamente. Mantenha firewall/rede permitindo que apenas o equipamento alcance o proxy.

## Checks

```powershell
dotnet build .\tools\ControlIdCallbackSigningProxy\ControlIdCallbackSigningProxy.csproj --no-restore -v:minimal
dotnet format .\tools\ControlIdCallbackSigningProxy\ControlIdCallbackSigningProxy.csproj --verify-no-changes --no-restore -v:minimal
```
