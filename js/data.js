/* =============================================
   data.js — Dữ liệu sản phẩm BioWraps Vietnam
   Lấy từ API backend
   ============================================= */

let PRODUCTS = [];

/**
 * Lấy dữ liệu sản phẩm từ API
 */
async function loadProductsFromAPI() {
  try {
    const API_URL = window.API_URL;
    if (!API_URL) {
      throw new Error('API_URL chưa được cấu hình');
    }
    
    const response = await fetch(API_URL);
    if (!response.ok) throw new Error(`HTTP ${response.status}`);
    
    const data = await response.json();
    
    // Map API response để match với frontend structure
    const now = new Date();
    PRODUCTS = data
      .filter(p => p.isActive !== false)
      .map(p => {
        // Kiểm tra KM còn hạn không
        const discountActive = p.discountPercent > 0 &&
          (!p.discountEndDate || new Date(p.discountEndDate) > now);
        const discountPercent = discountActive ? p.discountPercent : 0;
        const originalPrice   = p.price;
        const salePrice       = discountPercent > 0
          ? Math.round(originalPrice * (1 - discountPercent / 100))
          : originalPrice;
        return {
          id:              p.id,
          name:            p.name,
          cat:             p.category,
          badge:           p.badge,
          badgeClass:      p.badgeClass,
          icon:            p.icon,
          img:             p.imageUrl,
          desc:            p.description,
          price:           salePrice,
          originalPrice:   discountPercent > 0 ? originalPrice : null,
          discountPercent: discountPercent,
          discountEndDate: p.discountEndDate,
          stock:           p.stock || 0,
          featured:        p.isFeatured,
        };
      });
    
    // Merge sản phẩm nhà phân phối đã được duyệt
    try {
      const distRes = await fetch((window.API_URL || '').replace('/api/products', '') + '/api/distributor/approved');
      if (distRes.ok) {
        const distProducts = await distRes.json();
        const mapped = distProducts.map(p => ({
          id:              'dist_' + p.id,
          name:            p.name,
          cat:             p.category,
          badge:           '🏪 Đối tác',
          badgeClass:      'badge-new',
          icon:            p.icon || '📦',
          img:             p.imageUrl || '',
          desc:            p.description,
          price:           p.price,
          originalPrice:   null,
          discountPercent: 0,
          stock:           p.stock,
          featured:        false,
          isDistributor:   true,
          distributorName: p.distributorName,
        }));
        PRODUCTS = [...PRODUCTS, ...mapped];
        console.log(`✅ Đã thêm ${mapped.length} sản phẩm từ nhà phân phối`);
      }
    } catch { /* bỏ qua nếu lỗi */ }

    console.log(`✅ Đã tải ${PRODUCTS.length} sản phẩm từ API`);
    return PRODUCTS;
  } catch (error) {
    console.error('❌ Lỗi khi tải dữ liệu từ API:', error);
    console.warn('⚠️ Sử dụng dữ liệu demo');
    
    // Fallback: Dữ liệu demo
    PRODUCTS = [
      {
        id: 1,
        name: 'Bao BioWraps Cơ Bản',
        cat: 'Bao Bì Thực Phẩm',
        badge: 'Bán chạy',
        badgeClass: 'badge-hot',
        icon: '📦',
        img: 'images/product1.jpg',
        desc: 'Bao bì sinh học tự phân hủy',
        price: 45000,
        stock: 150,
        featured: true
      },
      {
        id: 2,
        name: 'Màng FreshSense Pro',
        cat: 'Bio Wraps',
        badge: 'Mới',
        badgeClass: 'badge-new',
        icon: '🌿',
        img: 'images/product2.jpg',
        desc: 'Màng thông minh theo dõi độ tươi',
        price: 120000,
        stock: 75,
        featured: true
      }
    ];
    
    console.log(`⚠️ Đã tải ${PRODUCTS.length} sản phẩm demo`);
    return PRODUCTS;
  }
}
