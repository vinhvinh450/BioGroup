/* =============================================
   main.js — Khởi tạo & tiện ích chung
   BioWraps Vietnam Shop
   ============================================= */

// Global API URLs
const _BASE = 'http://localhost:5207';
window.API_URL          = _BASE + '/api/products';
window.AUTH_API_URL     = _BASE + '/api/auth';
window.ORDERS_API_URL   = _BASE + '/api/orders';
window.PAYMENTS_API_URL = _BASE + '/api/payments';
window.CHAT_API_URL     = _BASE + '/api/chat';

/* =============================================
   TOAST NOTIFICATION
   ============================================= */
let _toastTimer = null;

function showToast(message) {
  const toast = document.getElementById('toast');
  if (!toast) return;

  toast.textContent = message;
  toast.classList.add('show');

  clearTimeout(_toastTimer);
  _toastTimer = setTimeout(() => toast.classList.remove('show'), 2800);
}

/* =============================================
   SEARCH
   ============================================= */
function doSearch() {
  const input = document.getElementById('searchInput');
  if (!input) return;
  const keyword = input.value.trim();
  if (!keyword) return;

  const anchor = document.getElementById('productsAnchor');
  if (anchor) anchor.scrollIntoView({ behavior: 'smooth' });

  searchProducts(keyword);
}

/* =============================================
   NAV — Active state
   ============================================= */
function setNavActive(el) {
  document.querySelectorAll('.nav-item').forEach(i => i.classList.remove('active'));
  el.classList.add('active');
}

/* =============================================
   HERO BUTTON — Scroll to products
   ============================================= */
function scrollToProducts() {
  const anchor = document.getElementById('productsAnchor');
  if (anchor) anchor.scrollIntoView({ behavior: 'smooth' });
}

/* =============================================
   INIT
   ============================================= */
document.addEventListener('DOMContentLoaded', () => {
  const searchInput = document.getElementById('searchInput');
  if (searchInput) {
    searchInput.addEventListener('keydown', e => {
      if (e.key === 'Enter') doSearch();
    });
  }

  document.querySelectorAll('.nav-item').forEach(item => {
    item.addEventListener('click', function() { setNavActive(this); });
  });

  console.log('✅ Bio Shop — BioWraps Vietnam đã khởi động');
});

// ⚠️ Đã xóa: import("./order-tracking.js") — không dùng được trong script thường
// order-tracking.js được load bằng thẻ <script> trực tiếp trong HTML