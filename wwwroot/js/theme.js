/* ── AUTOHUB Theme — centralized light/dark state.
   Dark is the default identity; persists to localStorage('apex-theme'),
   dispatches 'apex:themechange'. No reload, no image/model invalidation. ── */
(() => {
  const STORAGE_KEY = 'apex-theme';
  const root = document.documentElement;

  function readInitial() {
    const saved = root.getAttribute('data-theme');
    return saved === 'light' ? 'light' : 'dark'; /* invalid value → dark fallback */
  }

  let current = readInitial();

  function syncToggle() {
    document.querySelectorAll('[data-theme-toggle]').forEach((btn) => {
      btn.setAttribute('aria-pressed', String(current === 'light'));
    });
  }

  function apply(next) {
    current = next;
    root.setAttribute('data-theme', next);
    try { localStorage.setItem(STORAGE_KEY, next); } catch (_) { /* private mode */ }
    syncToggle();
    document.dispatchEvent(new CustomEvent('apex:themechange', { detail: { theme: current } }));
  }

  function setTheme(next, animate) {
    if ((next !== 'dark' && next !== 'light') || next === current) return;
    const reduced = window.matchMedia('(prefers-reduced-motion: reduce)').matches;

    if (animate && !reduced) {
      root.classList.add('is-theme-switching');
      requestAnimationFrame(() => requestAnimationFrame(() => {
        apply(next);
        setTimeout(() => root.classList.remove('is-theme-switching'), 420);
      }));
    } else {
      apply(next);
    }
  }

  function init() {
    syncToggle();
    document.querySelectorAll('[data-theme-toggle]').forEach((btn) => {
      btn.addEventListener('click', () => setTheme(current === 'dark' ? 'light' : 'dark', true));
    });
  }

  if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', init);
  } else {
    init();
  }
})();
