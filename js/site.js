(() => {
  const headerLogos = document.querySelectorAll(".brand-header .brand-logo-horizontal");
  headerLogos.forEach((logo) => {
    logo.src = "/images/logo/olsen-data-horizontal-v3.png?v=20260311b";
  });

  const form = document.querySelector("[data-contact-form]");
  const yearNodes = document.querySelectorAll("[data-year]");

  yearNodes.forEach((node) => {
    node.textContent = String(new Date().getFullYear());
  });

  if (!form) {
    return;
  }

  const status = form.querySelector("[data-form-status]");
  const submitButton = form.querySelector('button[type="submit"]');

  const setFieldError = (fieldName, message) => {
    const errorNode = form.querySelector(`[data-field-error="${fieldName}"]`);
    if (errorNode) {
      errorNode.textContent = message || "";
    }
  };

  const clearErrors = () => {
    ["name", "email", "message"].forEach((fieldName) => setFieldError(fieldName, ""));
  };

  const setStatus = (type, message) => {
    if (!status) {
      return;
    }

    status.textContent = message;
    status.className = `form-status visible ${type}`;
  };

  const validate = (payload) => {
    const errors = {};

    if (!payload.name.trim()) {
      errors.name = "Skriv inn navnet ditt.";
    }

    if (!payload.email.trim()) {
      errors.email = "Skriv inn e-postadressen din.";
    } else if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(payload.email)) {
      errors.email = "Skriv inn en gyldig e-postadresse.";
    }

    if (!payload.message.trim()) {
      errors.message = "Skriv en kort melding.";
    } else if (payload.message.trim().length < 10) {
      errors.message = "Meldingen må være minst 10 tegn.";
    }

    return errors;
  };

  form.addEventListener("submit", async (event) => {
    event.preventDefault();

    clearErrors();

    const payload = {
      name: form.name.value.trim(),
      email: form.email.value.trim(),
      message: form.message.value.trim()
    };

    const clientErrors = validate(payload);
    if (Object.keys(clientErrors).length > 0) {
      Object.entries(clientErrors).forEach(([field, message]) => setFieldError(field, message));
      setStatus("error", "Skjemaet inneholder noen feil. Rett dem opp og prøv igjen.");
      return;
    }

    submitButton.disabled = true;
    setStatus("loading", "Sender melding...");

    try {
      const response = await fetch("/api/contact", {
        method: "POST",
        headers: {
          "Content-Type": "application/json"
        },
        body: JSON.stringify(payload)
      });

      const data = await response.json().catch(() => ({}));

      if (!response.ok) {
        const errors = data.errors || {};

        Object.entries(errors).forEach(([field, messages]) => {
          setFieldError(field, Array.isArray(messages) ? messages[0] : String(messages));
        });

        setStatus("error", "Kunne ikke sende meldingen. Kontroller feltene og prøv igjen.");
        return;
      }

      form.reset();
      setStatus("success", data.message || "Meldingen ble sendt.");
    } catch {
      setStatus("error", "Noe gikk galt under sending. Prøv igjen om litt.");
    } finally {
      submitButton.disabled = false;
    }
  });
})();
