(() => {
    const buttons = document.querySelectorAll("[data-role='quick-view']");
    const returnUrl = window.perfumierReturnUrl || window.location.pathname + window.location.search;
    buttons.forEach((button) => {
        button.addEventListener("click", async () => {
            const productId = button.getAttribute("data-product-id");
            const res = await fetch(`/api/products?id=${productId}`);
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
                    <form action="/checkout/cart/add" method="post" class="mt-3">
                        <input type="hidden" name="productId" value="${product.id}" />
                        <input type="hidden" name="quantity" value="1" />
                        <input type="hidden" name="returnUrl" value="${returnUrl}" />
                        <button class="btn btn-dark w-100" type="submit">Add to Cart</button>
                    </form>
                </div>`;

            modal.querySelector(".quick-view-close")?.addEventListener("click", () => modal.remove());
            modal.addEventListener("click", (e) => {
                if (e.target === modal) modal.remove();
            });

            document.body.appendChild(modal);
        });
    });
})();
