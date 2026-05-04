/* =============================================
   cart.js — Giỏ hàng + Checkout (CLEAN VERSION)
   BioWraps Vietnam Shop
   ============================================= */

let cart = [];

const ORDERS_API = window.ORDERS_API_URL || 'http://localhost:5207/api/orders';

/* ---- OPEN / CLOSE CART ---- */
function openCart() {
  document.getElementById('cartPanel').classList.add('show');
}

function closeCart() {
  document.getElementById('cartPanel').classList.remove('show');
}

/* ---- THÊM SẢN PHẨM ---- */
function addToCart(productId) {
  const product = PRODUCTS.find(p => p.id === productId);
  if (!product) {
    showToast('❌ Không tìm thấy sản phẩm');
    return;
  }

  // Kiểm tra tồn kho
  if (product.stock <= 0) {
    showToast('❌ Sản phẩm đã hết hàng');
    return;
  }

  const existing = cart.find(item => item.id === productId);

  if (existing) {
    // Kiểm tra không vượt quá tồn kho
    if (existing.qty >= product.stock) {
      showToast(`❌ Chỉ còn ${product.stock} sản phẩm trong kho`);
      return;
    }
    existing.qty++;
  } else {
    cart.push({ ...product, qty: 1 });
  }

  renderCart();
  showToast(`✅ Đã thêm "${product.name.substring(0, 28)}..." vào giỏ!`);
}

/* ---- THAY ĐỔI SỐ LƯỢNG ---- */
function changeQty(productId, delta) {
  const item = cart.find(i => i.id === productId);
  if (!item) return;

  item.qty += delta;

  if (item.qty <= 0) {
    cart = cart.filter(i => i.id !== productId);
  }

  renderCart();
}

/* ---- XÓA SẢN PHẨM ---- */
function removeFromCart(productId) {
  cart = cart.filter(i => i.id !== productId);
  renderCart();
  showToast('🗑️ Đã xóa sản phẩm khỏi giỏ hàng');
}

/* =============================================
   CHECKOUT
   ============================================= */
function doCheckout() {
  const token = localStorage.getItem('token');
  const loggedIn = token && typeof _isTokenExpired === 'function' && !_isTokenExpired(token);
  if (!loggedIn) {
    if (token) { localStorage.removeItem('token'); localStorage.removeItem('user'); }
    closeCart();
    openLogin();
    showToast('⚠️ Vui lòng đăng nhập để tiếp tục');
    return;
  }

  if (cart.length === 0) {
    showToast('⚠️ Giỏ hàng đang trống!');
    return;
  }

  openCheckoutModal();
}

function openCheckoutModal() {
  const total = cart.reduce((sum, i) => sum + (i.price || 0) * i.qty, 0);

  document.getElementById('co-items').innerHTML = cart.map(item => `
    <div class="co-item">
      <span class="co-item-name">${item.icon || '📦'} ${item.name}</span>
      <span class="co-item-meta">x${item.qty} · ${fmt((item.price || 0) * item.qty)}</span>
    </div>
  `).join('');

  document.getElementById('co-total').textContent = fmt(total);
  document.getElementById('co-error').style.display = 'none';
  document.getElementById('co-address').value = '';
  document.getElementById('co-note').value = '';

  selectPayment('COD');
  document.getElementById('checkoutModal').classList.add('show');
}

function closeCheckoutModal() {
  document.getElementById('checkoutModal').classList.remove('show');
}

/* ---- PAYMENT UI (COD / BANK QR ONLY) ---- */
function selectPayment(method) {
  document.querySelectorAll('.pm-option')
    .forEach(el => el.classList.remove('active'));

  const el = document.querySelector(`.pm-option[data-method="${method}"]`);
  if (el) el.classList.add('active');

  document.getElementById('bank-info').style.display =
    method === 'BankTransfer' ? 'block' : 'none';

  const total = cart.reduce((sum, i) => sum + (i.price || 0) * i.qty, 0);

  if (method === 'BankTransfer') {
    generateVietQR(total);
  }
}

function generateVietQR(amount) {
  const bankId = 'techcombank';
  const accountNo = '4548301004';
  const accountName = encodeURIComponent('NGUYEN KHANH VINH');

  const addInfo = encodeURIComponent('Thanh toan BioWraps');

  const url = `https://img.vietqr.io/image/${bankId}-${accountNo}-qr_only.png?amount=${amount}&addInfo=${addInfo}&accountName=${accountName}`;

  const img = document.getElementById('bank-qr-img');
  const loading = document.getElementById('bank-qr-loading');

  img.style.display = 'none';
  loading.style.display = 'block';
  loading.textContent = '⏳ Đang tạo QR...';

  img.onload = () => {
    loading.style.display = 'none';
    img.style.display = 'block';
  };

  img.onerror = () => {
    loading.textContent = '❌ Không tải được QR';
  };

  img.src = url;
}

