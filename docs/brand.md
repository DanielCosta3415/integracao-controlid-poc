# Identidade visual e sistema de design

Este guia registra a linha de base visual da PoC Control iD para orientar evolução de UI, documentação, assets e agentes de código. Ele não substitui o manual oficial da marca Control iD nem autoriza copiar marcas registradas; o objetivo e governar a identidade própria desta PoC operacional.

## Diagnóstico visual

- Nome atual: `Integracao.ControlID.PoC`, apresentado na interface como `Control iD PoC` e `Console operacional da Access API`.
- Produto: console web técnico para integrar, operar, auditar e diagnosticar fluxos da Access API da Control iD.
- Público: desenvolvedores, integradores, operadores técnicos, QA, segurança e times internos que validam equipamento físico, callbacks, push, banco local e contratos.
- Tom: operacional, preciso, seguro, técnico e confiável. A interface deve parecer ferramenta de trabalho, não landing page.
- Identidade existente: vermelho como cor primaria, grafite/ink para estrutura, superfícies quentes claras, cards densos, chips de status, tabelas e navegação por domínios.
- Tipografia existente: `Sora` para títulos e marca; `Manrope` para a interface e o texto.
- Componentes principais: shell fixo, topbar, busca de módulos, chips, cards, tabelas, formulários, painéis de conexão, code panels e alertas.
- Assets existentes: `wwwroot/favicon.ico` e marca construída em CSS no shell. Foi adicionado `wwwroot/img/brand/controlid-poc-mark.svg` como símbolo independente da PoC.

## Estratégia de marca

- Promessa: dar controle operacional seguro e rastreável sobre integrações Control iD sem esconder risco técnico.
- Personalidade: vigilante, clara, pragmática, responsável e preparada para auditoria.
- Sensação desejada: "console de comando confiável", com leitura rápida de estado, ação e evidência.
- Palavras-chave: acesso, sinal, equipamento, auditoria, contrato, telemetria, segurança, rastreabilidade.
- Diferenciação: unir catálogo técnico, operação local e diagnóstico em uma interface única orientada por contexto.
- O que evitar: visual promocional, excesso de hero marketing, gráficos decorativos sem função, dependência exclusiva de cor, copiar o logotipo Control iD, prometer conformidade total ou segurança perfeita.
- Referências permitidas: painéis operacionais, consoles de monitoramento, linguagem de status industrial, mapas de endpoint e ferramentas de QA.

## Sistema visual

### Tokens de cor

| Papel | Token | Valor | Uso |
| --- | --- | --- | --- |
| Primaria | `--color-brand-primary` | `#b61b24` | Ações principais, links, foco de marca |
| Primaria hover | `--color-brand-primary-hover` | `#8a1820` | Hover/pressed de marca |
| Acento | `--color-brand-accent` | `#f03b3f` | Destaques, anel da marca, detalhes |
| Texto forte | `--color-text-primary` | `#171214` | Títulos e números de métrica |
| Texto padrão | `--color-text-body` | `#231d20` | Corpo e labels importantes |
| Texto secundario | `--color-text-muted` | `#51474c` | Hints, metadados e descrições |
| Fundo | `--color-surface-page` | `#fbf7f6` | Fundo geral quente e baixo contraste visual |
| Painel | `--color-surface-panel` | `#fffdfd` | Cards, formulários e superfícies de leitura |
| Borda sutil | `--color-border-subtle` | `rgba(39, 26, 30, 0.08)` | Divisores e containers |
| Borda forte | `--color-border-strong` | `rgba(39, 26, 30, 0.14)` | Estados ativos e separações maiores |
| Sucesso | `--success` | `#0f766e` | Sessão ativa, equipamento pronto, sucesso |
| Alerta | `--warning` | `#b45309` | Pendente, atenção, confirmação |
| Info | `--info` | `#2563eb` | Informação técnica e apoio |
| Perigo | `--danger` | `#b61b24` | Exclusão, falha, bloqueio |

Use vermelho como acento de decisão e criticidade, não como preenchimento dominante de toda a tela. Combine com grafite, branco quente, teal e azul para reduzir monotonia e manter hierarquia.

### Tokenização aplicada no CSS

