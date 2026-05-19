(function () {
    const cartKey = "winterStoreCartV3";
    const profileKey = "winterStoreProfileV3";
    const storeSession = window.storeSession || { isAuthenticated: false, userId: "" };
    let cartItemsCache = [];
<<<<<<< HEAD

    // Giữ nguyên các map data phụ trợ cũ
    const stylePhotos = { /* ... */ };
    const productPhotoMap = { /* ... */ };
    const metaMap = { /* ... */ };
    const categories = [ /* ... */];
=======
    const stylePhotos = {
        parka: { url: "https://images.pexels.com/photos/15246022/pexels-photo-15246022.jpeg?auto=compress&cs=tinysrgb&w=1200", position: "center 14%" },
        jacket: { url: "https://images.pexels.com/photos/35295619/pexels-photo-35295619.jpeg?auto=compress&cs=tinysrgb&w=1200", position: "center 12%" },
        coat: { url: "https://images.pexels.com/photos/36211191/pexels-photo-36211191.jpeg?auto=compress&cs=tinysrgb&w=1200", position: "center 10%" },
        hoodie: { url: "https://images.pexels.com/photos/8346216/pexels-photo-8346216.jpeg?auto=compress&cs=tinysrgb&w=1200", position: "center 10%" },
        sweater: { url: "https://images.pexels.com/photos/29491953/pexels-photo-29491953.jpeg?auto=compress&cs=tinysrgb&w=1200", position: "center 10%" },
        vest: { url: "https://images.pexels.com/photos/5303811/pexels-photo-5303811.jpeg?auto=compress&cs=tinysrgb&w=1200", position: "center 12%" }
    };
    const productPhotoMap = {
        SP0001: {
            default: { url: "https://images.pexels.com/photos/6311613/pexels-photo-6311613.jpeg?auto=compress&cs=tinysrgb&w=1200", position: "center 20%" },
            variants: {
                BLK: { url: "https://images.pexels.com/photos/6311613/pexels-photo-6311613.jpeg?auto=compress&cs=tinysrgb&w=1200", position: "center 20%" },
                CRM: { url: "https://images.pexels.com/photos/7691228/pexels-photo-7691228.jpeg?auto=compress&cs=tinysrgb&w=1200", position: "center 24%" },
                NVY: { url: "https://images.pexels.com/photos/6311392/pexels-photo-6311392.jpeg?auto=compress&cs=tinysrgb&w=1200", position: "center 18%" }
            }
        }
    };
    const metaMap = {
        SP0001: { collection: "Arctic Edit", parentCategory: "outerwear", parentLabel: "Áo khoác ngoài", category: "ao-phao", categoryLabel: "Áo phao", badge: "Sale", tagline: "Giữ ấm tốt, phom hiện đại", toneA: "#d8e1ea", toneB: "#7d94a9", style: "parka", popularity: 97, originalFactor: 1.22, icon: "bi-box-seam" },
        SP0002: { collection: "City Luxe", parentCategory: "outerwear", parentLabel: "Áo khoác ngoài", category: "ao-da", categoryLabel: "Áo da", badge: "Mới", tagline: "Thanh lịch cho ngày lạnh", toneA: "#eadbcf", toneB: "#6d4b3f", style: "jacket", popularity: 94, originalFactor: 1.18, icon: "bi-stars" },
        SP0003: { collection: "Soft Layer", parentCategory: "layering", parentLabel: "Áo len layer", category: "ao-len", categoryLabel: "Áo len", badge: "Best seller", tagline: "Mềm nhẹ, dễ phối hằng ngày", toneA: "#f0e5d8", toneB: "#a18b75", style: "sweater", popularity: 99, originalFactor: 1.15, icon: "bi-grid" },
        SP0004: { collection: "Tailored Warmth", parentCategory: "outerwear", parentLabel: "Áo khoác ngoài", category: "mang-to", categoryLabel: "Măng tô", badge: "Premium", tagline: "Phom coat sang và tối giản", toneA: "#e6dfd8", toneB: "#727d88", style: "coat", popularity: 90, originalFactor: 1.2, icon: "bi-bag" },
        SP0005: { collection: "Street Comfort", parentCategory: "heattech", parentLabel: "Áo giữ nhiệt", category: "giu-nhiet", categoryLabel: "Áo giữ nhiệt", badge: "Hot", tagline: "Ấm áp, trẻ và dễ phối", toneA: "#d4dfd1", toneB: "#667b5f", style: "hoodie", popularity: 93, originalFactor: 1.14, icon: "bi-shield-check" },
        SP0006: { collection: "Urban Layer", parentCategory: "short-jacket", parentLabel: "Áo khoác ngắn", category: "gile", categoryLabel: "Gile phao", badge: "Sale", tagline: "Gọn nhẹ cho nhịp sống thành phố", toneA: "#efe4d2", toneB: "#84755f", style: "vest", popularity: 91, originalFactor: 1.16, icon: "bi-handbag" },
        SP0007: { collection: "Metro Ease", parentCategory: "short-jacket", parentLabel: "Áo khoác ngắn", category: "ao-khoac-ngan", categoryLabel: "Áo khoác ngắn", badge: "Mới", tagline: "Dáng gọn, mặc đẹp mỗi ngày", toneA: "#d9dee7", toneB: "#69788b", style: "jacket", popularity: 88, originalFactor: 1.15, icon: "bi-stars" },
        SP0008: { collection: "Soft Layer", parentCategory: "layering", parentLabel: "Áo len layer", category: "ao-len", categoryLabel: "Áo len", badge: "Premium", tagline: "Len mịn, layer nhẹ và ấm", toneA: "#ede2d7", toneB: "#a78974", style: "sweater", popularity: 92, originalFactor: 1.14, icon: "bi-grid" },
        SP0009: { collection: "Core Base", parentCategory: "heattech", parentLabel: "Áo giữ nhiệt", category: "giu-nhiet", categoryLabel: "Áo giữ nhiệt", badge: "Sale", tagline: "Lớp trong gọn nhẹ, co giãn tốt", toneA: "#dfe3de", toneB: "#808a7f", style: "hoodie", popularity: 95, originalFactor: 1.13, icon: "bi-shield-check" },
        SP0010: { collection: "Snow Field", parentCategory: "outerwear", parentLabel: "Áo khoác ngoài", category: "ao-phao", categoryLabel: "Áo phao", badge: "Hot", tagline: "Parka chống gió cho ngày rất lạnh", toneA: "#d5deea", toneB: "#6f86a2", style: "parka", popularity: 96, originalFactor: 1.19, icon: "bi-box-seam" },
        SP0011: { collection: "Layer Studio", parentCategory: "layering", parentLabel: "Áo len layer", category: "ao-len", categoryLabel: "Áo len", badge: "Mới", tagline: "Cardigan mềm cho set layer linh hoạt", toneA: "#eadfda", toneB: "#8d7469", style: "sweater", popularity: 87, originalFactor: 1.14, icon: "bi-grid" },
        SP0012: { collection: "Street Frost", parentCategory: "short-jacket", parentLabel: "Áo khoác ngắn", category: "ao-khoac-ngan", categoryLabel: "Áo khoác ngắn", badge: "Best seller", tagline: "Bomber trẻ, gọn và dễ phối", toneA: "#dddfe4", toneB: "#70757f", style: "jacket", popularity: 94, originalFactor: 1.17, icon: "bi-stars" }
    };
    const categories = [
        { key: "outerwear", label: "Áo khoác ngoài", icon: "bi-box-seam", description: "Tủ đồ mùa lạnh" },
        { key: "short-jacket", label: "Áo khoác ngắn", icon: "bi-stars", description: "Dễ mặc hằng ngày" },
        { key: "layering", label: "Áo len layer", icon: "bi-grid", description: "Mềm và dễ phối" },
        { key: "heattech", label: "Áo giữ nhiệt", icon: "bi-shield-check", description: "Giữ ấm nhẹ" }
    ];
    const provinces = [
        { value: "ha-noi", label: "Hà Nội", districts: [{ value: "cau-giay", label: "Cầu Giấy", wards: ["Dịch Vọng", "Quan Hoa", "Nghĩa Tân"] }, { value: "dong-da", label: "Đống Đa", wards: ["Láng Hạ", "Ô Chợ Dừa", "Văn Chương"] }] },
        { value: "hai-phong", label: "Hải Phòng", districts: [{ value: "ngo-quyen", label: "Ngô Quyền", wards: ["Máy Chai", "Lạch Tray", "Đông Khê"] }, { value: "le-chan", label: "Lê Chân", wards: ["An Biên", "Dư Hàng", "Kênh Dương"] }] },
        { value: "tp-hcm", label: "TP. Hồ Chí Minh", districts: [{ value: "quan-1", label: "Quận 1", wards: ["Bến Nghé", "Đa Kao", "Nguyễn Cư Trinh"] }, { value: "binh-thanh", label: "Bình Thạnh", wards: ["Phường 1", "Phường 13", "Phường 25"] }] }
    ];
    const shippingMethods = [
        { code: "vnpost", label: "VietNam Post", fee: 20000, note: "Giao tiêu chuẩn toàn quốc." },
        { code: "express", label: "Giao nhanh tiết kiệm", fee: 30000, note: "Ưu tiên khu vực nội thành." },
        { code: "freeship", label: "Freeship đơn trên 500.000đ", fee: 0, note: "Tự động áp dụng nếu đủ điều kiện.", minimum: 500000 }
    ];
    const paymentMethods = [
        { code: "vnpay", label: "Ví điện tử/VNPAY", note: "Thanh toán online qua cổng mô phỏng." },
        { code: "atm", label: "Online banking", note: "Chuyển khoản ngân hàng nội địa." },
        { code: "cod", label: "Thanh toán khi nhận hàng (COD)", note: "Thanh toán cho đơn vị giao hàng khi nhận." }
    ];
    const coupons = [
        { code: "WINTER10", label: "Giảm 10%", type: "percent", value: 10, minSubtotal: 600000 },
        { code: "NEW50", label: "Giảm 50.000đ", type: "amount", value: 50000, minSubtotal: 500000 },
        { code: "FREESHIP", label: "Miễn phí vận chuyển", type: "shipping", value: 0, minSubtotal: 300000 }
    ];
>>>>>>> b2f0504c96bc3608d57fc3dc336ee4e756b36ed4

    const escapeHtml = value => String(value ?? "").replace(/&/g, "&amp;").replace(/</g, "&lt;").replace(/>/g, "&gt;").replace(/\"/g, "&quot;").replace(/'/g, "&#39;");
    const formatCurrency = value => Number(value || 0).toLocaleString("vi-VN") + " đ";
    const clamp = (value, min, max) => Math.min(Math.max(value, min), max);
<<<<<<< HEAD

    // Fallback Meta
    const getMeta = productOrId => metaMap[typeof productOrId === "string" ? productOrId : productOrId?.sanPhamID] || { collection: "Winter Capsule", parentCategory: "outerwear", parentLabel: "Áo khoác ngoài", category: "ao-phao", categoryLabel: "Áo phao", badge: "Hot", tagline: "Thiết kế tối giản cho mùa lạnh", toneA: "#ddd8cd", toneB: "#887a69", style: "jacket", popularity: 80, originalFactor: 1.12, icon: "bi-box-seam" };

=======
    const getMeta = productOrId => metaMap[typeof productOrId === "string" ? productOrId : productOrId?.sanPhamID] || { collection: "Winter Capsule", parentCategory: "outerwear", parentLabel: "Áo khoác ngoài", category: "ao-phao", categoryLabel: "Áo phao", badge: "Hot", tagline: "Thiết kế tối giản cho mùa lạnh", toneA: "#ddd8cd", toneB: "#887a69", style: "jacket", popularity: 80, originalFactor: 1.12, icon: "bi-box-seam" };
    const getCoupon = code => coupons.find(item => item.code.toLowerCase() === String(code || "").trim().toLowerCase()) || null;
>>>>>>> b2f0504c96bc3608d57fc3dc336ee4e756b36ed4
    const isValidPhoneNumber = phone => /^(0|\+84)\d{9,10}$/.test((phone || "").replace(/\s+/g, ""));
    const isAuthenticated = () => !!storeSession.isAuthenticated && !!storeSession.userId;
    const getStorageKey = baseKey => `${baseKey}:${isAuthenticated() ? storeSession.userId : "guest"}`;
    const getEmptyCartState = () => ({ items: [], note: "", couponCode: "", shippingCode: "vnpost", paymentCode: "cod" });
<<<<<<< HEAD

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
=======
    const redirectToLogin = (message = "Vui lòng đăng nhập để tiếp tục.") => {
        showToast(message, "warning");
        window.setTimeout(() => {
            window.location.href = "/Account/Login";
        }, 250);
    };
    const getCartPreferences = () => {
        if (!isAuthenticated()) {
            return getEmptyCartState();
        }
        try {
            const parsed = JSON.parse(localStorage.getItem(getStorageKey(cartKey)) || "{}");
            return { note: parsed.note || "", couponCode: parsed.couponCode || "", shippingCode: parsed.shippingCode || "vnpost", paymentCode: parsed.paymentCode || "cod" };
        } catch {
            return getEmptyCartState();
        }
    };
    const getCartState = () => ({ ...getCartPreferences(), items: [...cartItemsCache] });
    const emitCartChanged = () => {
        document.dispatchEvent(new CustomEvent("winterCartChanged", { detail: getCartState() }));
    };
    const saveCartState = state => {
        if (!isAuthenticated()) {
            return;
        }
        const nextState = state || getEmptyCartState();
        if (Array.isArray(nextState.items)) {
            cartItemsCache = nextState.items.map(item => ({ ...item }));
        }
        localStorage.setItem(getStorageKey(cartKey), JSON.stringify({
            note: nextState.note || "",
            couponCode: nextState.couponCode || "",
            shippingCode: nextState.shippingCode || "vnpost",
            paymentCode: nextState.paymentCode || "cod"
        }));
        emitCartChanged();
    };
    const getProfile = () => {
        if (!isAuthenticated()) {
            return {};
        }
        try { return JSON.parse(localStorage.getItem(getStorageKey(profileKey)) || "{}"); } catch { return {}; }
    };
    const saveProfile = profile => {
        if (!isAuthenticated()) {
            return;
        }
        localStorage.setItem(getStorageKey(profileKey), JSON.stringify(profile));
    };
    const getTotalQuantity = (state = getCartState()) => state.items.reduce((sum, item) => sum + Number(item.soLuong || 0), 0);
    const getSubtotal = (state = getCartState()) => state.items.reduce((sum, item) => sum + Number(item.donGia || 0) * Number(item.soLuong || 0), 0);

    function getPhotoAsset(product, variant) {
        const productId = typeof product === "string" ? product : product?.sanPhamID;
        const mapped = productPhotoMap[productId];
        if (mapped) {
            const variantKey = variant?.mauID || variant?.mauId || variant?.tenMau || "";
            return mapped.variants?.[variantKey] || mapped.default || null;
        }

        const meta = getMeta(product);
        return stylePhotos[meta.style] || null;
    }

    function buildArtwork(product, mode, variant = null) {
        const meta = getMeta(product);
        const photo = getPhotoAsset(product, variant);
        const modeClass = mode === "large" ? " large" : mode === "compact" ? " compact" : "";
        if (photo) {
            return `<div class="product-visual photo-mode${modeClass}" style="--tone-a:${meta.toneA};--tone-b:${meta.toneB};"><div class="product-photo-layer"><img class="product-photo" src="${photo.url}" alt="${escapeHtml(product?.ten || meta.categoryLabel)}" loading="lazy" style="object-position:${photo.position};" /></div></div>`;
        }
        const hoodMarkup = meta.style === "hoodie" ? '<span class="hood"></span>' : "";
        const collarMarkup = meta.style === "coat" || meta.style === "jacket" || meta.style === "parka" ? '<span class="garment-collar collar-left"></span><span class="garment-collar collar-right"></span>' : "";
        const pocketMarkup = meta.style === "sweater"
            ? ""
            : '<span class="garment-pocket pocket-left"></span><span class="garment-pocket pocket-right"></span>';
        const seamMarkup = meta.style === "vest"
            ? '<span class="garment-placket short"></span>'
            : '<span class="garment-placket"></span><span class="garment-hem"></span>';
        return `<div class="product-visual${modeClass}" style="--tone-a:${meta.toneA};--tone-b:${meta.toneB};"><span class="product-badge">${escapeHtml(meta.badge)}</span><span class="visual-shape shape-one"></span><span class="visual-shape shape-two"></span><div class="garment" data-style="${escapeHtml(meta.style)}"><span class="garment-body"></span>${hoodMarkup}${collarMarkup}${seamMarkup}${pocketMarkup}<span class="garment-accent"></span></div></div>`;
    }

    function enrichProduct(product) {
        const meta = getMeta(product);
        const giaThapNhat = Number(product.giaThapNhat || 0);
        const giaGoc = Number(product.giaGoc || giaThapNhat);
        return { ...product, meta, giaGoc, phanTramGiam: Number(product.phanTramGiam || 0) };
    }

>>>>>>> b2f0504c96bc3608d57fc3dc336ee4e756b36ed4
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

<<<<<<< HEAD
    // Hàm bổ trợ check ảnh an toàn chống lỗi 404 truyền chuỗi "undefined"
    function getSafeProductImgUrl(item) {
        const colorId = item.mauID || item.mauId || "";
        if (item.sanPhamID && colorId && colorId !== "undefined") {
            return `/images/products/${item.sanPhamID}-${colorId}.png`;
        }
        return item.hinhAnhUrl || item.imageUrl || '/images/default-product.png';
    }

=======
>>>>>>> b2f0504c96bc3608d57fc3dc336ee4e756b36ed4
    function syncCartItems(items) {
        cartItemsCache = Array.isArray(items) ? items.map(item => ({ ...item })) : [];
        emitCartChanged();
        return getCartState();
    }

    async function refreshCartFromServer() {
<<<<<<< HEAD
        if (!isAuthenticated()) { cartItemsCache = []; return getCartState(); }
=======
        if (!isAuthenticated()) {
            cartItemsCache = [];
            return getCartState();
        }

>>>>>>> b2f0504c96bc3608d57fc3dc336ee4e756b36ed4
        try {
            const response = await requestJson("/api/cart");
            return syncCartItems(response?.items || []);
        } catch (error) {
<<<<<<< HEAD
            if (error.status === 401) { redirectToLogin(); return getEmptyCartState(); }
=======
            if (error.status === 401) {
                redirectToLogin();
                return getEmptyCartState();
            }
            console.error(error);
>>>>>>> b2f0504c96bc3608d57fc3dc336ee4e756b36ed4
            showToast(error.message || "Không thể tải giỏ hàng.", "danger");
            return getCartState();
        }
    }

    async function addCartItemToServer(chiTietSanPhamID, soLuong) {
<<<<<<< HEAD
        const response = await requestJson("/api/cart/items", { method: "POST", headers: { "Content-Type": "application/json" }, body: JSON.stringify({ chiTietSanPhamID, soLuong }) });
=======
        const response = await requestJson("/api/cart/items", {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify({ chiTietSanPhamID, soLuong })
        });
>>>>>>> b2f0504c96bc3608d57fc3dc336ee4e756b36ed4
        return syncCartItems(response?.items || []);
    }

    async function updateCartItemOnServer(chiTietSanPhamID, soLuong) {
<<<<<<< HEAD
        const response = await requestJson(`/api/cart/items/${encodeURIComponent(chiTietSanPhamID)}`, { method: "PUT", headers: { "Content-Type": "application/json" }, body: JSON.stringify({ soLuong }) });
=======
        const response = await requestJson(`/api/cart/items/${encodeURIComponent(chiTietSanPhamID)}`, {
            method: "PUT",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify({ soLuong })
        });
>>>>>>> b2f0504c96bc3608d57fc3dc336ee4e756b36ed4
        return syncCartItems(response?.items || []);
    }

    async function removeCartItemFromServer(chiTietSanPhamID) {
<<<<<<< HEAD
        const response = await requestJson(`/api/cart/items/${encodeURIComponent(chiTietSanPhamID)}`, { method: "DELETE" });
=======
        const response = await requestJson(`/api/cart/items/${encodeURIComponent(chiTietSanPhamID)}`, {
            method: "DELETE"
        });
>>>>>>> b2f0504c96bc3608d57fc3dc336ee4e756b36ed4
        return syncCartItems(response?.items || []);
    }

    async function clearCartOnServer() {
<<<<<<< HEAD
        const response = await requestJson("/api/cart", { method: "DELETE" });
=======
        const response = await requestJson("/api/cart", {
            method: "DELETE"
        });
>>>>>>> b2f0504c96bc3608d57fc3dc336ee4e756b36ed4
        return syncCartItems(response?.items || []);
    }

    async function fetchProducts() {
        const products = await fetchJson("/api/products");
<<<<<<< HEAD
        return Array.isArray(products) ? products.map(p => ({ ...p, meta: getMeta(p), giaGoc: Number(p.giaGoc || p.giaThapNhat || 0), phanTramGiam: Number(p.phanTramGiam || 0) })) : [];
=======
        return Array.isArray(products) ? products.map(enrichProduct) : [];
    }

    async function fetchProduct(productId) {
        const product = enrichProduct(await fetchJson(`/api/products/${encodeURIComponent(productId)}`));
        product.bienThe = Array.isArray(product.bienThe) ? product.bienThe.map(item => ({ ...item, sizeLabel: String(item.tenKichCo || "").replace(/^Size\s*/i, "").trim() })) : [];
        if (product.bienThe.length) {
            product.giaThapNhat = Math.min(...product.bienThe.map(item => Number(item.giaNiemYet || 0)));
            product.giaGoc = Math.min(...product.bienThe.map(item => Number(item.giaGoc || item.giaNiemYet || 0)));
            product.tongSoLuongTon = product.bienThe.reduce((sum, item) => sum + Number(item.soLuongTon || 0), 0);
            product.phanTramGiam = Math.max(...product.bienThe.map(item => Number(item.phanTramGiam || 0)));
        }
        return product;
    }

    function renderProductCard(product) {
        const detailUrl = `/Home/Details?id=${encodeURIComponent(product.sanPhamID)}`;
        const discountBadge = product.phanTramGiam > 0 ? `<span class="sale-badge">-${product.phanTramGiam}%</span>` : "";
        const originalPrice = product.phanTramGiam > 0 ? `<span class="price-original">${formatCurrency(product.giaGoc)}</span>` : "";
        return `<article class="product-card"><a class="product-card-link" href="${detailUrl}" aria-label="Xem chi tiết ${escapeHtml(product.ten)}">${buildArtwork(product, "card")}<div class="product-info"><div class="price-stack">${discountBadge}<span class="muted-copy">${escapeHtml(product.meta.categoryLabel)}</span></div><span class="product-name">${escapeHtml(product.ten)}</span><div class="product-copy">${escapeHtml(product.meta.tagline)}</div><div class="price-stack"><strong class="price-sale">${formatCurrency(product.giaThapNhat)}</strong>${originalPrice}</div></div></a></article>`;
>>>>>>> b2f0504c96bc3608d57fc3dc336ee4e756b36ed4
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
<<<<<<< HEAD

    // 🔥 FIX 1: Đã dọn sạch lỗi cú pháp lặp thẻ img rác ở ô xem trước Header
=======
    function getShipping(state, subtotal) {
        const picked = shippingMethods.find(item => item.code === state.shippingCode) || shippingMethods[0];
        return picked.minimum && subtotal < picked.minimum ? shippingMethods[0] : picked;
    }

    function calculateTotals(state = getCartState()) {
        const subtotal = getSubtotal(state);
        const shipping = getShipping(state, subtotal);
        const coupon = getCoupon(state.couponCode);
        let discount = 0;
        if (coupon && subtotal >= coupon.minSubtotal) {
            if (coupon.type === "percent") discount = Math.round(subtotal * coupon.value / 100);
            if (coupon.type === "amount") discount = coupon.value;
            if (coupon.type === "shipping") discount = shipping.fee;
        }
        discount = Math.min(discount, subtotal + shipping.fee);
        return { subtotal, shipping, coupon, discount, total: Math.max(subtotal + shipping.fee - discount, 0) };
    }

>>>>>>> b2f0504c96bc3608d57fc3dc336ee4e756b36ed4
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
<<<<<<< HEAD

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
=======
        itemsNode.innerHTML = state.items.length ? state.items.slice(0, 4).map(item => `<article class="cart-preview-item"><div class="thumb-wrap">${buildArtwork({ sanPhamID: item.sanPhamID }, "compact")}</div><div><strong>${escapeHtml(item.tenSanPham)}</strong><div class="item-meta">${escapeHtml(item.phanLoai)} · SL ${item.soLuong}</div><div class="price-sale">${formatCurrency(item.donGia * item.soLuong)}</div></div></article>`).join("") : '<div class="cart-preview-empty">Giỏ hàng đang trống.</div>';
>>>>>>> b2f0504c96bc3608d57fc3dc336ee4e756b36ed4
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
<<<<<<< HEAD

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
=======
        node.innerHTML = matches.length ? matches.map(product => `<a class="header-search-item" href="/Home/Details?id=${encodeURIComponent(product.sanPhamID)}"><div class="thumb-wrap">${buildArtwork(product, "compact")}</div><div><strong>${escapeHtml(product.ten)}</strong><div class="item-meta">${escapeHtml(product.meta.parentLabel)}</div><div class="price-sale">${formatCurrency(product.giaThapNhat)}</div></div></a>`).join("") : '<div class="header-search-empty">Không tìm thấy sản phẩm phù hợp.</div>';
    }

    function renderFixedProductRow(items, slotCount = 4) {
        if (!items.length) {
            return '<div class="empty-state">Khong co san pham phu hop.</div>';
        }

        const markup = items.slice(0, slotCount).map(renderProductCard);
        while (markup.length < slotCount) {
            markup.push('<article class="product-card placeholder-card" aria-hidden="true"></article>');
        }

        return markup.join("");
    }

    async function addItemToCart(product, variant, quantity) {
        if (!isAuthenticated()) {
            redirectToLogin("Vui lòng đăng nhập trước khi thêm sản phẩm vào giỏ hàng.");
            return false;
        }
>>>>>>> b2f0504c96bc3608d57fc3dc336ee4e756b36ed4
        const safeQuantity = clamp(Number.parseInt(quantity, 10) || 1, 1, Math.min(20, Number(variant.soLuongTon || 1)));
        try {
            await addCartItemToServer(variant.chiTietSanPhamID, safeQuantity);
            return true;
<<<<<<< HEAD
        } catch (error) { showToast(error.message || "Không thể thêm sản phẩm vào giỏ hàng.", "danger"); return false; }
    }

    function initHeader(productsPromise) {
=======
        } catch (error) {
            console.error(error);
            showToast(error.message || "Không thể thêm sản phẩm vào giỏ hàng.", "danger");
            return false;
        }
    }

    function initHeader(productsPromise) {
        const bindHoverDropdown = selector => {
            document.querySelectorAll(selector).forEach(node => {
                let closeTimer = null;
                const open = () => {
                    if (closeTimer) window.clearTimeout(closeTimer);
                    node.classList.add("is-open");
                };
                const close = () => {
                    if (closeTimer) window.clearTimeout(closeTimer);
                    closeTimer = window.setTimeout(() => node.classList.remove("is-open"), 140);
                };
                node.addEventListener("mouseenter", open);
                node.addEventListener("mouseleave", close);
            });
        };

        bindHoverDropdown(".has-submenu");
        bindHoverDropdown(".account-menu");
        bindHoverDropdown(".cart-menu");

>>>>>>> b2f0504c96bc3608d57fc3dc336ee4e756b36ed4
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
<<<<<<< HEAD
        wrapper.addEventListener("mouseenter", () => { if (searchCloseTimer) window.clearTimeout(searchCloseTimer); wrapper.classList.add("is-open"); });
        wrapper.addEventListener("mouseleave", () => { if (searchCloseTimer) window.clearTimeout(searchCloseTimer); searchCloseTimer = window.setTimeout(() => wrapper.classList.remove("is-open"), 140); });
        toggle.addEventListener("click", () => { wrapper.classList.toggle("is-open"); if (wrapper.classList.contains("is-open")) input.focus(); });
=======
        const openSearch = () => {
            if (searchCloseTimer) window.clearTimeout(searchCloseTimer);
            wrapper.classList.add("is-open");
        };
        const closeSearch = () => {
            if (searchCloseTimer) window.clearTimeout(searchCloseTimer);
            searchCloseTimer = window.setTimeout(() => wrapper.classList.remove("is-open"), 140);
        };
        wrapper.addEventListener("mouseenter", openSearch);
        wrapper.addEventListener("mouseleave", closeSearch);
        toggle.addEventListener("click", () => {
            wrapper.classList.toggle("is-open");
            if (wrapper.classList.contains("is-open")) input.focus();
        });
>>>>>>> b2f0504c96bc3608d57fc3dc336ee4e756b36ed4
        document.addEventListener("click", event => { if (!wrapper.contains(event.target)) wrapper.classList.remove("is-open"); });
        input.addEventListener("input", async () => setSearchResults(await productsPromise, input.value));
    }

<<<<<<< HEAD
    async function initCart() {
        const page = document.getElementById("cartPage");
        if (!page) return;
        if (!isAuthenticated()) { redirectToLogin("Vui lòng đăng nhập để xem giỏ hàng."); return; }

=======
    function initHome(productsPromise) {
        const page = document.getElementById("homePage");
        if (!page) return;
        const bannerCount = Number(page.dataset.bannerCount || 0);
        if (bannerCount > 1) {
            const slides = [...page.querySelectorAll(".hero-slide")];
            const dots = [...page.querySelectorAll(".hero-dot")];
            const navButtons = [...page.querySelectorAll(".hero-nav-button")];
            const interval = Number(page.querySelector(".hero-banner")?.dataset.interval || 7000);
            let activeSlide = 0;
            let timer = null;
            const setActiveSlide = index => {
                activeSlide = index;
                slides.forEach((slide, slideIndex) => slide.classList.toggle("active", slideIndex === index));
                dots.forEach((dot, dotIndex) => dot.classList.toggle("active", dotIndex === index));
            };
            const restartTimer = () => {
                if (timer) window.clearInterval(timer);
                timer = window.setInterval(() => {
                    setActiveSlide((activeSlide + 1) % slides.length);
                }, interval);
            };
            restartTimer();
            dots.forEach(dot => {
                dot.addEventListener("click", () => {
                    setActiveSlide(Number(dot.dataset.slideTo || 0));
                    restartTimer();
                });
            });
            navButtons.forEach(button => {
                button.addEventListener("click", () => {
                    const direction = button.dataset.slideDirection === "prev" ? -1 : 1;
                    setActiveSlide((activeSlide + direction + slides.length) % slides.length);
                    restartTimer();
                });
            });
        }
        productsPromise.then(products => {
            document.getElementById("homeCategories").innerHTML = categories.map(category => `<a class="category-card" href="/Home/Products?category=${category.key}"><div class="category-icon"><i class="bi ${escapeHtml(category.icon)}"></i></div><strong>${escapeHtml(category.label)}</strong><span>${escapeHtml(category.description)}</span></a>`).join("");
            const deepDeals = products
                .filter(item => Number(item.phanTramGiam || 0) >= 25)
                .sort((a, b) => Number(b.phanTramGiam || 0) - Number(a.phanTramGiam || 0));
            document.getElementById("homeFeaturedProducts").innerHTML = renderFixedProductRow(deepDeals, 4);
            const tabsNode = document.getElementById("homeTabs");
            const listNode = document.getElementById("homeTabProducts");
            let active = categories[0].key;
            const render = () => {
                tabsNode.innerHTML = categories.map(category => `<button type="button" class="chip-button ${active === category.key ? "active" : ""}" data-key="${category.key}">${escapeHtml(category.label)}</button>`).join("");
                listNode.innerHTML = renderFixedProductRow(products.filter(item => item.meta.parentCategory === active), 4);
            };
            tabsNode.addEventListener("click", event => {
                const button = event.target.closest("[data-key]");
                if (!button) return;
                active = button.dataset.key;
                render();
            });
            render();
        }).catch(() => {
            document.getElementById("homeFeaturedProducts").innerHTML = '<div class="empty-state">Không thể tải sản phẩm.</div>';
            document.getElementById("homeTabProducts").innerHTML = '<div class="empty-state">Không thể tải sản phẩm.</div>';
        });
    }
    function initProducts(productsPromise) {
        const page = document.getElementById("productsPage");
        if (!page) return;
        const query = new URLSearchParams(window.location.search);
        const state = { parent: query.get("category") || "all", sub: query.get("subcategory") || "all", keyword: "", sort: "featured", min: 0, max: 3000000, size: "all", page: 1, pageSize: 8 };
        productsPromise.then(async products => {
            const details = await Promise.all(products.map(item => fetchProduct(item.sanPhamID).catch(() => item)));
            const parentSelect = document.getElementById("filterParentCategory");
            const subSelect = document.getElementById("filterSubCategory");
            const searchInput = document.getElementById("catalogSearchInput");
            const sortSelect = document.getElementById("catalogSortSelect");
            const minRange = document.getElementById("priceRangeMin");
            const maxRange = document.getElementById("priceRangeMax");
            const sizeFilters = document.getElementById("sizeFilters");
            const subTabs = document.getElementById("catalogSubTabs");
            const grid = document.getElementById("catalogGrid");
            const countNode = document.getElementById("catalogCount");
            const summaryNode = document.getElementById("catalogSummary");
            const paginationNode = document.getElementById("catalogPagination");
            const setRangeText = () => {
                document.getElementById("priceRangeMinLabel").textContent = formatCurrency(state.min);
                document.getElementById("priceRangeMaxLabel").textContent = formatCurrency(state.max);
            };
            const getSubs = () => [...new Set(details.filter(item => state.parent === "all" || item.meta.parentCategory === state.parent).map(item => item.meta.category))];
            const renderSelects = () => {
                parentSelect.innerHTML = ['<option value="all">Tất cả</option>'].concat(categories.map(item => `<option value="${item.key}">${escapeHtml(item.label)}</option>`)).join("");
                parentSelect.value = state.parent;
                const subs = getSubs();
                if (!subs.includes(state.sub)) state.sub = "all";
                subSelect.innerHTML = ['<option value="all">Tất cả</option>'].concat(subs.map(key => `<option value="${key}">${escapeHtml((details.find(item => item.meta.category === key) || {}).meta?.categoryLabel || key)}</option>`)).join("");
                subSelect.value = state.sub;
                subTabs.innerHTML = ['<button type="button" class="chip-button ' + (state.sub === "all" ? "active" : "") + '" data-sub="all">Tất cả</button>'].concat(subs.map(key => `<button type="button" class="chip-button ${state.sub === key ? "active" : ""}" data-sub="${key}">${escapeHtml((details.find(item => item.meta.category === key) || {}).meta?.categoryLabel || key)}</button>`)).join("");
                sizeFilters.innerHTML = ["all", "S", "M", "L", "XL"].map(size => `<button type="button" class="chip-button ${state.size === size ? "active" : ""}" data-size="${size}">${size === "all" ? "Tất cả" : size}</button>`).join("");
            };
            const getFiltered = () => details.filter(item => {
                const sizeMatch = state.size === "all" || (item.bienThe || []).some(variant => variant.sizeLabel === state.size);
                return (state.parent === "all" || item.meta.parentCategory === state.parent) && (state.sub === "all" || item.meta.category === state.sub) && (!state.keyword || item.ten.toLowerCase().includes(state.keyword) || item.meta.categoryLabel.toLowerCase().includes(state.keyword)) && item.giaThapNhat >= state.min && item.giaThapNhat <= state.max && sizeMatch;
            }).sort((a, b) => {
                if (state.sort === "name-asc") return a.ten.localeCompare(b.ten, "vi");
                if (state.sort === "name-desc") return b.ten.localeCompare(a.ten, "vi");
                if (state.sort === "price-asc") return a.giaThapNhat - b.giaThapNhat;
                if (state.sort === "price-desc") return b.giaThapNhat - a.giaThapNhat;
                return b.meta.popularity - a.meta.popularity;
            });
            const render = () => {
                renderSelects();
                setRangeText();
                const filtered = getFiltered();
                const pageCount = filtered.length ? Math.ceil(filtered.length / state.pageSize) : 0;
                state.page = pageCount ? clamp(state.page, 1, pageCount) : 1;
                const pageItems = filtered.slice((state.page - 1) * state.pageSize, state.page * state.pageSize);
                countNode.textContent = filtered.length;
                summaryNode.textContent = filtered.length ? `Đang xem ${state.page}/${pageCount}` : "Không có sản phẩm phù hợp.";
                grid.innerHTML = pageItems.length ? pageItems.map(renderProductCard).join("") : '<div class="empty-state">Không có sản phẩm phù hợp.</div>';
                paginationNode.innerHTML = pageCount > 1
                    ? Array.from({ length: pageCount }, (_, i) => `<button type="button" class="pagination-chip ${state.page === i + 1 ? "active" : ""}" data-page="${i + 1}">${i + 1}</button>`).join("")
                    : "";
            };
            parentSelect.addEventListener("change", () => { state.parent = parentSelect.value; state.sub = "all"; state.page = 1; render(); });
            subSelect.addEventListener("change", () => { state.sub = subSelect.value; state.page = 1; render(); });
            searchInput.addEventListener("input", () => { state.keyword = searchInput.value.trim().toLowerCase(); state.page = 1; render(); });
            sortSelect.addEventListener("change", () => { state.sort = sortSelect.value; render(); });
            minRange.addEventListener("input", () => { state.min = Math.min(Number(minRange.value), state.max); minRange.value = state.min; render(); });
            maxRange.addEventListener("input", () => { state.max = Math.max(Number(maxRange.value), state.min); maxRange.value = state.max; render(); });
            sizeFilters.addEventListener("click", event => { const button = event.target.closest("[data-size]"); if (!button) return; state.size = button.dataset.size; state.page = 1; render(); });
            subTabs.addEventListener("click", event => { const button = event.target.closest("[data-sub]"); if (!button) return; state.sub = button.dataset.sub; state.page = 1; render(); });
            paginationNode.addEventListener("click", event => { const button = event.target.closest("[data-page]"); if (!button) return; state.page = Number(button.dataset.page); render(); });
            sortSelect.value = state.sort;
            render();
        }).catch(() => {
            document.getElementById("catalogGrid").innerHTML = '<div class="empty-state">Không thể tải danh sách sản phẩm.</div>';
            document.getElementById("catalogSummary").textContent = "Không thể tải dữ liệu.";
        });
    }

    function getDefaultVariant(product) {
        const whiteS = product.bienThe.find(item => /kem|trắng|white/i.test(item.tenMau) && item.sizeLabel === "S");
        return whiteS || product.bienThe.slice().sort((a, b) => a.giaNiemYet - b.giaNiemYet)[0] || null;
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
            description: current => `<p>${escapeHtml(current.moTa || "Sản phẩm được tối ưu cho nhu cầu mặc mùa đông hằng ngày.")}</p><ul><li>Giá hiển thị đổi theo màu và size.</li><li>Tối đa 20 sản phẩm cho mỗi biến thể trong một đơn hàng.</li><li>Chỉ thay đổi số lượng ở giỏ hàng, muốn đổi màu hoặc size cần quay lại trang chi tiết.</li></ul>`,
            returns: () => `<ul><li>Đổi size trong 7 ngày nếu sản phẩm còn nguyên tem.</li><li>Không đổi trực tiếp màu và size trong giỏ hàng.</li><li>Đơn đổi trả phụ thuộc tồn kho thời điểm xử lý.</li></ul>`,
            warranty: () => `<ul><li>Thông tin khách hàng chỉ dùng để xử lý đơn hàng.</li><li>Dữ liệu checkout gần nhất được lưu cục bộ để tự điền lại.</li><li>Thanh toán online trong bản demo là mô phỏng giao diện.</li></ul>`
        };
        const getDetailQuantity = () => Number(document.getElementById("detailQuantityValue")?.textContent || 1);
        const setDetailQuantity = nextValue => {
            const node = document.getElementById("detailQuantityValue");
            if (!node) return;
            node.textContent = String(nextValue);
        };
        const getSelected = () => product.bienThe.find(item => item.tenMau === selectedColor && item.sizeLabel === selectedSize) || null;
        const renderTabs = () => {
            page.querySelectorAll(".detail-tab").forEach(button => button.classList.toggle("active", button.dataset.tab === activeTab));
            document.getElementById("detailTabContent").innerHTML = tabContent[activeTab](product);
        };
        const renderGallery = () => {
            const uniqueByColor = [...new Map(product.bienThe.map(item => [item.mauID, item])).values()];
            document.getElementById("detailGalleryThumbs").innerHTML = uniqueByColor.map(item => `<button type="button" class="detail-thumb-button ${selectedColor === item.tenMau ? "active" : ""}" data-color="${escapeHtml(item.tenMau)}">${buildArtwork(product, "compact", item)}</button>`).join("");
            document.getElementById("detailArtwork").innerHTML = buildArtwork(product, "large", getSelected());
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
                document.getElementById("detailOriginalPrice").textContent = "";
                document.getElementById("detailDiscountBadge").textContent = "";
                document.getElementById("detailStockNote").textContent = "Biến thể đã chọn hiện không khả dụng.";
                add.disabled = true;
                buy.disabled = true;
                return;
            }
            document.getElementById("detailPrice").textContent = formatCurrency(variant.giaNiemYet);
            document.getElementById("detailOriginalPrice").textContent = Number(variant.phanTramGiam || 0) > 0 ? formatCurrency(variant.giaGoc) : "";
            document.getElementById("detailDiscountBadge").textContent = Number(variant.phanTramGiam || 0) > 0 ? `-${variant.phanTramGiam}%` : "";
            document.getElementById("detailVariantSummary").textContent = `Đã chọn ${variant.tenMau} / ${variant.sizeLabel}. Giá thay đổi theo biến thể này.`;
            document.getElementById("detailStockNote").textContent = `Còn ${variant.soLuongTon} sản phẩm · Tối đa mỗi đơn 20 sản phẩm.`;
            setDetailQuantity(clamp(getDetailQuantity(), 1, Math.max(1, Math.min(20, variant.soLuongTon))));
            add.disabled = variant.soLuongTon < 1;
            buy.disabled = variant.soLuongTon < 1;
            renderGallery();
        };
        fetchProduct(productId).then(detail => {
            product = detail;
            const variant = getDefaultVariant(product);
            if (!variant) {
                document.getElementById("detailArtwork").innerHTML = '<div class="empty-state">Sản phẩm hiện chưa có biến thể để bán.</div>';
                return;
            }
            selectedColor = variant.tenMau;
            selectedSize = variant.sizeLabel;
            document.getElementById("detailTitle").textContent = product.ten.toUpperCase();
            document.getElementById("detailBreadcrumb").textContent = `Winter Shop / ${product.meta.parentLabel} / ${product.meta.categoryLabel}`;
            setDetailQuantity(1);
            renderVariant();
            renderTabs();
            productsPromise.then(products => {
                const sameCategory = products.filter(item => item.sanPhamID !== product.sanPhamID && item.meta.category === product.meta.category);
                const sameParent = products.filter(item => item.sanPhamID !== product.sanPhamID && item.meta.parentCategory === product.meta.parentCategory && !sameCategory.some(x => x.sanPhamID === item.sanPhamID));
                document.getElementById("relatedProducts").innerHTML = [...sameCategory, ...sameParent].slice(0, 4).map(renderProductCard).join("");
            });
        }).catch(() => {
            document.getElementById("detailArtwork").innerHTML = '<div class="empty-state">Không thể tải sản phẩm.</div>';
        });
        document.getElementById("detailColorOptions").addEventListener("click", event => {
            const button = event.target.closest("[data-color]");
            if (!button || !product) return;
            selectedColor = button.dataset.color;
            const sizes = [...new Set(product.bienThe.filter(item => item.tenMau === selectedColor).map(item => item.sizeLabel))];
            if (!sizes.includes(selectedSize)) selectedSize = sizes[0];
            renderVariant();
        });
        document.getElementById("detailSizeOptions").addEventListener("click", event => {
            const button = event.target.closest("[data-size]");
            if (!button) return;
            selectedSize = button.dataset.size;
            renderVariant();
        });
        document.getElementById("detailGalleryThumbs").addEventListener("click", event => {
            const button = event.target.closest("[data-color]");
            if (!button || !product) return;
            selectedColor = button.dataset.color;
            const sizes = [...new Set(product.bienThe.filter(item => item.tenMau === selectedColor).map(item => item.sizeLabel))];
            if (!sizes.includes(selectedSize)) selectedSize = sizes[0];
            renderVariant();
        });
        page.querySelector(".detail-tabs").addEventListener("click", event => {
            const button = event.target.closest("[data-tab]");
            if (!button || !product) return;
            activeTab = button.dataset.tab;
            renderTabs();
        });
        document.getElementById("detailDecreaseQty").addEventListener("click", () => {
            const variant = product ? getSelected() : null;
            setDetailQuantity(clamp(getDetailQuantity() - 1, 1, Math.min(20, Number(variant?.soLuongTon || 20))));
        });
        document.getElementById("detailIncreaseQty").addEventListener("click", () => {
            const variant = product ? getSelected() : null;
            setDetailQuantity(clamp(getDetailQuantity() + 1, 1, Math.min(20, Number(variant?.soLuongTon || 20))));
        });
        document.getElementById("detailAddToCart").addEventListener("click", async () => {
            const variant = product ? getSelected() : null;
            if (!variant) return showToast("Vui lòng chọn màu và kích cỡ khả dụng.", "warning");
            if (!await addItemToCart(product, variant, getDetailQuantity())) return;
            showToast("Đã thêm sản phẩm vào giỏ hàng.", "success");
        });
        document.getElementById("detailBuyNow").addEventListener("click", async () => {
            const variant = product ? getSelected() : null;
            if (!variant) return showToast("Vui lòng chọn màu và kích cỡ khả dụng.", "warning");
            if (!await addItemToCart(product, variant, getDetailQuantity())) return;
            window.location.href = "/Home/Cart";
        });
    }
    async function initCart() {
        const page = document.getElementById("cartPage");
        if (!page) return;
        if (!isAuthenticated()) {
            redirectToLogin("Vui lòng đăng nhập để xem giỏ hàng.");
            return;
        }
>>>>>>> b2f0504c96bc3608d57fc3dc336ee4e756b36ed4
        await refreshCartFromServer();
        const container = document.getElementById("cartItemsContainer");
        const noteInput = document.getElementById("cartOrderNote");
        const checkoutLink = document.getElementById("cartCheckoutLink");
<<<<<<< HEAD

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

=======
        const render = () => {
            const state = getCartState();
            const totals = calculateTotals(state);
            document.getElementById("cartHeadCount").textContent = getTotalQuantity(state);
            document.getElementById("cartSubtotalAmount").textContent = formatCurrency(totals.subtotal);
            document.getElementById("cartDiscountAmount").textContent = `- ${formatCurrency(totals.discount)}`;
            document.getElementById("cartTotalAmount").textContent = formatCurrency(totals.total);
            noteInput.value = state.note;
            checkoutLink.style.pointerEvents = state.items.length ? "auto" : "none";
            checkoutLink.style.opacity = state.items.length ? "1" : "0.6";
            container.innerHTML = state.items.length ? state.items.map(item => `<article class="cart-item"><div class="thumb-wrap">${buildArtwork({ sanPhamID: item.sanPhamID }, "compact")}</div><div><a class="product-name" href="/Home/Details?id=${encodeURIComponent(item.sanPhamID)}">${escapeHtml(item.tenSanPham)}</a><div class="item-meta">${escapeHtml(item.phanLoai)}</div><div class="cart-qty-actions"><input class="store-input mini-qty cart-qty-input" type="number" min="0" max="${Math.min(20, item.tonKho || 20)}" value="${item.soLuong}" data-id="${item.chiTietSanPhamID}" /><button type="button" class="link-button cart-remove-btn" data-id="${item.chiTietSanPhamID}">Xóa</button></div></div><strong class="price-sale">${formatCurrency(item.donGia * item.soLuong)}</strong></article>`).join("") : '<div class="empty-state">Giỏ hàng đang trống.</div>';
        };
>>>>>>> b2f0504c96bc3608d57fc3dc336ee4e756b36ed4
        container.addEventListener("change", async event => {
            const input = event.target.closest(".cart-qty-input");
            if (!input) return;
            const quantity = Number(input.value);
            if (quantity <= 0) {
<<<<<<< HEAD
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

=======
                if (window.confirm("Bạn có muốn xóa sản phẩm này khỏi giỏ hàng không?")) {
                    try {
                        await removeCartItemFromServer(input.dataset.id);
                        showToast("Đã xóa sản phẩm khỏi giỏ hàng.", "success");
                    } catch (error) {
                        console.error(error);
                        showToast(error.message || "Không thể cập nhật giỏ hàng.", "danger");
                        render();
                    }
                } else {
                    render();
                }
                return;
            }
            const item = getCartState().items.find(entry => entry.chiTietSanPhamID === input.dataset.id);
            if (!item) return;
            try {
                await updateCartItemOnServer(input.dataset.id, clamp(quantity, 1, Math.min(20, item.tonKho || 20)));
            } catch (error) {
                console.error(error);
                showToast(error.message || "Không thể cập nhật giỏ hàng.", "danger");
                render();
            }
        });
        container.addEventListener("click", async event => {
            const button = event.target.closest(".cart-remove-btn");
            if (!button) return;
            try {
                await removeCartItemFromServer(button.dataset.id);
                showToast("Đã xóa sản phẩm khỏi giỏ hàng.", "success");
            } catch (error) {
                console.error(error);
                showToast(error.message || "Không thể cập nhật giỏ hàng.", "danger");
            }
        });
        noteInput.addEventListener("input", () => {
            const state = getCartState();
            state.note = noteInput.value;
            saveCartState(state);
        });
        checkoutLink.addEventListener("click", event => {
            if (!getCartState().items.length) {
                event.preventDefault();
                showToast("Giỏ hàng đang trống.", "warning");
            }
        });
>>>>>>> b2f0504c96bc3608d57fc3dc336ee4e756b36ed4
        document.addEventListener("winterCartChanged", render);
        render();
    }

    async function initCheckout() {
        const page = document.getElementById("checkoutPage");
        if (!page) return;
<<<<<<< HEAD
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

        document.getElementById("placeOrderButton").addEventListener("click", async () => {
            const fullName = document.getElementById("checkoutFullName");
            const phone = document.getElementById("checkoutPhone");
            const street = document.getElementById("checkoutStreet");
            const email = document.getElementById("checkoutEmail");
            const province = document.getElementById("checkoutProvince");
            const district = document.getElementById("checkoutDistrict");
            const ward = document.getElementById("checkoutWard");

            [fullName, phone, street].forEach(input => input.classList.toggle("is-invalid", !input.value.trim()));
            phone.classList.toggle("is-invalid", !isValidPhoneNumber(phone.value));

            if (!fullName.value.trim() || !street.value.trim() || !isValidPhoneNumber(phone.value)) {
                return showToast("Vui lòng nhập đầy đủ Tên, Số điện thoại và Địa chỉ chi tiết.", "warning");
            }
            [province, district, ward].forEach(input => input.classList.toggle("is-invalid", !input.value));
            if (!province.value || !district.value || !ward.value) {
                return showToast("Lỗi: Vui lòng chọn đầy đủ Tỉnh/Thành, Quận/Huyện và Phường/Xã để tính phí vận chuyển!", "danger");
            }

            const currentSelectedAddressId = document.getElementById("selectedAddressId")?.value || "";
            const currentCustomerId = document.getElementById("realKhachHangId")?.value || window.storeSession?.userId || "";

            if (!currentCustomerId) return showToast("Lỗi phiên đăng nhập.", "danger");

            const pOption = province.options[province.selectedIndex];
            const dOption = district.options[district.selectedIndex];
            const pName = pOption ? pOption.text : province.value;
            const dName = dOption ? dOption.text : district.value;

            const paymentMethodInput = document.querySelector("input[name='paymentMethod']:checked");
            const couponInput = document.getElementById("checkoutCouponInput");

            const requestData = {
                khachHangID: currentCustomerId,
                diaChiID: currentSelectedAddressId,
                tenKhachHang: fullName.value.trim(),
                soDienThoai: phone.value.replace(/\s+/g, ""),
                diaChiGiaoHang: `${street.value.trim()}, ${ward.value}, ${dName}, ${pName}`,
                phuongThucThanhToan: paymentMethodInput ? paymentMethodInput.value : "COD",
                maGiamGia: couponInput ? couponInput.value : "",
                items: getCartState().items.map(item => ({ chiTietSanPhamID: item.chiTietSanPhamID, soLuong: item.soLuong }))
            };

            const button = document.getElementById("placeOrderButton");
            try {
                button.disabled = true;
                button.innerHTML = '<span class="spinner-border spinner-border-sm me-2"></span> Đang xử lý...';

                // Gọi cổng kết nối API Đơn hàng công khai vừa xây dựng ở HomeController
                // Gọi cổng kết nối API Đơn hàng công khai vừa xây dựng ở HomeController (Đã đổi đường dẫn)
                const response = await fetch("/api/web-orders", { method: "POST", headers: { "Content-Type": "application/json" }, body: JSON.stringify(requestData) });
                const result = await response.json();

                if (!response.ok) return showToast(result.message || "Không thể tạo đơn hàng.", "danger");

                if (result.kieuThanhToan === 0) {
                    showToast(`Đặt hàng thành công! Mã đơn: ${result.hoaDonID}`, "success");
                    try { await clearCartOnServer(); } catch (e) { console.error(e); }
                    saveCartState(getEmptyCartState());
                    setTimeout(() => { window.location.href = "/"; }, 1500);
                }
                else if (result.kieuThanhToan === 1) {
                    document.getElementById("webPayOSIframe").src = result.checkoutUrl;
                    const qrModal = new bootstrap.Modal(document.getElementById('webQrModal'));
                    qrModal.show();

                    try { await clearCartOnServer(); } catch (e) { console.error(e); }
                    saveCartState(getEmptyCartState());

                    const checkInterval = setInterval(async () => {
                        try {
                            const checkRes = await fetch(`/api/admin/orders/${result.hoaDonID}`);
                            if (checkRes.ok) {
                                const orderDetail = await checkRes.json();
                                if (orderDetail.trangThaiKey === "confirmed" || orderDetail.trangThaiKey === "success") {
                                    clearInterval(checkInterval);
                                    qrModal.hide();
                                    showToast("Winter Shop đã nhận được tiền! Thanh toán đơn hàng thành công.", "success");
                                    setTimeout(() => { window.location.href = "/"; }, 2000);
                                }
                            }
                        } catch (err) { console.error("Lỗi dò trạng thái hóa đơn:", err); }
                    }, 3000);
                }
=======
        if (!isAuthenticated()) {
            redirectToLogin("Vui lòng đăng nhập để thanh toán.");
            return;
        }
        await refreshCartFromServer();
        if (!getCartState().items.length) {
            window.location.href = "/Home/Cart";
            return;
        }
        const serverProfile = window.checkoutBootstrap || {};
        const localProfile = getProfile();
        const profile = {
            fullName: serverProfile.fullName || localProfile.fullName || "",
            email: serverProfile.email || localProfile.email || "",
            phone: serverProfile.phone || localProfile.phone || "",
            street: serverProfile.street || localProfile.street || "",
            province: serverProfile.province || localProfile.province || "",
            district: serverProfile.district || localProfile.district || "",
            ward: serverProfile.ward || localProfile.ward || ""
        };
        const savedAddresses = Array.isArray(serverProfile.savedAddresses) ? serverProfile.savedAddresses : [];
        const fullName = document.getElementById("checkoutFullName");
        const email = document.getElementById("checkoutEmail");
        const phone = document.getElementById("checkoutPhone");
        const street = document.getElementById("checkoutStreet");
        const province = document.getElementById("checkoutProvince");
        const district = document.getElementById("checkoutDistrict");
        const ward = document.getElementById("checkoutWard");
        const selectedAddressId = document.getElementById("selectedAddressId");
        const savedAddressList = document.getElementById("savedAddressList");
        const useCustomAddressButton = document.getElementById("useCustomAddressButton");
        const shippingNode = document.getElementById("shippingOptions");
        const paymentNode = document.getElementById("paymentOptions");
        const couponInput = document.getElementById("checkoutCouponInput");
        const couponSuggestions = document.getElementById("couponSuggestions");
        const syncAddressSelection = addressId => {
            if (!savedAddressList) return;
            savedAddressList.querySelectorAll("[data-address-id]").forEach(card => {
                card.classList.toggle("active", card.dataset.addressId === addressId);
            });
        };
        const applyProfileToForm = nextProfile => {
            fullName.value = nextProfile.fullName || "";
            email.value = nextProfile.email || "";
            phone.value = nextProfile.phone || "";
            street.value = nextProfile.street || "";
            fillProvince(nextProfile.province, nextProfile.district, nextProfile.ward);
        };
        const applySavedAddress = address => {
            if (!address) return;
            if (selectedAddressId) selectedAddressId.value = address.id || "";
            applyProfileToForm({
                fullName: address.recipientName || profile.fullName,
                email: profile.email,
                phone: address.phone || "",
                street: address.street || "",
                province: address.province || "",
                district: address.district || "",
                ward: address.ward || ""
            });
            syncAddressSelection(address.id || "");
        };
        const clearSavedAddressSelection = () => {
            if (selectedAddressId) selectedAddressId.value = "";
            syncAddressSelection("");
        };
        const fillProvince = (preferredProvince, preferredDistrict, preferredWard) => {
            province.innerHTML = provinces.map(item => `<option value="${item.value}">${escapeHtml(item.label)}</option>`).join("");
            province.value = preferredProvince || provinces[0].value;
            fillDistrict(preferredDistrict, preferredWard);
        };
        const fillDistrict = (preferredDistrict, preferredWard) => {
            const currentProvince = provinces.find(item => item.value === province.value) || provinces[0];
            district.innerHTML = currentProvince.districts.map(item => `<option value="${item.value}">${escapeHtml(item.label)}</option>`).join("");
            district.value = preferredDistrict || currentProvince.districts[0].value;
            fillWard(preferredWard);
        };
        const fillWard = preferredWard => {
            const currentProvince = provinces.find(item => item.value === province.value) || provinces[0];
            const currentDistrict = currentProvince.districts.find(item => item.value === district.value) || currentProvince.districts[0];
            ward.innerHTML = currentDistrict.wards.map(item => `<option value="${item}">${escapeHtml(item)}</option>`).join("");
            ward.value = preferredWard || currentDistrict.wards[0];
        };
        const render = () => {
            const cart = getCartState();
            const totals = calculateTotals(cart);
            document.getElementById("checkoutItems").innerHTML = cart.items.map(item => `<article class="checkout-item"><div class="thumb-wrap">${buildArtwork({ sanPhamID: item.sanPhamID }, "compact")}</div><div><div class="product-name">${escapeHtml(item.tenSanPham)}</div><div class="item-meta">${escapeHtml(item.phanLoai)} · SL ${item.soLuong}</div></div><strong class="price-sale">${formatCurrency(item.donGia * item.soLuong)}</strong></article>`).join("");
            document.getElementById("checkoutSubtotal").textContent = formatCurrency(totals.subtotal);
            document.getElementById("checkoutShipping").textContent = formatCurrency(totals.shipping.fee);
            document.getElementById("checkoutDiscount").textContent = `- ${formatCurrency(totals.discount)}`;
            document.getElementById("checkoutGrandTotal").textContent = formatCurrency(totals.total);
            document.getElementById("checkoutSummaryMessage").textContent = totals.coupon ? `Đã áp dụng mã ${totals.coupon.code}.` : "Bạn có thể chọn mã giảm giá phù hợp trước khi hoàn tất.";
            couponInput.value = cart.couponCode;
            shippingNode.innerHTML = shippingMethods.map(item => {
                const disabled = item.minimum && totals.subtotal < item.minimum;
                const active = getShipping(cart, totals.subtotal).code === item.code;
                return `<label class="stack-option ${active ? "active" : ""} ${disabled ? "opacity-50" : ""}"><div class="stack-option-copy"><strong>${escapeHtml(item.label)}</strong><span>${escapeHtml(item.note)}</span></div><div><input type="radio" name="shippingMethod" value="${item.code}" ${active ? "checked" : ""} ${disabled ? "disabled" : ""} /><strong>${formatCurrency(item.fee)}</strong></div></label>`;
            }).join("");
            paymentNode.innerHTML = paymentMethods.map(item => `<label class="stack-option ${cart.paymentCode === item.code ? "active" : ""}"><div class="stack-option-copy"><strong>${escapeHtml(item.label)}</strong><span>${escapeHtml(item.note)}</span></div><input type="radio" name="paymentMethod" value="${item.code}" ${cart.paymentCode === item.code ? "checked" : ""} /></label>`).join("");
        };
        province.addEventListener("change", () => {
            fillDistrict();
            clearSavedAddressSelection();
        });
        district.addEventListener("change", () => {
            fillWard();
            clearSavedAddressSelection();
        });
        [fullName, email, phone, street, ward].forEach(input => input.addEventListener("input", clearSavedAddressSelection));
        if (savedAddressList) {
            savedAddressList.addEventListener("click", event => {
                const card = event.target.closest("[data-address-id]");
                if (!card) return;
                applySavedAddress({
                    id: card.dataset.addressId,
                    recipientName: card.dataset.recipient,
                    phone: card.dataset.phone,
                    street: card.dataset.street,
                    province: card.dataset.province,
                    district: card.dataset.district,
                    ward: card.dataset.ward
                });
            });
        }
        if (useCustomAddressButton) {
            useCustomAddressButton.addEventListener("click", () => {
                clearSavedAddressSelection();
                applyProfileToForm(profile);
                street.focus();
            });
        }
        shippingNode.addEventListener("change", event => { const input = event.target.closest("[name='shippingMethod']"); if (!input) return; const cart = getCartState(); cart.shippingCode = input.value; saveCartState(cart); });
        paymentNode.addEventListener("change", event => { const input = event.target.closest("[name='paymentMethod']"); if (!input) return; const cart = getCartState(); cart.paymentCode = input.value; saveCartState(cart); });
        couponSuggestions.addEventListener("click", event => { const button = event.target.closest("[data-code]"); if (!button) return; couponInput.value = button.dataset.code; const cart = getCartState(); cart.couponCode = button.dataset.code; saveCartState(cart); });
        document.getElementById("applyCouponButton").addEventListener("click", () => {
            const coupon = getCoupon(couponInput.value);
            if (!coupon) return showToast("Mã giảm giá không hợp lệ.", "warning");
            const cart = getCartState();
            cart.couponCode = coupon.code;
            saveCartState(cart);
            showToast(`Đã áp dụng mã ${coupon.code}.`, "success");
        });
        document.getElementById("placeOrderButton").addEventListener("click", async () => {
            [fullName, phone, street].forEach(input => input.classList.toggle("is-invalid", !input.value.trim()));
            phone.classList.toggle("is-invalid", !isValidPhoneNumber(phone.value));
            email.classList.toggle("is-invalid", !!email.value.trim() && !/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email.value.trim()));
            if (!fullName.value.trim() || !street.value.trim() || !isValidPhoneNumber(phone.value) || (email.value.trim() && !/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email.value.trim()))) return showToast("Vui lòng kiểm tra lại thông tin khách hàng.", "warning");
            const currentProvince = provinces.find(item => item.value === province.value) || provinces[0];
            const currentDistrict = currentProvince.districts.find(item => item.value === district.value) || currentProvince.districts[0];
            const requestData = {
                tenKhachHang: fullName.value.trim(),
                soDienThoai: phone.value.replace(/\s+/g, ""),
                diaChiGiaoHang: `${street.value.trim()}, ${ward.value}, ${currentDistrict.label}, ${currentProvince.label}`,
                items: getCartState().items.map(item => ({ chiTietSanPhamID: item.chiTietSanPhamID, soLuong: item.soLuong }))
            };
            saveProfile({ fullName: fullName.value.trim(), email: email.value.trim(), phone: phone.value.trim(), street: street.value.trim(), province: province.value, district: district.value, ward: ward.value });
            const button = document.getElementById("placeOrderButton");
            try {
                button.disabled = true;
                button.textContent = "Đang xử lý...";
                const response = await fetch("/api/orders", { method: "POST", headers: { "Content-Type": "application/json" }, body: JSON.stringify(requestData) });
                const result = await response.json();
                if (!response.ok) return showToast(result.message || "Không thể tạo đơn hàng.", "danger");
                showToast(`Đặt hàng thành công. Mã đơn: ${result.hoaDonID}`, "success");
                try {
                    await clearCartOnServer();
                } catch (clearError) {
                    console.error(clearError);
                }
                saveCartState(getEmptyCartState());
                setTimeout(() => { window.location.href = "/"; }, 1200);
>>>>>>> b2f0504c96bc3608d57fc3dc336ee4e756b36ed4
            } catch (error) {
                console.error(error);
                showToast("Không thể kết nối tới máy chủ.", "danger");
            } finally {
                button.disabled = false;
                button.textContent = "Hoàn tất đơn hàng";
            }
        });
<<<<<<< HEAD
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
    window.winterCart = { getCartState, getTotalQuantity, addItemToCart, showToast };
})();
=======
        couponSuggestions.innerHTML = coupons.map(item => `<button type="button" class="coupon-chip" data-code="${item.code}">${item.code} · ${item.label}</button>`).join("");
        applyProfileToForm(profile);
        if (serverProfile.selectedAddressId) {
            syncAddressSelection(serverProfile.selectedAddressId);
        }
        document.addEventListener("winterCartChanged", render);
        render();
    }

    const productsPromise = fetchProducts().catch(error => {
        console.error(error);
        return [];
    });

    initHeader(productsPromise);
    initHome(productsPromise);
    initProducts(productsPromise);
    initDetail(productsPromise);
    initCart();
    initCheckout();
    updateCartPreview();
    if (isAuthenticated()) {
        refreshCartFromServer().catch(error => {
            console.error(error);
        });
    }
    document.addEventListener("winterCartChanged", event => updateCartPreview(event.detail));

    window.winterStore = { escapeHtml, formatCurrency, fetchProducts, fetchProduct, buildArtwork, getProductMeta: getMeta, renderProductCard };
    window.winterCart = { getCartState, getTotalQuantity, getSubtotal, calculateTotals, addItemToCart, formatCurrency, showToast, isValidPhoneNumber };
})();
>>>>>>> b2f0504c96bc3608d57fc3dc336ee4e756b36ed4
