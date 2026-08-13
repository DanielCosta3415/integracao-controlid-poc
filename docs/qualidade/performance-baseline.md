# Linha de base de desempenho e capacidade

> **Referência** · Público: desenvolvimento, QA, SRE e FinOps · Responsável: QA · Última validação: 2026-08-12.

Este documento governa medições reproduzíveis sem hardware. Ele complementa
[docs/operacao/finops-capacity.md](../operacao/finops-capacity.md); números do simulador não representam limite de um
equipamento Control iD.

## Melhorias protegidas

- paginação oficial de 100 itens com lookahead;
- páginas locais limitadas e consultas `AsNoTracking()`;
- transferência binária direta em blocos, limitada a 256 MiB;
- respostas JSON paginadas sem cópia adicional de `JsonDocument`;
- até quatro chamadas simultâneas por equipamento e fila máxima de 16;
- galeria de logos paralela sob o limitador por equipamento;
- SQLite com `WAL`, espera ocupada de 5 s, chaves estrangeiras e `synchronous=NORMAL`;
- simulador sem serialização intermediária na resposta de objetos;
- cancelamento propagado nos downloads e no upload de vídeo.

## Método

`tools/performance-baseline.ps1` inicia um stub isolado, recria massas de 100,
1.000 e 10.000 registros, aquece o endpoint, coleta 20 amostras sequenciais e
executa uma rajada de 20 requisições. Cada leitura solicita a última página de
100 usuários; dessa forma, o simulador atravessa o conjunto até o deslocamento
correspondente, em vez de medir somente o custo constante da primeira página. O
relatório inclui p50, p95, p99, req/s, bytes, CPU, memória e delta de memória. A
massa de 100.000 é opcional com `-IncludeMaximumDataset`.

```powershell
powershell -ExecutionPolicy Bypass -File .\tools\performance-baseline.ps1 -FailOnBudget
```

Artefatos ignorados pelo Git:

- `artifacts/performance/baseline-latest.md`;
- `artifacts/performance/baseline-latest.json`.

## Resultado de referência

Execução local em 2026-08-04:

| Massa | p95 observado | Vazão observada | Memória do stub |
| ---: | ---: | ---: | ---: |
| 100 | 1,97 ms | 54,82 req/s | 63,27 MiB |
| 1.000 | 1,01 ms | 59,82 req/s | 74,57 MiB |
| 10.000 | 1,02 ms | 62,66 req/s | 117,07 MiB |
| 100.000 | 4,28 ms | 57,14 req/s | 427,88 MiB |

Os valores variam por máquina e carga concorrente. O orçamento bloqueante e
deliberadamente tolerante é p95 de até 1.000 ms e memória de até 768 MiB. Ele
detecta regressões graves; comparação fina exige mesma máquina, SDK, processo e
amostra.

## Complexidade esperada

| Operação | Tempo | Espaço adicional |
| --- | --- | --- |
| Busca de endpoint no catálogo indexado | `O(1)` | `O(1)` |
| Paginação de `load_objects` | `O(offset + pageSize)` | `O(pageSize)` |
| Create-or-modify no stub | `O(n + m)` | `O(n)` para índice |
| Sanitização de consulta | `O(c)` | `O(c)` |
| Leitura/stream binário | `O(bytes)` | `O(buffer)` de aproximadamente 80 KiB |
| Limpeza de nonce expirado | `O(k log n)` | `O(n)` |
| Listagem SQLite paginada | Dependente de índice e limite | `O(pageSize)` |

## Interpretação e limite

Não compare esses números diretamente com SLA de produção. O teste físico deve
medir latência de LAN, tamanho real de payload, concorrência aceita pelo firmware,
tempo de mídia, reinicialização e efeitos de licença. Regressão local acima do
orçamento bloqueia a entrega; resultado dentro dele não elimina o gate físico.

## Navegação documental

- [Voltar ao índice deste domínio](README.md).
- [Abrir a central de documentação](../README.md).
