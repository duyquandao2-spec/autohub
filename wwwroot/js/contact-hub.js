/* ── AUTOHUB Floating Contact Hub — UI only.
   Real links arrive later by filling `href` in APEX_CONTACT_CHANNELS; the UI never changes. ── */
(() => {
  /* ContactChannel config — href:null = placeholder (no navigation, dev-safe state) */
  const APEX_CONTACT_CHANNELS = [
    { id: 'phone', label: 'Gọi tư vấn', icon: 'phone', href: null, enabled: true },
    { id: 'chatbot', label: 'Chatbot', icon: 'bot', href: null, enabled: true },
    { id: 'email', label: 'Email', icon: 'mail', href: null, enabled: true },
    { id: 'facebook', label: 'Facebook', icon: 'facebook', href: null, enabled: true },
    { id: 'youtube', label: 'YouTube', icon: 'youtube', href: null, enabled: true },
    { id: 'zalo', label: 'Zalo', icon: 'messages-square', href: null, enabled: true }
  ];

  const hub = document.querySelector('[data-contact-hub]');
  const toggle = document.querySelector('[data-contact-hub-toggle]');
  if (!hub || !toggle) return;

  const menu = hub.querySelector('#contactHubMenu');
  const items = [...hub.querySelectorAll('.contact-hub__item')];
  let isOpen = false;
  let hoverCloseTimer = null;
  const canHover = window.matchMedia('(hover:hover) and (pointer:fine)');
  let hoverBound = false;

  function setOpen(next) {
    if (next === isOpen) return;
    isOpen = next;
    hub.classList.toggle('is-open', isOpen);
    toggle.setAttribute('aria-expanded', String(isOpen));
    toggle.setAttribute('aria-label', isOpen ? 'Đóng menu liên hệ' : 'Mở menu liên hệ');
  }

  /* ── Main button: tap/click toggles; desktop hover opens with close intent delay ── */
  toggle.addEventListener('click', () => setOpen(!isOpen));

  canHover.addEventListener('change', syncHoverMode);
  function syncHoverMode() {
    if (!canHover.matches || hoverBound) return;
    hoverBound = true;
    toggle.addEventListener('pointerenter', () => {
      clearTimeout(hoverCloseTimer);
      setOpen(true);
    });
    hub.addEventListener('pointerleave', () => {
      clearTimeout(hoverCloseTimer);
      hoverCloseTimer = setTimeout(() => setOpen(false), 220);
    });
  }
  syncHoverMode();

  /* ── Close on outside click / Escape (focus returns to main button) ── */
  document.addEventListener('pointerdown', (e) => {
    if (isOpen && !hub.contains(e.target)) setOpen(false);
  });

  document.addEventListener('keydown', (e) => {
    if (e.key !== 'Escape' || !isOpen) return;
    setOpen(false);
    toggle.focus();
  });

  /* ── Channel actions — placeholders while href is null ── */
  items.forEach((item) => {
    item.addEventListener('click', () => {
      const channel = APEX_CONTACT_CHANNELS.find((c) => c.id === item.dataset.channel);
      if (!channel || !channel.enabled) return;

      if (channel.id === 'chatbot') {
        setOpen(false);
        document.dispatchEvent(new CustomEvent('apex:chatbot:toggle'));
        return;
      }

      if (channel.href) {
        setOpen(false);
        console.info('[APEX] Điều hướng tới kênh:', channel.id, channel.href);
        return;
      }

      showPendingState(item, channel.label);
    });
  });

  function showPendingState(item, label) {
    if (item.classList.contains('is-pending')) return;
    const tooltip = item.querySelector('.contact-hub__tooltip');
    const defaultLabel = tooltip.textContent;

    tooltip.textContent = `${label} · sắp ra mắt`;
    item.classList.add('is-pending', 'is-pulse');
    setTimeout(() => item.classList.remove('is-pulse'), 520);

    setTimeout(() => {
      tooltip.textContent = defaultLabel;
      item.classList.remove('is-pending');
    }, 1300);

    console.info(`[APEX] Kênh "${label}" chưa được kết nối — UI placeholder.`);
  }

  /* ── Avoid covering footer actions when scrolled to the bottom (no layout shift: fixed element) ── */
  const footer = document.querySelector('.site-footer');
  if (footer && menu) {
    let ticking = false;
    const updateLift = () => {
      ticking = false;
      const rect = footer.getBoundingClientRect();
      const overlap = window.innerHeight - rect.top;
      hub.style.setProperty('--lift', overlap > 0 ? `${Math.min(overlap + 12, rect.height)}px` : '0px');
    };
    const requestLift = () => {
      if (!ticking) { ticking = true; requestAnimationFrame(updateLift); }
    };
    window.addEventListener('scroll', requestLift, { passive: true });
    window.addEventListener('resize', requestLift);
    updateLift();
  }
})();
