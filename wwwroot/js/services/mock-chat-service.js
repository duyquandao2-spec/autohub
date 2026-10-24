/* ── AUTOHUB Mock Chat Service — local keyword-matching responses, UI-only phase ── */
(() => {
  /**
   * @typedef {import('./chat-types.js').ChatAction} ChatAction
   * @typedef {import('./chat-types.js').ChatResponse} ChatResponse
   */

  const RULES = [
    {
      test: /tìm xe|tìm mẫu|phù hợp|ngân sách|nhu cầu|suv|sedan|hatchback|mpv|coupe|pickup|crossover/i,
      respond: () => ({
        content: 'Bạn có thể cho tôi biết thêm ngân sách dự kiến và nhu cầu sử dụng để tôi hỗ trợ tìm mẫu xe phù hợp.',
        actions: [
          { label: 'Xem tất cả xe', action: 'view-all-vehicles' },
          { label: 'Đặt lịch tư vấn', action: 'consultation' },
        ],
      }),
    },
    {
      test: /xe điện|ev|electric|pin|sạc|range|km.*sạc/i,
      respond: () => ({
        content: 'Hiện tại đây là giao diện demo của trợ lý APEX. Hệ thống AI và dữ liệu xe sẽ được kết nối ở giai đoạn tiếp theo.',
        actions: [
          { label: 'Xe điện EV', action: 'filter-ev' },
          { label: 'Tìm hiểu thêm', action: 'default' },
        ],
      }),
    },
    {
      test: /so sánh|compare|đối chiếu|khác nhau/i,
      respond: () => ({
        content: 'Để so sánh chính xác, vui lòng chọn 2 mẫu xe bạn quan tâm. Hiện tại đây là giao diện demo.',
        actions: [
          { label: 'Xem danh sách xe', action: 'view-all-vehicles' },
        ],
      }),
    },
    {
      test: /công nghệ|tech|động cơ|turbo|hybrid|truyền động|AWD|FWD|RWD/i,
      respond: () => ({
        content: 'Công nghệ ô tô là thế mạnh của APEX. Hiện tại đây là giao diện demo — hệ thống AI sẽ trả lời chi tiết ở giai đoạn tiếp theo.',
        actions: [
          { label: 'Khám phá công nghệ', action: 'view-experience' },
        ],
      }),
    },
    {
      test: /đặt lịch|tư vấn|consultation|hẹn|gặp|liên hệ/i,
      respond: () => ({
        content: 'Tôi sẽ mở form tư vấn cho bạn ngay.',
        actions: [
          { label: 'Mở form tư vấn', action: 'consultation' },
        ],
      }),
    },
    {
      test: /3d|showroom|trải nghiệm|experience|360|xoay/i,
      respond: () => ({
        content: 'Trải nghiệm 3D showroom đang chờ bạn. Hiện tại đây là giao diện demo.',
        actions: [
          { label: 'Xem 3D showroom', action: 'view-3d' },
        ],
      }),
    },
    {
      test: /xin chào|hello|hi|chào|alo|hey/i,
      respond: () => ({
        content: 'Xin chào! Tôi là trợ lý APEX MOTORS. Tôi có thể giúp bạn tìm hiểu về xe, công nghệ và trải nghiệm tại APEX.',
        actions: [
          { label: 'Tìm xe phù hợp', action: 'search-vehicle' },
          { label: 'Xe điện', action: 'filter-ev' },
          { label: 'Đặt lịch tư vấn', action: 'consultation' },
        ],
      }),
    },
    {
      test: /giá|bao nhiêu|price|triệu|tỷ|vnd/i,
      respond: () => ({
        content: 'Giá xe tại APEX rất cạnh tranh. Hiện tại đây là giao diện demo — dữ liệu giá chính xác sẽ được kết nối ở giai đoạn tiếp theo.',
        actions: [
          { label: 'Xem tất cả xe', action: 'view-all-vehicles' },
        ],
      }),
    },
  ];

  const DEFAULT_RESPONSE = {
    content: 'Xin lỗi, tôi chưa hiểu yêu cầu. Hiện tại đây là giao diện demo. Hệ thống AI sẽ được kết nối ở giai đoạn tiếp theo.',
    actions: [
      { label: 'Tìm xe', action: 'search-vehicle' },
      { label: 'Đặt lịch tư vấn', action: 'consultation' },
    ],
  };

  /** @returns {ChatResponse} */
  function getResponse(message) {
    for (const rule of RULES) {
      if (rule.test.test(message)) return rule.respond();
    }
    return DEFAULT_RESPONSE;
  }

  const MockChatService = {
    /**
     * @param {string} message
     * @param {import('./chat-types.js').ChatContext} [context]
     * @returns {Promise<ChatResponse>}
     */
    sendMessage(message, _context) {
      return new Promise((resolve) => {
        const delay = 600 + Math.random() * 800;
        setTimeout(() => resolve(getResponse(message)), delay);
      });
    },
  };

  window.ApexChatService = MockChatService;
})();
