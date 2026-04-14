(() => {
  const storageKeys = {
    favorites: "controlid-ui-favorites",
    recent: "controlid-ui-recent",
    groups: "controlid-ui-nav-groups"
  };

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

  const body = document.body;
  const links = Array.from(document.querySelectorAll("[data-module-link]"));
  const searchEntries = Array.from(document.querySelectorAll("[data-search-entry]"));
  const catalogEntries = [...links, ...searchEntries];
  const linkMap = new Map(catalogEntries.map((link) => [link.dataset.moduleKey || link.getAttribute("href"), {
    key: link.dataset.moduleKey || link.getAttribute("href"),
    label: link.dataset.moduleLabel || link.textContent.trim(),
    group: link.dataset.moduleGroup || "Módulos",
    tags: link.dataset.moduleTags || "",
    href: link.getAttribute("href") || "#",
    short: (link.dataset.moduleShort || link.querySelector(".app-nav-link__icon")?.textContent || "PO").trim()
  }]));

  const favoriteSet = new Set(readStore(storageKeys.favorites, []));
  let recentKeys = readStore(storageKeys.recent, []);
  const groupState = readStore(storageKeys.groups, {});

  const favoriteContainer = document.getElementById("favoriteModules");
  const recentContainer = document.getElementById("recentModules");
  const homeRecentContainer = document.getElementById("homeRecentModules");
  const searchInput = document.getElementById("moduleSearchInput");
  const searchResults = document.getElementById("moduleSearchResults");

  const renderCollection = (container, keys, emptyMessage) => {
    if (!container) {
      return;
    }

    container.innerHTML = "";
    const validItems = keys.map((key) => linkMap.get(key)).filter(Boolean);

    if (!validItems.length) {
      const empty = document.createElement("div");
      empty.className = "collection-list__empty";
      empty.textContent = emptyMessage;
      container.appendChild(empty);
      return;
    }

    validItems.forEach((item) => {
      const anchor = document.createElement("a");
      anchor.className = "collection-link";
      anchor.href = item.href;
      anchor.dataset.moduleKey = item.key;
      anchor.innerHTML = `<span>${item.short}</span><div><strong>${item.label}</strong><small>${item.group}</small></div>`;
      anchor.addEventListener("click", () => rememberModule(item.key));
      container.appendChild(anchor);
    });
  };

  const renderCollections = () => {
    renderCollection(favoriteContainer, Array.from(favoriteSet), "Fixe os módulos mais usados para acesso rápido.");
    renderCollection(recentContainer, recentKeys, "Os últimos módulos visitados aparecerão aqui.");
    renderCollection(homeRecentContainer, recentKeys, "Os fluxos visitados recentemente aparecerão aqui depois da sua navegação.");

    document.querySelectorAll("[data-favorite-toggle]").forEach((button) => {
      const key = button.dataset.moduleKey || "";
      const active = favoriteSet.has(key);
      button.classList.toggle("is-favorite", active);
      button.textContent = active ? "★" : "☆";
      button.title = active ? "Remover dos favoritos" : "Fixar nos favoritos";
    });
  };

  const rememberModule = (key) => {
    if (!key) {
      return;
    }

    recentKeys = [key, ...recentKeys.filter((current) => current !== key)].slice(0, 6);
    writeStore(storageKeys.recent, recentKeys);
    renderCollections();
  };

  links.forEach((link) => {
    const key = link.dataset.moduleKey || link.getAttribute("href");
    link.addEventListener("click", () => rememberModule(key));
  });

  document.querySelectorAll("[data-favorite-toggle]").forEach((button) => {
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
    });
  });

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

  const renderSearchResults = (query) => {
    if (!searchResults) {
      return;
    }

    const term = (query || "").trim().toLowerCase();
    const modules = Array.from(linkMap.values());
    const items = term
      ? modules.filter((item) => `${item.label} ${item.group} ${item.tags}`.toLowerCase().includes(term)).slice(0, 8)
      : [
          ...Array.from(favoriteSet).map((key) => linkMap.get(key)).filter(Boolean),
          ...recentKeys.map((key) => linkMap.get(key)).filter(Boolean)
        ].filter((value, index, self) => self.findIndex((candidate) => candidate.key === value.key) === index).slice(0, 8);

    searchResults.innerHTML = "";

    if (!items.length) {
      const empty = document.createElement("div");
      empty.className = "command-empty";
      empty.textContent = "Nenhum módulo encontrado para a busca informada.";
      searchResults.appendChild(empty);
    } else {
      items.forEach((item) => {
        const anchor = document.createElement("a");
        anchor.className = "command-result";
        anchor.href = item.href;
        anchor.dataset.moduleKey = item.key;
        anchor.innerHTML = `<strong>${item.label}</strong><small>${item.group}</small>`;
        anchor.addEventListener("click", () => rememberModule(item.key));
        searchResults.appendChild(anchor);
      });
    }

    searchResults.hidden = false;
  };

  if (searchInput && searchResults) {
    searchInput.addEventListener("focus", () => renderSearchResults(searchInput.value));
    searchInput.addEventListener("input", () => renderSearchResults(searchInput.value));
    searchInput.addEventListener("keydown", (event) => {
      if (event.key === "Escape") {
        searchResults.hidden = true;
      }

      if (event.key === "Enter") {
        const firstResult = searchResults.querySelector("a.command-result");
        if (firstResult) {
          event.preventDefault();
          window.location.href = firstResult.href;
        }
      }
    });

    document.addEventListener("click", (event) => {
      if (!searchResults.contains(event.target) && event.target !== searchInput) {
        searchResults.hidden = true;
      }
    });
  }

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
        setTimeout(() => { button.textContent = originalLabel; }, 1400);
      } catch {
        button.textContent = "Falhou";
        setTimeout(() => { button.textContent = originalLabel; }, 1400);
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
})();

(() => {
  const topMenus = Array.from(document.querySelectorAll("[data-topnav-menu]"));
  if (!topMenus.length) {
    return;
  }

  const closeTopMenus = (exceptMenu = null) => {
    topMenus.forEach((menu) => {
      if (menu !== exceptMenu) {
        menu.removeAttribute("open");
      }
    });
  };

  topMenus.forEach((menu) => {
    menu.addEventListener("toggle", () => {
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
