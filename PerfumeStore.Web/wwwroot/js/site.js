(() => {
    const storageKey = "perfumier-theme";
    const root = document.documentElement;
    const themeToggle = document.getElementById("theme-toggle");
    const themeToggleIcon = document.getElementById("theme-toggle-icon");

    const applyTheme = (theme) => {
        root.setAttribute("data-theme", theme);
        root.setAttribute("data-bs-theme", theme);

        if (themeToggleIcon) {
            themeToggleIcon.className = theme === "dark" ? "bi bi-sun" : "bi bi-moon-stars";
        }

        if (themeToggle) {
            themeToggle.setAttribute("aria-label", theme === "dark" ? "Switch to light mode" : "Switch to dark mode");
            themeToggle.setAttribute("title", theme === "dark" ? "Light mode" : "Dark mode");
        }
    };

    applyTheme(root.getAttribute("data-theme") || "light");

    themeToggle?.addEventListener("click", () => {
        const nextTheme = root.getAttribute("data-theme") === "dark" ? "light" : "dark";
        localStorage.setItem(storageKey, nextTheme);
        applyTheme(nextTheme);
    });

    const subtotalNode = document.getElementById("checkout-subtotal");
    const deliveryNode = document.getElementById("checkout-delivery");
    const processingNode = document.getElementById("checkout-processing");
    const totalNode = document.getElementById("checkout-total");
    const paymentPanel = document.getElementById("manual-payment-panel");
    const receiptWrapper = document.getElementById("payment-receipt-wrapper");
    const receiptInput = document.getElementById("payment-receipt");

    const currencyFormatter = new Intl.NumberFormat("en-US", {
        style: "currency",
        currency: "USD"
    });

    const getSelected = (selector) => document.querySelector(`${selector}:checked`);
    const getSelectedFee = (selector) => {
        const selected = getSelected(selector);
        const fee = selected?.getAttribute("data-fee");
        return fee ? Number.parseFloat(fee) : 0;
    };

    const setFieldValue = (id, value) => {
        const field = document.getElementById(id);
        if (field) {
            field.value = value || "";
        }
    };

    const updateTotals = () => {
        if (!subtotalNode || !deliveryNode || !processingNode || !totalNode) {
            return;
        }

        const subtotal = Number.parseFloat(subtotalNode.getAttribute("data-amount") || "0");
        const deliveryFee = getSelectedFee(".js-delivery-option");
        const processingFee = getSelectedFee(".js-payment-option");
        const total = subtotal + deliveryFee + processingFee;

        deliveryNode.textContent = currencyFormatter.format(deliveryFee);
        processingNode.textContent = currencyFormatter.format(processingFee);
        totalNode.textContent = currencyFormatter.format(total);
    };

    const updatePaymentPanel = () => {
        if (!paymentPanel) {
            return;
        }

        const selected = getSelected(".js-payment-option");
        const paymentType = selected?.getAttribute("data-type") || "";
        const requiresReceipt = selected?.getAttribute("data-requires-receipt") === "true";
        const shouldShowPanel = paymentType === "manual" || paymentType === "partner_bank";

        paymentPanel.classList.toggle("d-none", !shouldShowPanel);
        if (!shouldShowPanel) {
            if (receiptInput) {
                receiptInput.required = false;
            }
            return;
        }

        setFieldValue("manual-bank-name", selected?.getAttribute("data-bank-name"));
        setFieldValue("manual-account-title", selected?.getAttribute("data-account-title"));
        setFieldValue("manual-account-number", selected?.getAttribute("data-account-number"));
        setFieldValue("manual-iban", selected?.getAttribute("data-iban"));
        setFieldValue("manual-instructions", selected?.getAttribute("data-instructions") || selected?.getAttribute("data-partner-name"));

        if (receiptWrapper) {
            receiptWrapper.classList.toggle("d-none", !requiresReceipt);
        }

        if (receiptInput) {
            receiptInput.required = requiresReceipt;
        }
    };

    document.querySelectorAll(".js-delivery-option, .js-payment-option").forEach((input) => {
        input.addEventListener("change", () => {
            updateTotals();
            updatePaymentPanel();
        });
    });

    updateTotals();
    updatePaymentPanel();
})();
