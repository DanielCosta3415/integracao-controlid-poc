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

    const label = (link.dataset.moduleLabel || link.textContent || "").trim();
    const group = (link.dataset.moduleGroup || "Modulos").trim();
    const tags = (link.dataset.moduleTags || "").trim();
    const shortLabel = (link.dataset.moduleShort || link.querySelector(".app-nav-link__icon")?.textContent || "PO").trim();

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
  let lastSearchRenderToken = "";

  const favoriteContainer = document.getElementById("favoriteModules");
  const recentContainer = document.getElementById("recentModules");
  const homeRecentContainer = document.getElementById("homeRecentModules");
  const searchInput = document.getElementById("moduleSearchInput");
  const searchResults = document.getElementById("moduleSearchResults");
  const favoriteButtons = Array.from(document.querySelectorAll("[data-favorite-toggle]"));

  const setSearchResultsVisibility = (isVisible) => {
    if (!searchResults) {
      return;
    }

    searchResults.hidden = !isVisible;
    searchResults.setAttribute("aria-hidden", String(!isVisible));
    if (searchInput) {
      searchInput.setAttribute("aria-expanded", String(isVisible));
    }
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
      anchor.appendChild(createSafeModuleFragment(item, true));
      fragment.appendChild(anchor);
    });

    container.replaceChildren(fragment);
  };

  const renderFavoriteButtons = () => {
    favoriteButtons.forEach((button) => {
      const key = button.dataset.moduleKey || "";
      const active = favoriteSet.has(key);
      const label = button.dataset.moduleLabel || "modulo";
      button.classList.toggle("is-favorite", active);
      button.textContent = active ? "\u2605" : "\u2606";
      button.title = active ? "Remover dos favoritos" : "Fixar nos favoritos";
      button.setAttribute("aria-pressed", String(active));
      button.setAttribute("aria-label", active
        ? `Remover ${label} dos favoritos`
        : `Fixar ${label} nos favoritos`);
    });
  };

  const buildDefaultSearchItems = () => {
    return dedupeByKey([
      ...Array.from(favoriteSet).map((key) => linkMap.get(key)),
      ...recentKeys.map((key) => linkMap.get(key))
    ].filter(Boolean)).slice(0, MAX_SEARCH_RESULTS);
  };

  const renderCollections = () => {
    renderCollection(favoriteContainer, Array.from(favoriteSet), "Fixe os modulos mais usados para acesso rapido.");
    renderCollection(recentContainer, recentKeys, "Os ultimos modulos visitados aparecerao aqui.");
    renderCollection(homeRecentContainer, recentKeys, "Os fluxos visitados recentemente aparecerao aqui depois da sua navegacao.");
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
    const items = term
      ? modules.filter((item) => item.searchIndex.includes(term)).slice(0, MAX_SEARCH_RESULTS)
      : buildDefaultSearchItems();
    const token = `${term}|${items.map((item) => item.key).join("|")}`;

    if (!force && token === lastSearchRenderToken && !searchResults.hidden) {
      setSearchResultsVisibility(true);
      return;
    }

    lastSearchRenderToken = token;

    const fragment = document.createDocumentFragment();

    if (!items.length) {
      const empty = document.createElement("div");
      empty.className = "command-empty";
      empty.textContent = "Nenhum modulo encontrado para a busca informada.";
      fragment.appendChild(empty);
    } else {
      items.forEach((item) => {
        const anchor = document.createElement("a");
        anchor.className = "command-result";
        anchor.href = item.href;
        anchor.dataset.moduleKey = item.key;
        anchor.setAttribute("role", "option");
        anchor.setAttribute("aria-label", `${item.label} no grupo ${item.group}`);
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
      }

      if (event.key === "Enter") {
        const firstResult = searchResults.querySelector("a.command-result");
        if (firstResult) {
          event.preventDefault();
          window.location.href = firstResult.href;
        }
      }
    });
  }

  document.addEventListener("click", (event) => {
    const target = event.target;
    if (!(target instanceof Element)) {
      return;
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
    // o bootstrap.bundle inteiro quando a aplicacao so precisa desta interacao.
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

  const topMenus = Array.from(document.querySelectorAll("[data-topnav-menu]"));
  if (!topMenus.length) {
    return;
  }

  const updateTopMenuLayoutState = () => {
    // UX: reservamos espaco real para o megamenu aberto no header para que o
    // painel nao pareca uma camada solta cobrindo a pagina inteira.
    body.classList.toggle("app-has-open-topnav", topMenus.some((menu) => menu.open));
  };

  // O menu superior usa <details> nativo; aqui mantemos a camada acessivel e
  // o comportamento exclusivo sem recalcular a arvore de navegacao inteira.
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
