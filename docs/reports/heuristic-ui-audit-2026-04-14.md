# Avaliação heurística de UX/UI - 14/04/2026

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
- Capturas headless de páginas-chave para revisão visual rápida

## Achados principais

1. **Visibilidade de contexto**: o painel persistente de conexão já resolve bem IP, porta, protocolo e status do equipamento.
2. **Consistência do shell**: o topo horizontal e o mapa funcional agora estão coerentes, mas várias telas ainda usavam layout Bootstrap cru, destoando da home e dos hubs.
3. **Linguagem visual desigual**: páginas técnicas e de configuração alternavam entre superfícies novas e cards antigos, o que reduzia a sensação de produto unificado.
4. **Copy e encoding**: ainda havia textos com codificação quebrada em fluxos importantes, especialmente em `RemoteActions`, `System` e partes auxiliares da navegação.
5. **Hierarquia de ação**: em páginas técnicas, formulário, leitura operacional e resposta bruta nem sempre estavam claramente separados.

## Correções aplicadas nesta rodada

- Modernização visual dos fluxos de **Autorização remota**, **Rede e SSL** e **OpenVPN**, usando o mesmo arquétipo visual do shell principal.
- Separação clara entre leitura operacional, formulário de ação e painel técnico de resposta.
- Normalização da copy quebrada em view models e controllers diretamente ligados a essas telas.
- Ajustes globais no CSS para elevar telas ainda baseadas em containers e cards Bootstrap a um nível visual mais próximo do design system da PoC.

## Resultado esperado

- Menor sensação de “telas soltas” dentro do produto
- Melhor leitura operacional em páginas técnicas
- Menos ruído visual entre conteúdo funcional e resposta técnica
- Consistência mais forte entre dashboard, hubs, CRUDs e operação
