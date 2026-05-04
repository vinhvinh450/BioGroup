/* =============================================
   auth.js — Đăng nhập / Đăng ký
   BioWraps Vietnam Shop
   ============================================= */

const AUTH_API_URL = window.AUTH_API_URL;
const ORDERS_API_URL = window.ORDERS_API_URL;
const PAYMENTS_API_URL = window.PAYMENTS_API_URL;

// Helper dùng trước khi _isTokenExpired được khai báo đầy đủ
function _isTokenExpiredRaw(token) {
  try {
    const payload = JSON.parse(atob(token.split('.')[1]));
    return payload.exp * 1000 < Date.now();
  } catch { return true; }
}

// Khôi phục trạng thái đăng nhập từ localStorage khi tải lại trang
const _initToken = localStorage.getItem('token');
window.isLoggedIn = !!(_initToken && !_isTokenExpiredRaw(_initToken));

/* ---- OPEN / CLOSE MODAL ---- */
function openLogin() {
  const overlay = document.getElementById('loginOverlay');
  if (overlay) overlay.classList.add('show');
}

function closeLogin() {
  const overlay = document.getElementById('loginOverlay');
  if (overlay) overlay.classList.remove('show');
}

/* ---- CHUYỂN TAB ---- */
function switchTab(tab) {
  const isLogin = (tab === 'login');

  document.getElementById('tabLogin').classList.toggle('active', isLogin);
  document.getElementById('tabReg').classList.toggle('active', !isLogin);
  document.getElementById('formLogin').style.display = isLogin ? 'block' : 'none';
  document.getElementById('formReg').style.display   = isLogin ? 'none'  : 'block';
}

/* ---- ĐĂNG NHẬP ---- */
async function doRegister() {
  const name    = document.getElementById('regName')?.value?.trim();
  const email   = document.getElementById('regEmail')?.value?.trim();
  const phone   = document.getElementById('regPhone')?.value?.trim();
  const pass    = document.getElementById('regPass')?.value?.trim();
  const confirm = document.getElementById('regConfirm')?.value?.trim();

  if (!name || !email || !phone || !pass || !confirm) {
    showToast('⚠️ Vui lòng nhập đầy đủ thông tin'); return;
  }
  if (pass !== confirm) {
    showToast('⚠️ Mật khẩu xác nhận không khớp'); return;
  }

  try {
    const res = await fetch(window.AUTH_API_URL + '/register', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ name, email, phone, password: pass })
    });

    const data = await res.json();
    if (!res.ok) { showToast('❌ ' + (data.message || 'Đăng ký thất bại')); return; }

    localStorage.setItem('token', data.token); // Lưu JWT token
    localStorage.setItem('user', JSON.stringify(data.user)); // Lưu thông tin user
    _setLoggedIn(data.user.name);
    closeLogin();
    showToast('🎉 Tạo tài khoản thành công!');
  } catch (e) {
    showToast('❌ Không thể kết nối server!');
  }
}

async function doLogin() {
  const email = document.getElementById('loginEmail')?.value?.trim();
  const pass  = document.getElementById('loginPass')?.value?.trim();

  if (!email || !pass) {
    showToast('⚠️ Vui lòng nhập đầy đủ thông tin'); return;
  }

  try {
    const res = await fetch(window.AUTH_API_URL + '/login', {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify({ email, password: pass })
    });

    const data = await res.json();
    if (!res.ok) { showToast('❌ ' + (data.message || 'Đăng nhập thất bại')); return; }

    localStorage.setItem('token', data.token);
    localStorage.setItem('user', JSON.stringify(data.user));
    _setLoggedIn(data.user.name);
    closeLogin();
    showToast('✅ Đăng nhập thành công!');

    const redirect = sessionStorage.getItem('authRedirect');
    if (redirect) {
      sessionStorage.removeItem('authRedirect');
      setTimeout(() => { window.location.href = redirect; }, 800);
    }
  } catch (e) {
    showToast('❌ Không thể kết nối server!');
  }
}
/* ---- ĐĂNG NHẬP GOOGLE (Demo) ---- */
function doGoogleLogin() {
  /* TODO: Tích hợp Google OAuth thực tế */
  _setLoggedIn('Google User');
  closeLogin();
  showToast('✅ Đăng nhập bằng Google thành công!');
}

/* ---- ĐĂNG XUẤT ---- */
function doLogout() {
  window.isLoggedIn = false;
  localStorage.removeItem('token');
  localStorage.removeItem('user');
  const userIcon  = document.getElementById('userIcon');
  const userLabel = document.getElementById('userLabel');
  if (userIcon) {
    userIcon.classList.remove('logged-in');
    userIcon.innerHTML = `<svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M20 21v-2a4 4 0 0 0-4-4H8a4 4 0 0 0-4 4v2"/><circle cx="12" cy="7" r="4"/></svg>`;
  }
  if (userLabel) userLabel.textContent = 'Tài khoản';
  showToast('👋 Đã đăng xuất');
}

/* ---- INTERNAL: cập nhật UI sau khi login ---- */
function _setLoggedIn(displayName) {
  window.isLoggedIn = true;
  const initial   = displayName.trim()[0]?.toUpperCase() || 'U';
  const shortName = displayName.split(' ').pop().substring(0, 10);
  const userIcon  = document.getElementById('userIcon');
  const userLabel = document.getElementById('userLabel');
  if (userIcon) {
    userIcon.classList.add('logged-in');
    userIcon.textContent = initial;
  }
  if (userLabel) userLabel.textContent = shortName;
}

/* ---- ĐÓNG MODAL KHI CLICK NGOÀI ---- */
document.addEventListener('DOMContentLoaded', () => {
  const loginOverlay = document.getElementById('loginOverlay');
  if (loginOverlay) {
    loginOverlay.addEventListener('click', function(e) {
      if (e.target === this) closeLogin();
    });
  }
});
function _isTokenExpired(token) {
  return _isTokenExpiredRaw(token);
}

function goToProfile() {
  const token = localStorage.getItem('token');
  if (token && !_isTokenExpired(token)) {
    window.location.href = 'components/profile.html';
  } else {
    if (token) {
      localStorage.removeItem('token');
      localStorage.removeItem('user');
    }
    openLogin();
  }
}

function goToAdmin() {
  const user = JSON.parse(localStorage.getItem('user') || '{}');
  if (user.role === 'Admin') {
    window.location.href = 'components/admin.html';
  } else {
    showToast('❌ Bạn không có quyền truy cập trang admin');
  }
}