/* =============================================
   product-detail.js — Chi tiết sản phẩm
   BioWraps Vietnam Shop
   ============================================= */

let _pdProduct = null;

/* =============================================
   LOAD & RENDER
   ============================================= */
async function loadProductDetail() {
  const params = new URLSearchParams(window.location.search);
  const rawId = params.get('id');

  if (!rawId) { showPdError(); return; }

  // Tìm trong PRODUCTS đã tải (ưu tiên, tránh gọi API thêm)
  let product = PRODUCTS.find(p => String(p.id) === String(rawId));

  // Fallback: gọi API cho sản phẩm thường (id là số nguyên)
  if (!product && !rawId.startsWith('dist_')) {
    try {
      const base = (window.API_URL || '').replace('/api/products', '');
      const res = await fetch(`${base}/api/products/${rawId}`);
      if (res.ok) {
        const raw = await res.json();
        product = _mapRawProduct(raw);
      }
    } catch (e) {
      console.error('Lỗi fetch product detail:', e);
    }
  }

  if (!product) { showPdError(); return; }

  _pdProduct = product;
  document.title = `${product.name} — BioWraps Vietnam`;

  renderProductDetail(product);
  loadRelatedProducts(product.cat, product.id);

  document.getElementById('pdLoading').style.display = 'none';
  document.getElementById('pdContent').style.display = 'block';
}

function _mapRawProduct(raw) {
  const now = new Date();
  const discountActive = raw.discountPercent > 0 &&
    (!raw.discountEndDate || new Date(raw.discountEndDate) > now);
  const discountPercent = discountActive ? raw.discountPercent : 0;
  const originalPrice = raw.price;
  const salePrice = discountPercent > 0
    ? Math.round(originalPrice * (1 - discountPercent / 100))
    : originalPrice;
  return {
    id:              raw.id,
    name:            raw.name,
    cat:             raw.category,
    badge:           raw.badge,
    badgeClass:      raw.badgeClass,
    icon:            raw.icon,
    img:             raw.imageUrl,
    desc:            raw.description,
    price:           salePrice,
    originalPrice:   discountPercent > 0 ? originalPrice : null,
    discountPercent,
    discountEndDate: raw.discountEndDate,
    stock:           raw.stock || 0,
    featured:        raw.isFeatured,
  };
}

function renderProductDetail(p) {
  // Breadcrumb
  document.getElementById('bc-category').textContent = p.cat;
  document.getElementById('bc-name').textContent = p.name;

  // Ảnh chính — reset display trước (src="" ban đầu có thể trigger onerror)
  const mainImg = document.getElementById('pdMainImg');
  const fallback = document.getElementById('pdImgFallback');
  mainImg.style.display = '';
  fallback.style.display = 'none';
  fallback.textContent = p.icon || '📦';

  if (p.img) {
    mainImg.alt = p.name;
    mainImg.src = p.img;
  } else {
    mainImg.style.display = 'none';
    fallback.style.display = 'flex';
  }

  // Badge
  const badgeEl = document.getElementById('pdBadge');
  if (p.discountPercent > 0) {
    badgeEl.textContent = `-${p.discountPercent}%`;
    badgeEl.className = 'pd-badge badge-sale';
  } else if (p.badge) {
    badgeEl.textContent = p.badge;
    badgeEl.className = `pd-badge ${p.badgeClass || ''}`;
  } else {
    badgeEl.style.display = 'none';
  }

  // Thumbnail — chỉ ảnh chính vì API chưa trả về nhiều ảnh
  const thumbsEl = document.getElementById('pdThumbs');
  if (p.img) {
    thumbsEl.innerHTML = `
      <div class="pd-thumb active">
        <img src="${p.img}" alt="${p.name}" />
      </div>`;
  } else {
    thumbsEl.innerHTML = `
      <div class="pd-thumb active">
        <span class="pd-thumb-icon">${p.icon || '📦'}</span>
      </div>`;
  }

  // Danh mục & tên
  document.getElementById('pdCat').textContent = p.cat;
  document.getElementById('pdName').textContent = p.name;

  // Giá
  const priceRow = document.getElementById('pdPriceRow');
  priceRow.innerHTML = formatDualPrice(p.price, p.originalPrice);
  if (p.discountEndDate) {
    const endDate = new Date(p.discountEndDate);
    if (endDate > new Date()) {
      const days = Math.ceil((endDate - new Date()) / 86400000);
      priceRow.innerHTML += `<div class="pd-discount-expiry">⏱️ Ưu đãi còn ${days} ngày</div>`;
    }
  }

  // Tồn kho
  const stockEl = document.getElementById('pdStockRow');
  if (p.stock <= 0) {
    stockEl.innerHTML = `<span class="pd-stock out">❌ Hết hàng</span>`;
  } else if (p.stock <= 10) {
    stockEl.innerHTML = `<span class="pd-stock low">⚠️ Còn ${p.stock} sản phẩm — Sắp hết!</span>`;
  } else {
    stockEl.innerHTML = `<span class="pd-stock ok">✅ Còn hàng (${p.stock} sản phẩm)</span>`;
  }

  // Mô tả ngắn
  document.getElementById('pdDesc').textContent =
    p.desc || 'Sản phẩm bao bì sinh học cao cấp từ BioWraps Vietnam.';

  // Số lượng tối đa
  document.getElementById('pdQtyMax').textContent =
    p.stock > 0 ? `Tối đa: ${p.stock}` : '';
  const qtyInput = document.getElementById('pdQtyInput');
  qtyInput.max = p.stock;
  if (p.stock <= 0) qtyInput.value = 0;

  // Nút hành động
  const btnCart = document.getElementById('pdBtnCart');
  const btnBuy  = document.getElementById('pdBtnBuy');
  if (p.stock <= 0) {
    btnCart.disabled = true;
    btnBuy.disabled  = true;
    btnCart.innerHTML = '❌ Hết hàng';
    btnBuy.innerHTML  = '❌ Hết hàng';
  }

  // Tab mô tả
  const desc = p.desc || 'Sản phẩm bao bì sinh học cao cấp từ BioWraps Vietnam.';
  document.getElementById('tab-desc').innerHTML = `
    <div class="pd-desc-full">
      <p>${desc}</p>
      <ul class="pd-desc-features">
        <li>🌿 Chất liệu hoàn toàn sinh học, tự phân hủy 100%</li>
        <li>🍊 Nguyên liệu từ vỏ cam tự nhiên — an toàn cho thực phẩm</li>
        <li>✅ Đạt chứng nhận an toàn thực phẩm quốc tế</li>
        <li>♻️ Giảm thiểu rác thải nhựa, bảo vệ môi trường</li>
        <li>🚚 Giao hàng toàn quốc, đóng gói kỹ lưỡng</li>
      </ul>
    </div>
  `;

  // Meta
  document.getElementById('pdMetaCat').textContent = p.cat;
  document.getElementById('pdMetaId').textContent  = `SP-${p.id}`;
}

