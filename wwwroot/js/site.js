(() => {
  if (window.__controlIdUiBootstrapped) {
    return;
  }

  // PERFORMANCE: o Browser Refresh do Visual Studio pode reinjetar o script do shell
  // sem recarregar a aba inteira. Este guarda evita listeners duplicados e leaks sutis.
  window.__controlIdUiBootstrapped = true;

  const storageKeys = {
    favorites: "controlid-ui-favorites",
    recent: "controlid-ui-recent",
    groups: "controlid-ui-nav-groups"
  };

  const SEARCH_DEBOUNCE_MS = 140;
  const SEARCH_CACHE_LIMIT = 40;
  const MAX_SEARCH_RESULTS = 8;

  const readStore = (key, fallback) => {
    try {
      const raw = window.localStorage.getItem(key);
      return raw ? JSON.parse(raw) : fallback;
    } catch {
      return fallback;
    }
  };

  const writeStore = (key, value) => {
    try {
      window.localStorage.setItem(key, JSON.stringify(value));
    } catch {
      // Ignore storage quota and private-mode failures.
    }
  };

  const debounce = (callback, delay) => {
    let timeoutId = 0;
    return (...args) => {
      window.clearTimeout(timeoutId);
      timeoutId = window.setTimeout(() => callback(...args), delay);
    };
  };

  const dedupeByKey = (items) => {
    const seen = new Set();
    return items.filter((item) => {
      if (!item || seen.has(item.key)) {
        return false;
      }

      seen.add(item.key);
      return true;
    });
  };

  const body = document.body;
  const moduleAnchors = Array.from(document.querySelectorAll("a[data-module-key][href]"));
  const searchEntries = Array.from(document.querySelectorAll("[data-search-entry]"));
  const catalogEntries = [...moduleAnchors, ...searchEntries];
  const linkMap = new Map();

  catalogEntries.forEach((link) => {
    const key = link.dataset.moduleKey || link.getAttribute("href");
    if (!key || linkMap.has(key)) {
      return;
    }

    const label = normalizeUiText(link.dataset.moduleLabel || link.textContent || "");
    const group = normalizeUiText(link.dataset.moduleGroup || "Módulos");
    const tags = normalizeUiText(link.dataset.moduleTags || "");
    const shortLabel = normalizeUiText(link.dataset.moduleShort || link.querySelector(".app-nav-link__icon")?.textContent || "PO");

    linkMap.set(key, {
      key,
      label,
      group,
      tags,
      href: link.getAttribute("href") || "#",
      short: shortLabel,
      searchIndex: `${label} ${group} ${tags}`.toLowerCase()
    });
  });

  const modules = Array.from(linkMap.values());
  const favoriteSet = new Set(readStore(storageKeys.favorites, []));
  let recentKeys = readStore(storageKeys.recent, []);
  const groupState = readStore(storageKeys.groups, {});
  const searchResultCache = new Map();
  let lastSearchRenderToken = "";
  let defaultSearchToken = "";
  let defaultSearchItems = [];

  const favoriteContainer = document.getElementById("favoriteModules");
  const recentContainer = document.getElementById("recentModules");
  const homeRecentContainer = document.getElementById("homeRecentModules");
  const searchInput = document.getElementById("moduleSearchInput");
  const searchResults = document.getElementById("moduleSearchResults");
  const searchStatus = document.getElementById("moduleSearchStatus");
  const favoriteButtons = Array.from(document.querySelectorAll("[data-favorite-toggle]"));
  let activeSearchIndex = -1;

  const setSearchResultsVisibility = (isVisible) => {
    if (!searchResults) {
      return;
    }

    searchResults.hidden = !isVisible;
    searchResults.setAttribute("aria-hidden", String(!isVisible));
    body.classList.toggle("app-search-open", isVisible);
    if (searchInput) {
      searchInput.setAttribute("aria-expanded", String(isVisible));
      if (!isVisible) {
        searchInput.removeAttribute("aria-activedescendant");
      }
    }

    if (isVisible && typeof closeTopMenus === "function") {
      closeTopMenus();
    }
  };

  const announceSearchStatus = (message) => {
    if (searchStatus) {
      searchStatus.textContent = message;
    }
  };

  const getSearchResultOptions = () => Array.from(searchResults?.querySelectorAll(".command-result") || []);

  const setActiveSearchResult = (index) => {
    const options = getSearchResultOptions();
    if (!options.length || !searchInput) {
      activeSearchIndex = -1;
      searchInput?.removeAttribute("aria-activedescendant");
      return;
    }

    activeSearchIndex = (index + options.length) % options.length;
    options.forEach((option, optionIndex) => {
      const isActive = optionIndex === activeSearchIndex;
      option.classList.toggle("is-active", isActive);
      option.setAttribute("aria-selected", String(isActive));
      if (isActive) {
        searchInput.setAttribute("aria-activedescendant", option.id);
        option.scrollIntoView({ block: "nearest" });
      }
    });
  };

  const createSafeModuleFragment = (item, includeShortLabel) => {
    const fragment = document.createDocumentFragment();

    if (includeShortLabel) {
      const shortLabel = document.createElement("span");
      shortLabel.textContent = item.short;
      fragment.appendChild(shortLabel);
    }

    const content = document.createElement("div");
    const title = document.createElement("strong");
    const subtitle = document.createElement("small");

    title.textContent = item.label;
    subtitle.textContent = item.group;
    content.appendChild(title);
    content.appendChild(subtitle);
    fragment.appendChild(content);

    return fragment;
  };

  function repairEncodingArtifacts(value) {
    const input = value || "";
    return input
      .replace(/\u00C3\u00A1/g, "á")
      .replace(/\u00C3\u00A2/g, "â")
      .replace(/\u00C3\u00A3/g, "ã")
      .replace(/\u00C3\u00A0/g, "à")
      .replace(/\u00C3\u00A9/g, "é")
      .replace(/\u00C3\u00AA/g, "ê")
      .replace(/\u00C3\u00AD/g, "í")
      .replace(/\u00C3\u00B3/g, "ó")
      .replace(/\u00C3\u00B4/g, "ô")
      .replace(/\u00C3\u00B5/g, "õ")
      .replace(/\u00C3\u00BA/g, "ú")
      .replace(/\u00C3\u00A7/g, "ç")
      .replace(/\u00C2\u00BA/g, "º")
      .replace(/\u00C2\u00AA/g, "ª")
      .replace(/\u00C2/g, "")
      .replace(/\uFFFD/g, "");
  }

  function normalizeUiText(value) {
    return repairEncodingArtifacts(value).replace(/\s+/g, " ").trim();
  }

  const getContextLabel = (element) => {
    const scope = element.closest("section, article, form, .app-surface-card, .detail-card, .hero-panel") || document;
    const heading = scope.querySelector(".app-page-section__heading strong, .app-page-header__title, h1, h2, h3, legend");
    return normalizeUiText(heading?.textContent || "");
  };

  const applyInteractionAriaFallbacks = () => {
    try {
      document.querySelectorAll("button, a.btn, input:not([type='hidden']), select, textarea").forEach((element) => {
        if (element.hasAttribute("aria-label") || element.hasAttribute("aria-labelledby")) {
          return;
        }

        if ("labels" in element && element.labels?.length) {
          return;
        }

        if (normalizeUiText(element.textContent || "")) {
          return;
        }

        if (element instanceof HTMLInputElement && ["submit", "button"].includes(element.type) && normalizeUiText(element.value)) {
          return;
        }

        let label = normalizeUiText(element.getAttribute("title") || element.getAttribute("placeholder") || "");
        if (!label) {
          const context = getContextLabel(element);
          const tagName = element.tagName.toLowerCase();
          label = context ? `${tagName} em ${context}` : "";
        }

        if (label) {
          element.setAttribute("aria-label", label);
        }
      });
    } catch {
      // A11Y: o reforço de rótulos precisa falhar em silêncio para não
      // interromper a navegação caso algum nó legado fuja do contrato esperado.
    }
  };

  const applyAlertAccessibilityFallbacks = () => {
    try {
      document.querySelectorAll(".alert").forEach((alertElement) => {
        const isUrgent = alertElement.classList.contains("alert-danger") || alertElement.classList.contains("alert-warning");
        if (!alertElement.hasAttribute("role")) {
          alertElement.setAttribute("role", isUrgent ? "alert" : "status");
        }

        if (!alertElement.hasAttribute("aria-live")) {
          alertElement.setAttribute("aria-live", isUrgent ? "assertive" : "polite");
        }

        if (!alertElement.hasAttribute("aria-atomic")) {
          alertElement.setAttribute("aria-atomic", "true");
        }
      });
    } catch {
      // A11Y: alertas antigos continuam funcionais mesmo que o reforço falhe.
    }
  };

  const applyTableAccessibilityFallbacks = () => {
    try {
      document.querySelectorAll(".app-content table").forEach((table, index) => {
        const label = normalizeUiText(table.getAttribute("aria-label") || getContextLabel(table) || `Tabela operacional ${index + 1}`);
        table.setAttribute("aria-label", label);

        if (!table.querySelector("caption")) {
          const caption = document.createElement("caption");
          caption.className = "visually-hidden";
          caption.textContent = label;
          table.prepend(caption);
        }
      });
    } catch {
      // A11Y: o fallback semântico das tabelas é progressivo; não deve quebrar
      // a tela caso uma estrutura específica já tenha markup customizado.
    }
  };

  const renderCollection = (container, keys, emptyMessage) => {
    if (!container) {
      return;
    }

    const validItems = keys.map((key) => linkMap.get(key)).filter(Boolean);
    const fragment = document.createDocumentFragment();

    if (!validItems.length) {
      const empty = document.createElement("div");
      empty.className = "collection-list__empty";
      empty.textContent = emptyMessage;
      fragment.appendChild(empty);
      container.replaceChildren(fragment);
      return;
    }

    validItems.forEach((item) => {
      const anchor = document.createElement("a");
      anchor.className = "collection-link";
      anchor.href = item.href;
      anchor.dataset.moduleKey = item.key;
      anchor.setAttribute("aria-label", `${item.label} no grupo ${item.group}`);
      anchor.appendChild(createSafeModuleFragment(item, true));
      fragment.appendChild(anchor);
    });

    container.replaceChildren(fragment);
  };

  const renderFavoriteButtons = () => {
    favoriteButtons.forEach((button) => {
      const key = button.dataset.moduleKey || "";
      const active = favoriteSet.has(key);
      const label = button.dataset.moduleLabel || "módulo";
      button.classList.toggle("is-favorite", active);
      button.textContent = active ? "\u2605" : "\u2606";
      button.title = active ? "Remover dos favoritos" : "Fixar nos favoritos";
      button.setAttribute("aria-pressed", String(active));
      button.setAttribute("aria-label", active
        ? `Remover ${label} dos favoritos`
        : `Fixar ${label} nos favoritos`);
    });
  };

  const setCachedSearchItems = (term, items) => {
    if (searchResultCache.has(term)) {
      searchResultCache.delete(term);
    }

    searchResultCache.set(term, items);

    if (searchResultCache.size > SEARCH_CACHE_LIMIT) {
      const oldestKey = searchResultCache.keys().next().value;
      if (oldestKey) {
        searchResultCache.delete(oldestKey);
      }
    }
  };

  const buildDefaultSearchItems = () => {
    const token = `${Array.from(favoriteSet).sort().join("|")}::${recentKeys.join("|")}`;
    if (token === defaultSearchToken) {
      return defaultSearchItems;
    }

    defaultSearchToken = token;
    defaultSearchItems = dedupeByKey([
      ...Array.from(favoriteSet).map((key) => linkMap.get(key)),
      ...recentKeys.map((key) => linkMap.get(key))
    ].filter(Boolean)).slice(0, MAX_SEARCH_RESULTS);
    return defaultSearchItems;
  };

  const resolveSearchItems = (term) => {
    if (!term) {
      return buildDefaultSearchItems();
    }

    if (searchResultCache.has(term)) {
      return searchResultCache.get(term);
    }

    // PERFORMANCE: os módulos são estáticos durante a sessão; um cache pequeno
    // evita repetir filtros de string para buscas digitadas com frequência.
    const items = modules.filter((item) => item.searchIndex.includes(term)).slice(0, MAX_SEARCH_RESULTS);
    setCachedSearchItems(term, items);
    return items;
  };

  const renderCollections = () => {
    renderCollection(favoriteContainer, Array.from(favoriteSet), "Fixe os módulos mais usados para acesso rápido.");
    renderCollection(recentContainer, recentKeys, "Os últimos módulos visitados aparecerão aqui.");
    renderCollection(homeRecentContainer, recentKeys, "Os fluxos visitados recentemente aparecerão aqui depois da sua navegação.");
    renderFavoriteButtons();
  };

  const rememberModule = (key) => {
    if (!key) {
      return;
    }

    recentKeys = [key, ...recentKeys.filter((current) => current !== key)].slice(0, 6);
    writeStore(storageKeys.recent, recentKeys);
    renderCollections();

    if (searchInput && document.activeElement === searchInput) {
      renderSearchResults(searchInput.value, true);
    }
  };

  const renderSearchResults = (query, force = false) => {
    if (!searchResults) {
      return;
    }

    const term = (query || "").trim().toLowerCase();
    const items = resolveSearchItems(term);
    const token = `${term}|${items.map((item) => item.key).join("|")}`;

    if (!force && token === lastSearchRenderToken && !searchResults.hidden) {
      setSearchResultsVisibility(true);
      return;
    }

    lastSearchRenderToken = token;

    const fragment = document.createDocumentFragment();
    activeSearchIndex = -1;
    searchInput?.removeAttribute("aria-activedescendant");

    if (!items.length) {
      const empty = document.createElement("div");
      empty.className = "command-empty";
      empty.setAttribute("role", "option");
      empty.setAttribute("aria-disabled", "true");
      empty.setAttribute("aria-selected", "false");
      empty.textContent = term
        ? "Nenhum módulo encontrado para a busca informada."
        : "Digite para buscar por domínio, módulo, API ou auditoria.";
      fragment.appendChild(empty);
      announceSearchStatus(empty.textContent);
    } else {
      const resultLabel = items.length === 1 ? "1 resultado encontrado." : `${items.length} resultados encontrados.`;
      announceSearchStatus(`${resultLabel} Use seta para baixo e seta para cima para navegar.`);
      items.forEach((item, index) => {
        const anchor = document.createElement("a");
        anchor.className = "command-result";
        anchor.id = `moduleSearchOption-${index}`;
        anchor.href = item.href;
        anchor.dataset.moduleKey = item.key;
        anchor.dataset.searchResultIndex = String(index);
        anchor.setAttribute("role", "option");
        anchor.setAttribute("aria-selected", "false");
        anchor.setAttribute("aria-label", `${item.label} no grupo ${item.group}`);
        anchor.tabIndex = -1;
        anchor.appendChild(createSafeModuleFragment(item, false));
        fragment.appendChild(anchor);
      });
    }

    searchResults.replaceChildren(fragment);
    setSearchResultsVisibility(true);
  };

  const debouncedSearchResults = debounce((value) => {
    renderSearchResults(value);
  }, SEARCH_DEBOUNCE_MS);

  renderCollections();

  const setSidebarOpen = (open) => body.classList.toggle("sidebar-open", open);
  document.querySelectorAll("[data-sidebar-toggle]").forEach((button) => button.addEventListener("click", () => setSidebarOpen(true)));
  document.querySelectorAll("[data-sidebar-close]").forEach((button) => button.addEventListener("click", () => setSidebarOpen(false)));

  document.querySelectorAll("[data-group-toggle]").forEach((button) => {
    const groupId = button.dataset.groupToggle;
    const wrapper = button.closest("[data-nav-group]");
    if (!groupId || !wrapper) {
      return;
    }

    const collapsed = Object.prototype.hasOwnProperty.call(groupState, groupId)
      ? groupState[groupId] === true
      : wrapper.classList.contains("is-collapsed");
    wrapper.classList.toggle("is-collapsed", collapsed);
    button.setAttribute("aria-expanded", String(!collapsed));

    button.addEventListener("click", () => {
      const nextState = !wrapper.classList.contains("is-collapsed");
      wrapper.classList.toggle("is-collapsed", nextState);
      button.setAttribute("aria-expanded", String(!nextState));
      groupState[groupId] = nextState;
      writeStore(storageKeys.groups, groupState);
    });
  });

  if (searchInput && searchResults) {
    searchInput.addEventListener("focus", () => renderSearchResults(searchInput.value, true));
    searchInput.addEventListener("input", () => debouncedSearchResults(searchInput.value));
    searchInput.addEventListener("keydown", (event) => {
      if (event.key === "Escape") {
        setSearchResultsVisibility(false);
        announceSearchStatus("Busca fechada.");
        return;
      }

      if (event.key === "ArrowDown" || event.key === "ArrowUp") {
        event.preventDefault();
        if (searchResults.hidden || !getSearchResultOptions().length) {
          renderSearchResults(searchInput.value, true);
        }

        const options = getSearchResultOptions();
        if (options.length) {
          const nextIndex = event.key === "ArrowDown"
            ? activeSearchIndex + 1
            : activeSearchIndex - 1;
          setActiveSearchResult(nextIndex);
        }

        return;
      }

      if ((event.key === "Home" || event.key === "End") && !searchResults.hidden) {
        const options = getSearchResultOptions();
        if (options.length) {
          event.preventDefault();
          setActiveSearchResult(event.key === "Home" ? 0 : options.length - 1);
        }

        return;
      }

      if (event.key === "Enter") {
        if (searchResults.hidden || !getSearchResultOptions().length) {
          renderSearchResults(searchInput.value, true);
        }

        const options = getSearchResultOptions();
        const selectedResult = activeSearchIndex >= 0 ? options[activeSearchIndex] : options[0];
        if (selectedResult) {
          event.preventDefault();
          window.location.href = selectedResult.href;
        }
      }
    });

    searchResults.addEventListener("mouseover", (event) => {
      const option = event.target instanceof Element
        ? event.target.closest(".command-result")
        : null;
      const index = Number(option?.getAttribute("data-search-result-index"));
      if (Number.isInteger(index)) {
        setActiveSearchResult(index);
      }
    });
  }

  document.addEventListener("click", (event) => {
    const target = event.target;
    if (!(target instanceof Element)) {
      return;
    }

    const confirmElement = target.closest("[data-confirm]");
    if (confirmElement) {
      const message = normalizeUiText(confirmElement.getAttribute("data-confirm") || "Confirmar operação?");
      if (!window.confirm(message)) {
        event.preventDefault();
        event.stopPropagation();
        return;
      }
    }

    const moduleLink = target.closest("a[data-module-key][href]");
    if (moduleLink) {
      rememberModule(moduleLink.dataset.moduleKey || moduleLink.getAttribute("href"));
    }

    if (searchResults && searchInput && !searchResults.contains(target) && target !== searchInput) {
      setSearchResultsVisibility(false);
    }

    const dismissButton = target.closest("[data-bs-dismiss='alert']");
    if (!dismissButton) {
      return;
    }

    const alertElement = dismissButton.closest(".alert");
    if (!alertElement) {
      return;
    }

    // PERFORMANCE: mantemos o comportamento de fechamento do alerta sem carregar
    // o bootstrap.bundle inteiro quando a aplicação só precisa desta interação.
    alertElement.classList.remove("show");
    window.setTimeout(() => {
      alertElement.remove();
    }, 150);
  });

  favoriteButtons.forEach((button) => {
    button.addEventListener("click", (event) => {
      event.preventDefault();
      const key = button.dataset.moduleKey || "";
      if (!key) {
        return;
      }

      if (favoriteSet.has(key)) {
        favoriteSet.delete(key);
      } else {
        favoriteSet.add(key);
      }

      writeStore(storageKeys.favorites, Array.from(favoriteSet));
      renderCollections();

      if (searchInput && document.activeElement === searchInput && !searchInput.value.trim()) {
        renderSearchResults("", true);
      }
    });
  });

  document.querySelectorAll("[data-copy-target]").forEach((button) => {
    button.addEventListener("click", async () => {
      const source = document.getElementById(button.dataset.copyTarget || "");
      if (!source) {
        return;
      }

      const originalLabel = button.textContent;
      const text = source.textContent || "";

      try {
        if (navigator.clipboard?.writeText) {
          await navigator.clipboard.writeText(text);
        } else {
          const helper = document.createElement("textarea");
          helper.value = text;
          document.body.appendChild(helper);
          helper.select();
          document.execCommand("copy");
          helper.remove();
        }

        button.textContent = "Copiado";
        window.setTimeout(() => { button.textContent = originalLabel; }, 1400);
      } catch {
        button.textContent = "Falhou";
        window.setTimeout(() => { button.textContent = originalLabel; }, 1400);
      }
    });
  });

  document.querySelectorAll(".app-content table.table").forEach((table) => {
    if (table.parentElement?.classList.contains("table-responsive")) {
      return;
    }

    const wrapper = document.createElement("div");
    wrapper.className = "table-responsive app-table-shell";
    table.parentNode?.insertBefore(wrapper, table);
    wrapper.appendChild(table);
  });

  document.querySelectorAll(".app-content pre:not(.app-code-panel)").forEach((pre) => pre.classList.add("app-code-panel"));
  applyTableAccessibilityFallbacks();
  applyInteractionAriaFallbacks();
  applyAlertAccessibilityFallbacks();

  const topMenus = Array.from(document.querySelectorAll("[data-topnav-menu]"));
  if (!topMenus.length) {
    return;
  }

  const updateTopMenuLayoutState = () => {
    // UX: reservamos espaço real para o megamenu aberto no header para que o
    // painel não pareça uma camada solta cobrindo a página inteira.
    body.classList.toggle("app-has-open-topnav", topMenus.some((menu) => menu.open));
  };

  // O menu superior usa <details> nativo; aqui mantemos a camada acessível e
  // o comportamento exclusivo sem recalcular a árvore de navegação inteira.
  const syncMenuState = (menu) => {
    const summary = menu.querySelector("[data-topnav-summary]");
    const panel = menu.querySelector("[data-topnav-panel]");
    if (summary) {
      summary.setAttribute("aria-expanded", String(menu.open));
    }

    if (panel) {
      panel.setAttribute("aria-hidden", String(!menu.open));
    }

    updateTopMenuLayoutState();
  };

  const closeTopMenus = (exceptMenu = null) => {
    topMenus.forEach((menu) => {
      if (menu !== exceptMenu) {
        menu.removeAttribute("open");
        syncMenuState(menu);
      }
    });
  };

  topMenus.forEach((menu) => {
    syncMenuState(menu);
    menu.addEventListener("toggle", () => {
      syncMenuState(menu);
      if (menu.open) {
        closeTopMenus(menu);
      }
    });
  });

  document.addEventListener("click", (event) => {
    const target = event.target;
    if (!(target instanceof Node)) {
      return;
    }

    if (!topMenus.some((menu) => menu.contains(target))) {
      closeTopMenus();
    }
  });

  document.addEventListener("keydown", (event) => {
    if (event.key === "Escape") {
      closeTopMenus();
    }
  });
})();