function getSelectedPayment() {
  const el = document.querySelector('.pm-option.active');
  return el ? el.dataset.method : 'COD';
}

/* =============================================
   SUBMIT ORDER (NO PAYMENT API)
   ============================================= */
async function submitOrder() {
  const phone = document.getElementById('co-phone').value.trim();
  const address = document.getElementById('co-address').value.trim();
  const note = document.getElementById('co-note').value.trim();
  const method = getSelectedPayment();
  const token = localStorage.getItem('token');
  const errEl = document.getElementById('co-error');

  if (!phone) {
    errEl.textContent = '⚠️ Vui lòng nhập số điện thoại';
    errEl.style.display = 'block';
    return;
  }

  if (!address) {
    errEl.textContent = '⚠️ Vui lòng nhập địa chỉ giao hàng';
    errEl.style.display = 'block';
    return;
  }

  const btn = document.getElementById('co-submit-btn');
  btn.disabled = true;
  btn.innerHTML = '⏳ Đang xử lý...';

  try {
    const orderRes = await fetch(ORDERS_API, {
      method: 'POST',
      headers: {
        'Content-Type': 'application/json',
        'Authorization': 'Bearer ' + token
      },
      body: JSON.stringify({
        items: cart.map(i => ({
          productId: i.id,
          quantity: i.qty
        })),
        shippingAddress: address,
        phone: phone,
        notes: note
      })
    });

    if (!orderRes.ok) {
      const e = await orderRes.json().catch(() => ({}));
      throw new Error(e.message || 'Tạo đơn hàng thất bại');
    }

    const order = await orderRes.json();

    cart = [];
    renderCart();
    closeCheckoutModal();
    closeCart();

    showOrderSuccess(order.id, method);

  } catch (e) {
    errEl.textContent = '❌ ' + e.message;
    errEl.style.display = 'block';
  } finally {
    btn.disabled = false;
    btn.innerHTML = '✅ Xác nhận đặt hàng';
  }
}

/* ---- SUCCESS ---- */
function showOrderSuccess(orderId, method) {
  const label = {
    COD: '💵 COD',
    BankTransfer: '🏦 Chuyển khoản'
  }[method] || method;

  document.getElementById('success-order-id').textContent = '#' + orderId;
  document.getElementById('success-method').textContent = label;

  document.getElementById('orderSuccessModal').classList.add('show');
}

function closeSuccessModal() {
  document.getElementById('orderSuccessModal').classList.remove('show');
}

/* ---- FORMAT ---- */
function fmt(amount) {
  return new Intl.NumberFormat('vi-VN', {
    style: 'currency',
    currency: 'VND'
  }).format(amount);
}

/* ---- RENDER CART ---- */
function renderCart() {
  updateBadge();
  renderCartBody();
  renderCartTotal();
}

function updateBadge() {
  const total = cart.reduce((sum, i) => sum + i.qty, 0);
  const badge = document.getElementById('cartBadge');

  if (!badge) return;

  badge.style.display = total > 0 ? 'flex' : 'none';
  badge.textContent = total;
}

function renderCartBody() {
  const body = document.getElementById('cartBody');
  if (!body) return;

  if (cart.length === 0) {
    body.innerHTML = `
      <div class="cart-empty">
        <span class="ce-icon">🌿</span>
        <div class="ce-title">Giỏ hàng trống</div>
        <div>Hãy thêm sản phẩm xanh vào giỏ!</div>
      </div>
    `;
    return;
  }

  body.innerHTML = cart.map(item => `
    <div class="cart-item">
      <div class="ci-img">
        <img src="${item.img}" />
      </div>
      <div class="ci-info">
        <div class="ci-name">${item.name}</div>
        <div class="ci-price">${fmt(item.price || 0)}</div>
        <div class="ci-qty">
          <button onclick="changeQty(${item.id}, -1)">−</button>
          <span>${item.qty}</span>
          <button onclick="changeQty(${item.id}, 1)">+</button>
          <button onclick="removeFromCart(${item.id})">🗑</button>
        </div>
      </div>
    </div>
  `).join('');
}

function renderCartTotal() {
  const totalEl = document.getElementById('cartTotal');

  const totalQty = cart.reduce((sum, i) => sum + i.qty, 0);
  const totalAmount = cart.reduce(
    (sum, i) => sum + (i.price || 0) * i.qty,
    0
  );

  totalEl.textContent =
    totalQty > 0
      ? `${totalQty} sản phẩm · ${fmt(totalAmount)}`
      : '0 sản phẩm';
}