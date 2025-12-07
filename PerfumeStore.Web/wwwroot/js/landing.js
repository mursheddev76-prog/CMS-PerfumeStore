(() => {
    const buttons = document.querySelectorAll("[data-role='quick-view']");
    buttons.forEach((button) => {
        button.addEventListener("click", async () => {
            const productId = button.getAttribute("data-product-id");
            const res = await fetch(`/api/products?q=${productId}`);
            if (!res.ok) {
                console.warn("Unable to fetch product", productId);
                return;
            }

            const [product] = await res.json();
            if (!product) {
                alert("Product not found anymore. Refreshing catalog.");
                window.location.reload();
                return;
            }

            const modal = document.createElement("div");
            modal.className = "quick-view-backdrop";
            modal.innerHTML = `
                <div class="quick-view-modal">
                    <button class="quick-view-close">&times;</button>
                    <img src="${product.imageUrl}" alt="${product.name}" />
                    <h3>${product.name}</h3>
                    <p>${product.description}</p>
                    <strong>${(product.discountPrice ?? product.price).toLocaleString("en-US", { style: "currency", currency: "USD" })}</strong>
                    <a class="btn btn-dark w-100 mt-3" href="/checkout">Add to Ritual</a>
                </div>`;

            modal.querySelector(".quick-view-close")?.addEventListener("click", () => modal.remove());
            modal.addEventListener("click", (e) => {
                if (e.target === modal) modal.remove();
            });

            document.body.appendChild(modal);
        });
    });
})();

