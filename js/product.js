/* =============================================
   products.js — Render danh sách sản phẩm
   BioWraps Vietnam Shop
   ============================================= */

/* ---- TỶ GIÁ USD/VND ---- */
let USD_RATE = null; // null = chưa tải xong

async function fetchExchangeRate() {
  try {
    const res = await fetch('https://open.er-api.com/v6/latest/USD');
    if (!res.ok) throw new Error();
    const data = await res.json();
    USD_RATE = data.rates?.VND || null;
  } catch {
    // Fallback: tỷ giá tham khảo nếu API lỗi
    USD_RATE = 25400;
    console.warn('Không lấy được tỷ giá thực tế, dùng tỷ giá dự phòng 25,400');
  }
}

function _fmtVND(amount) {
  return new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(amount);
}

function _fmtUSD(amount) {
  return new Intl.NumberFormat('en-US', { style: 'currency', currency: 'USD', minimumFractionDigits: 2 }).format(amount);
}

/**
 * Format giá hiển thị — hỗ trợ cả giá KM lẫn giá gốc gạch ngang
 */
function formatDualPrice(priceVND, originalPriceVND) {
  if (!priceVND && priceVND !== 0) return '<span class="price-contact">Liên hệ báo giá</span>';

  const saleStr = _fmtVND(priceVND);
  const usdPart = USD_RATE
    ? `<span class="price-sep">·</span><span class="price-usd">${_fmtUSD(priceVND / USD_RATE)}</span>`
    : `<span class="price-sep">·</span><span class="price-usd price-loading">$…</span>`;

  if (originalPriceVND && originalPriceVND > priceVND) {
    const origStr = _fmtVND(originalPriceVND);
    return `
      <span class="price-original">${origStr}</span>
      <span class="price-vnd price-sale">${saleStr}</span>${usdPart}`;
  }

  return `<span class="price-vnd">${saleStr}</span>${usdPart}`;
}

/**
 * Tạo HTML cho 1 product card
 */
function makeProductCard(product) {
  const saleBadge = product.discountPercent > 0
    ? `<div class="prod-badge badge-sale">-${product.discountPercent}%</div>`
    : `<div class="prod-badge ${product.badgeClass}">${product.badge}</div>`;
  const detailUrl = `product-detail.html?id=${product.id}`;
  return `
    <div class="product-card" onclick="window.location.href='${detailUrl}'" style="cursor:pointer">
      <div class="product-img">
        <img
          src="${product.img}"
          alt="${product.name}"
          onerror="this.style.display='none'; this.nextElementSibling.style.display='flex'"
        />
        <div class="pimg-fallback">${product.icon}</div>
        ${saleBadge}
      </div>
      <div class="product-info">
        <div class="product-cat">${product.cat}</div>
        <div class="product-name" title="${product.name}">${product.name}</div>
        <div class="product-price dual-price">${formatDualPrice(product.price, product.originalPrice)}</div>
        <div class="product-stock ${product.stock <= 0 ? 'out-of-stock' : product.stock <= 10 ? 'low-stock' : ''}">
          ${product.stock <= 0 ? 'Hết hàng' : product.stock <= 10 ? `Còn ${product.stock} sản phẩm` : `Còn ${product.stock} sản phẩm`}
        </div>
        <button class="add-cart-btn"
          onclick="event.stopPropagation(); addToCart(${JSON.stringify(product.id)})"
          ${product.stock <= 0 ? 'disabled' : ''}>
          ${product.stock <= 0 ? 'Hết hàng' : '+ Thêm vào giỏ hàng'}
        </button>
      </div>
    </div>
  `;
}

/**
 * Render bộ lọc sản phẩm
 */
function renderProductFilters() {
  const container = document.getElementById('productFilters');
  if (!container) return;

  const categories = [...new Set(PRODUCTS.map(p => p.cat))];
  container.innerHTML = `
    <button class="filter-btn active" onclick="filterProducts('all', this)">Tất cả</button>
    ${categories.map(cat => `<button class="filter-btn" onclick="filterProducts('${cat}', this)">${cat}</button>`).join('')}
  `;
}

/**
 * Lọc sản phẩm theo category
 */
function filterProducts(category, btn) {
  const container = document.getElementById('allProducts');
  if (!container) return;

  document.querySelectorAll('.filter-btn').forEach(b => b.classList.remove('active'));
  if (btn) btn.classList.add('active');

  const filtered = category === 'all' ? PRODUCTS : PRODUCTS.filter(p => p.cat === category);
  container.innerHTML = filtered.map(makeProductCard).join('');
  showToast(`🔍 Hiển thị ${filtered.length} sản phẩm${category === 'all' ? '' : ' trong "' + category + '"'}`);
}

/**
 * Render toàn bộ sản phẩm
 */
function renderAllProducts() {
  renderProductFilters();
  const container = document.getElementById('allProducts');
  if (!container) return;
  container.innerHTML = PRODUCTS.map(makeProductCard).join('');
}

/**
 * Render sản phẩm nổi bật
 */
function renderFeaturedProducts() {
  const container = document.getElementById('featProducts');
  if (!container) return;
  container.innerHTML = PRODUCTS.filter(p => p.featured).map(makeProductCard).join('');
}

/**
 * Tìm kiếm sản phẩm
 */
function searchProducts(keyword) {
  const kw = keyword.trim().toLowerCase();
  if (!kw) { renderAllProducts(); return; }

  const container = document.getElementById('allProducts');
  if (!container) return;

  const filtered = PRODUCTS.filter(p =>
    p.name.toLowerCase().includes(kw) ||
    p.cat.toLowerCase().includes(kw) ||
    (p.desc || '').toLowerCase().includes(kw)
  );

  container.innerHTML = filtered.length === 0
    ? `<div style="grid-column:1/-1;text-align:center;padding:40px;color:#888">
         <div style="font-size:40px;margin-bottom:12px">🔍</div>
         <div style="font-size:15px;font-weight:600">Không tìm thấy sản phẩm nào</div>
         <div style="font-size:12px;margin-top:6px">Thử từ khóa khác như "túi", "màng", "nhãn"...</div>
       </div>`
    : filtered.map(makeProductCard).join('');

  showToast(`🔍 Tìm thấy ${filtered.length} sản phẩm cho "${keyword}"`);
}

/* =============================================
   KHỞI TẠO — Được xử lý trong index.html
   ============================================= */
// Dữ liệu sẽ được load qua index.html script
