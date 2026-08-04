(() => {
  document.querySelectorAll("form[data-pending-label]").forEach((form) => {
    form.addEventListener("submit", (event) => {
      const submitter = event.submitter instanceof HTMLElement
        ? event.submitter
        : form.querySelector("button[type='submit'], input[type='submit']");
      form.setAttribute("aria-busy", "true");
      if (submitter instanceof HTMLButtonElement) {
        submitter.textContent = form.dataset.pendingLabel || "Processando...";
        submitter.disabled = true;
      } else if (submitter instanceof HTMLInputElement) {
        submitter.value = form.dataset.pendingLabel || "Processando...";
        submitter.disabled = true;
      }
    });
  });

  document.querySelectorAll("form[data-upload-progress]").forEach((form) => {
    const status = form.querySelector("[data-upload-status]");
    const statusText = form.querySelector("[data-upload-status-text]");
    const progress = form.querySelector("[role='progressbar']");
    const progressBar = form.querySelector("[data-upload-progress-bar]");
    const cancelButton = form.querySelector("[data-upload-cancel]");
    const submitButton = form.querySelector("button[type='submit'], input[type='submit']");
    let activeRequest = null;

    form.addEventListener("submit", (event) => {
      if (!(form instanceof HTMLFormElement) || !form.reportValidity()) {
        return;
      }

      event.preventDefault();
      const request = new XMLHttpRequest();
      activeRequest = request;
      status?.removeAttribute("hidden");
      cancelButton?.removeAttribute("hidden");
      if (submitButton instanceof HTMLButtonElement || submitButton instanceof HTMLInputElement) {
        submitButton.disabled = true;
      }
      form.setAttribute("aria-busy", "true");

      request.upload.addEventListener("progress", (progressEvent) => {
        if (!progressEvent.lengthComputable) {
          return;
        }

        const percentage = Math.min(100, Math.round((progressEvent.loaded / progressEvent.total) * 100));
        progress?.setAttribute("aria-valuenow", String(percentage));
        if (progressBar instanceof HTMLElement) {
          progressBar.style.width = `${percentage}%`;
        }
        if (statusText) {
          statusText.textContent = `Enviando arquivo: ${percentage}%`;
        }
      });

      request.upload.addEventListener("load", () => {
        if (statusText) {
          statusText.textContent = "Arquivo recebido. Processando blocos no equipamento...";
        }
      });

      request.addEventListener("load", () => {
        activeRequest = null;
        document.open();
        document.write(request.responseText);
        document.close();
        window.history.replaceState({}, "", "/Media/AdMode");
      });

      request.addEventListener("error", () => {
        activeRequest = null;
        form.removeAttribute("aria-busy");
        if (statusText) {
          statusText.textContent = "Falha de rede durante o envio. Tente novamente.";
        }
        if (submitButton instanceof HTMLButtonElement || submitButton instanceof HTMLInputElement) {
          submitButton.disabled = false;
        }
      });

      request.addEventListener("abort", () => {
        activeRequest = null;
        form.removeAttribute("aria-busy");
        if (statusText) {
          statusText.textContent = "Envio cancelado.";
        }
        if (submitButton instanceof HTMLButtonElement || submitButton instanceof HTMLInputElement) {
          submitButton.disabled = false;
        }
        cancelButton?.setAttribute("hidden", "");
      });

      request.open((form.method || "POST").toUpperCase(), form.action, true);
      request.setRequestHeader("X-Requested-With", "XMLHttpRequest");
      request.send(new FormData(form));
    });

    cancelButton?.addEventListener("click", () => activeRequest?.abort());
  });
})();