/* =============================================
   LIÊN QUAN
   ============================================= */
async function loadRelatedProducts(category, currentId) {
  // Lấy từ PRODUCTS đã tải trước
  let related = PRODUCTS
    .filter(p => p.cat === category && String(p.id) !== String(currentId))
    .slice(0, 5);

  // Nếu chưa có, gọi API
  if (related.length === 0) {
    try {
      const base = (window.API_URL || '').replace('/api/products', '');
      const res  = await fetch(`${base}/api/products/category/${encodeURIComponent(category)}`);
      if (res.ok) {
        const data = await res.json();
        related = data
          .filter(p => p.id !== currentId && p.isActive !== false)
          .slice(0, 5)
          .map(_mapRawProduct);
      }
    } catch (e) { /* bỏ qua */ }
  }

  if (related.length === 0) return;

  document.getElementById('relatedSection').style.display = 'block';
  document.getElementById('relatedProducts').innerHTML =
    related.map(makeProductCard).join('');
}

/* =============================================
   SỐ LƯỢNG
   ============================================= */
function pdChangeQty(delta) {
  const input = document.getElementById('pdQtyInput');
  const max   = _pdProduct ? _pdProduct.stock : 999;
  let val = parseInt(input.value) || 1;
  val = Math.max(1, Math.min(val + delta, max));
  input.value = val;
}

/* =============================================
   THÊM GIỎ / MUA NGAY
   ============================================= */
function pdAddToCart() {
  if (!_pdProduct || _pdProduct.stock <= 0) return;
  const qty = Math.max(1, Math.min(
    parseInt(document.getElementById('pdQtyInput').value) || 1,
    _pdProduct.stock
  ));

  const existing = cart.find(i => String(i.id) === String(_pdProduct.id));
  if (existing) {
    existing.qty = Math.min(existing.qty + qty, _pdProduct.stock);
  } else {
    cart.push({ ..._pdProduct, qty });
  }

  renderCart();
  showToast(`✅ Đã thêm ${qty} "${_pdProduct.name.substring(0, 24)}..." vào giỏ!`);
}

function pdBuyNow() {
  pdAddToCart();
  openCart();
}

/* =============================================
   TABS
   ============================================= */
function switchTab(tabId, btn) {
  document.querySelectorAll('.pd-tab-content').forEach(el => el.style.display = 'none');
  document.querySelectorAll('.pd-tab-btn').forEach(el => el.classList.remove('active'));
  document.getElementById('tab-' + tabId).style.display = 'block';
  btn.classList.add('active');
}

/* =============================================
   ERROR STATE
   ============================================= */
function showPdError() {
  document.getElementById('pdLoading').style.display = 'none';
  document.getElementById('pdError').style.display   = 'block';
}
