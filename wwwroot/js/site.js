/* Monolith Velocity interaction layer: motion stays tactile, brief, and opt-out friendly. */
(() => {
  const header = document.querySelector('[data-header]');
  const hero = document.querySelector('[data-hero]');
  const menuButton = document.querySelector('[data-menu-button]');
  const mobileNav = document.querySelector('[data-mobile-nav]');
  let searchInput = document.querySelector('[data-search-input]');
  let searchSuggestions = document.querySelector('[data-search-suggestions]');

  /* ── Scroll reveal (observer ở module scope) ──────────── */
  const revealObserver = new IntersectionObserver((entries) => entries.forEach((entry) => {
    if (entry.isIntersecting) entry.target.classList.add('is-visible');
  }), { threshold: 0.12 });

  function bindRevealElements(root) {
    root.querySelectorAll('.reveal').forEach((el) => {
      revealObserver.observe(el);
    });
  }

  function forceRevealMachinesSection() {
    const machines = document.getElementById('machines');
    if (!machines) return;
    machines.querySelectorAll('.reveal').forEach(el => {
      el.classList.add('is-visible');
    });
  }

  function syncBrandCarouselActive() {
    const params = new URLSearchParams(window.location.search);
    const brand = params.get('brand');
    const activeBrand = brand || 'ALL';
    document.querySelectorAll('[data-carousel-track] .brand-chip').forEach(chip => {
      const chipBrand = chip.textContent.trim();
      const isActive = activeBrand === 'ALL'
        ? chipBrand === 'TẤT CẢ'
        : chipBrand.toUpperCase() === activeBrand.toUpperCase();
      chip.classList.toggle('is-active', isActive);
    });
  }

  /* ── Header scroll effect ─────────────────────────────── */
  const updateHeader = () => header?.classList.toggle('is-scrolled', window.scrollY > 18);
  window.addEventListener('scroll', updateHeader, { passive: true });
  updateHeader();

  /* ── Logo click → scroll to top ──────────────────────── */
  document.querySelector('.brand')?.addEventListener('click', (e) => {
    if (window.location.pathname === '/') {
      e.preventDefault();
      window.scrollTo({ top: 0, behavior: 'smooth' });
    }
  });

  /* ── Hero parallax ────────────────────────────────────── */
  hero?.addEventListener('pointermove', (event) => {
    const bounds = hero.getBoundingClientRect();
    hero.style.setProperty('--hero-x', `${((event.clientX - bounds.left) / bounds.width) * 100}%`);
    hero.style.setProperty('--hero-y', `${((event.clientY - bounds.top) / bounds.height) * 100}%`);
  });

  /* ── Mobile menu ──────────────────────────────────────── */
  menuButton?.addEventListener('click', () => {
    const open = mobileNav?.classList.toggle('is-open') ?? false;
    menuButton.setAttribute('aria-expanded', String(open));
    menuButton.textContent = open ? '×' : '☰';
  });

  mobileNav?.querySelectorAll('a').forEach((link) => link.addEventListener('click', () => {
    mobileNav.classList.remove('is-open');
    menuButton?.setAttribute('aria-expanded', 'false');
    if (menuButton) menuButton.textContent = '☰';
  }));

  /* ── Scroll-to targets ────────────────────────────────── */
  document.querySelectorAll('[data-scroll-target]').forEach((button) => button.addEventListener('click', () => {
    document.getElementById(button.dataset.scrollTarget)?.scrollIntoView({ behavior: 'smooth', block: 'start' });
  }));

  /* ── Test drive button ────────────────────────────────── */
  document.querySelectorAll('[data-appointment]').forEach((button) => button.addEventListener('click', () => {
    window.alert('Yêu cầu lái thử đã được ghi nhận. Đội ngũ AUTOHUB sẽ liên hệ để xác nhận khung giờ phù hợp.');
  }));

  /* ── AJAX page loading ────────────────────────────────── */
  let isLoading = false;

  function loadPage(url) {
    if (isLoading) return;
    isLoading = true;

    const absoluteUrl = new URL(url, window.location.origin).href;
    const bustUrl = absoluteUrl + (absoluteUrl.includes('?') ? '&' : '?') + '_=' + Date.now();

    fetch(bustUrl, { cache: 'no-store' })
      .then(r => r.text())
      .then(html => {
        const doc = new DOMParser().parseFromString(html, 'text/html');

        const newMachines = doc.getElementById('machines');
        const machinesSection = document.getElementById('machines');
        if (!newMachines || !machinesSection) { isLoading = false; return; }

        machinesSection.replaceWith(newMachines);

        const cleanUrl = new URL(url, window.location.origin);
        cleanUrl.searchParams.delete('_');
        history.pushState({}, '', cleanUrl.pathname + cleanUrl.search);

        rebindAll();

        forceRevealMachinesSection();
        syncBrandCarouselActive();

        document.getElementById('machines')?.scrollIntoView({ behavior: 'smooth', block: 'start' });

        isLoading = false;
      })
      .catch(() => {
        isLoading = false;
        window.location.href = url;
      });
  }

  function rebindAll() {
    searchInput = document.querySelector('[data-search-input]');
    searchSuggestions = document.querySelector('[data-search-suggestions]');

    bindPaginationLinks();
    bindSearchNavigation();
    bindBrandChips();
    bindRevealElements(document);
  }

  /* ── Pagination ───────────────────────────────────────── */
  function paginationClickHandler(e) {
    const href = this.getAttribute('href');
    if (!href || href === '#' || this.classList.contains('is-disabled')) return;
    e.preventDefault();
    loadPage(href);
  }

  function bindPaginationLinks() {
    document.querySelectorAll('[data-pagination] a[href]').forEach((link) => {
      link.removeEventListener('click', paginationClickHandler);
      link.addEventListener('click', paginationClickHandler);
    });
  }

  /* ── Brand chips ──────────────────────────────────────── */
  function brandChipClickHandler(e) {
    const href = this.getAttribute('href');
    if (!href) return;
    e.preventDefault();
    loadPage(href);
  }

  function bindBrandChips() {
    document.querySelectorAll('[data-carousel-track] a[href]').forEach((chip) => {
      chip.removeEventListener('click', brandChipClickHandler);
      chip.addEventListener('click', brandChipClickHandler);
    });
  }

  /* ── Search ───────────────────────────────────────────── */
  let searchTimeout = null;
  let searchNavigateTimeout = null;

  function buildSearchUrl(query) {
    const params = new URLSearchParams();
    if (query) params.set('q', query);
    params.set('p', '1');
    return '/?' + params.toString();
  }

  function getSearchBtn() {
    return document.querySelector('[data-search-btn]');
  }

  function bindSearchNavigation() {
    if (!searchInput || !searchSuggestions) return;

    searchInput.addEventListener('input', onSearchInput);
    searchInput.addEventListener('keydown', onSearchKeydown);
    searchInput.addEventListener('blur', onSearchBlur);
    searchInput.addEventListener('focus', onSearchFocus);

    const btn = getSearchBtn();
    if (btn) {
      btn.addEventListener('click', onSearchBtnClick);
    }
  }

  function onSearchBtnClick() {
    clearTimeout(searchTimeout);
    clearTimeout(searchNavigateTimeout);
    const query = searchInput.value.trim();
    if (query.length > 0) {
      loadPage(buildSearchUrl(query));
    }
  }

  function onSearchInput() {
    clearTimeout(searchTimeout);
    clearTimeout(searchNavigateTimeout);
    const query = searchInput.value.trim();

    if (query.length < 1) {
      searchSuggestions.innerHTML = '';
      searchSuggestions.classList.remove('is-open');
      const currentParams = new URLSearchParams(window.location.search);
      if (currentParams.has('q')) {
        searchNavigateTimeout = setTimeout(() => {
          loadPage('/');
        }, 300);
      }
      return;
    }

    searchTimeout = setTimeout(() => {
      fetch(`/api/vehicles/search?q=${encodeURIComponent(query)}`)
        .then(r => r.json())
        .then(vehicles => {
          if (vehicles.length === 0) {
            searchSuggestions.innerHTML = '<div class="search-no-result">Không tìm thấy xe phù hợp</div>';
          } else {
            const q = query.toLowerCase();
            const prefixMatches = [];
            const containsMatches = [];
            vehicles.forEach(v => {
              const name = (v.name || '').toLowerCase();
              const brand = (v.brand || '').toLowerCase();
              const model = (v.model || '').toLowerCase();
              if (name.startsWith(q) || brand.startsWith(q) || model.startsWith(q)) {
                prefixMatches.push(v);
              } else {
                containsMatches.push(v);
              }
            });
            const sorted = [...prefixMatches, ...containsMatches].slice(0, 8);
            searchSuggestions.innerHTML = sorted.map(v => {
              const highlighted = v.name.replace(
                new RegExp(`(${query.replace(/[.*+?^${}()|[\]\\]/g, '\\$&')})`, 'gi'),
                '<mark>$1</mark>'
              );
              return `
              <a class="search-suggestion-item" href="/Vehicles/${v.slug}">
                <span class="search-suggestion-brand">${v.brand}</span>
                <span class="search-suggestion-name">${highlighted}</span>
                <span class="search-suggestion-meta">${v.category} · ${v.drivetrain}</span>
              </a>`;
            }).join('');
          }
          searchSuggestions.classList.add('is-open');
        })
        .catch(() => {
          searchSuggestions.innerHTML = '<div class="search-no-result">Lỗi tìm kiếm</div>';
          searchSuggestions.classList.add('is-open');
        });
    }, 200);
  }

  function onSearchKeydown(e) {
    if (e.key === 'Enter') {
      clearTimeout(searchTimeout);
      clearTimeout(searchNavigateTimeout);
      const query = searchInput.value.trim();
      if (query.length > 0) {
        loadPage(buildSearchUrl(query));
      }
    }
  }

  function onSearchBlur() {
    setTimeout(() => {
      searchSuggestions?.classList.remove('is-open');
    }, 200);
  }

  function onSearchFocus() {
    if (searchSuggestions?.innerHTML.trim()) {
      searchSuggestions.classList.add('is-open');
    }
  }

  /* ── Init ─────────────────────────────────────────────── */
  bindPaginationLinks();
  bindSearchNavigation();
  bindBrandChips();
  bindRevealElements(document);

  window.addEventListener('popstate', () => {
    loadPage(window.location.href);
  });

  /* ── Color picker (detail page) ────────────────────────── */
  const colorSwatches = document.querySelectorAll('.color-swatch');
  const productImage = document.querySelector('[data-product-image]');

  colorSwatches.forEach(swatch => {
    swatch.addEventListener('click', () => {
      colorSwatches.forEach(s => s.classList.remove('is-active'));
      swatch.classList.add('is-active');

      const newImage = swatch.dataset.colorImage;
      if (productImage && newImage) {
        productImage.style.opacity = '0.4';
        const img = new Image();
        img.onload = () => {
          productImage.src = newImage;
          productImage.style.opacity = '1';
        };
        img.onerror = () => {
          productImage.style.opacity = '1';
        };
        img.src = newImage;
      }
    });
  });

  /* ── Brand carousel ──────────────────────────────── */
  const carousel = document.querySelector('[data-brand-carousel]');
  if (carousel) {
    const track = carousel.querySelector('[data-carousel-track]');
    const prevBtn = carousel.querySelector('[data-carousel-prev]');
    const nextBtn = carousel.querySelector('[data-carousel-next]');

    function getChipWidth() {
      const chips = track.querySelectorAll('.brand-chip--slide');
      if (chips.length < 2) return 180;
      const gap = parseFloat(getComputedStyle(track).gap) || 10;
      return chips[0].offsetWidth + gap;
    }

    function updateArrows() {
      const atStart = track.scrollLeft <= 2;
      const atEnd = track.scrollLeft + track.clientWidth >= track.scrollWidth - 2;
      if (prevBtn) prevBtn.classList.toggle('is-disabled', atStart);
      if (nextBtn) nextBtn.classList.toggle('is-disabled', atEnd);
    }

    prevBtn?.addEventListener('click', () => {
      if (prevBtn.classList.contains('is-disabled')) return;
      track.scrollBy({ left: -getChipWidth(), behavior: 'smooth' });
    });
    nextBtn?.addEventListener('click', () => {
      if (nextBtn.classList.contains('is-disabled')) return;
      track.scrollBy({ left: getChipWidth(), behavior: 'smooth' });
    });

    track.addEventListener('scroll', updateArrows, { passive: true });
    updateArrows();

    const activeChip = track.querySelector('.is-active');
    if (activeChip) {
      requestAnimationFrame(() => {
        const trackRect = track.getBoundingClientRect();
        const chipRect = activeChip.getBoundingClientRect();
        if (chipRect.left < trackRect.left || chipRect.right > trackRect.right) {
          activeChip.scrollIntoView({ behavior: 'smooth', block: 'nearest', inline: 'center' });
        }
      });
    }
  }
})();
