/* ═══════════════════════════════════════════════════════════════
   AUTOHUB Vehicle Showcase — Orchestrator
   Opens a centered premium 3D overlay from the
   "Mở dữ liệu hiệu năng" button. Lazy-loads Three.js/GSAP,
   fetches real vehicle data, binds controls, manages lifecycle.
   Exposes: window.openVehicleShowcase()
   ═══════════════════════════════════════════════════════════════ */
(function (root) {
  'use strict';

  const CDN = {
    three:   'https://cdnjs.cloudflare.com/ajax/libs/three.js/r128/three.min.js',
    orbit:   'https://cdn.jsdelivr.net/npm/three@0.128.0/examples/js/controls/OrbitControls.js',
    gltf:    'https://cdn.jsdelivr.net/npm/three@0.128.0/examples/js/loaders/GLTFLoader.js',
    gsap:    'https://cdnjs.cloudflare.com/ajax/libs/gsap/3.12.5/gsap.min.js',
  };

  const DOM = {
    overlay: () => document.querySelector('[data-vsc-overlay]'),
    shell:   () => document.querySelector('[data-vsc-shell]'),
    canvas:  () => document.querySelector('[data-vsc-canvas]'),
    close:   () => document.querySelector('[data-vsc-close]'),
    loading: () => document.querySelector('[data-vsc-loading]'),
    loadingBar: () => document.querySelector('[data-vsc-loading-bar]'),
    loadingPct: () => document.querySelector('[data-vsc-loading-pct]'),
    error:   () => document.querySelector('[data-vsc-error]'),
    errorMsg:() => document.querySelector('[data-vsc-error-msg]'),
    retry:   () => document.querySelector('[data-vsc-retry]'),
    quit:    () => document.querySelector('[data-vsc-quit]'),
    hint:    () => document.querySelector('[data-vsc-hint]'),
    specsGrid: () => document.querySelector('[data-vsc-specs-grid]'),
    specsName: () => document.querySelector('[data-vsc-specs-name]'),
    colors:  () => document.querySelector('[data-vsc-colors]'),
    hotspot: () => document.querySelector('[data-vsc-hotspot]'),
    hotTitle:() => document.querySelector('[data-vsc-hotspot-title]'),
    hotDesc: () => document.querySelector('[data-vsc-hotspot-desc]'),
    rpmVal:  () => document.querySelector('[data-vsc-rpm-value]'),
    rpmBar:  () => document.querySelector('[data-vsc-rpm-bar]'),
    rpm:     () => document.querySelector('[data-vsc-rpm]'),
    panels:  () => document.querySelectorAll('[data-vsc-panel]'),
  };

  const state = {
    open: false,
    loading: false,
    ready: false,
    failed: false,
    vehicle: null,
    reducedMotion: false,
    isMobile: false,
    audioInited: false,
    introTimer: null,
  };

  const colorsPreset = [
    { name: 'Graphite', hex: 0x2a2d30 },
    { name: 'Đỏ',       hex: 0x8a1f1f },
    { name: 'Trắng',    hex: 0xcdd0d1 },
    { name: 'Bạc',      hex: 0x9aa0a3 },
    { name: 'Xanh',     hex: 0x1f3a5c },
    { name: 'Đen',      hex: 0x111213 },
  ];

  /* ── Helper: inject a script and resolve on load ─────────── */
  function loadScript(src) {
    return new Promise((resolve, reject) => {
      const s = document.createElement('script');
      s.src = src;
      s.async = true;
      s.onload = () => resolve();
      s.onerror = () => reject(new Error('Failed to load ' + src));
      document.head.appendChild(s);
    });
  }

  /* ── Lazy-load required libraries ────────────────────────── */
  let libsPromise = null;
  function ensureLibs() {
    if (!libsPromise) {
      const tasks = [];
      if (typeof root.THREE === 'undefined' || !root.THREE.Scene) {
        tasks.push(loadScript(CDN.three));
      }
      libsPromise = (async () => {
        if (typeof root.THREE === 'undefined' || !root.THREE.Scene) {
          await tasks[0];
        }
        // wait for THREE to exist
        const waitThree = () => new Promise((res) => {
          if (root.THREE && root.THREE.Scene) return res();
          const iv = setInterval(() => {
            if (root.THREE && root.THREE.Scene) { clearInterval(iv); res(); }
          }, 50);
        });
        await waitThree();

        if (!root.THREE.OrbitControls) await loadScript(CDN.orbit);
        if (!root.THREE.GLTFLoader) await loadScript(CDN.gltf);
        if (typeof root.gsap === 'undefined') {
          await loadScript(CDN.gsap).catch(() => {});
        }
      })();
    }
    return libsPromise;
  }

  /* ── Vehicle data ────────────────────────────────────────── */
  async function loadVehicle() {
    const res = await fetch('/api/vehicles');
    const list = await res.json();
    return Array.isArray(list) && list.length
      ? (list.find((v) => v.isFeatured) || list[0])
      : null;
  }

  /* ── Build specs panel ───────────────────────────────────── */
  function buildSpecs(v) {
    const grid = DOM.specsGrid();
    if (!grid) return;
    const rows = [
      ['CÔNG SUẤT', v.power || v.powerDisplay || '-'],
      ['MÔ-MEN XOẮN', v.torque ? v.torque + ' Nm' : '-'],
      ['0-100 KM/H', v.accelerationDisplay || (v.acceleration ? v.acceleration + 's' : '-')],
      ['TỐC ĐỘ TỐI ĐA', v.topSpeedDisplay || '-'],
      ['HỆ DẪN ĐỘNG', v.drivetrain || '-'],
      ['NHIÊN LIỆU', v.fuelType || '-'],
      ['TRUYỀN ĐỘNG', v.transmission || '-'],
      ['GIÁ THAM KHẢO', v.priceDisplay || '-'],
    ];
    grid.innerHTML = rows
      .map(([label, value]) => `<div class="vsc-spec"><small>${label}</small><strong>${value}</strong></div>`)
      .join('');
    const name = DOM.specsName();
    if (name) name.textContent = v.name || 'Thông số';
  }

  /* ── Build color configurator ────────────────────────────── */
  function buildColors() {
    const box = DOM.colors();
    if (!box) return;
    box.innerHTML = colorsPreset.map((c, i) =>
      `<button type="button" class="vsc-color${i === 1 ? ' is-active' : ''}" data-vsc-color="${c.hex}" style="background:#${c.hex.toString(16).padStart(6, '0')}" aria-label="Chọn màu ${c.name}" title="${c.name}"></button>`
    ).join('');
    box.querySelectorAll('[data-vsc-color]').forEach((btn) => {
      btn.addEventListener('click', () => {
        box.querySelectorAll('[data-vsc-color]').forEach((b) => b.classList.remove('is-active'));
        btn.classList.add('is-active');
        root.AUTOHUB.scene.setBodyColor(parseInt(btn.dataset.vscColor, 10));
      });
    });
  }

  /* ── Open / close ────────────────────────────────────────── */
  async function openShowcase() {
    if (state.open) return;
    state.open = true;
    state.failed = false;

    const overlay = DOM.overlay();
    if (!overlay) return;

    // Lock body scroll, block background interaction
    document.body.classList.add('vsc-locked');
    overlay.setAttribute('aria-hidden', 'false');
    overlay.classList.remove('is-closing');
    overlay.classList.add('is-open');

    // reset inner states
    DOM.error().classList.remove('is-show');
    DOM.loading().classList.remove('is-done');
    setLoading(0);

    showPhase('loading');

    try {
      await ensureLibs();

      // fetch vehicle data (non-blocking for scene; specs fill when ready)
      loadVehicle().then((v) => { state.vehicle = v; if (v) buildSpecs(v); }).catch(() => {});

      await initScene();

      const hint = DOM.hint();
      if (!state.reducedMotion) {
        hint.classList.add('is-show');
        setTimeout(() => hint.classList.remove('is-show'), 4000);
      }
    } catch (err) {
      console.error('[AUTOHUB showcase] init failed:', err);
      failExperience();
    }
  }

  function initScene() {
    return new Promise((resolve, reject) => {
      const canvas = DOM.canvas();
      const stage = canvas.parentElement;
      if (!canvas || !stage) return reject(new Error('stage missing'));

      const sc = root.AUTOHUB.scene;
      sc.init(stage, canvas, {
        reducedMotion: state.reducedMotion,
        isMobile: state.isMobile,
        bodyColor: colorsPreset[1].hex,
      }, {
        onError: (message) => {
          state.failed = true;
          failExperience(
            'Vehicle 3D model is not installed. ' +
            'Need a real GLB/GLTF vehicle asset in /models/cars/.' +
            (message ? ' (' + message + ')' : '')
          );
        },
        onProgress: setLoading,
        onModelLoaded: (modelInfo) => {
          applyModelCapabilities(modelInfo);
        },
        onReady: () => {
          // after car ready, mark UI ready
          setTimeout(() => {
            state.ready = true;
            finishIntro();
            DOM.shell().classList.add('is-ready');
          }, state.reducedMotion ? 50 : 300);
        },
        onEntered: () => {
          finishIntro();
        },
      });
      resolve();
    });
  }

  function finishIntro() {
    if (state.introTimer) clearTimeout(state.introTimer);
    DOM.loading().classList.add('is-done');
    state.loading = false;
    bindControls();
    applyKeyboardState();
  }

  function setLoading(pct) {
    const bar = DOM.loadingBar();
    const p = DOM.loadingPct();
    if (bar) bar.style.width = pct + '%';
    if (p) p.textContent = pct + '%';
  }

  let modelCapabilities = {
    doorSupport: false,
    hasInteriorAnim: false,
    interiorSupport: false,
    animationCount: 0,
  };

  function applyModelCapabilities(info) {
    modelCapabilities = Object.assign(modelCapabilities, info || {});

    const doorBtn = document.querySelector('[data-vsc-action="doors"]');
    if (doorBtn) {
      const supported = !!modelCapabilities.doorSupport;
      doorBtn.disabled = !supported;
      doorBtn.title = supported
        ? 'Mở đóng cửa xe'
        : 'Door animation unavailable';
      if (!supported) {
        doorBtn.classList.remove('is-active');
      }
    }

    const interiorSupported = !!modelCapabilities.interiorSupport;
    const interiorBtn = document.querySelector('[data-vsc-action="interior"]');
    if (interiorBtn) {
      interiorBtn.disabled = !interiorSupported;
      interiorBtn.title = interiorSupported
        ? 'Xem nội thất'
        : 'Interior unavailable';
    }

    const animNames = modelCapabilities.animationNames || [];
    console.log('[AUTOHUB showcase] Model animation clips:', animNames);
  }

  function failExperience(msg) {
    state.failed = true;
    DOM.loading().classList.add('is-done');
    DOM.errorMsg().textContent = msg || 'Không thể tải mô hình 3D. Vui lòng thử lại.';
    DOM.error().classList.add('is-show');
  }

  let toastTimer = null;
  function showToast(message) {
    let toast = document.querySelector('.vsc-toast');
    if (!toast) {
      toast = document.createElement('div');
      toast.className = 'vsc-toast';
      toast.setAttribute('role', 'status');
      const shell = DOM.shell();
      if (shell) shell.appendChild(toast);
      else document.body.appendChild(toast);
    }
    toast.textContent = message;
    toast.classList.add('is-show');
    if (toastTimer) clearTimeout(toastTimer);
    toastTimer = setTimeout(() => toast.classList.remove('is-show'), 2200);
  }

  function closeShowcase() {
    if (!state.open) return;
    state.open = false;
    state.ready = false;

    // stop audio
    root.AUTOHUB.audio.stopEngine();
    root.AUTOHUB.audio.shutdown();

    const overlay = DOM.overlay();
    const shell = DOM.shell();
    if (shell) shell.classList.remove('is-ready');
    if (overlay) {
      overlay.classList.add('is-closing');
      overlay.classList.remove('is-open');
      document.body.classList.remove('vsc-locked');
      overlay.setAttribute('aria-hidden', 'true');
      setTimeout(() => {
        overlay.classList.remove('is-closing');
        // destroy the scene to free GPU memory
        try { root.AUTOHUB.scene.dispose(); } catch (e) {}
      }, 450);
    }
    root.removeEventListener('keydown', onKey);
    DOM.shell().querySelectorAll('[data-vsc-panel].is-open').forEach((p) => p.classList.remove('is-open'));
  }

  /* ── Control handlers ────────────────────────────────────── */
  function togglePanel(name) {
    document.querySelectorAll(`[data-vsc-panel]`).forEach((p) => {
      const isTarget = p.getAttribute('data-vsc-panel') === name;
      p.classList.toggle('is-open', isTarget && !p.classList.contains('is-open'));
    });
    if (name === 'colors') {
      const c = DOM.colors();
      const willOpen = !c.classList.contains('is-open');
      c.classList.toggle('is-open');
      if (willOpen) buildColors();
    }
  }

  function bindControls() {
    if (state.bound) {
      applyKeyboardState();
      return;
    }
    state.bound = true;

    DOM.close().addEventListener('click', closeShowcase);
    DOM.retry().addEventListener('click', () => { state.failed = false; openShowcase(); });
    DOM.quit().addEventListener('click', closeShowcase);

    // actions
    document.querySelectorAll('[data-vsc-action]').forEach((btn) => {
      btn.addEventListener('click', () => {
        const action = btn.dataset.vscAction;
        const sc = root.AUTOHUB.scene;
        switch (action) {
          case 'rotate':
            sc.setAutoRotate(!sc.autoRotate);
            btn.classList.toggle('is-active', sc.autoRotate);
            sc._scheduleAutoRotate();
            break;
          case 'lights': {
            const on = !sc.lightsOn;
            sc.setLights(on);
            btn.classList.toggle('is-active', on);
            break;
          }
          case 'hazard': {
            const on = !sc.hazardOn;
            sc.setHazard(on);
            btn.classList.toggle('is-active', on);
            break;
          }
          case 'brake': {
            const on = !sc.brakeOn;
            sc.setBrake(on);
            btn.classList.toggle('is-active', on);
            break;
          }
          case 'doors': {
            if (!sc.hasDoorSupport()) {
              showToast('Door animation unavailable');
              break;
            }
            const ok = sc.doorsOpen ? sc.closeDoors() : sc.openDoors();
            if (ok) btn.classList.toggle('is-active');
            else showToast('Door animation unavailable');
            break;
          }
          case 'interior':
            if (btn.disabled) {
              showToast('Interior unavailable');
              break;
            }
            sc.goToPreset('interior');
            break;
          case 'engine': {
            const started = root.AUTOHUB.audio.startEngine();
            if (started) {
              btn.classList.add('is-active');
              DOM.rpm().hidden = false;
            } else {
              btn.classList.remove('is-active');
            }
            break;
          }
          case 'rev': {
            root.AUTOHUB.audio.ensure();
            root.AUTOHUB.audio.setRev(true);
            DOM.rpm().hidden = false;
            startRevLoops();
            break;
          }
          case 'specs':
            togglePanel('specs');
            break;
          case 'colors':
            togglePanel('colors');
            break;
        }
      });
    });

    // presets
    document.querySelectorAll('[data-vsc-preset]').forEach((btn) => {
      btn.addEventListener('click', () => {
        document.querySelectorAll('[data-vsc-preset]').forEach((b) => {
          b.classList.remove('is-active');
          b.setAttribute('aria-selected', 'false');
        });
        btn.classList.add('is-active');
        btn.setAttribute('aria-selected', 'true');
        root.AUTOHUB.scene.goToPreset(btn.dataset.vscPreset);
      });
    });

    // panel close buttons
    document.querySelectorAll('[data-vsc-panel-close]').forEach((btn) => {
      btn.addEventListener('click', () => {
        const panel = btn.closest('[data-vsc-panel]');
        if (panel) panel.classList.remove('is-open');
        DOM.colors().classList.remove('is-open');
      });
    });

    // outside interactions
    DOM.overlay().addEventListener('pointerdown', (e) => {
      if (e.target === DOM.overlay()) closeShowcase();
    });

    root.addEventListener('keydown', onKey);
  }

  let revTimer = null;
  function startRevLoops() {
    if (revTimer) return;
    const tap = () => { root.AUTOHUB.audio.setRev(false); if (revTimer) { clearInterval(revTimer); revTimer = null; } };
    // release on pointer up anywhere / key up
    root.addEventListener('pointerup', tap, { once: true });
    root.addEventListener('keyup', (e) => { if (e.key === 'Control' || true) tap(); }, { once: true });
    revTimer = setInterval(() => {
      // keep display in sync while held
    }, 100);
  }

  function applyKeyboardState() {
    // no-op: keyboard bound globally
  }

  function onKey(e) {
    if (!state.open) return;
    const tag = (e.target.tagName || '').toLowerCase();
    if (tag === 'input' || tag === 'textarea' || e.target.isContentEditable) return;

    const sc = root.AUTOHUB.scene;
    const audio = root.AUTOHUB.audio;
    switch (e.key.toLowerCase()) {
      case 'escape': closeShowcase(); e.preventDefault(); break;
      case 'l':
        sc.setLights(!sc.lightsOn);
        document.querySelector('[data-vsc-action="lights"]').classList.toggle('is-active', sc.lightsOn);
        break;
      case 'd':
        if (sc.hasDoorSupport()) { sc.doorsOpen ? sc.closeDoors() : sc.openDoors(); }
        else showToast('Door animation unavailable');
        break;
      case 'i':
        if (modelCapabilities.interiorSupport) sc.goToPreset('interior');
        else showToast('Interior unavailable');
        break;
      case 'm':
        audio.ensure();
        audio.setMuted(!audio.getMuted());
        break;
      case 'arrowleft':
        if (sc.controls) { sc.controls.autoRotate = false; sc.controls.rotateLeft(0.35); }
        break;
      case 'arrowright':
        if (sc.controls) { sc.controls.autoRotate = false; sc.controls.rotateLeft(-0.35); }
        break;
      case '+': case '=':
        if (sc.camera) sc.camera.position.multiplyScalar(0.92);
        break;
      case '-': case '_':
        if (sc.camera) sc.camera.position.multiplyScalar(1.08);
        break;
    }
  }

  /* ── RPM display loop ────────────────────────────────────── */
  let rpmLoop = null;
  function startRpmDisplay() {
    if (rpmLoop) return;
    rpmLoop = setInterval(() => {
      const rpm = root.AUTOHUB.audio.getRpm();
      const val = DOM.rpmVal();
      const bar = DOM.rpmBar();
      if (val) val.innerHTML = `${Math.round(rpm)} <span>RPM</span>`;
      if (bar) bar.style.width = Math.min(100, (rpm / 6800) * 100) + '%';
    }, 100);
  }
  // keep RPM display alive
  setInterval(() => {
    if (state.open && document.body.contains(DOM.rpmVal())) startRpmDisplay();
  }, 300);

  /* ── Phase label helper (visual only) ────────────────────── */
  function showPhase() {}

  /* ── Global open function ────────────────────────────────── */
  root.openVehicleShowcase = openShowcase;

  /* ── Wire the button(s) ──────────────────────────────────── */
  function bindTriggers() {
    document.querySelectorAll('[data-vsc-open]').forEach((btn) => {
      btn.addEventListener('click', (e) => {
        e.preventDefault();
        e.stopPropagation();
        openShowcase();
      });
    });
  }

  // expose for re-binding after AJAX page loads
  root.bindVehicleShowcaseTriggers = bindTriggers;

  /* ── Init on DOM ready ───────────────────────────────────── */
  function init() {
    state.reducedMotion = root.matchMedia && root.matchMedia('(prefers-reduced-motion: reduce)').matches;
    state.isMobile = (root.innerWidth || 0) <= 768;
    // handle orientation/resize for mobile flag
    root.addEventListener('resize', () => {
      state.isMobile = (root.innerWidth || 0) <= 768;
    });

    // Lazy-load libs once on first open (not on page load)
    bindTriggers();
  }

  if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', init);
  } else {
    init();
  }

  // Re-bind after SPA-style AJAX loads (site.js replaces #machines section)
  if (root.autohubLoadPage) {
    // no reliable hook; site.js calls window.autohubLoadPage. We re-bind on requestAnimationFrame ticks.
    const rb = () => { bindTriggers(); root.requestAnimationFrame(rb); };
    root.requestAnimationFrame(rb);
  }
})(window);
