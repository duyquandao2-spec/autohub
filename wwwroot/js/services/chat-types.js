/* ── AUTOHUB Chat Types — JSDoc typed structures for chatbot system ── */

/**
 * @typedef {{ id: string, role: 'user'|'assistant', content: string, timestamp: string }} ChatMessage
 * @typedef {{ vehicleSlug?: string, brand?: string }} ChatContext
 * @typedef {{ label: string, action: string }} ChatAction
 * @typedef {{ content: string, actions?: ChatAction[] }} ChatResponse
 */
