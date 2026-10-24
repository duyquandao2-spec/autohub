/* ═══════════════════════════════════════════════════════════════
   AUTOHUB Experience Page — Interactive Layer
   Three.js 3D Viewer · GSAP Cinematic Motion · Vehicle Selection
   ═══════════════════════════════════════════════════════════════ */
(() => {
  'use strict';

  /* ── Vehicle data from embedded JSON ───────────────────── */
  const dataEl = document.getElementById('exp-vehicles-data');
  const vehicles = dataEl ? JSON.parse(dataEl.textContent) : [];
  let selectedVehicle = vehicles[0] || null;

  /* ── Scroll reveal (reuse from site.js pattern) ───────── */
  const revealObserver = new IntersectionObserver(
    (entries) => entries.forEach((e) => {
      if (e.isIntersecting) e.target.classList.add('is-visible');
    }),
    { threshold: 0.1 }
  );
  document.querySelectorAll('.reveal').forEach((el) => revealObserver.observe(el));

  /* ── Vehicle selector ──────────────────────────────────── */
  const selectorGrid = document.querySelector('[data-vehicle-selector]');
  const detailSection = document.querySelector('[data-exp-detail]');
  const allCards = selectorGrid ? selectorGrid.querySelectorAll('[data-vehicle-card]') : [];

  function selectVehicle(card) {
    allCards.forEach((c) => c.classList.remove('is-selected'));
    card.classList.add('is-selected');

    const slug = card.dataset.slug;
    selectedVehicle = vehicles.find((v) => v.slug === slug) || null;
    if (!selectedVehicle) return;

    /* Show detail panel */
    if (detailSection) {
      detailSection.hidden = false;
      detailSection.scrollIntoView({ behavior: 'smooth', block: 'start' });
    }

    /* Update detail fields */
    setDetailField('[data-exp-image]', 'src', selectedVehicle.imagePath);
    setDetailField('[data-exp-image]', 'alt', selectedVehicle.name);
    setText('[data-exp-brand-code]', `${selectedVehicle.brandName} · ${selectedVehicle.variant}`);
    setText('[data-exp-name]', selectedVehicle.name);
    setText('[data-exp-kind]', `${selectedVehicle.category} · ${selectedVehicle.drivetrain}`);
    setText('[data-exp-power]', selectedVehicle.powerDisplay);
    setText('[data-exp-accel]', selectedVehicle.accelerationDisplay);
    setText('[data-exp-speed]', selectedVehicle.topSpeedDisplay);
    setText('[data-exp-fuel-label]', selectedVehicle.fuelLabel);
    setText('[data-exp-fuel]', selectedVehicle.fuelValue || '-');
    setText('[data-exp-fuel-desc]', selectedVehicle.fuelDesc);
    setText('[data-exp-drivetrain]', selectedVehicle.drivetrain);
    setText('[data-exp-seats]', `${selectedVehicle.seats} chỗ ngồi`);
    setText('[data-exp-price]', selectedVehicle.priceDisplay);
    setText('[data-exp-readout-name]', selectedVehicle.name);

    /* Update links */
    const testdriveLink = document.querySelector('[data-exp-testdrive]');
    if (testdriveLink) testdriveLink.href = `/TestDrive?vehicle=${selectedVehicle.slug}`;
    const detailLink = document.querySelector('[data-exp-detail-link]');
    if (detailLink) detailLink.href = `/Vehicles/${selectedVehicle.slug}`;

    /* Source */
    const sourceEl = document.querySelector('[data-exp-source]');
    if (sourceEl) {
      sourceEl.hidden = false;
      setText('[data-exp-source-name]', `${selectedVehicle.brandName} Việt Nam`);
    }

    /* Trajectory animation */
    const traj = document.querySelector('.exp-detail__trajectory');
    if (traj) {
      traj.classList.remove('is-animate');
      void traj.offsetWidth;
      traj.classList.add('is-animate');
    }

    /* Update 3D viewer color if available */
    if (window.__exp3d && window.__exp3d.setColor) {
      window.__exp3d.setColor(getVehicleAccent(selectedVehicle));
    }

    /* GSAP stagger reveal for spec cards */
    if (typeof gsap !== 'undefined') {
      gsap.from('.exp-spec-card', {
        y: 20, opacity: 0, duration: 0.4, stagger: 0.06, ease: 'power2.out',
        clearProps: 'all'
      });
    }
  }

  function setDetailField(selector, attr, value) {
    const el = document.querySelector(selector);
    if (el) el.setAttribute(attr, value || '');
  }
  function setText(selector, value) {
    const el = document.querySelector(selector);
    if (el) el.textContent = value || '';
  }
  function getVehicleAccent(v) {
    const brandColors = {
      'BMW': 0x1b69b5, 'Mercedes-Benz': 0x222222, 'Audi': 0xbb0a30,
      'Toyota': 0xeb0a1e, 'Honda': 0xcc0000, 'Mazda': 0x910a2a,
      'Lexus': 0x1a1a2e, 'Porsche': 0xb1252b, 'Volvo': 0x003057,
      'Ford': 0x003478, 'VinFast': 0x0057b8, 'Hyundai': 0x002c5f,
      'Kia': 0x05141f, 'Volkswagen': 0x001e50, 'Nissan': 0xc3002f
    };
    return brandColors[v.brandName] || 0xe7332d;
  }

  /* Bind clicks */
  allCards.forEach((card) => {
    card.addEventListener('click', () => selectVehicle(card));
    card.addEventListener('keydown', (e) => {
      if (e.key === 'Enter' || e.key === ' ') { e.preventDefault(); selectVehicle(card); }
    });
  });

  /* Auto-select first */
  if (allCards.length > 0) {
    allCards[0].classList.add('is-selected');
    if (selectedVehicle && detailSection) {
      detailSection.hidden = false;
      setDetailField('[data-exp-image]', 'src', selectedVehicle.imagePath);
      setDetailField('[data-exp-image]', 'alt', selectedVehicle.name);
      setText('[data-exp-brand-code]', `${selectedVehicle.brandName} · ${selectedVehicle.variant}`);
      setText('[data-exp-name]', selectedVehicle.name);
      setText('[data-exp-kind]', `${selectedVehicle.category} · ${selectedVehicle.drivetrain}`);
      setText('[data-exp-power]', selectedVehicle.powerDisplay);
      setText('[data-exp-accel]', selectedVehicle.accelerationDisplay);
      setText('[data-exp-speed]', selectedVehicle.topSpeedDisplay);
      setText('[data-exp-fuel-label]', selectedVehicle.fuelLabel);
      setText('[data-exp-fuel]', selectedVehicle.fuelValue || '-');
      setText('[data-exp-fuel-desc]', selectedVehicle.fuelDesc);
      setText('[data-exp-drivetrain]', selectedVehicle.drivetrain);
      setText('[data-exp-seats]', `${selectedVehicle.seats} chỗ ngồi`);
      setText('[data-exp-price]', selectedVehicle.priceDisplay);
      setText('[data-exp-readout-name]', selectedVehicle.name);
      const testdriveLink = document.querySelector('[data-exp-testdrive]');
      if (testdriveLink) testdriveLink.href = `/TestDrive?vehicle=${selectedVehicle.slug}`;
      const detailLink = document.querySelector('[data-exp-detail-link]');
      if (detailLink) detailLink.href = `/Vehicles/${selectedVehicle.slug}`;
      const sourceEl = document.querySelector('[data-exp-source]');
      if (sourceEl) {
        sourceEl.hidden = false;
        setText('[data-exp-source-name]', `${selectedVehicle.brandName} Việt Nam`);
      }
    }
  }

  /* ═══════════════════════════════════════════════════════════════
     THREE.JS 3D VIEWER — Showroom with realistic GLB car model
     ═══════════════════════════════════════════════════════════════ */
  const canvas = document.querySelector('[data-3d-canvas]');
  const loadingEl = document.querySelector('[data-3d-loading]');

  if (!canvas || typeof THREE === 'undefined') {
    if (loadingEl) loadingEl.hidden = true;
  } else {
    initThreeJS();
  }

  function initThreeJS() {
    const container = canvas.parentElement;
    let width = container.clientWidth;
    let height = container.clientHeight;

    /* Renderer */
    const renderer = new THREE.WebGLRenderer({
      canvas, antialias: true, alpha: false,
      powerPreference: 'high-performance'
    });
    renderer.setSize(width, height);
    renderer.setPixelRatio(Math.min(window.devicePixelRatio, 2));
    renderer.shadowMap.enabled = true;
    renderer.shadowMap.type = THREE.PCFSoftShadowMap;
    renderer.toneMapping = THREE.ACESFilmicToneMapping;
    renderer.toneMappingExposure = 1.15;
    renderer.outputEncoding = THREE.sRGBEncoding;

    /* Scene */
    const scene = new THREE.Scene();
    scene.background = new THREE.Color(0x080a0c);
    scene.fog = new THREE.FogExp2(0x080a0c, 0.013);

    /* Camera */
    const camera = new THREE.PerspectiveCamera(40, width / height, 0.1, 200);
    camera.position.set(5, 2.8, 6);

    /* Controls */
    let controls = null;
    if (THREE.OrbitControls) {
      controls = new THREE.OrbitControls(camera, canvas);
      controls.enableDamping = true;
      controls.dampingFactor = 0.06;
      controls.enablePan = true;
      controls.panSpeed = 0.6;
      controls.minDistance = 3;
      controls.maxDistance = 18;
      controls.maxPolarAngle = Math.PI / 2.1;
      controls.target.set(0, 0.6, 0);
      controls.update();
    }

    /* ── Lighting ──────────────────────────────────────── */
    const ambientLight = new THREE.AmbientLight(0x404858, 0.5);
    scene.add(ambientLight);

    const keyLight = new THREE.DirectionalLight(0xffffff, 1.3);
    keyLight.position.set(5, 8, 4);
    keyLight.castShadow = true;
    keyLight.shadow.mapSize.set(1024, 1024);
    keyLight.shadow.camera.near = 0.5;
    keyLight.shadow.camera.far = 30;
    keyLight.shadow.camera.left = -8;
    keyLight.shadow.camera.right = 8;
    keyLight.shadow.camera.top = 8;
    keyLight.shadow.camera.bottom = -8;
    keyLight.shadow.bias = -0.0008;
    keyLight.shadow.normalBias = 0.02;
    scene.add(keyLight);

    const fillLight = new THREE.DirectionalLight(0x8899bb, 0.4);
    fillLight.position.set(-4, 3, -3);
    scene.add(fillLight);

    const rimLight = new THREE.PointLight(0xe7332d, 0.6, 15);
    rimLight.position.set(-3, 2, -4);
    scene.add(rimLight);

    const bottomGlow = new THREE.PointLight(0x1a2a40, 0.3, 10);
    bottomGlow.position.set(0, -1, 0);
    scene.add(bottomGlow);

    const spotLight = new THREE.SpotLight(0xffffff, 0.6, 22, Math.PI / 6, 0.5, 1);
    spotLight.position.set(0, 7, 5);
    spotLight.target.position.set(0, 0.6, 0);
    scene.add(spotLight);
    scene.add(spotLight.target);

    /* ── Showroom Floor ────────────────────────────────── */
    const floorGeo = new THREE.PlaneGeometry(40, 40);
    const floorMat = new THREE.MeshStandardMaterial({
      color: 0x0c0f12, roughness: 0.32, metalness: 0.65,
    });
    const floor = new THREE.Mesh(floorGeo, floorMat);
    floor.rotation.x = -Math.PI / 2;
    floor.position.y = -0.01;
    floor.receiveShadow = true;
    scene.add(floor);

    /* Floor grid */
    const gridHelper = new THREE.GridHelper(40, 40, 0x1a2028, 0x12161a);
    gridHelper.position.y = 0.005;
    scene.add(gridHelper);

    /* ── Car Model (GLB, no procedural fallback) ───────── */
    const carGroup = new THREE.Group();
    scene.add(carGroup);

    let loadedCar = null;
    let carModelBoundingBox = { width: 4.6, height: 1.4, depth: 2.1 };
    let doorSupport = false;
    let interiorSupport = false;
    let modelLoadFailed = false;

    const MODEL_URL = '/models/cars/hero-car.glb';

    function showModelError(message) {
      modelLoadFailed = true;
      const err = document.querySelector('[data-3d-error]');
      if (!err) return;
      err.hidden = false;
      const msgEl = document.querySelector('[data-3d-error-msg]');
      if (msgEl) msgEl.textContent = message || 'Vehicle 3D model is not installed. Need a real GLB/GLTF vehicle asset.';
    }

    function loadCar() {
      if (!THREE.GLTFLoader) {
        if (loadingEl) loadingEl.hidden = true;
        showModelError('Vehicle 3D model loader is not available.');
        return;
      }

      const loader = new THREE.GLTFLoader();
      loader.load(
        MODEL_URL,
        (gltf) => {
          const model = gltf.scene || gltf.scenes[0];
          if (!model) {
            if (loadingEl) loadingEl.hidden = true;
            showModelError();
            return;
          }

          const box = new THREE.Box3().setFromObject(model);
          const size = new THREE.Vector3();
          box.getSize(size);
          const center = new THREE.Vector3();
          box.getCenter(center);
          const targetLen = 5.0;
          const scale = targetLen / Math.max(size.x, size.z, 0.001);
          model.scale.setScalar(scale);
          model.position.x = -center.x * scale;
          model.position.y = -box.min.y * scale;
          model.position.z = -center.z * scale;

          carModelBoundingBox = {
            width: size.x * scale,
            height: size.y * scale,
            depth: size.z * scale,
          };

          model.traverse((o) => {
            if (o.isMesh) {
              o.castShadow = true;
              const m = Array.isArray(o.material) ? o.material[0] : o.material;
              if (m && m.emissive && (m.emissive.r + m.emissive.g + m.emissive.b > 0.01)) {
                o.userData.isEmissive = true;
              }
            }
          });

          carGroup.add(model);
          loadedCar = model;

          const animNames = (gltf.animations || []).map((a) => a.name || '(unnamed)');
          doorSupport = (gltf.animations || []).some((a) =>
            /door|door_open|door_close/i.test(a.name || '')
          );
          interiorSupport = (gltf.animations || []).some((a) =>
            /interior|camera/i.test(a.name || '')
          );

          if (window.__exp3d && window.__exp3d.setOnModelLoaded) {
            window.__exp3d.setOnModelLoaded({
              colorMaterial: null,
              doorSupport,
              interiorSupport,
            });
          }

          applyCapabilitiesToUI();

          buildHotspots();
          updatePresetsForModel();

          if (loadingEl) loadingEl.hidden = true;
        },
        (evt) => {
          if (loadingEl && evt.total) {
            const pct = Math.round((evt.loaded / evt.total) * 100);
            const pctEl = document.querySelector('[data-3d-loading-pct]');
            if (pctEl) pctEl.textContent = pct + '%';
          }
        },
        (err) => {
          console.error('[AUTOHUB experience] GLB load failed:', err);
          if (loadingEl) loadingEl.hidden = true;
          showModelError();
        }
      );
    }

    function applyCapabilitiesToUI() {
      const interiorBtn = document.querySelector('[data-3d-preset="interior"]');
      if (interiorBtn) {
        interiorBtn.disabled = !interiorSupport;
        interiorBtn.title = interiorSupport ? 'Nội thất' : 'Interior unavailable';
      }
    }

    function updatePresetsForModel() {
      const bb = carModelBoundingBox;
      const halfLen = bb.depth / 2;
      const halfW = bb.width / 2;
      const h = bb.height;
      presets = {
        front:    { pos: [0, h * 0.42, halfLen * 2.5], target: [0, h * 0.3, 0] },
        side:     { pos: [halfW * 2.6, h * 0.4, 0],    target: [0, h * 0.3, 0] },
        rear:     { pos: [0, h * 0.42, -halfLen * 2.5], target: [0, h * 0.3, 0] },
        top:      { pos: [0, h * 2.4, 0.3],            target: [0, 0, 0] },
        interior: { pos: [0, h * 0.5, 0],              target: [0.3, h * 0.5, 0] },
      };
      if (controls) {
        controls.minDistance = Math.max(2, halfLen * 0.8);
        controls.maxDistance = Math.max(10, halfLen * 4);
        controls.target.set(0, h * 0.3, 0);
        controls.update();
      }
      animateCamera(presets.front);
    }

    /* ── Hotspots (relative to loaded model) ───────────── */
    let hotspotMeshes = [];
    function buildHotspots() {
      hotspotMeshes.forEach((m) => scene.remove(m));
      hotspotMeshes = [];

      const bb = carModelBoundingBox;
      const f = bb.depth / 2;
      const s = bb.width / 2;
      const h = bb.height;

      const hotspots = [
        { label: 'Đầu xe', desc: 'Hệ thống đèn pha LED thích ứng, thiết kế thể thao đặc trưng.', pos: [-f * 0.95, h * 0.5, 0], color: 0xe7332d },
        { label: 'La-zăng hợp kim', desc: 'La-zăng hợp kim nhẹ, thiết kế đa chấu thể thao, kết hợp lốp hiệu năng cao.', pos: [-f * 0.45, h * 0.18, s * 1.05], color: 0xe7332d },
        { label: 'Đuôi xe', desc: 'Đèn hậu LED liền mạch thể hiện chiều rộng, cánh gió tích hợp thể thao.', pos: [f * 0.95, h * 0.5, 0], color: 0xe7332d },
        { label: 'Khoang lái', desc: 'Vị trí ngồi thể thao, vô-lăng tích hợp điều khiển, màn hình lái kỹ thuật số.', pos: [f * 0.15, h * 0.75, 0], color: 0xe7332d },
      ];

      const raycaster = new THREE.Raycaster();
      const mouse = new THREE.Vector2();
      const hotspotInfoEl = document.querySelector('[data-hotspot-info]');

      hotspots.forEach((hs) => {
        const dotGeo = new THREE.SphereGeometry(0.08, 16, 16);
        const dotMat = new THREE.MeshBasicMaterial({ color: hs.color, transparent: true, opacity: 0.9 });
        const dot = new THREE.Mesh(dotGeo, dotMat);
        dot.position.set(...hs.pos);
        dot.userData = { label: hs.label, desc: hs.desc };
        scene.add(dot);
        hotspotMeshes.push(dot);

        const ringGeo = new THREE.RingGeometry(0.1, 0.16, 32);
        const ringMat = new THREE.MeshBasicMaterial({
          color: hs.color, transparent: true, opacity: 0.3, side: THREE.DoubleSide
        });
        const ring = new THREE.Mesh(ringGeo, ringMat);
        ring.position.set(...hs.pos);
        ring.lookAt(camera.position);
        ring.userData.isPulse = true;
        scene.add(ring);
      });

      canvas.addEventListener('click', (e) => {
        const rect = canvas.getBoundingClientRect();
        mouse.x = ((e.clientX - rect.left) / rect.width) * 2 - 1;
        mouse.y = -((e.clientY - rect.top) / rect.height) * 2 + 1;
        raycaster.setFromCamera(mouse, camera);
        const intersects = raycaster.intersectObjects(hotspotMeshes);
        if (intersects.length > 0 && hotspotInfoEl) {
          const hs = intersects[0].object.userData;
          hotspotInfoEl.hidden = false;
          document.querySelector('[data-hotspot-title]').textContent = hs.label;
          document.querySelector('[data-hotspot-desc]').textContent = hs.desc;
        }
      });
    }

    document.querySelector('[data-hotspot-close]')?.addEventListener('click', () => {
      const hotspotInfoEl = document.querySelector('[data-hotspot-info]');
      if (hotspotInfoEl) hotspotInfoEl.hidden = true;
    });

    /* ── Camera presets ────────────────────────────────── */
    let presets = {
      front:    { pos: [0, 1.8, 6],    target: [0, 0.6, 0] },
      side:     { pos: [6, 1.5, 0],    target: [0, 0.6, 0] },
      rear:     { pos: [0, 1.8, -6],   target: [0, 0.6, 0] },
      top:      { pos: [0, 7, 0.5],    target: [0, 0.4, 0] },
      interior: { pos: [0, 1.3, 0],    target: [0.3, 1.0, 0] },
    };

    function animateCamera(preset) {
      if (typeof gsap !== 'undefined') {
        gsap.to(camera.position, {
          x: preset.pos[0], y: preset.pos[1], z: preset.pos[2],
          duration: 1.4, ease: 'power3.inOut',
          onUpdate: () => { if (controls) controls.update(); }
        });
        if (controls) {
          gsap.to(controls.target, {
            x: preset.target[0], y: preset.target[1], z: preset.target[2],
            duration: 1.4, ease: 'power3.inOut'
          });
        }
      } else {
        camera.position.set(...preset.pos);
        if (controls) {
          controls.target.set(...preset.target);
          controls.update();
        }
      }
    }

    document.querySelectorAll('[data-3d-preset]').forEach((btn) => {
      btn.addEventListener('click', () => {
        if (btn.disabled) return;
        document.querySelectorAll('[data-3d-preset]').forEach((b) => {
          b.classList.remove('is-active');
          b.setAttribute('aria-selected', 'false');
        });
        btn.classList.add('is-active');
        btn.setAttribute('aria-selected', 'true');
        const preset = presets[btn.dataset.preset];
        if (preset) animateCamera(preset);
      });
    });

    /* ── Controls ──────────────────────────────────────── */
    let autoRotating = false;

    document.querySelector('[data-3d-reset]')?.addEventListener('click', () => {
      animateCamera(presets.front);
      document.querySelectorAll('[data-3d-preset]').forEach((b) => {
        b.classList.remove('is-active');
        b.setAttribute('aria-selected', 'false');
      });
      const frontBtn = document.querySelector('[data-3d-preset="front"]');
      if (frontBtn) { frontBtn.classList.add('is-active'); frontBtn.setAttribute('aria-selected', 'true'); }
    });

    document.querySelector('[data-3d-zoom-in]')?.addEventListener('click', () => {
      if (typeof gsap !== 'undefined') {
        gsap.to(camera.position, {
          z: camera.position.z - 1.5, duration: 0.6, ease: 'power2.out',
          onUpdate: () => { if (controls) controls.update(); }
        });
      } else {
        camera.position.z -= 1.5;
        if (controls) controls.update();
      }
    });

    document.querySelector('[data-3d-zoom-out]')?.addEventListener('click', () => {
      if (typeof gsap !== 'undefined') {
        gsap.to(camera.position, {
          z: camera.position.z + 1.5, duration: 0.6, ease: 'power2.out',
          onUpdate: () => { if (controls) controls.update(); }
        });
      } else {
        camera.position.z += 1.5;
        if (controls) controls.update();
      }
    });

    const autoRotateBtn = document.querySelector('[data-3d-auto-rotate]');
    autoRotateBtn?.addEventListener('click', () => {
      autoRotating = !autoRotating;
      autoRotateBtn.classList.toggle('is-active', autoRotating);
      if (controls) controls.autoRotate = autoRotating;
      if (controls) controls.autoRotateSpeed = 1.5;
    });

    /* ── Load the GLB model ────────────────────────────── */
    loadCar();

    /* ── Animation Loop ────────────────────────────────── */
    let time = 0;
    function animate() {
      requestAnimationFrame(animate);
      time += 0.01;

      if (controls) controls.update();

      /* Pulse hotspot rings */
      scene.children.forEach((child) => {
        if (child.userData?.isPulse) {
          child.lookAt(camera.position);
          const scale = 1 + Math.sin(time * 3) * 0.3;
          child.scale.set(scale, scale, scale);
          child.material.opacity = 0.3 - Math.sin(time * 3) * 0.15;
        }
      });

      /* Subtle car breathing */
      if (loadedCar) {
        carGroup.position.y = Math.sin(time * 0.8) * 0.008;
      }

      renderer.render(scene, camera);
    }
    animate();

    /* ── Resize ────────────────────────────────────────── */
    function onResize() {
      width = container.clientWidth;
      height = container.clientHeight;
      camera.aspect = width / height;
      camera.updateProjectionMatrix();
      renderer.setSize(width, height);
    }
    window.addEventListener('resize', onResize);

    /* ── Public API for vehicle color updates ──────────── */
    window.__exp3d = {
      setColor(hex) {
        if (!loadedCar) return;
        loadedCar.traverse((o) => {
          if (!o.isMesh) return;
          const m = Array.isArray(o.material) ? o.material : [o.material];
          m.forEach((mat) => {
            const n = (mat.name || '').toLowerCase();
            if (/paint|body|exterior|coloured|metal/i.test(n) &&
                !/glass|window|light|chrome|grille|badge|carbon|interior|seat|trim|rubber|tire/i.test(n)) {
              mat.color.setHex(hex);
            }
          });
        });
      },
      setOnModelLoaded(info) {
        applyCapabilitiesToUI();
      }
    };
  }

  /* ═══════════════════════════════════════════════════════════════
     GSAP SCROLL ANIMATIONS
     ═══════════════════════════════════════════════════════════════ */
  function initGSAP() {
    if (typeof gsap === 'undefined' || typeof ScrollTrigger === 'undefined') return;

    gsap.registerPlugin(ScrollTrigger);

    /* Hero parallax */
    gsap.to('.exp-hero__mark', {
      y: -60, opacity: 0.02,
      scrollTrigger: { trigger: '.exp-hero', start: 'top top', end: 'bottom top', scrub: 1 }
    });

    /* Category cards stagger */
    gsap.utils.toArray('.exp-cat-card').forEach((card, i) => {
      gsap.from(card, {
        y: 40, opacity: 0, duration: 0.6,
        scrollTrigger: { trigger: card, start: 'top 85%', toggleActions: 'play none none none' },
        delay: (i % 3) * 0.1
      });
    });

    /* Showroom cards */
    gsap.utils.toArray('.exp-showroom-card').forEach((card, i) => {
      gsap.from(card, {
        y: 30, opacity: 0, duration: 0.5,
        scrollTrigger: { trigger: card, start: 'top 85%', toggleActions: 'play none none none' },
        delay: i * 0.08
      });
    });

    /* 3D viewer entrance */
    gsap.from('.exp-3d-viewer', {
      scale: 0.95, opacity: 0, duration: 0.8, ease: 'power2.out',
      scrollTrigger: { trigger: '.exp-3d-viewer', start: 'top 80%', toggleActions: 'play none none none' }
    });

    /* CTA section */
    gsap.from('.exp-cta > div', {
      y: 40, opacity: 0, duration: 0.6, stagger: 0.15, ease: 'power2.out',
      scrollTrigger: { trigger: '.exp-cta', start: 'top 80%', toggleActions: 'play none none none' }
    });
  }

  /* Wait for GSAP to load */
  if (typeof gsap !== 'undefined') {
    initGSAP();
  } else {
    window.addEventListener('load', initGSAP);
  }

})();
