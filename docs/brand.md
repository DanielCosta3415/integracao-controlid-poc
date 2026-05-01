# Identidade visual e design system

Este guia registra a linha de base visual da PoC Control iD para orientar evolucao de UI, documentacao, assets e agentes de codigo. Ele nao substitui o manual oficial da marca Control iD nem autoriza copiar marcas registradas; o objetivo e governar a identidade propria desta PoC operacional.

## Diagnostico visual

- Nome atual: `Integracao.ControlID.PoC`, apresentado na interface como `Control iD PoC` e `Console operacional da Access API`.
- Produto: console web tecnico para integrar, operar, auditar e diagnosticar fluxos da Access API da Control iD.
- Publico: desenvolvedores, integradores, operadores tecnicos, QA, seguranca e times internos que validam equipamento fisico, callbacks, push, banco local e contratos.
- Tom: operacional, preciso, seguro, tecnico e confiavel. A interface deve parecer ferramenta de trabalho, nao landing page.
- Identidade existente: vermelho como cor primaria, grafite/ink para estrutura, superficies quentes claras, cards densos, chips de status, tabelas e navegacao por dominios.
- Tipografia existente: `Sora` para titulos e marca; `Manrope` para UI e texto.
- Componentes principais: shell fixo, topbar, busca de modulos, chips, cards, tabelas, formularios, paineis de conexao, code panels e alertas.
- Assets existentes: `wwwroot/favicon.ico` e marca construida em CSS no shell. Foi adicionado `wwwroot/img/brand/controlid-poc-mark.svg` como simbolo independente da PoC.

## Estrategia de marca

- Promessa: dar controle operacional seguro e rastreavel sobre integracoes Control iD sem esconder risco tecnico.
- Personalidade: vigilante, clara, pragmatica, responsavel e preparada para auditoria.
- Sensacao desejada: "console de comando confiavel", com leitura rapida de estado, acao e evidencia.
- Palavras-chave: acesso, sinal, equipamento, auditoria, contrato, telemetria, seguranca, rastreabilidade.
- Diferenciacao: unir catalogo tecnico, operacao local e diagnostico em uma interface unica orientada por contexto.
- O que evitar: visual promocional, excesso de hero marketing, graficos decorativos sem funcao, dependencia exclusiva de cor, copiar o logotipo Control iD, prometer conformidade total ou seguranca perfeita.
- Referencias permitidas: paineis operacionais, consoles de monitoramento, linguagem de status industrial, mapas de endpoint e ferramentas de QA.

## Sistema visual

### Tokens de cor

| Papel | Token | Valor | Uso |
| --- | --- | --- | --- |
| Primaria | `--color-brand-primary` | `#b61b24` | Acoes principais, links, foco de marca |
| Primaria hover | `--color-brand-primary-hover` | `#8a1820` | Hover/pressed de marca |
| Acento | `--color-brand-accent` | `#f03b3f` | Destaques, anel da marca, detalhes |
| Texto forte | `--color-text-primary` | `#171214` | Titulos e numeros de metrica |
| Texto padrao | `--color-text-body` | `#231d20` | Corpo e labels importantes |
| Texto secundario | `--color-text-muted` | `#51474c` | Hints, metadados e descricoes |
| Fundo | `--color-surface-page` | `#fbf7f6` | Fundo geral quente e baixo contraste visual |
| Painel | `--color-surface-panel` | `#fffdfd` | Cards, formularios e superficies de leitura |
| Borda sutil | `--color-border-subtle` | `rgba(39, 26, 30, 0.08)` | Divisores e containers |
| Borda forte | `--color-border-strong` | `rgba(39, 26, 30, 0.14)` | Estados ativos e separacoes maiores |
| Sucesso | `--success` | `#0f766e` | Sessao ativa, equipamento pronto, sucesso |
| Alerta | `--warning` | `#b45309` | Pendente, atencao, confirmacao |
| Info | `--info` | `#2563eb` | Informacao tecnica e apoio |
| Perigo | `--danger` | `#b61b24` | Exclusao, falha, bloqueio |

Use vermelho como acento de decisao e criticidade, nao como preenchimento dominante de toda a tela. Combine com grafite, branco quente, teal e azul para reduzir monotonia e manter hierarquia.

### Tokenizacao aplicada no CSS

- `wwwroot/css/site.css` centraliza valores `hex` e `rgba()` no bloco `:root`.
- Fora de `:root`, cores, overlays, sombras, fundos transluidos, estados e textos inversos devem usar `var(--...)`.
- Tokens `--white-alpha-*`, `--ink-alpha-*`, `--brand-primary-alpha-*`, `--brand-accent-alpha-*`, `--surface-*-alpha-*`, `--success-alpha-*`, `--warning-alpha-*` e `--info-alpha-*` existem para evitar novos valores soltos.
- `--color-text-inverse`, `--warning-text-on-dark` e `--success-text-on-dark` governam texto sobre superficies escuras.
- `--surface-sidebar-*`, `--surface-header-*`, `--surface-topbar-alpha-98`, `--page-glow-*` e `--modal-backdrop` governam camadas de shell, header e overlay.
- Valores literais novos so devem ser adicionados em `:root`, com papel claro e contraste avaliado.

