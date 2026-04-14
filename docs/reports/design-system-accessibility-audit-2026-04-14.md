# Avaliacao Heuristica Focada em Design System e Acessibilidade

Data: 2026-04-14
Escopo: shell global, navegacao superior, painel de conexao persistente, busca global e paineis tecnicos compartilhados.

## Problemas encontrados

1. Contraste insuficiente em microcopy e estados discretos do shell.
   - Tokens `--ink-500` e `--ink-600` deixavam placeholders, texto auxiliar e subtitulos do topo mais apagados do que o desejavel.
   - Os chips do topo usavam a semantica global, mas sobre um header escuro com pouco contraste visual.

2. Espacamento horizontal inconsistente no shell.
   - Topbar, faixa de navegacao e conteudo principal usavam valores proximos, mas nao unificados, o que criava desalinhamento fino entre os blocos.

3. Feedback de foco pouco claro na busca e nos controles do topo.
   - A paleta de busca removia o outline do campo, mas nao reforcava o foco no container.
   - Links, summaries e botoes dependiam quase exclusivamente do box-shadow do Bootstrap.

4. Controles interativos sem nome acessivel suficientemente explicito.
   - Busca global precisava de ajuda contextual e semantica mais rica para leitores de tela.
   - Menus de dominio nao expunham `aria-expanded`/`aria-hidden` sincronizados.
   - Copia de payload/resposta e fechamento de alerta ainda tinham nomes menos claros ou nao localizados.
   - Favoritos nao refletiam estado em `aria-pressed`.

5. Navegacao contextual e documentacao tecnica sem semantica suficiente.
   - Breadcrumbs ainda eram apenas texto visual.
   - Tabelas de contrato da `OfficialApi` nao declaravam cabecalhos semanticamente.
   - O painel de resposta bruta continha texto quebrado e nao ligava o resumo ao conteudo expandido.

## Correcoes aplicadas

1. Reforco de contraste e legibilidade.
   - Ajuste dos tokens `--ink-500` e `--ink-600`.
   - Overrides especificos para chips do topo em fundo escuro.
   - Subtitulos do menu superior com contraste maior.

2. Unificacao de largura e margens do shell.
   - Reuso de `--app-shell-max-width` e `--shell-inline` nas faixas principais do shell.
   - Aplicacao de `box-sizing: border-box` nas lanes para eliminar o desalinhamento fino entre header e conteudo.

3. Foco acessivel e consistente.
   - Reforco do `focus-ring` global.
   - Tratamento de `focus-within` no container da busca global com o mesmo token de foco do design system.

4. Semantica acessivel nos controles compartilhados.
   - `aria-describedby`, `aria-haspopup`, `aria-controls`, `aria-expanded`, `aria-hidden`, `role="listbox"` e `role="option"` na busca, no menu de dominio e no painel de conexao.
   - `aria-pressed`, `aria-current` e `aria-label` explicitos nos favoritos, atalhos do topo e links de navegacao.
   - `role="alert"`, `aria-live` e `aria-atomic` alinhados nos alertas.
   - Painel de resposta bruta conectado semanticamente ao seu resumo e com texto normalizado.

5. Documentacao tecnica com semantica mais forte.
   - Breadcrumbs promovidos para `nav`/lista com `aria-current`.
   - Tabelas do contrato visual da `OfficialApi` com `scope="col"` e rotulos mais claros.
   - Copy quebrada normalizada em componentes compartilhados da documentacao tecnica.

## Validacao

- `dotnet build .\\Integracao.ControlID.PoC.sln -clp:ErrorsOnly`
- `dotnet test .\\Integracao.ControlID.PoC.sln --no-build -clp:ErrorsOnly`

Resultado:
- Build verde com 0 warnings e 0 errors.
- Testes verdes com 20 testes aprovados.
