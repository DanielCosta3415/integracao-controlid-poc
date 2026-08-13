# Identidade visual e sistema de design

> **Referência** · Público: design e frontend · Responsável: Produto · Última validação: 2026-08-12.

Este guia registra a linha de base visual da PoC Control iD para orientar evolução de UI, documentação, assets e agentes de código. Ele não substitui o manual oficial da marca Control iD nem autoriza copiar marcas registradas; o objetivo é governar a identidade própria desta PoC operacional.

## Diagnóstico visual

- Nome atual: `Integracao.ControlID.PoC`, apresentado na interface como `Control iD PoC` e `Console operacional da Access API`.
- Produto: console web técnico para integrar, operar, auditar e diagnosticar fluxos da Access API da Control iD.
- Público: desenvolvedores, integradores, operadores técnicos, QA, segurança e times internos que validam equipamento físico, callbacks, push, banco local e contratos.
- Tom: operacional, preciso, seguro, técnico e confiável. A interface deve parecer ferramenta de trabalho, não landing page.
- Identidade existente: vermelho como cor primária, grafite para estrutura, superfícies claras, cartões densos, indicadores de estado, tabelas e navegação por domínios.
- Tipografia existente: famílias locais `Segoe UI Variable Display` para títulos
  e `Segoe UI Variable Text` para interface e texto, com fallback para
  `Segoe UI` e `sans-serif`. A PoC não depende de fontes externas.
- Componentes principais: estrutura fixa, barra superior, busca de módulos, marcadores, cartões, tabelas, formulários, painéis de conexão, painéis de código e alertas.
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

- `wwwroot/css/site.css` concentra tokens e base;
  `wwwroot/css/site-content.css` reúne superfícies de conteúdo; e
  `wwwroot/css/site-shell.css` contém o shell desktop. Regras responsivas do
  shell permanecem em `site-shell-responsive.css`.
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

- Destaques: `Segoe UI Variable Display`, peso 600-800, títulos curtos e métricas.
- UI/texto: `Segoe UI Variable Text`, peso 400-800, formulários, tabelas, botões, dicas e navegação.
- Espaçamento entre letras: manter `0` em títulos, marca e números grandes. Use espaçamento positivo apenas em rótulos curtos em caixa alta, com moderação.
- Tamanho mínimo: 16px para corpo; 14px apenas para metadados, chips e labels auxiliares.

### Espaçamento, bordas e sombra

- Escala base: `--space-1` 4px, `--space-2` 8px, `--space-3` 12px, `--space-4` 16px, `--space-5` 24px, `--space-6` 32px, `--space-7` 48px.
- Raios existentes: `--radius-sm`, `--radius-md`, `--radius-lg`, `--radius-xl`. Para componentes novos, prefira o menor raio que ainda preserve consistência com o shell.
- Sombras: usar `--shadow-card` para cartões e `--shadow-soft` para sobreposições ou painéis flutuantes. Evite sombra em excesso em tabelas densas.

## Diretrizes de componentes

- Botões: `btn-primary` para ação principal da tela; `btn-outline-secondary` para cancelar/voltar; `btn-danger` para operação destrutiva; `btn-warning` para alteração sensível que exige atenção.
- Marcadores: sempre combinar cor com texto explícito (`Sessão ativa`, `Pendente`, `Restrito`), nunca depender somente da cor.
- Cartões: usar para itens repetidos, formulários e painéis de detalhe. Evite cartões dentro de cartões.
- Tabelas: manter densidade, cabeçalho claro, ações alinhadas e botões `btn-sm`.
- Formulários: rótulos visíveis e texto de ajuda quando o campo altera equipamento, segredo, carga útil ou estado local.
- Estados de erro: mensagem segura, sem rastreamento de pilha, segredo, carga útil bruta ou IP sensível.
- Foco: preservar `:focus-visible` com anel vermelho translúcido; não remover o contorno.
- Desabilitado: deve parecer inativo por opacidade e cursor, mas manter contraste suficiente para texto essencial.

## Logotipo e símbolo

Arquivo criado: `wwwroot/img/brand/controlid-poc-mark.svg`.

