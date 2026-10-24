/* ── AUTOHUB Header Navigation Hub — hover-to-open (desktop) / click accordion (mobile).
   Hover uses global mousemove + bounding-box math (zero flicker).
   One menu open at a time · outside click closes · Escape closes ·
   keyboard (Tab/Enter/Space/Arrows) supported · mobile accordion.
   Overlay-only: never pushes page content. ── */
(() => {
  const header = document.querySelector('[data-header]');
  const root = document.querySelector('[data-nav-root]');
  const backdrop = document.querySelector('[data-nav-backdrop]');
  if (!header || !root || !backdrop) return;

  const items = [...root.querySelectorAll('[data-nav-item]')];
  let openItem = null;
  let isMobile = window.matchMedia('(max-width:900px)').matches;
  let closeTimer = null;
  const CLOSE_DELAY = 250;

  root.querySelectorAll('[data-nav-panel]').forEach((p) => { p.hidden = false; });

  const triggerOf = (item) => item.querySelector('[data-nav-trigger]');
  const panelOf = (item) => item.querySelector('[data-nav-panel]');
  const focusablesIn = (panel) => [...panel.querySelectorAll('a[href],button:not([disabled])')];

  function setOpen(next) {
    if (openItem === next) return;
    if (openItem) {
      openItem.classList.remove('is-open');
      triggerOf(openItem).setAttribute('aria-expanded', 'false');
    }
    openItem = next;
    if (next) {
      next.classList.add('is-open');
      triggerOf(next).setAttribute('aria-expanded', 'true');
      panelOf(next).style.setProperty('--panel-shift', '0px');
      requestAnimationFrame(() => clampPanel(next));
    }
    backdrop.classList.toggle('is-visible', !!next);
    header.classList.toggle('has-open-menu', !!next);
  }

  /* Get the bounding box of an element, expanded vertically & horizontally.
     We expand aggressively so the trigger zone and panel zone overlap
     and cover the gap between them completely. */
  function expandedRect(el, padX, padY) {
    const r = el.getBoundingClientRect();
    return {
      left:   r.left   - padX,
      right:  r.right  + padX,
      top:    r.top    - padY,
      bottom: r.bottom + padY,
    };
  }

  function pointInRect(x, y, rect) {
    return x >= rect.left && x <= rect.right && y >= rect.top && y <= rect.bottom;
  }

  /* Check if pointer is inside the "safe zone" of the active nav item.
     The safe zone = expanded trigger rect ∪ expanded panel rect.
     We use generous vertical padding so the two rects always overlap,
     no matter how far the panel sits below the trigger. */
  function isPointerInsideActiveZone(x, y) {
    if (!openItem) return false;
    const trigger = triggerOf(openItem);
    const panel = panelOf(openItem);
    /* 80px horizontal, 120px vertical — more than enough to cover
       any gap between trigger button and the dropdown panel. */
    const tr = expandedRect(trigger, 80, 120);
    const pr = expandedRect(panel, 80, 120);
    return pointInRect(x, y, tr) || pointInRect(x, y, pr);
  }

  function clampPanel(item) {
    const panel = panelOf(item);
    const rect = panel.getBoundingClientRect();
    const pad = 16;
    let dx = 0;
    if (rect.left < pad) dx = pad - rect.left;
    else if (rect.right > window.innerWidth - pad) dx = (window.innerWidth - pad) - rect.right;
    panel.style.setProperty('--panel-shift', `${Math.round(dx)}px`);
  }

  /* ── Desktop hover: open on mouseenter, close via global mousemove ── */
  items.forEach((item) => {
    const trigger = triggerOf(item);
    const panel = panelOf(item);

    item.addEventListener('mouseenter', () => {
      if (isMobile) return;
      clearTimeout(closeTimer);
      setOpen(item);
    });

    trigger.addEventListener('click', () => {
      if (isMobile) return;
      setOpen(openItem === item ? null : item);
    });

    trigger.addEventListener('keydown', (e) => {
      if (e.key === 'ArrowDown') {
        e.preventDefault();
        if (openItem !== item) setOpen(item);
        const first = focusablesIn(panel)[0];
        if (first) first.focus();
      } else if (e.key === 'ArrowUp') {
        e.preventDefault();
        if (openItem !== item) setOpen(item);
        const list = focusablesIn(panel);
        if (list.length) list[list.length - 1].focus();
      }
    });

    panel.addEventListener('keydown', (e) => {
      if (e.key === 'Escape') {
        e.stopPropagation();
        setOpen(null);
        trigger.focus();
        return;
      }
      if (e.key === 'ArrowDown' || e.key === 'ArrowUp') {
        const list = focusablesIn(panel);
        const idx = list.indexOf(document.activeElement);
        if (idx === -1) return;
        e.preventDefault();
        const nextIdx = e.key === 'ArrowDown'
          ? (idx + 1) % list.length
          : (idx - 1 + list.length) % list.length;
        list[nextIdx].focus();
      }
    });
  });

  /* ── Global mousemove: switch items + decide whether to close ── */
  document.addEventListener('mousemove', (e) => {
    if (isMobile) return;
    const mx = e.clientX;
    const my = e.clientY;

    /* 1) If there IS an open item, check if pointer left its zone */
    if (openItem) {
      if (isPointerInsideActiveZone(mx, my)) {
        clearTimeout(closeTimer);
      } else if (closeTimer === null) {
        closeTimer = setTimeout(() => {
          closeTimer = null;
          setOpen(null);
        }, CLOSE_DELAY);
      }
    }

    /* 2) Detect which nav-item the pointer is over (works even when
          a panel is visually on top — we use trigger bounding rects,
          not DOM hover events). Switch immediately. */
    for (const candidate of items) {
      if (candidate === openItem) continue;
      const tr = triggerOf(candidate).getBoundingClientRect();
      if (my >= tr.top && my <= tr.bottom && mx >= tr.left && mx <= tr.right) {
        clearTimeout(closeTimer);
        closeTimer = null;
        setOpen(candidate);
        break;
      }
    }
  }, { passive: true });

  /* Click outside → close */
  document.addEventListener('pointerdown', (e) => {
    if (openItem && !e.target.closest('[data-nav-item]')) setOpen(null);
  });
  backdrop.addEventListener('pointerdown', () => setOpen(null));

  document.addEventListener('keydown', (e) => {
    if (e.key === 'Escape' && openItem) {
      const trigger = triggerOf(openItem);
      setOpen(null);
      trigger.focus();
    }
  });

  document.addEventListener('focusin', (e) => {
    if (openItem && !root.contains(e.target)) setOpen(null);
  });

  /* ── Leaf actions ─────────────────────────────────────────────── */
  function closeAll() {
    if (openItem) setOpen(null);
  }

  root.addEventListener('click', (e) => {
    const drawerBtn = e.target.closest('[data-nav-drawer]');
    if (drawerBtn) {
      closeAll();
      document.querySelector('[data-contact-hub-toggle]')?.click();
      return;
    }

    const link = e.target.closest('a[href]');
    if (!link) return;
    closeAll();

    const href = link.getAttribute('href') || '';
    const url = new URL(href, window.location.origin);

    if (
      link.hasAttribute('data-ajax-load') &&
      url.pathname === '/' &&
      window.location.pathname === '/' &&
      typeof window.autohubLoadPage === 'function'
    ) {
      e.preventDefault();
      window.autohubLoadPage(href);
    }
  });

  const homeLink = root.querySelector('[data-nav-home]');
  homeLink?.addEventListener('click', (e) => {
    closeAll();
    if (
      window.location.pathname === '/' &&
      !window.location.search &&
      !window.location.hash
    ) {
      e.preventDefault();
      window.scrollTo({ top: 0, behavior: 'smooth' });
    }
  });

  /* ── Mobile accordion ── */
  const accButtons = [...document.querySelectorAll('[data-acc-toggle]')];
  accButtons.forEach((btn) => {
    btn.addEventListener('click', () => {
      const acc = btn.closest('.mobile-acc');
      const sub = document.getElementById(btn.getAttribute('aria-controls'));
      if (!acc || !sub) return;
      const willOpen = !sub.classList.contains('is-open');

      accButtons.forEach((other) => {
        if (other === btn) return;
        const otherSub = document.getElementById(other.getAttribute('aria-controls'));
        otherSub?.classList.remove('is-open');
        other.closest('.mobile-acc')?.classList.remove('is-open');
        other.setAttribute('aria-expanded', 'false');
      });

      sub.classList.toggle('is-open', willOpen);
      acc.classList.toggle('is-open', willOpen);
      btn.setAttribute('aria-expanded', String(willOpen));
    });
  });

  document.querySelectorAll('.mobile-drawer-btn[data-nav-drawer]').forEach((btn) => {
    btn.addEventListener('click', () => {
      const mobileNav = document.querySelector('[data-mobile-nav]');
      const menuButton = document.querySelector('[data-menu-button]');
      mobileNav?.classList.remove('is-open');
      menuButton?.setAttribute('aria-expanded', 'false');
      if (menuButton) menuButton.textContent = '☰';
      document.querySelector('[data-contact-hub-toggle]')?.click();
    });
  });

  window.matchMedia('(max-width:900px)').addEventListener('change', (e) => {
    isMobile = e.matches;
    closeAll();
  });
})();
