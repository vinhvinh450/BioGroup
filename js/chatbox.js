/* =============================================
   chatbox.js — AI Chat Widget
   BioWraps Vietnam
   ============================================= */

(function () {
  const _imgBase = window.location.href.replace(/\/[^/]*(\?.*)?$/, '/');
  let history = []; // [{role, content}]
  let isOpen  = false;
  let isTyping = false;

  /* ---- Build widget HTML ---- */
  function _mount() {
    const wrap = document.createElement('div');
    wrap.innerHTML = `
      <!-- Toggle button -->
      <button class="chat-toggle" id="chatToggle" title="Chat tư vấn">
        <img src="${_imgBase}images/icon-chatbox.png" alt="Chat" onerror="this.style.display='none'" style="width:60px;height:60px;object-fit:cover;" />
        <span class="chat-badge" id="chatBadge">1</span>
      </button>

      <!-- Chat window -->
      <div class="chat-window" id="chatWindow">
        <div class="chat-header">
          <div class="chat-avatar">🤖</div>
          <div class="chat-header-info">
            <div class="chat-header-name">Trợ lý BioWraps</div>
            <div class="chat-header-status">Đang trực tuyến</div>
          </div>
          <button class="chat-close" id="chatClose" title="Đóng">
            <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" stroke-linecap="round">
              <line x1="18" y1="6" x2="6" y2="18"/><line x1="6" y1="6" x2="18" y2="18"/>
            </svg>
          </button>
        </div>

        <!-- Quick suggestions -->
        <div class="chat-suggestions" id="chatSuggestions">
          <button class="chat-chip" onclick="chatSend('Sản phẩm nào bán chạy nhất?')">🔥 Bán chạy nhất</button>
          <button class="chat-chip" onclick="chatSend('Có khuyến mãi gì không?')">🎁 Khuyến mãi</button>
          <button class="chat-chip" onclick="chatSend('Bao bì có tự phân hủy không?')">♻️ Thân thiện môi trường</button>
          <button class="chat-chip" onclick="chatSend('Đặt hàng sỉ như thế nào?')">📦 Đặt sỉ</button>
        </div>

        <div class="chat-messages" id="chatMessages"></div>

        <div class="chat-input-area">
          <textarea
            class="chat-input"
            id="chatInput"
            placeholder="Nhập câu hỏi..."
            rows="1"
          ></textarea>
          <button class="chat-send" id="chatSendBtn" title="Gửi">
            <svg viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2.5" stroke-linecap="round" stroke-linejoin="round">
              <line x1="22" y1="2" x2="11" y2="13"/>
              <polygon points="22 2 15 22 11 13 2 9 22 2"/>
            </svg>
          </button>
        </div>
      </div>
    `;
    document.body.appendChild(wrap);

    /* Events */
    document.getElementById('chatToggle').addEventListener('click', toggleChat);
    document.getElementById('chatClose').addEventListener('click', closeChat);
    document.getElementById('chatSendBtn').addEventListener('click', _sendFromInput);

    const input = document.getElementById('chatInput');
    input.addEventListener('keydown', e => {
      if (e.key === 'Enter' && !e.shiftKey) {
        e.preventDefault();
        _sendFromInput();
      }
    });
    input.addEventListener('input', () => {
      input.style.height = 'auto';
      input.style.height = Math.min(input.scrollHeight, 80) + 'px';
    });

    /* Hiện badge sau 2 giây để kích thích mở */
    setTimeout(() => {
      if (!isOpen) document.getElementById('chatBadge').classList.add('show');
    }, 2000);

    /* Tin nhắn chào mừng */
    _appendAI('Xin chào! Tôi là trợ lý tư vấn của BioWraps Vietnam 🌿\nBạn cần tư vấn về sản phẩm nào?');
  }

  /* ---- Toggle / Open / Close ---- */
  function toggleChat() {
    isOpen ? closeChat() : openChat();
  }

  function openChat() {
    isOpen = true;
    document.getElementById('chatWindow').classList.add('open');
    document.getElementById('chatBadge').classList.remove('show');
    setTimeout(() => document.getElementById('chatInput').focus(), 250);
  }

  function closeChat() {
    isOpen = false;
    document.getElementById('chatWindow').classList.remove('open');
  }

  /* Expose để chip có thể gọi */
  window.chatSend = function (text) {
    if (!isOpen) openChat();
    _send(text);
    const sugg = document.getElementById('chatSuggestions');
    if (sugg) sugg.style.display = 'none';
  };

  /* ---- Send helpers ---- */
  function _sendFromInput() {
    const input = document.getElementById('chatInput');
    const text  = input.value.trim();
    if (!text || isTyping) return;
    input.value = '';
    input.style.height = 'auto';
    const sugg = document.getElementById('chatSuggestions');
    if (sugg) sugg.style.display = 'none';
    _send(text);
  }

  async function _send(text) {
    if (isTyping) return;
    _appendUser(text);

    isTyping = true;
    document.getElementById('chatSendBtn').disabled = true;
    const typingEl = _showTyping();

    await new Promise(r => setTimeout(r, 600 + Math.random() * 400));

    const reply = _getRuleBasedReply(text);
    typingEl.remove();
    _appendAI(reply);

    isTyping = false;
    document.getElementById('chatSendBtn').disabled = false;
    document.getElementById('chatInput').focus();
  }

  /* ---- Rule-based engine ---- */
  function _getRuleBasedReply(text) {
    const m = text.toLowerCase();
    const P = typeof PRODUCTS !== 'undefined' ? PRODUCTS : [];

    // Chào hỏi
    if (/xin chào|hello|hi\b|chào|hey|alo/.test(m)) {
      return 'Xin chào! Tôi có thể giúp gì cho bạn? 😊\nBạn có thể hỏi về sản phẩm, giá cả, khuyến mãi hoặc cách đặt hàng.';
    }

    // Cảm ơn
    if (/cảm ơn|thanks|thank you|tks/.test(m)) {
      return 'Không có gì ạ! Nếu cần thêm thông tin hãy cứ hỏi tôi nhé 🌿';
    }

    // Khuyến mãi / giảm giá
    if (/khuyến mãi|giảm giá|sale|ưu đãi|discount|km|đang giảm/.test(m)) {
      const onSale = P.filter(p => p.discountPercent > 0);
      if (!onSale.length) return 'Hiện chưa có chương trình khuyến mãi nào. Hãy theo dõi thường xuyên để cập nhật sớm nhất!';
      const list = onSale.slice(0, 4).map(p =>
        `• ${p.name} — giảm ${p.discountPercent}% còn ${p.price.toLocaleString('vi-VN')}đ`
      ).join('\n');
      return `🎁 Đang có ${onSale.length} sản phẩm khuyến mãi:\n${list}${onSale.length > 4 ? `\n...và ${onSale.length - 4} sản phẩm khác.` : ''}`;
    }

    // Bán chạy / nổi bật
    if (/bán chạy|nổi bật|hot\b|phổ biến|best seller|featured/.test(m)) {
      const featured = P.filter(p => p.featured);
      if (!featured.length) return 'Chưa có sản phẩm nổi bật nào được đánh dấu lúc này.';
      const list = featured.slice(0, 4).map(p =>
        `• ${p.name} — ${p.price.toLocaleString('vi-VN')}đ`
      ).join('\n');
      return `⭐ Sản phẩm nổi bật:\n${list}`;
    }

    // Hết hàng
    if (/hết hàng|out of stock/.test(m)) {
      const out = P.filter(p => p.stock <= 0);
      if (!out.length) return '✅ Hiện tất cả sản phẩm đều còn hàng!';
      return `Các sản phẩm đang hết hàng:\n${out.map(p => `• ${p.name}`).join('\n')}`;
    }

    // Còn hàng / tồn kho
    if (/còn hàng|tồn kho|stock|số lượng/.test(m)) {
      const inStock = P.filter(p => p.stock > 0);
      return `Hiện có ${inStock.length}/${P.length} sản phẩm còn hàng. Bạn quan tâm sản phẩm nào?`;
    }

    // Giá cả
    if (/giá|bao nhiêu|price|cost|phí|tiền/.test(m)) {
      if (!P.length) return 'Vui lòng tải lại trang để xem giá sản phẩm.';
      const sorted = [...P].sort((a, b) => a.price - b.price);
      const min = sorted[0], max = sorted[sorted.length - 1];
      return `💰 Giá sản phẩm từ ${min.price.toLocaleString('vi-VN')}đ đến ${max.price.toLocaleString('vi-VN')}đ.\nBạn muốn tư vấn sản phẩm cụ thể nào?`;
    }

    // Danh sách sản phẩm
    if (/sản phẩm|có gì|bán gì|danh sách|list|loại nào/.test(m)) {
      if (!P.length) return 'Đang tải danh sách sản phẩm, vui lòng thử lại sau.';
      const cats = [...new Set(P.map(p => p.cat))];
      return `📦 Chúng tôi có ${P.length} sản phẩm thuộc ${cats.length} danh mục:\n${cats.map(c => `• ${c}`).join('\n')}\nBạn quan tâm danh mục nào?`;
    }

    // Môi trường / sinh học
    if (/phân hủy|sinh học|môi trường|eco|green|organic|tự nhiên|vỏ cam|an toàn/.test(m)) {
      return '🌿 BioWraps sử dụng vật liệu từ vỏ cam tự nhiên:\n• Tự phân hủy sinh học trong 90–180 ngày\n• An toàn thực phẩm, không BPA\n• Giảm 70% khí thải CO₂ so với nhựa thường\n• Được chứng nhận tiêu chuẩn quốc tế';
    }

    // Đặt hàng / mua sỉ
    if (/đặt hàng|mua|order|sỉ|lẻ|số lượng lớn|b2b|đại lý/.test(m)) {
      return '🛒 Cách đặt hàng:\n• Thêm vào giỏ hàng → thanh toán trực tuyến\n• Đặt sỉ / B2B: liên hệ hotline 1800-BIOWRAPS\n• Email: sales@biowrapsvietnam.com\nĐơn từ 500.000đ được miễn phí vận chuyển!';
    }

    // Giao hàng / ship
    if (/giao hàng|ship|vận chuyển|delivery|express|nhanh/.test(m)) {
      return '🚚 Thông tin giao hàng:\n• TP.HCM & Hà Nội: 1–2 ngày làm việc\n• Tỉnh thành khác: 3–5 ngày làm việc\n• Miễn phí ship cho đơn từ 500.000đ\n• Hỗ trợ giao hỏa tốc trong nội thành';
    }

    // Thanh toán
    if (/thanh toán|payment|chuyển khoản|momo|tiền mặt|cod|thẻ/.test(m)) {
      return '💳 Hình thức thanh toán:\n• Chuyển khoản ngân hàng\n• Ví MoMo\n• Tiền mặt khi nhận hàng (COD)\n• Thẻ tín dụng / ghi nợ';
    }

    // Liên hệ
    if (/liên hệ|contact|hotline|điện thoại|email|địa chỉ|address|zalo/.test(m)) {
      return '📞 Liên hệ BioWraps Vietnam:\n• Hotline: 1800-BIOWRAPS\n• Email: info@biowrapsvietnam.com\n• Zalo: 0909-BIOWRAPS\n• Địa chỉ: TP. Hồ Chí Minh\n• Giờ làm việc: 8h–18h, Thứ 2–Thứ 7';
    }

    // Tìm sản phẩm theo tên hoặc danh mục
    const matched = P.filter(p =>
      m.includes(p.name.toLowerCase()) ||
      m.includes((p.cat || '').toLowerCase()) ||
      (p.desc || '').toLowerCase().split(' ').some(w => w.length > 3 && m.includes(w))
    );
    if (matched.length) {
      const p = matched[0];
      const discLine = p.discountPercent > 0
        ? ` (đang giảm ${p.discountPercent}%, giá gốc ${(p.originalPrice || p.price).toLocaleString('vi-VN')}đ)`
        : '';
      return `📌 ${p.name}\n• Giá: ${p.price.toLocaleString('vi-VN')}đ${discLine}\n• Tồn kho: ${p.stock > 0 ? p.stock + ' sản phẩm' : '⚠️ Hết hàng'}\n• ${p.desc || ''}\nBạn muốn thêm vào giỏ hàng không?`;
    }

    // Mặc định
    return 'Xin lỗi, tôi chưa hiểu câu hỏi của bạn 😅\nBạn có thể hỏi về:\n• Sản phẩm & giá cả\n• Khuyến mãi\n• Giao hàng & thanh toán\n• Đặt hàng sỉ\nHoặc liên hệ hotline 1800-BIOWRAPS để được hỗ trợ trực tiếp!';
  }

  /* ---- DOM helpers ---- */
  function _appendUser(text) {
    _appendBubble('user', text);
  }

  function _appendAI(text) {
    _appendBubble('ai', text);
  }

  function _appendBubble(role, text) {
    const msgs = document.getElementById('chatMessages');
    const now  = new Date().toLocaleTimeString('vi-VN', { hour: '2-digit', minute: '2-digit' });
    const div  = document.createElement('div');
    div.className = `msg ${role}`;
    div.innerHTML = `
      <div class="msg-bubble">${_escape(text).replace(/\n/g, '<br>')}</div>
      <div class="msg-time">${now}</div>
    `;
    msgs.appendChild(div);
    msgs.scrollTop = msgs.scrollHeight;
  }

  function _showTyping() {
    const msgs = document.getElementById('chatMessages');
    const div  = document.createElement('div');
    div.className = 'msg ai';
    div.innerHTML = `<div class="typing-indicator"><span></span><span></span><span></span></div>`;
    msgs.appendChild(div);
    msgs.scrollTop = msgs.scrollHeight;
    return div;
  }

  function _escape(str) {
    return str
      .replace(/&/g, '&amp;')
      .replace(/</g, '&lt;')
      .replace(/>/g, '&gt;')
      .replace(/"/g, '&quot;');
  }

  /* ---- Init ---- */
  if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', _mount);
  } else {
    _mount();
  }
})();