Conceito:

- Símbolo 1:1 para favicon, ícone da aplicação e documentação.
- Metáfora: anel de leitura/acesso, ponto central de decisão e nós de sinal para integração.
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
Símbolo profissional para painel operacional de uma prova de conceito ASP.NET Core de integração com controle de acesso; ícone quadrado; fundo grafite escuro; anel vermelho de varredura; nó central de acesso; nós discretos de sinal em verde-azulado e azul; geométrico; minimalista; alto contraste; software corporativo; operação segura; sem texto.
```

Prompt negativo:

```text
Sem logotipo oficial da Control iD; sem marca copiada; sem clichê de cadeado; sem dispositivo fotorrealista; sem texto pequeno; sem gradientes como ideia principal; sem mascote; sem formas decorativas; sem cores de baixo contraste; sem estilo de página promocional.
```

## Governança

- Ao criar componente novo, use tokens existentes antes de adicionar cor fixa; valores literais pertencem ao `:root`.
- Ao adicionar nova cor, documente papel, contraste e motivo.
- Ao alterar marca, estrutura, navegação, cartões, botões ou estados, atualize este guia.
- Ao criar recurso visual, prefira SVG simples e auditável; imagens raster devem ter fonte, finalidade e alternativa textual.
- Qualquer mudança visual em fluxo crítico deve ser validada com teclado, foco visível, dispositivo móvel e contraste.

## Referência visual versionada

![Símbolo da PoC Control iD](../../wwwroot/img/brand/controlid-poc-mark.svg)

O SVG é a referência para favicon, avatar e identificação compacta da PoC. Use a
marca completa do shell em contextos com espaço e o símbolo 1:1 em contextos
compactos. Não distorça, não recolora fora dos tokens e não combine com o
logotipo oficial do fabricante de modo que sugira produto oficial.

## Matriz de componentes e estados

| Componente | Estados obrigatórios | Verificação mínima |
| --- | --- | --- |
| Botão | padrão, hover, foco, active, disabled e loading | Nome acessível, alvo de 44 px e foco visível |
| Campo | vazio, preenchido, inválido, disabled e somente leitura | Label persistente e erro associado |
| Alerta | informação, sucesso, aviso e erro | Ícone/texto além da cor e região anunciável |
| Tabela | carregando, vazia, preenchida, truncada e erro | Cabeçalhos semânticos e overflow responsivo |
| Modal/confirmação | aberto, foco inicial, cancelamento e confirmação | Trap de foco, Escape e retorno do foco |
| Indicador de estado | ativo, pendente, restrito e indisponível | Texto explícito e contraste suficiente |

Mudanças visuais devem registrar viewport, tema, navegação por teclado, contraste
e screenshot sanitizado quando a diferença não puder ser demonstrada por teste.

## Evidência visual reproduzível

As referências de interface devem ser capturadas somente com o stub e dados
fictícios. Versione a composição em viewport desktop e registre a dimensão real
do arquivo; valide também a experiência em 390 × 844 e retenha a captura móvel
apenas quando ela demonstrar um estado responsivo distinto. Remova banco e
registros locais antes de compartilhar e nunca exponha IP, sessão, e-mail,
telefone ou biometria.

| Evidência | Caminho versionado | Dimensão atual | Quando atualizar |
| --- | --- | ---: | --- |
| Entrada local | `wwwroot/img/docs/local-login.png` | 1425 × 891 | Alteração no login, shell ou identidade visual |
| Área inicial autenticada | `wwwroot/img/docs/authenticated-home.png` | 1425 × 891 | Alteração na navegação ou hierarquia principal |
| Catálogo oficial | `wwwroot/img/docs/official-api.png` | 1425 × 891 | Alteração no catálogo, filtros ou retorno exibido |

Uma captura é referência de composição, não teste de acessibilidade. Mantenha os
testes de contrato, a navegação por teclado e a inspeção de contraste como
evidências independentes.

## Navegação documental

- [Voltar ao índice deste domínio](README.md).
- [Abrir a central de documentação](../README.md).
