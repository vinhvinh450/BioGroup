# 🌿 Bio Shop — BioWraps Vietnam

Trang web thương mại điện tử theo phong cách AliExpress cho sản phẩm sinh học BioWraps Vietnam.

---

## 📁 Cấu trúc thư mục

```
bio_shop/
│
├── index.html                  ← Trang chủ chính
│
├── css/                        ← Toàn bộ style CSS
│   ├── base.css                ← Reset, biến màu, typography, responsive
│   ├── header.css              ← Topbar, logo, search, nav
│   ├── footer.css              ← Footer layout
│   ├── modal.css               ← Popup đăng nhập / đăng ký
│   ├── cart.css                ← Panel giỏ hàng
│   └── products.css            ← Hero, danh mục, sản phẩm, banner
│
├── js/                         ← Toàn bộ logic JavaScript
│   ├── data.js                 ← Dữ liệu sản phẩm (chỉnh sửa tại đây)
│   ├── main.js                 ← Khởi tạo, toast, search, utils
│   ├── auth.js                 ← Đăng nhập / đăng ký / đăng xuất
│   ├── cart.js                 ← Thêm/xóa/cập nhật giỏ hàng
│   └── products.js             ← Render danh sách & tìm kiếm sản phẩm
│
└── components/                 ← HTML tái sử dụng (load qua fetch)
    ├── header.html             ← Topbar + Header + Nav
    ├── footer.html             ← Footer
    ├── modal.html              ← Modal đăng nhập/đăng ký
    └── cart.html               ← Panel giỏ hàng
```

---

## 🚀 Cách chạy

**⚠️ Quan trọng:** Do dùng `fetch()` để load components, bạn cần chạy qua server:

### Option 1 — VS Code (khuyên dùng)
Cài extension **Live Server** → Click chuột phải `index.html` → *Open with Live Server*

### Option 2 — Python
```bash
cd bio_shop
python -m http.server 8000
# Mở: http://localhost:8000
```

### Option 3 — Node.js
```bash
npx serve bio_shop
```

---

## ✏️ Cách chỉnh sửa

| Muốn thay đổi gì | Sửa file |
|---|---|
| Thêm / xóa / sửa sản phẩm | `js/data.js` |
| Màu sắc thương hiệu | `css/base.css` (phần `:root`) |
| Logo, menu nav | `components/header.html` |
| Thông tin footer | `components/footer.html` |
| Logic đăng nhập (kết nối API) | `js/auth.js` → hàm `doLogin()` |
| Logic thanh toán (kết nối API) | `js/cart.js` → hàm `doCheckout()` |
| Bố cục hero, banner, sections | `index.html` + `css/products.css` |

---

## 🔧 TODO — Tính năng có thể phát triển thêm

- [ ] Kết nối backend API (Node.js / PHP / Firebase)
- [ ] Xác thực Google OAuth thực tế
- [ ] Trang chi tiết sản phẩm (`product-detail.html`)
- [ ] Trang thanh toán (`checkout.html`)
- [ ] Lưu giỏ hàng vào `localStorage`
- [ ] Tích hợp cổng thanh toán (VNPay, MoMo, ZaloPay)
- [ ] Bộ lọc sản phẩm theo danh mục

---

## 📞 Liên hệ BioWraps Vietnam

- 🌐 [biowrapsvietnam.com](https://www.biowrapsvietnam.com)
- 📱 0901 234 567
- ✉️ info@biowrapsvietnam.com