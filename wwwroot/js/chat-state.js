/* ── AUTOHUB Chat State — centralized chatbot state management ── */
(() => {
  /** @type {{ isOpen: boolean, messages: import('./services/chat-types.js').ChatMessage[], isLoading: boolean, inputValue: string }} */
  window.ApexChatState = {
    isOpen: false,
    messages: [],
    isLoading: false,
    inputValue: '',
  };
})();