- `wwwroot/css/site.css` centraliza valores `hex` e `rgba()` no bloco `:root`.
- Fora de `:root`, cores, overlays, sombras, fundos transluidos, estados e textos inversos devem usar `var(--...)`.
- Tokens `--white-alpha-*`, `--ink-alpha-*`, `--brand-primary-alpha-*`, `--brand-accent-alpha-*`, `--surface-*-alpha-*`, `--success-alpha-*`, `--warning-alpha-*` e `--info-alpha-*` existem para evitar novos valores soltos.
- `--color-text-inverse`, `--warning-text-on-dark` e `--success-text-on-dark` governam texto sobre superfícies escuras.
- `--surface-sidebar-*`, `--surface-header-*`, `--surface-topbar-alpha-98`, `--page-glow-*` e `--modal-backdrop` governam camadas de shell, header e overlay.
- Valores literais novos só devem ser adicionados em `:root`, com papel claro e contraste avaliado.

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

- Destaques: `Sora`, peso 600-800, títulos curtos e métricas.
- UI/texto: `Manrope`, peso 400-800, formulários, tabelas, botões, hints e navegação.
- Espaçamento entre letras: manter `0` em títulos, marca e números grandes. Use espaçamento positivo apenas em rótulos curtos em caixa alta, com moderação.
- Tamanho mínimo: 16px para corpo; 14px apenas para metadados, chips e labels auxiliares.

### Espaçamento, bordas e sombra

- Escala base: `--space-1` 4px, `--space-2` 8px, `--space-3` 12px, `--space-4` 16px, `--space-5` 24px, `--space-6` 32px, `--space-7` 48px.
- Raios existentes: `--radius-sm`, `--radius-md`, `--radius-lg`, `--radius-xl`. Para componentes novos, prefira o menor raio que ainda preserve consistência com o shell.
- Sombras: usar `--shadow-card` para cards e `--shadow-soft` para overlays ou painéis flutuantes. Evite sombra em excesso em tabelas densas.

## Diretrizes de componentes

- Botões: `btn-primary` para ação principal da tela; `btn-outline-secondary` para cancelar/voltar; `btn-danger` para operação destrutiva; `btn-warning` para alteração sensível que exige atenção.
- Chips: sempre combinar cor com texto explícito (`Sessão ativa`, `Pendente`, `Restrito`), nunca depender somente da cor.
- Cards: usar para itens repetidos, formulários e painéis de detalhe. Evite cards dentro de cards.
- Tabelas: manter densidade, cabeçalho claro, ações alinhadas e botões `btn-sm`.
- Forms: labels visíveis, texto de ajuda quando o campo mexe com equipamento, segredo, payload ou estado local.
- Estados de erro: mensagem segura, sem stack trace, segredo, payload bruto ou IP sensível.
- Foco: preservar `:focus-visible` com anel vermelho translucido; não remover outline.
- Disabled: deve parecer inativo por opacidade e cursor, mas manter contraste suficiente para texto essencial.

## Logotipo e símbolo

Arquivo criado: `wwwroot/img/brand/controlid-poc-mark.svg`.

Conceito:

- Símbolo 1:1 para favicon/app icon/documentação.
- Metafora: anel de leitura/acesso, ponto central de decisão e nos de sinal para integração.
- Estilo: geométrico, simples, sem texto pequeno, com cantos suaves e cores do sistema.
- Tamanho mínimo recomendado: 24px para UI; 48px para documentação; 128px para avatar/exportação.
- Versões futuras: clara, monocromática e favicon gerado a partir do SVG.

Uso incorreto:

- Não aplicar o símbolo como logotipo oficial da Control iD.
- Não distorcer proporção 1:1.
- Não adicionar texto pequeno dentro do SVG.
- Não usar em fundos com contraste insuficiente.

## Instrução para geração de imagem

Prompt positivo:

```text
Professional operational dashboard brand mark for an ASP.NET Core access-control integration proof of concept, square icon, dark graphite background, red scanning ring, central access node, subtle teal and blue signal nodes, geometric, minimal, high contrast, enterprise software, secure operations, no text.
```

Prompt negativo:

```text
No official Control iD logo, no copied brand, no padlock cliche, no photorealistic device, no tiny text, no gradients as the main idea, no mascot, no decorative blobs, no low-contrast colors, no marketing landing page style.
```

## Governança

- Ao criar componente novo, use tokens existentes antes de adicionar cor hardcoded; valores literais pertencem ao `:root`.
- Ao adicionar nova cor, documente papel, contraste e motivo.
- Ao alterar brand, shell, navegação, cards, botões ou estados, atualize este guia.
- Ao criar asset, preferir SVG simples e auditável; imagens raster devem ter fonte, finalidade e alternativa textual.
- Qualquer mudança visual em fluxo crítico deve ser validada com teclado, foco visível, mobile e contraste.
