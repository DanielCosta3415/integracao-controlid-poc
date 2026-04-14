# Avaliação heurística focada em design system e acessibilidade

Data: 2026-04-14  
Escopo: shell global, navegação superior, painel de conexão persistente, busca global e painéis técnicos compartilhados.

## Problemas encontrados

1. Contraste insuficiente em microcopy e estados discretos do shell.
   - Os tokens `--ink-500` e `--ink-600` deixavam placeholders, texto auxiliar e subtítulos do topo mais apagados do que o desejável.
   - Os chips do topo usavam a semântica global, mas sobre um header escuro com pouco contraste visual.

2. Espaçamento horizontal inconsistente no shell.
   - Topbar, faixa de navegação e conteúdo principal usavam valores próximos, mas não unificados, o que criava desalinhamento fino entre os blocos.

3. Feedback de foco pouco claro na busca e nos controles do topo.
   - A paleta de busca removia o outline do campo, mas não reforçava o foco no container.
   - Links, summaries e botões dependiam quase exclusivamente do box-shadow do Bootstrap.

4. Controles interativos sem nome acessível suficientemente explícito.
   - A busca global precisava de ajuda contextual e semântica mais rica para leitores de tela.
   - Menus de domínio não expunham `aria-expanded` e `aria-hidden` sincronizados.
   - A cópia de payload e resposta, assim como o fechamento de alertas, ainda tinham nomes menos claros ou não localizados.
   - Favoritos não refletiam estado em `aria-pressed`.

5. Navegação contextual e documentação técnica sem semântica suficiente.
   - Breadcrumbs ainda eram apenas texto visual.
   - Tabelas de contrato da `OfficialApi` não declaravam cabeçalhos de forma semântica.
   - O painel de resposta bruta continha texto quebrado e não ligava o resumo ao conteúdo expandido.

## Correções aplicadas

1. Reforço de contraste e legibilidade.
   - Ajuste dos tokens `--ink-500` e `--ink-600`.
   - Overrides específicos para chips do topo em fundo escuro.
   - Subtítulos do menu superior com contraste maior.

2. Unificação de largura e margens do shell.
   - Reuso de `--app-shell-max-width` e `--shell-inline` nas faixas principais do shell.
   - Aplicação de `box-sizing: border-box` nas lanes para eliminar o desalinhamento fino entre header e conteúdo.

3. Foco acessível e consistente.
   - Reforço do `focus-ring` global.
   - Tratamento de `focus-within` no container da busca global com o mesmo token de foco do design system.

4. Semântica acessível nos controles compartilhados.
   - `aria-describedby`, `aria-haspopup`, `aria-controls`, `aria-expanded`, `aria-hidden`, `role="listbox"` e `role="option"` na busca, no menu de domínio e no painel de conexão.
   - `aria-pressed`, `aria-current` e `aria-label` explícitos nos favoritos, nos atalhos do topo e nos links de navegação.
   - `role="alert"`, `aria-live` e `aria-atomic` alinhados nos alertas.
   - Painel de resposta bruta conectado semanticamente ao seu resumo e com texto normalizado.

5. Documentação técnica com semântica mais forte.
   - Breadcrumbs promovidos para `nav` e lista com `aria-current`.
   - Tabelas do contrato visual da `OfficialApi` com `scope="col"` e rótulos mais claros.
   - Copy quebrada normalizada em componentes compartilhados da documentação técnica.

## Validação

- `dotnet build .\Integracao.ControlID.PoC.sln -clp:ErrorsOnly`
- `dotnet test .\Integracao.ControlID.PoC.sln --no-build -clp:ErrorsOnly`

Resultado:

- Build verde com 0 warnings e 0 errors.
- Testes verdes com 20 testes aprovados.
