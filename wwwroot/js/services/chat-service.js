/* ── AUTOHUB Real-Time AI Chat Service — connects to /api/chat/message ── */
(() => {
  'use strict';

  function getCurrentVehicleSlug() {
    const path = window.location.pathname;
    const match = path.match(/\/Vehicles\/([a-zA-Z0-9_-]+)/i);
    return match ? match[1] : null;
  }

  const LiveChatService = {
    /**
     * @param {string} message
     * @param {any} [context]
     * @returns {Promise<{ content: string, vehicles?: any[], actions?: any[], intent?: string }>}
     */
    async sendMessage(message, context) {
      const slug = (context && context.currentSlug) || getCurrentVehicleSlug();

      try {
        const response = await fetch('/api/chat/message', {
          method: 'POST',
          headers: {
            'Content-Type': 'application/json',
            'Accept': 'application/json'
          },
          body: JSON.stringify({
            message: message.trim(),
            currentSlug: slug
          })
        });

        if (!response.ok) {
          throw new Error('Server returned HTTP ' + response.status);
        }

        const data = await response.json();
        return {
          content: data.content || data.Content || '',
          vehicles: data.vehicles || data.Vehicles || [],
          actions: data.actions || data.Actions || [],
          intent: data.intent || data.Intent || 'GENERAL'
        };
      } catch (err) {
        console.warn('[APEX AI] Khong ket noi duoc API chat, su dung fallback:', err);
        // Fallback gracefully if offline
        return {
          content: 'Hệ thống đang bảo trì kết nối thời gian thực. Bạn có thể sử dụng các nút thao tác nhanh dưới đây hoặc liên hệ trực tiếp với chuyên viên tư vấn.',
          vehicles: [],
          actions: [
            { label: 'Xem tất cả dòng xe', action: 'view-all-vehicles' },
            { label: 'Dự toán trả góp', action: 'navigate', href: '/Finance' },
            { label: 'Đặt lịch tư vấn', action: 'consultation' }
          ],
          intent: 'FALLBACK'
        };
      }
    }
  };

  window.ApexChatService = LiveChatService;
})();