### Contraste verificado

- `#b61b24` sobre `#fffdfd`: 6.53:1.
- `#8a1820` sobre `#fffdfd`: 9.28:1.
- `#231d20` sobre `#fffdfd`: 16.34:1.
- `#51474c` sobre `#fffdfd`: 8.80:1.
- `#ffffff` sobre `#b61b24`: 6.62:1.
- `#ffffff` sobre `#171214`: 18.53:1.
- `#0f766e` sobre `#fffdfd`: 5.40:1.
- `#b45309` sobre `#fffdfd`: 4.95:1.
- `#2563eb` sobre `#fffdfd`: 5.10:1.

Esses pares atendem WCAG AA para texto normal. Para texto pequeno sobre fundos transluidos, valide o contraste final no contexto.

### Tipografia

- Display: `Sora`, peso 600-800, titulos curtos e metricas.
- UI/texto: `Manrope`, peso 400-800, formularios, tabelas, botoes, hints e navegacao.
- Letter spacing: manter `0` em titulos, marca e numeros grandes. Use tracking positivo apenas em labels curtos uppercase, com moderacao.
- Tamanho minimo: 16px para corpo; 14px apenas para metadados, chips e labels auxiliares.

### Espacamento, bordas e sombra

- Escala base: `--space-1` 4px, `--space-2` 8px, `--space-3` 12px, `--space-4` 16px, `--space-5` 24px, `--space-6` 32px, `--space-7` 48px.
- Raios existentes: `--radius-sm`, `--radius-md`, `--radius-lg`, `--radius-xl`. Para componentes novos, prefira o menor raio que ainda preserve consistencia com o shell.
- Sombras: usar `--shadow-card` para cards e `--shadow-soft` para overlays ou paineis flutuantes. Evite sombra em excesso em tabelas densas.

## Diretrizes de componentes

- Botoes: `btn-primary` para acao principal da tela; `btn-outline-secondary` para cancelar/voltar; `btn-danger` para operacao destrutiva; `btn-warning` para alteracao sensivel que exige atencao.
- Chips: sempre combinar cor com texto explicito (`Sessao ativa`, `Pendente`, `Restrito`), nunca depender so de cor.
- Cards: usar para itens repetidos, formularios e paineis de detalhe. Evite cards dentro de cards.
- Tabelas: manter densidade, cabecalho claro, acoes alinhadas e botoes `btn-sm`.
- Forms: labels visiveis, texto de ajuda quando o campo mexe com equipamento, segredo, payload ou estado local.
- Estados de erro: mensagem segura, sem stack trace, segredo, payload bruto ou IP sensivel.
- Foco: preservar `:focus-visible` com anel vermelho translucido; nao remover outline.
- Disabled: deve parecer inativo por opacidade e cursor, mas manter contraste suficiente para texto essencial.

## Logo e simbolo

Arquivo criado: `wwwroot/img/brand/controlid-poc-mark.svg`.

Conceito:

- Simbolo 1:1 para favicon/app icon/documentacao.
- Metafora: anel de leitura/acesso, ponto central de decisao e nos de sinal para integracao.
- Estilo: geometrico, simples, sem texto pequeno, com cantos suaves e cores do sistema.
- Tamanho minimo recomendado: 24px para UI; 48px para documentacao; 128px para avatar/exportacao.
- Versoes futuras: clara, monocromatica e favicon gerado a partir do SVG.

Uso incorreto:

- Nao aplicar o simbolo como logotipo oficial da Control iD.
- Nao distorcer proporcao 1:1.
- Nao adicionar texto pequeno dentro do SVG.
- Nao usar em fundos com contraste insuficiente.

## Prompt de imagem

Prompt positivo:

```text
Professional operational dashboard brand mark for an ASP.NET Core access-control integration proof of concept, square icon, dark graphite background, red scanning ring, central access node, subtle teal and blue signal nodes, geometric, minimal, high contrast, enterprise software, secure operations, no text.
```

Prompt negativo:

```text
No official Control iD logo, no copied brand, no padlock cliche, no photorealistic device, no tiny text, no gradients as the main idea, no mascot, no decorative blobs, no low-contrast colors, no marketing landing page style.
```

## Governanca

- Ao criar componente novo, use tokens existentes antes de adicionar cor hardcoded; valores literais pertencem ao `:root`.
- Ao adicionar nova cor, documente papel, contraste e motivo.
- Ao alterar brand, shell, navegacao, cards, botoes ou estados, atualize este guia.
- Ao criar asset, preferir SVG simples e auditavel; imagens raster devem ter fonte, finalidade e alternativa textual.
- Qualquer mudanca visual em fluxo critico deve ser validada com teclado, foco visivel, mobile e contraste.
