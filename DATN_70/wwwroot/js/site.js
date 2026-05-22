    (function () {
        const cartKey = "winterStoreCartV3";
        const profileKey = "winterStoreProfileV3";
        const storeSession = window.storeSession || { isAuthenticated: false, userId: "" };
        let cartItemsCache = [];

        // Giữ nguyên các map data phụ trợ cũ
        const stylePhotos = { /* ... */ };
        const productPhotoMap = { /* ... */ };
        const metaMap = { /* ... */ };
        const categories = [ /* ... */];

        const escapeHtml = value => String(value ?? "").replace(/&/g, "&amp;").replace(/</g, "&lt;").replace(/>/g, "&gt;").replace(/\"/g, "&quot;").replace(/'/g, "&#39;");
        const formatCurrency = value => Number(value || 0).toLocaleString("vi-VN") + " đ";
        const clamp = (value, min, max) => Math.min(Math.max(value, min), max);

        // Fallback Meta
        const getMeta = productOrId => metaMap[typeof productOrId === "string" ? productOrId : productOrId?.sanPhamID] || { collection: "Winter Capsule", parentCategory: "outerwear", parentLabel: "Áo khoác ngoài", category: "ao-phao", categoryLabel: "Áo phao", badge: "Hot", tagline: "Thiết kế tối giản cho mùa lạnh", toneA: "#ddd8cd", toneB: "#887a69", style: "jacket", popularity: 80, originalFactor: 1.12, icon: "bi-box-seam" };

        const isValidPhoneNumber = phone => /^(0|\+84)\d{9,10}$/.test((phone || "").replace(/\s+/g, ""));
        const isAuthenticated = () => !!storeSession.isAuthenticated && !!storeSession.userId;
        const getStorageKey = baseKey => `${baseKey}:${isAuthenticated() ? storeSession.userId : "guest"}`;
        const getEmptyCartState = () => ({ items: [], note: "", couponCode: "", shippingCode: "vnpost", paymentCode: "cod" });

        const redirectToLogin = (message = "Vui lòng đăng nhập để tiếp tục.") => {
            showToast(message, "warning");
            window.setTimeout(() => { window.location.href = "/Account/Login"; }, 250);
        };

        const getCartPreferences = () => {
            if (!isAuthenticated()) return getEmptyCartState();
            try {
                const parsed = JSON.parse(localStorage.getItem(getStorageKey(cartKey)) || "{}");
                return { note: parsed.note || "", couponCode: parsed.couponCode || "", shippingCode: parsed.shippingCode || "vnpost", paymentCode: parsed.paymentCode || "cod" };
            } catch { return getEmptyCartState(); }
        };

        const getCartState = () => ({ ...getCartPreferences(), items: [...cartItemsCache] });

        const emitCartChanged = () => { document.dispatchEvent(new CustomEvent("winterCartChanged", { detail: getCartState() })); };

        const saveCartState = state => {
            if (!isAuthenticated()) return;
            const nextState = state || getEmptyCartState();
            if (Array.isArray(nextState.items)) cartItemsCache = nextState.items.map(item => ({ ...item }));
            localStorage.setItem(getStorageKey(cartKey), JSON.stringify({
                note: nextState.note || "", couponCode: nextState.couponCode || "", shippingCode: nextState.shippingCode || "vnpost", paymentCode: nextState.paymentCode || "cod"
            }));
            emitCartChanged();
        };

        const getProfile = () => {
            if (!isAuthenticated()) return {};
            try { return JSON.parse(localStorage.getItem(getStorageKey(profileKey)) || "{}"); } catch { return {}; }
        };

        const saveProfile = profile => {
            if (!isAuthenticated()) return;
            localStorage.setItem(getStorageKey(profileKey), JSON.stringify(profile));
        };

        const getTotalQuantity = (state = getCartState()) => state.items.reduce((sum, item) => sum + Number(item.soLuong || 0), 0);
        const getSubtotal = (state = getCartState()) => state.items.reduce((sum, item) => sum + Number(item.donGia || 0) * Number(item.soLuong || 0), 0);

        // API & Fetch Logic
        async function fetchJson(url) {
            const response = await fetch(url);
            if (!response.ok) throw new Error(`Request failed: ${response.status}`);
            return response.json();
        }

        async function requestJson(url, options) {
            const response = await fetch(url, options);
            const contentType = response.headers.get("content-type") || "";
            const payload = contentType.includes("application/json") ? await response.json() : null;
            if (!response.ok) {
                const error = new Error(payload?.message || `Request failed: ${response.status}`);
                error.status = response.status;
                throw error;
            }
            return payload;
        }

        // Hàm bổ trợ check ảnh an toàn chống lỗi 404 truyền chuỗi "undefined"
        function getSafeProductImgUrl(item) {
            const colorId = item.mauID || item.mauId || "";
            if (item.sanPhamID && colorId && colorId !== "undefined") {
                return `/images/products/${item.sanPhamID}-${colorId}.png`;
            }
            return item.hinhAnhUrl || item.imageUrl || '/images/default-product.png';
        }

        function syncCartItems(items) {
            cartItemsCache = Array.isArray(items) ? items.map(item => ({ ...item })) : [];
            emitCartChanged();
            return getCartState();
        }

        async function refreshCartFromServer() {
            if (!isAuthenticated()) { cartItemsCache = []; return getCartState(); }
            try {
                const response = await requestJson("/api/cart");
                return syncCartItems(response?.items || []);
            } catch (error) {
                if (error.status === 401) { redirectToLogin(); return getEmptyCartState(); }
                showToast(error.message || "Không thể tải giỏ hàng.", "danger");
                return getCartState();
            }
        }

        async function addCartItemToServer(chiTietSanPhamID, soLuong) {
            const response = await requestJson("/api/cart/items", { method: "POST", headers: { "Content-Type": "application/json" }, body: JSON.stringify({ chiTietSanPhamID, soLuong }) });
            return syncCartItems(response?.items || []);
        }

        async function updateCartItemOnServer(chiTietSanPhamID, soLuong) {
            const response = await requestJson(`/api/cart/items/${encodeURIComponent(chiTietSanPhamID)}`, { method: "PUT", headers: { "Content-Type": "application/json" }, body: JSON.stringify({ soLuong }) });
            return syncCartItems(response?.items || []);
        }

        async function removeCartItemFromServer(chiTietSanPhamID) {
            const response = await requestJson(`/api/cart/items/${encodeURIComponent(chiTietSanPhamID)}`, { method: "DELETE" });
            return syncCartItems(response?.items || []);
        }

        async function clearCartOnServer() {
            const response = await requestJson("/api/cart", { method: "DELETE" });
            return syncCartItems(response?.items || []);
        }

        async function fetchProducts() {
            const products = await fetchJson("/api/products");
            return Array.isArray(products) ? products.map(p => ({ ...p, meta: getMeta(p), giaGoc: Number(p.giaGoc || p.giaThapNhat || 0), phanTramGiam: Number(p.phanTramGiam || 0) })) : [];
        }

        function showToast(message, type = "info") {
            const container = document.getElementById("toastContainer");
            if (!container || !window.bootstrap) return;
            const bgClass = { success: "text-bg-success", danger: "text-bg-danger", warning: "text-bg-warning", info: "text-bg-dark" }[type] || "text-bg-dark";
            const toastElement = document.createElement("div");
            toastElement.className = `toast align-items-center border-0 ${bgClass}`;
            toastElement.innerHTML = `<div class="d-flex"><div class="toast-body">${escapeHtml(message)}</div><button type="button" class="btn-close btn-close-white me-2 m-auto" data-bs-dismiss="toast" aria-label="Close"></button></div>`;
            container.appendChild(toastElement);
            const toast = new window.bootstrap.Toast(toastElement, { delay: 2600 });
            toastElement.addEventListener("hidden.bs.toast", () => toastElement.remove());
            toast.show();
        }

        // 🔥 FIX 1: Đã dọn sạch lỗi cú pháp lặp thẻ img rác ở ô xem trước Header
        function updateCartPreview(state = getCartState()) {
            const badge = document.getElementById("cartBadge");
            const count = document.getElementById("cartPreviewCount");
            const itemsNode = document.getElementById("cartPreviewItems");
            if (!isAuthenticated()) {
                if (badge) badge.textContent = "0";
                if (count) count.textContent = "Đăng nhập để xem";
                if (itemsNode) itemsNode.innerHTML = '<div class="cart-preview-empty">Đăng nhập để lưu và thanh toán giỏ hàng của riêng bạn.</div>';
                return;
            }
            if (badge) badge.textContent = getTotalQuantity(state);
            if (count) count.textContent = `${getTotalQuantity(state)} sản phẩm`;
            if (!itemsNode) return;

            itemsNode.innerHTML = state.items.length ? state.items.slice(0, 4).map(item => `
                <article class="cart-preview-item d-flex align-items-center gap-3">
                    <div class="thumb-wrap" style="width: 50px; height: 50px; flex-shrink: 0;">
                        <img src="${getSafeProductImgUrl(item)}" onerror="this.src='/images/default-product.png'" alt="${escapeHtml(item.tenSanPham)}" style="width: 100%; height: 100%; object-fit: cover; border-radius: 8px;" />
                    </div>
                    <div style="flex: 1;">
                        <strong style="font-size: 0.9rem;">${escapeHtml(item.tenSanPham)}</strong>
                        <div class="item-meta text-muted" style="font-size: 0.75rem;">${escapeHtml(item.phanLoai)} · SL ${item.soLuong}</div>
                        <div class="price-sale text-danger fw-bold" style="font-size: 0.85rem;">${formatCurrency(item.donGia * item.soLuong)}</div>
                    </div>
                </article>
            `).join("") : '<div class="cart-preview-empty text-center text-muted py-3">Giỏ hàng đang trống.</div>';
        }

        function setSearchResults(products, keyword) {
            const node = document.getElementById("headerSearchResults");
            if (!node) return;
            const term = String(keyword || "").trim().toLowerCase();
            if (!term) {
                node.innerHTML = '<div class="header-search-empty">Nhập tên sản phẩm để tìm.</div>';
                return;
            }
            const matches = products.filter(product => product.ten.toLowerCase().includes(term) || product.meta.categoryLabel.toLowerCase().includes(term)).slice(0, 5);

            node.innerHTML = matches.length ? matches.map(product => `
                <a class="header-search-item" href="/Home/Details?id=${encodeURIComponent(product.sanPhamID)}">
                    <div class="thumb-wrap" style="width: 50px; height: 50px; flex-shrink: 0;">
                        <img src="${product.hinhAnhUrl || product.imageUrl || '/images/default-product.png'}" alt="${escapeHtml(product.ten)}" style="width: 100%; height: 100%; object-fit: cover; border-radius: 8px;" />
                    </div>
                    <div class="item-info">
                        <strong>${escapeHtml(product.ten)}</strong>
                        <div class="item-meta">${escapeHtml(product.meta.parentLabel)}</div>
                        <div class="price-sale text-danger fw-bold">${formatCurrency(product.giaThapNhat)}</div>
                    </div>
                </a>
            `).join("") : '<div class="header-search-empty">Không tìm thấy sản phẩm phù hợp.</div>';
        }

        async function addItemToCart(product, variant, quantity) {
            if (!isAuthenticated()) { redirectToLogin("Vui lòng đăng nhập trước khi thêm sản phẩm vào giỏ hàng."); return false; }
            const safeQuantity = clamp(Number.parseInt(quantity, 10) || 1, 1, Math.min(20, Number(variant.soLuongTon || 1)));
            try {
                await addCartItemToServer(variant.chiTietSanPhamID, safeQuantity);
                return true;
            } catch (error) { showToast(error.message || "Không thể thêm sản phẩm vào giỏ hàng.", "danger"); return false; }
        }

        function initHeader(productsPromise) {
            document.querySelectorAll(".cart-trigger, #cartCheckoutLink, .cart-preview-actions a[href='/Home/Cart'], .cart-preview-actions a[href='/Home/Checkout']").forEach(link => {
                link.addEventListener("click", event => {
                    if (isAuthenticated()) return;
                    event.preventDefault();
                    redirectToLogin("Vui lòng đăng nhập để xem giỏ hàng và thanh toán.");
                });
            });

            const wrapper = document.querySelector(".header-search");
            const toggle = document.getElementById("headerSearchToggle");
            const input = document.getElementById("headerSearchInput");
            if (!wrapper || !toggle || !input) return;
            let searchCloseTimer = null;
            wrapper.addEventListener("mouseenter", () => { if (searchCloseTimer) window.clearTimeout(searchCloseTimer); wrapper.classList.add("is-open"); });
            wrapper.addEventListener("mouseleave", () => { if (searchCloseTimer) window.clearTimeout(searchCloseTimer); searchCloseTimer = window.setTimeout(() => wrapper.classList.remove("is-open"), 140); });
            toggle.addEventListener("click", () => { wrapper.classList.toggle("is-open"); if (wrapper.classList.contains("is-open")) input.focus(); });
            document.addEventListener("click", event => { if (!wrapper.contains(event.target)) wrapper.classList.remove("is-open"); });
            input.addEventListener("input", async () => setSearchResults(await productsPromise, input.value));
        }

        async function initCart() {
            const page = document.getElementById("cartPage");
            if (!page) return;
            if (!isAuthenticated()) { redirectToLogin("Vui lòng đăng nhập để xem giỏ hàng."); return; }

            await refreshCartFromServer();
            const container = document.getElementById("cartItemsContainer");
            const noteInput = document.getElementById("cartOrderNote");
            const checkoutLink = document.getElementById("cartCheckoutLink");

            const render = () => {
                const state = getCartState();
                document.getElementById("cartHeadCount").textContent = getTotalQuantity(state);

                const subtotal = getSubtotal(state);
                document.getElementById("cartSubtotalAmount").textContent = formatCurrency(subtotal);
                document.getElementById("cartTotalAmount").textContent = formatCurrency(subtotal);

                if (noteInput) noteInput.value = state.note;
                checkoutLink.style.pointerEvents = state.items.length ? "auto" : "none";
                checkoutLink.style.opacity = state.items.length ? "1" : "0.6";

                container.innerHTML = state.items.length ? state.items.map(item => `
                    <article class="cart-item d-flex align-items-center gap-3 mb-3 border-bottom pb-3">
                        <div class="thumb-wrap" style="width: 80px; height: 80px; flex-shrink: 0;">
                            <img src="${getSafeProductImgUrl(item)}" onerror="this.src='/images/default-product.png'" alt="${escapeHtml(item.tenSanPham)}" style="width: 100%; height: 100%; object-fit: cover; border-radius: 8px;" />
                        </div>
                        <div style="flex: 1;">
                            <a class="product-name text-dark fw-bold text-decoration-none" href="/Home/Details?id=${encodeURIComponent(item.sanPhamID)}">${escapeHtml(item.tenSanPham)}</a>
                            <div class="item-meta text-muted small mb-2">${escapeHtml(item.phanLoai)}</div>
                            <div class="cart-qty-actions d-flex gap-3 align-items-center">
                                <input class="form-control form-control-sm text-center cart-qty-input" style="width: 70px;" type="number" min="0" max="${Math.min(20, item.tonKho || 20)}" value="${item.soLuong}" data-id="${item.chiTietSanPhamID}" />
                                <button type="button" class="btn btn-sm btn-outline-danger cart-remove-btn" data-id="${item.chiTietSanPhamID}">Xóa</button>
                            </div>
                        </div>
                        <strong class="price-sale fs-5 text-danger">${formatCurrency(item.donGia * item.soLuong)}</strong>
                    </article>
                `).join("") : '<div class="empty-state text-center py-5">Giỏ hàng đang trống.</div>';
            };

            container.addEventListener("change", async event => {
                const input = event.target.closest(".cart-qty-input");
                if (!input) return;
                const quantity = Number(input.value);
                if (quantity <= 0) {
                    if (window.confirm("Bạn có muốn xóa sản phẩm này?")) {
                        await removeCartItemFromServer(input.dataset.id); render();
                    } else { render(); }
                    return;
                }
                const item = getCartState().items.find(entry => entry.chiTietSanPhamID === input.dataset.id);
                if (item) { await updateCartItemOnServer(input.dataset.id, clamp(quantity, 1, Math.min(20, item.tonKho || 20))); render(); }
            });

            container.addEventListener("click", async event => {
                const button = event.target.closest(".cart-remove-btn");
                if (button) { await removeCartItemFromServer(button.dataset.id); render(); }
            });

            document.addEventListener("winterCartChanged", render);
            render();
        }

        async function initCheckout() {
            return;
            const page = document.getElementById("checkoutPage");
            if (!page) return;
            if (!isAuthenticated()) { redirectToLogin("Vui lòng đăng nhập để thanh toán."); return; }

            await refreshCartFromServer();
            if (!getCartState().items.length) { window.location.href = "/Home/Cart"; return; }

            const renderCheckoutItems = () => {
                const cart = getCartState();
                document.getElementById("checkoutItems").innerHTML = cart.items.map(item => `
                    <article class="checkout-item d-flex align-items-center gap-3">
                        <div class="thumb-wrap" style="width: 60px; height: 60px; flex-shrink: 0;">
                            <img src="${getSafeProductImgUrl(item)}" onerror="this.src='/images/default-product.png'" alt="${escapeHtml(item.tenSanPham)}" style="width: 100%; height: 100%; object-fit: cover; border-radius: 8px;" />
                        </div>
                        <div style="flex: 1;">
                            <div class="product-name fw-bold text-dark" style="font-size: 0.9rem;">${escapeHtml(item.tenSanPham)}</div>
                            <div class="item-meta text-muted" style="font-size: 0.8rem;">${escapeHtml(item.phanLoai)} · SL: ${item.soLuong}</div>
                        </div>
                        <strong class="price-sale text-danger">${formatCurrency(item.donGia * item.soLuong)}</strong>
                    </article>
                `).join("");

                const subtotalNode = document.getElementById("checkoutSubtotal");
                if (subtotalNode) {
                    subtotalNode.textContent = formatCurrency(getSubtotal(cart));
                    const pSelect = document.getElementById("checkoutProvince");
                    if (pSelect && pSelect.value) pSelect.dispatchEvent(new Event("change"));
                }
            };

            renderCheckoutItems();
            document.addEventListener("winterCartChanged", renderCheckoutItems);

        //    document.getElementById("placeOrderButton").addEventListener("click", async () => {
        //        const fullName = document.getElementById("checkoutFullName");
        //        const phone = document.getElementById("checkoutPhone");
        //        const street = document.getElementById("checkoutStreet");
        //        const email = document.getElementById("checkoutEmail");
        //        const province = document.getElementById("checkoutProvince");
        //        const district = document.getElementById("checkoutDistrict");
        //        const ward = document.getElementById("checkoutWard");

        //        [fullName, phone, street].forEach(input => input.classList.toggle("is-invalid", !input.value.trim()));
        //        phone.classList.toggle("is-invalid", !isValidPhoneNumber(phone.value));

        //        if (!fullName.value.trim() || !street.value.trim() || !isValidPhoneNumber(phone.value)) {
        //            return showToast("Vui lòng nhập đầy đủ Tên, Số điện thoại và Địa chỉ chi tiết.", "warning");
        //        }
        //        [province, district, ward].forEach(input => input.classList.toggle("is-invalid", !input.value));
        //        if (!province.value || !district.value || !ward.value) {
        //            return showToast("Lỗi: Vui lòng chọn đầy đủ Tỉnh/Thành, Quận/Huyện và Phường/Xã để tính phí vận chuyển!", "danger");
        //        }

        //        const currentSelectedAddressId = document.getElementById("selectedAddressId")?.value || "";
        //        const currentCustomerId = document.getElementById("realKhachHangId")?.value || window.storeSession?.userId || "";

        //        if (!currentCustomerId) return showToast("Lỗi phiên đăng nhập.", "danger");

        //        const pOption = province.options[province.selectedIndex];
        //        const dOption = district.options[district.selectedIndex];
        //        const pName = pOption ? pOption.text : province.value;
        //        const dName = dOption ? dOption.text : district.value;

        //        const paymentMethodInput = document.querySelector("input[name='paymentMethod']:checked");
        //        const couponInput = document.getElementById("checkoutCouponInput");

        //        const requestData = {
        //            khachHangID: currentCustomerId,
        //            diaChiID: currentSelectedAddressId,
        //            tenKhachHang: fullName.value.trim(),
        //            soDienThoai: phone.value.replace(/\s+/g, ""),
        //            diaChiGiaoHang: `${street.value.trim()}, ${ward.value}, ${dName}, ${pName}`,
        //            phuongThucThanhToan: paymentMethodInput ? paymentMethodInput.value : "COD",
        //            maGiamGia: couponInput ? couponInput.value : "",
        //            items: getCartState().items.map(item => ({ chiTietSanPhamID: item.chiTietSanPhamID, soLuong: item.soLuong }))
        //        };

        //        const button = document.getElementById("placeOrderButton");
        //        try {
        //            button.disabled = true;
        //            button.innerHTML = '<span class="spinner-border spinner-border-sm me-2"></span> Đang xử lý...';

        //             Gọi cổng kết nối API Đơn hàng công khai vừa xây dựng ở HomeController
        //             Gọi cổng kết nối API Đơn hàng công khai vừa xây dựng ở HomeController (Đã đổi đường dẫn)
        //            const response = await fetch("/api/web-orders", { method: "POST", headers: { "Content-Type": "application/json" }, body: JSON.stringify(requestData) });
        //            const result = await response.json();

        //            if (!response.ok) return showToast(result.message || "Không thể tạo đơn hàng.", "danger");

        //            if (result.kieuThanhToan === 0) {
        //                showToast(`Đặt hàng thành công! Mã đơn: ${result.hoaDonID}`, "success");
        //                try { await clearCartOnServer(); } catch (e) { console.error(e); }
        //                saveCartState(getEmptyCartState());
        //                setTimeout(() => { window.location.href = "/"; }, 1500);
        //            }
        //            else if (result.kieuThanhToan === 1) {
        //                document.getElementById("webPayOSIframe").src = result.checkoutUrl;
        //                const qrModal = new bootstrap.Modal(document.getElementById('webQrModal'));
        //                qrModal.show();

        //                try { await clearCartOnServer(); } catch (e) { console.error(e); }
        //                saveCartState(getEmptyCartState());

        //                const checkInterval = setInterval(async () => {
        //                    try {
        //                        const checkRes = await fetch(`/api/admin/orders/${result.hoaDonID}`);
        //                        if (checkRes.ok) {
        //                            const orderDetail = await checkRes.json();
        //                            if (orderDetail.trangThaiKey === "confirmed" || orderDetail.trangThaiKey === "success") {
        //                                clearInterval(checkInterval);
        //                                qrModal.hide();
        //                                showToast("Winter Shop đã nhận được tiền! Thanh toán đơn hàng thành công.", "success");
        //                                setTimeout(() => { window.location.href = "/"; }, 2000);
        //                            }
        //                        }
        //                    } catch (err) { console.error("Lỗi dò trạng thái hóa đơn:", err); }
        //                }, 3000);
        //            }
        //        } catch (error) {
        //            console.error(error);
        //            showToast("Không thể kết nối tới máy chủ.", "danger");
        //        } finally {
        //            button.disabled = false;
        //            button.textContent = "Hoàn tất đơn hàng";
        //        }
        //    });
        }

        function getDefaultVariant(product) {
            return product.bienThe.slice().sort((a, b) => a.giaNiemYet - b.giaNiemYet)[0] || null;
        }

        function initDetail(productsPromise) {
            const page = document.getElementById("productDetailPage");
            if (!page) return;

            const productId = page.dataset.productId;
            let product;
            let selectedColor = "";
            let selectedSize = "";
            let activeTab = "description";

            const tabContent = {
                description: current => `<p>${escapeHtml(current.moTa || "Sản phẩm thiết kế độc quyền với chất liệu cao cấp, giữ ấm hoàn hảo cho mùa đông.")}</p>`,
                returns: () => `<ul><li>Đổi trả miễn phí trong 7 ngày nếu lỗi nhà sản xuất.</li><li>Sản phẩm đổi trả phải nguyên tem mác, chưa qua sử dụng.</li></ul>`,
                warranty: () => `<ul><li>Winter Shop cam kết bảo mật 100% thông tin mua sắm của bạn.</li></ul>`
            };

            const getDetailQuantity = () => Number(document.getElementById("detailQuantityValue")?.textContent || 1);
            const setDetailQuantity = nextValue => {
                const node = document.getElementById("detailQuantityValue");
                if (node) node.textContent = String(nextValue);
            };
            const getSelected = () => product.bienThe.find(item => item.tenMau === selectedColor && item.sizeLabel === selectedSize) || null;

            const renderTabs = () => {
                page.querySelectorAll(".detail-tab").forEach(button => button.classList.remove("active", "text-white", "bg-dark"));
                page.querySelectorAll(".detail-tab").forEach(button => {
                    if (button.dataset.tab === activeTab) button.classList.add("active", "text-white", "bg-dark");
                });
                document.getElementById("detailTabContent").innerHTML = tabContent[activeTab](product);
            };

            const renderGallery = () => {
                if (!product || !product.bienThe) return;
                const uniqueByColor = [...new Map(product.bienThe.map(item => [item.mauID || item.mauId, item])).values()];

                document.getElementById("detailGalleryThumbs").innerHTML = uniqueByColor.map(item => {
                    const mID = item.mauID || item.mauId;
                    const thumbImgUrl = `/images/products/${product.sanPhamID}-${mID}.png`;
                    const isActiveClass = selectedColor === item.tenMau ? "border-dark border-2 shadow-sm" : "opacity-60";

                    return `
                        <button type="button" class="btn p-0 border rounded-2 overflow-hidden ${isActiveClass}" data-color="${escapeHtml(item.tenMau)}" style="width: 55px; height: 55px; transition: all 0.15s;">
                            <img src="${thumbImgUrl}" onerror="this.src='/images/default-product.png'" class="w-100 h-100 object-fit-cover" />
                        </button>
                    `;
                }).join("");
            };

            const renderVariant = () => {
                const variant = getSelected();
                const colors = [...new Set(product.bienThe.map(item => item.tenMau))];
                const sizes = [...new Set(product.bienThe.filter(item => item.tenMau === selectedColor).map(item => item.sizeLabel))];

                document.getElementById("detailColorOptions").innerHTML = colors.map(color => `<button type="button" class="option-button ${selectedColor === color ? "active" : ""}" data-color="${escapeHtml(color)}">${escapeHtml(color)}</button>`).join("");
                document.getElementById("detailSizeOptions").innerHTML = sizes.map(size => `<button type="button" class="option-button ${selectedSize === size ? "active" : ""}" data-size="${size}">${escapeHtml(size)}</button>`).join("");

                const add = document.getElementById("detailAddToCart");
                const buy = document.getElementById("detailBuyNow");

                if (!variant) {
                    document.getElementById("detailPrice").textContent = "Liên hệ";
                    document.getElementById("detailStockNote").innerHTML = '<span class="text-danger"><i class="bi bi-x-circle me-1"></i>Hết hàng</span>';
                    add.disabled = true; buy.disabled = true;
                    return;
                }

                document.getElementById("detailPrice").textContent = formatCurrency(variant.giaNiemYet);
                document.getElementById("detailOriginalPrice").textContent = Number(variant.phanTramGiam || 0) > 0 ? formatCurrency(variant.giaGoc) : "";
                document.getElementById("detailDiscountBadge").textContent = Number(variant.phanTramGiam || 0) > 0 ? `-${variant.phanTramGiam}%` : "";
                document.getElementById("detailVariantSummary").textContent = `Đã chọn ${variant.tenMau} / ${variant.sizeLabel}`;
                document.getElementById("detailStockNote").innerHTML = `<span class="text-success"><i class="bi bi-check-circle me-1"></i>Còn ${variant.soLuongTon} sản phẩm</span>`;

                setDetailQuantity(clamp(getDetailQuantity(), 1, Math.max(1, Math.min(20, variant.soLuongTon))));
                add.disabled = variant.soLuongTon < 1;
                buy.disabled = variant.soLuongTon < 1;

                const mID = variant.mauID || variant.mauId;
                const imgUrl = `/images/products/${product.sanPhamID}-${mID}.png`;
                document.getElementById("detailArtwork").innerHTML = `<img src="${imgUrl}" onerror="this.src='/images/default-product.png'" class="img-fluid w-100 h-100 object-fit-cover shadow-sm animate-fade" alt="Ảnh sản phẩm">`;

                renderGallery();
            };

            fetch(`/api/products/${encodeURIComponent(productId)}`)
                .then(res => res.json())
                .then(detail => {
                    product = detail;
                    product.bienThe = Array.isArray(product.bienThe) ? product.bienThe.map(item => ({ ...item, sizeLabel: String(item.tenKichCo || "").replace(/^Size\s*/i, "").trim() })) : [];

                    if (product.bienThe.length > 0) {
                        product.giaThapNhat = Math.min(...product.bienThe.map(item => Number(item.giaNiemYet || 0)));
                        product.giaGoc = Math.min(...product.bienThe.map(item => Number(item.giaGoc || item.giaNiemYet || 0)));
                        product.phanTramGiam = Math.max(...product.bienThe.map(item => Number(item.phanTramGiam || 0)));
                    }

                    const variant = getDefaultVariant(product) || product.bienThe[0];
                    if (!variant) {
                        document.getElementById("detailArtwork").innerHTML = '<div class="text-danger p-3">Sản phẩm chưa có biến thể để bán.</div>';
                        return;
                    }

                    selectedColor = variant.tenMau;
                    selectedSize = variant.sizeLabel;
                    document.getElementById("detailTitle").textContent = product.ten;

                    renderVariant();
                    renderTabs();

                    if (productsPromise) {
                        productsPromise.then(allProducts => {
                            const related = allProducts.filter(p =>
                                p.sanPhamID !== product.sanPhamID &&
                                (p.danhMuc === product.danhMuc || p.meta?.parentCategory === product.meta?.parentCategory)
                            ).slice(0, 4);

                            const relatedContainer = document.getElementById("relatedProducts");
                            if (related.length > 0) {
                                relatedContainer.innerHTML = related.map(p => `
                                    <div class="col">
                                        <div class="card h-100 border-0 shadow-sm rounded-4 overflow-hidden position-relative product-card-hover" style="transition: transform 0.25s ease;">
                                            <span class="position-absolute top-0 start-0 badge bg-dark m-3 rounded-pill px-3 py-2 fw-bold text-uppercase" style="z-index: 2; font-size: 0.7rem;">${escapeHtml(p.danhMuc || p.meta?.categoryLabel || 'Hot')}</span>
                                            <a href="/Home/Details?id=${encodeURIComponent(p.sanPhamID)}" class="text-decoration-none text-dark d-block">
                                                <div class="ratio ratio-1x1 bg-light">
                                                    <img src="${p.hinhAnhUrl || p.imageUrl || '/images/default-product.png'}" class="img-fluid object-fit-cover w-100 h-100" loading="lazy" />
                                                </div>
                                                <div class="card-body p-3 d-flex flex-column justify-content-between" style="min-height: 120px;">
                                                    <h5 class="card-title fw-bold fs-6 text-truncate mb-1" title="${escapeHtml(p.ten || p.tenSanPham)}">${escapeHtml(p.ten || p.tenSanPham)}</h5>
                                                    <div class="d-flex align-items-center gap-2 mt-2">
                                                        <span class="text-danger fw-extrabold fs-5">${formatCurrency(p.giaThapNhat)}</span>
                                                    </div>
                                                </div>
                                            </a>
                                        </div>
                                    </div>
                                `).join("");
                            } else {
                                relatedContainer.innerHTML = '<div class="col-12 text-center text-muted py-4 bg-white rounded-4 border">Không có sản phẩm cùng loại nào.</div>';
                            }
                        });
                    }
                })
                .catch(e => {
                    console.error(e);
                    document.getElementById("detailArtwork").innerHTML = '<div class="text-danger p-3">Lỗi Hệ Thống! Không tìm thấy API sản phẩm.</div>';
                });

            document.getElementById("detailColorOptions").addEventListener("click", event => {
                const btn = event.target.closest("[data-color]");
                if (!btn || !product) return;
                selectedColor = btn.dataset.color;
                const sizes = [...new Set(product.bienThe.filter(item => item.tenMau === selectedColor).map(item => item.sizeLabel))];
                if (!sizes.includes(selectedSize)) selectedSize = sizes[0];
                renderVariant();
            });

            document.getElementById("detailSizeOptions").addEventListener("click", event => {
                const btn = event.target.closest("[data-size]");
                if (!btn) return;
                selectedSize = btn.dataset.size;
                renderVariant();
            });

            document.getElementById("detailGalleryThumbs").addEventListener("click", event => {
                const btn = event.target.closest("[data-color]");
                if (!btn || !product) return;
                selectedColor = btn.dataset.color;
                const sizes = [...new Set(product.bienThe.filter(item => item.tenMau === selectedColor).map(item => item.sizeLabel))];
                if (!sizes.includes(selectedSize)) selectedSize = sizes[0];
                renderVariant();
            });

            page.querySelector(".detail-tabs").addEventListener("click", event => {
                const btn = event.target.closest("[data-tab]");
                if (!btn) return;
                activeTab = btn.dataset.tab;
                renderTabs();
            });

            document.getElementById("detailDecreaseQty").addEventListener("click", () => setDetailQuantity(clamp(getDetailQuantity() - 1, 1, 20)));
            document.getElementById("detailIncreaseQty").addEventListener("click", () => {
                const variant = getSelected();
                setDetailQuantity(clamp(getDetailQuantity() + 1, 1, Math.min(20, variant?.soLuongTon || 20)));
            });

            document.getElementById("detailAddToCart").addEventListener("click", async () => {
                const variant = getSelected();
                if (!variant) return;
                if (!await window.winterCart.addItemToCart(product, variant, getDetailQuantity())) return;
                window.winterCart.showToast("Đã thêm sản phẩm vào giỏ hàng thành công.", "success");
                window.location.reload();
            });

            document.getElementById("detailBuyNow").addEventListener("click", async () => {
                const variant = getSelected();
                if (!variant) return;
                if (!await window.winterCart.addItemToCart(product, variant, getDetailQuantity())) return;
                window.location.href = "/Home/Cart";
            });
        }
        const productsPromise = fetchProducts().catch(() => []);

        initHeader(productsPromise);
        initCart();
        initCheckout();
        initDetail(productsPromise);
        updateCartPreview();

        if (isAuthenticated()) { refreshCartFromServer().catch(e => console.error(e)); }
        document.addEventListener("winterCartChanged", event => updateCartPreview(event.detail));

        // Expose APIs
        window.winterCart = { getCartState, getTotalQuantity, addItemToCart, showToast, clearCartOnServer, saveCartState };
    })();