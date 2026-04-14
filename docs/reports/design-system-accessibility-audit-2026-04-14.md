# Avaliação heurística focada em design system, língua portuguesa e acessibilidade

Data: 2026-04-14  
Escopo: todas as superfícies atuais da PoC, com ênfase em shell global, busca de módulos, navegação superior, painéis compartilhados, workspaces por domínio e telas de operação/documentação técnica.

## Problemas encontrados nesta rodada

1. Camada de interação da busca concorrendo com a navegação por domínios.
   - As evidências confirmaram que a primeira opção da busca podia ficar visualmente na frente, mas atrás da faixa interativa da navegação superior.
   - O efeito prático era perda de clique na primeira linha do dropdown de busca.

2. Respiro e margens ainda irregulares em telas que usam `container` legado.
   - Parte das páginas modernizadas já seguia o shell novo, mas alguns workspaces ainda dependiam do espaçamento herdado de `container mt-5`.
   - Isso criava densidade desigual entre o topo contextual e o conteúdo principal.

3. Texto legado com encoding inconsistente em fluxos compartilhados e técnicos.
   - Ainda havia rastros de mojibake em mensagens renderizadas na interface, especialmente em componentes compartilhados, fallback do JavaScript e catálogo técnico.
   - O problema também afetava a consistência visual em português e a clareza dos estados apresentados ao usuário.

4. Nome acessível incompleto em ações herdadas.
   - Ainda existiam botões, links de ação e tabelas dependendo apenas do texto visual ou de marcação legada.
   - Em algumas superfícies, isso reduzia a previsibilidade para leitores de tela e navegação assistiva.

5. Contraste e hierarquia de microcopy do shell.
   - Alguns textos auxiliares e estados discretos do topo continuavam mais apagados do que o restante do design system.
   - O objetivo desta passada foi reforçar consistência sem inflar ainda mais o shell.

6. Proporção visual irregular nos domínios e no mapa funcional.
   - Os domínios `Pessoas e credenciais` e `API e exploração técnica` exibiam cards grandes demais para a largura disponível, com CTAs dominando o bloco e sensação de vazamento visual.
   - A taxonomia principal versus apoio ainda estava fraca, especialmente quando módulos especializados apareciam com o mesmo peso dos módulos recomendados.

7. Densidade excessiva nas superfícies técnicas de catálogo.
   - `Catálogo oficial da API` e `Recursos oficiais avançados` ainda repetiam o padrão de cards com CTA inflado, pouca diferenciação entre bloco principal e apoio e excesso de elementos competindo no mesmo nível visual.
   - No catálogo, o volume de endpoints pedia uma leitura mais editorial e menos parecida com uma parede homogênea de cards.

8. Estouro visual de endpoints e rótulos técnicos longos.
   - Parte das rotas técnicas ainda ficava espremida no topo dos cards, causando sensação de vazamento para fora do bloco em telas mais densas.
   - Havia também resquícios de linguagem híbrida entre português e termos internos de engenharia, como `workspace` e `troubleshooting`, em superfícies já modernizadas.

9. Semântica inadequada no painel técnico compartilhado.
   - O painel de resposta bruta ainda trazia um botão de cópia dentro do `summary`, o que criava um controle interativo aninhado em outro.
   - Isso enfraquecia a acessibilidade para teclado, leitores de tela e comportamento previsível de clique.

10. Nome acessível e contraste ainda incompletos em pontos de alta frequência.
   - O formulário de filtros do catálogo oficial dependia quase totalmente do texto visível para contexto de navegação assistiva.
   - Microcopy de estatísticas, contexto de página e descrições técnicas ainda estava mais apagada do que o restante do sistema visual.

## Correções aplicadas

1. Busca global e navegação superior.
   - A camada de resultados da busca recebeu prioridade visual e de clique acima da faixa de domínios.
   - Quando a busca está aberta, a navegação superior deixa de capturar eventos de ponteiro por trás do dropdown.

2. Consistência de design system.
   - O conteúdo principal passou a usar uma malha compartilhada com `gap` consistente para reduzir diferenças de densidade entre telas.
   - O shell manteve o padrão visual existente, mas com contraste mais previsível para textos auxiliares do painel de conexão.

3. Normalização de texto em português.
   - A correção de artefatos de encoding foi centralizada no helper de segurança textual e reaproveitada em componentes compartilhados.
   - O JavaScript do shell também passou a reparar rótulos legados antes de montar resultados da busca, favoritos e recentes.
   - O catálogo oficial da API voltou a normalizar os textos técnicos legados sem quebrar compilação nem contrato público.

4. Acessibilidade.
   - Campos, botões, links de ação e tabelas continuam recebendo reforço progressivo de `aria-label`, `caption` e rótulos contextuais no shell compartilhado.
   - O painel de conexão agora normaliza melhor base ativa e nome do equipamento antes da renderização pública.

5. Recalibração dos hubs por domínio.
   - Os templates de `Workspace/Domain`, `Workspace/Index` e os atalhos da home passaram a usar uma hierarquia visual mais clara, com grids limitados, CTAs compactos, chips menores e diferenciação melhor entre entradas principais e apoio.
   - O catálogo de navegação também foi reorganizado para refletir melhor a semântica dos domínios, movendo `Logo do equipamento` para infraestrutura e reduzindo o peso inicial de superfícies especializadas em `API e exploração técnica`.

6. Reestruturação das superfícies técnicas densas.
   - `OfficialApi/Index` deixou de usar um muro homogêneo de cards e passou a apresentar o catálogo em grupos por categoria, com resumo superior, cards técnicos mais compactos e ações secundárias menos invasivas.
   - `AdvancedOfficial/Index` foi separado em fluxos avançados prioritários e exploradores correlatos, com CTAs menores, dicas de uso e grids mais adequados para texto técnico em português.

7. Tratamento de rotas longas e microcopy residual.
   - O identificador técnico do endpoint foi rebaixado para um bloco próprio, separado dos chips e do título, para impedir competição visual e vazamento de rota longa.
   - Textos residuais com anglicismos operacionais foram substituídos por termos em português mais diretos, como `diagnóstico`, `área técnica` e `abrir recurso`.

8. Painel técnico compartilhado e filtros de catálogo.
   - O botão de cópia do painel bruto saiu de dentro do `summary` e passou para uma barra própria, mantendo a ação acessível sem aninhar controles.
   - O formulário de filtros do catálogo oficial ganhou nome acessível explícito, assim como as ações de aplicar e limpar filtros.

9. Reforço de contraste em microcopy e metadados.
   - Estatísticas do catálogo, contexto de página, descrições do painel bruto e metadados técnicos receberam contraste mais firme para aproximar o shell e os catálogos do mesmo padrão visual.
   - As rotas técnicas também passaram a esconder overflow interno do bloco para evitar sensação de estouro mesmo em endpoints extensos.

## Revisão interna

As mudanças ficaram concentradas em infraestrutura compartilhada de UI e texto:
- helper central de normalização;
- shell de busca/navegação;
- painel de conexão;
- CSS global de layout;
- normalização do catálogo oficial.

Com isso, o risco de efeito colateral ficou baixo e controlado. Não houve alteração de contratos públicos dos controllers, rotas ou dependências do projeto.

## Validação

Comandos executados:

- `dotnet build .\Integracao.ControlID.PoC.sln -clp:ErrorsOnly`
- `dotnet test .\Integracao.ControlID.PoC.sln --no-build -clp:ErrorsOnly`

Resultado:

- Build verde com `0 warnings` e `0 errors`.
- Testes verdes com `40` testes aprovados.
