# Avaliação heurística de UX/UI - 14/04/2026

> **Evidência histórica** · Público: design e QA · Referência temporal: 2026-04-14 · Responsável: design/QA · Revalide conclusões na interface atual.

## Escopo

- Shell global da aplicação
- Navegação por domínio
- Painel persistente de conexão
- Dashboard inicial
- Catálogo técnico (`OfficialApi`)
- Famílias de CRUD (`Users`)
- Telas operacionais e técnicas (`System`, `RemoteActions`, `Workspace`)

## Método

- Leitura do código-fonte das views, view models e do CSS compartilhado
- Validação do HTML servido em `http://localhost:5001`
- Capturas sem interface gráfica de páginas-chave para revisão visual rápida

## Achados principais

1. **Visibilidade de contexto**: o painel persistente de conexão já resolve bem IP, porta, protocolo e status do equipamento.
2. **Consistência do shell**: o topo horizontal e o mapa funcional agora estão coerentes, mas várias telas ainda usavam layout Bootstrap cru, destoando da home e dos hubs.
3. **Linguagem visual desigual**: páginas técnicas e de configuração alternavam entre superfícies novas e cartões antigos, o que reduzia a sensação de produto unificado.
4. **Texto e codificação**: ainda havia textos com codificação quebrada em fluxos importantes, especialmente em `RemoteActions`, `System` e partes auxiliares da navegação.
5. **Hierarquia de ação**: em páginas técnicas, formulário, leitura operacional e resposta bruta nem sempre estavam claramente separados.

## Correções aplicadas nesta rodada

- Modernização visual dos fluxos de **Autorização remota**, **Rede e SSL** e **OpenVPN**, usando o mesmo arquétipo visual do shell principal.
- Separação clara entre leitura operacional, formulário de ação e painel técnico de resposta.
- Normalização do texto quebrado em modelos de visualização e controllers diretamente ligados a essas telas.
- Ajustes globais no CSS para elevar telas ainda baseadas em contêineres e cartões Bootstrap a um nível visual mais próximo do sistema de design da PoC.

## Resultado esperado

- Menor sensação de “telas soltas” dentro do produto
- Melhor leitura operacional em páginas técnicas
- Menos ruído visual entre conteúdo funcional e resposta técnica
- Consistência mais forte entre painel, centrais, CRUDs e operação

## Severidade e rastreabilidade histórica

| Achado | Severidade estimada | Correção registrada | Revalidação atual |
| --- | --- | --- | --- |
| Contexto de conexão | Baixa | Painel persistente | Teste renderizado e inspeção manual |
| Inconsistência do shell | Média | Arquétipo e CSS compartilhados | Desktop, tablet e celular |
| Texto/codificação | Alta | Normalização e correções de origem | Scan UTF-8/mojibake e UI |
| Hierarquia de ação | Média | Separação de leitura, formulário e resposta | Teclado e prevenção de erro |

O relatório original não registrou hash, navegador, viewport ou caminhos das
capturas. Uma nova auditoria deve incluir esses dados, vínculo achado→arquivo→teste
e estado `aberto`, `corrigido`, `aceito` ou `não reproduzido`.

## Vínculo com trabalho futuro

Uma nova auditoria deve atribuir ID estável a cada achado e ligá-lo a requisito,
critério de aceite, issue ou decisão de não correção. Recomendação sem dono,
prioridade e condição de verificação permanece observação, não plano executável.
