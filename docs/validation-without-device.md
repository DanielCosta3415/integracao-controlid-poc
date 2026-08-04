# Validação sem equipamento físico

> **Documento vivo** · Público: desenvolvimento, QA, produto e avaliação técnica · Responsável: liderança técnica · Última validação: 2026-08-04.

Este guia define até onde a PoC pode ser desenvolvida e validada sem um controle
de acesso físico da Control iD. O simulador aumenta a confiança em código,
contratos e interface, mas não converte comportamento simulado em homologação de
firmware ou hardware.

## O que pode ser concluído sem aparelho

| Frente | Evidência disponível | Limite |
| --- | --- | --- |
| Compilação e análise estática | .NET, formatação, Semgrep e gates locais | Não mede comportamento eletromecânico |
| Regras e segurança | Testes xUnit, fuzz determinístico, rate limit, HMAC e sanitização | Não valida configuração embarcada |
| Contratos HTTP | Stub, schemas, fixtures e contrato PowerShell | Respostas são representações controladas |
| Banco local | SQLite real temporário, migrações, WAL e concorrência | Não usa dados de produção |
| Jornadas web | Playwright autenticado, axe e referências visuais | Não comprova interação física |
| Desempenho de software | Percentis, vazão, CPU e memória contra massa sintética | Não mede LAN, firmware nem armazenamento do aparelho |
| Resiliência | Timeout, rede interrompida, 4xx, 5xx, JSON inválido e excesso de tamanho | Não reproduz todas as falhas elétricas ou de firmware |
| Documentação e operação | Runbooks, gates, rollback e matriz de evidência | RTO/RPO reais ainda exigem exercício operacional |

## Percurso reproduzível

1. Restaure e compile a solução em modo bloqueado.
2. Inicie o simulador em loopback.
3. Inicie a PoC em `Development`.
4. Registre uma conta local fictícia e autentique-se como administrador.
5. Conecte a PoC ao simulador com `stub-admin` e `stub-password`.
6. Use `/Development/Simulator` para escolher cenário, perfil e massa.
7. Execute contrato, testes, cobertura, benchmark e smoke.
8. Preserve somente relatórios sanitizados em `artifacts/`.

```powershell
dotnet restore .\Integracao.ControlID.PoC.sln --locked-mode
dotnet build .\Integracao.ControlID.PoC.sln --no-restore -v:minimal
dotnet test .\tests\Integracao.ControlID.PoC.Tests\Integracao.ControlID.PoC.Tests.csproj --no-build -v:minimal
dotnet test .\tests\Integracao.ControlID.PoC.E2E\Integracao.ControlID.PoC.E2E.csproj --no-build -v:minimal
powershell -ExecutionPolicy Bypass -File .\tools\contract-controlid-stub.ps1
powershell -ExecutionPolicy Bypass -File .\tools\performance-baseline.ps1 -FailOnBudget
powershell -ExecutionPolicy Bypass -File .\tools\test-readiness-gates.ps1 -RunCoverage -RunPerformanceBaseline
```

## Evidências mínimas

- compilação sem avisos;
- testes unitários e E2E aprovados;
- cobertura igual ou superior aos pisos versionados;
- contrato do simulador aprovado após restauração final para `normal`;
- comparação visual dentro da tolerância de 3%;
- zero violação crítica ou séria detectada pelo axe nas jornadas auditadas;
- benchmark dentro do orçamento local;
- Semgrep, OSV, auditoria NuGet e análise de segredos sem bloqueio;
- documentação, manutenibilidade e espaços em branco aprovados.

## Limite inevitável

Exigem equipamento real: compatibilidade por modelo e firmware, negociação TLS
do aparelho, capacidade da LAN, leitor facial/cartão/QR, relés, catraca, display,
áudio, câmera, reinicialização, recuperação, reset, callbacks originados pelo
firmware, modo on-line sob carga e efeitos de licença. Registre modelo, firmware,
licença, topologia e evidência no contrato físico; não marque esses itens como
aprovados com base apenas no stub.

## Critério de promoção

Uma capacidade sai de “simulada” para “homologada” somente quando a matriz em
`docs/endpoint-validation-matrix.md` referencia execução em aparelho compatível,
data, firmware e resultado. A ausência de hardware não impede evolução da PoC,
mas impede a declaração de compatibilidade física definitiva.
