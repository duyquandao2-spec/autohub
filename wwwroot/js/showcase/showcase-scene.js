/* ═══════════════════════════════════════════════════════════════
   AUTOHUB Vehicle Showcase — Three.js Scene
   Transparent display cube · showroom floor · studio lighting ·
   GLB car model · controls · presets · lights
   NOTE: depends on global THREE, OrbitControls, GLTFLoader, gsap
   which are lazy-loaded by vehicle-showcase.js before init().
   ═══════════════════════════════════════════════════════════════ */
(function (root) {
  'use strict';

  const MODEL_URL = '/models/cars/hero-car.glb';

  const SceneModule = {
    renderer: null,
    scene: null,
    camera: null,
    controls: null,
    mixers: [],
    clock: null,
    rafId: null,
    carGroup: null,
    carModel: null,
    carAnimations: null,
    cubeGroup: null,
    lightObjects: null,
    hotspots: [],
    floorMirror: null,
    reducedMotion: false,
    autoRotate: true,
    autoRotateTimer: null,
    lightsOn: false,
    hazardOn: false,
    brakeOn: false,
    doorsOpen: false,
    doorSupport: false,
    colorSupport: false,
    bodyColor: 0x8a1f1f,
    isMobile: false,
    entryDone: false,
    modelBoundingBox: null,
    variants: [],
    activeVariant: 0,
    doorNodes: [],
    doorBaseRotation: null,
    doorPivotSupport: false,
    interiorSupport: false,
    presets: {
      default:  { pos: [5.2, 2.2, 6.4], target: [0, 0.5, 0] },
      front:    { pos: [0, 1.5, 7.5],   target: [0, 0.55, 0] },
      side:     { pos: [8, 1.6, 0],     target: [0, 0.55, 0] },
      rear:     { pos: [0, 1.6, -7.5],  target: [0, 0.55, 0] },
      top:      { pos: [0, 8, 0.4],     target: [0, 0.4, 0] },
      wheel:    { pos: [3.6, 0.7, 3.4], target: [0, 0.35, 0] },
      headlight:{ pos: [-2.6, 1.1, 5.2],target: [-1.2, 0.6, -0.6] },
    },
    callbacks: {},

    /* ── Init ─────────────────────────────────────────────── */
    init(container, canvas, opts, callbacks) {
      this.callbacks = callbacks || {};
      this.reducedMotion = opts.reducedMotion || false;
      this.isMobile = opts.isMobile || false;
      this.bodyColor = (opts.bodyColor >> 0) || 0x8a1f1f;

      const w = container.clientWidth || container.offsetWidth;
      const h = container.clientHeight || container.offsetHeight;

      this.renderer = new THREE.WebGLRenderer({
        canvas,
        antialias: true,
        alpha: false,
        powerPreference: 'high-performance',
      });
      this.renderer.setSize(w, h);
      this.renderer.setPixelRatio(Math.min(root.devicePixelRatio || 1, this.isMobile ? 1.5 : 2));
      this.renderer.shadowMap.enabled = true;
      this.renderer.shadowMap.type = THREE.PCFSoftShadowMap;
      this.renderer.toneMapping = THREE.ACESFilmicToneMapping;
      this.renderer.toneMappingExposure = 1.2;
      if (this.renderer.outputEncoding) this.renderer.outputEncoding = THREE.sRGBEncoding;

      this.scene = new THREE.Scene();
      this.scene.background = new THREE.Color(0x050607);
      this.scene.fog = new THREE.FogExp2(0x050607, 0.012);

      this.camera = new THREE.PerspectiveCamera(38, w / h, 0.1, 200);

      if (root.THREE.OrbitControls) {
        this.controls = new THREE.OrbitControls(this.camera, canvas);
        this.controls.enableDamping = true;
        this.controls.dampingFactor = 0.08;
        this.controls.enablePan = false;
        this.controls.minDistance = 2.5;
        this.controls.maxDistance = 20;
        this.controls.minPolarAngle = Math.PI * 0.08;
        this.controls.maxPolarAngle = Math.PI * 0.52;
        this.controls.target.set(0, 0.55, 0);
        this.controls.addEventListener('start', () => this._stopAutoRotate());
        this.controls.addEventListener('end', () => this._scheduleAutoRotate());
        this.controls.update();
      }

      this.clock = new THREE.Clock();

      this._buildEnvironment();
      this._buildCube();

      if (!this.reducedMotion) {
        this.camera.position.set(7, 3.2, 9.5);
        this.camera.lookAt(0, 0.5, 0);
      } else {
        this.camera.position.set(...this.presets.default.pos);
        this.camera.lookAt(...this.presets.default.target);
      }

      this._loadCar();

      this.renderer.render(this.scene, this.camera);
      this._animate();
      this._onResize = () => this._resize();
      root.addEventListener('resize', this._onResize);
    },

    /* ── Environment: floor + studio lighting ─────────────── */
    _buildEnvironment() {
      const scene = this.scene;

      const floorGeo = new THREE.CircleGeometry(24, 64);
      const floorMat = new THREE.MeshStandardMaterial({
        color: 0x08090b,
        roughness: 0.38,
        metalness: 0.7,
        envMapIntensity: 0.6,
      });
      const floor = new THREE.Mesh(floorGeo, floorMat);
      floor.rotation.x = -Math.PI / 2;
      floor.position.y = 0;
      floor.receiveShadow = true;
      scene.add(floor);

      const mirrorMat = new THREE.MeshStandardMaterial({
        color: 0x0a0b0d,
        roughness: 0.06,
        metalness: 0.95,
        envMapIntensity: 0.4,
      });
      const mirror = new THREE.Mesh(new THREE.PlaneGeometry(26, 26), mirrorMat);
      mirror.rotation.x = -Math.PI / 2;
      mirror.position.y = 0.001;
      scene.add(mirror);

      const ringGeo = new THREE.RingGeometry(1.8, 1.82, 96);
      const ringMat = new THREE.MeshBasicMaterial({
        color: 0xe7332d, transparent: true, opacity: 0.15, side: THREE.DoubleSide,
      });
      const ring = new THREE.Mesh(ringGeo, ringMat);
      ring.rotation.x = -Math.PI / 2;
      ring.position.y = 0.002;
      scene.add(ring);

      const ambient = new THREE.AmbientLight(0x707880, 0.5);
      scene.add(ambient);

      const key = new THREE.DirectionalLight(0xffffff, 1.8);
      key.position.set(4, 10, 5);
      key.castShadow = true;
      key.shadow.mapSize.set(2048, 2048);
      key.shadow.camera.near = 1;
      key.shadow.camera.far = 30;
      const c = key.shadow.camera;
      c.left = -10; c.right = 10; c.top = 10; c.bottom = -10;
      key.shadow.bias = -0.0008;
      key.shadow.normalBias = 0.02;
      scene.add(key);

      const fill = new THREE.DirectionalLight(0x8fa0bb, 0.45);
      fill.position.set(-5, 3, -4);
      scene.add(fill);

      const rim = new THREE.PointLight(0xffd9a0, 1.0, 30);
      rim.position.set(-4, 3, -7);
      scene.add(rim);

      const redRim = new THREE.PointLight(0xe7332d, 0.7, 20);
      redRim.position.set(4.5, 1.2, -6);
      scene.add(redRim);

      const top = new THREE.PointLight(0xffffff, 0.5, 30);
      top.position.set(0, 8, 0);
      scene.add(top);

      const under = new THREE.PointLight(0x22304a, 0.35, 18);
      under.position.set(0, -1.5, 0);
      scene.add(under);

      const headBeam = new THREE.PointLight(0xfff2d8, 0, 18, 2);
      headBeam.position.set(0, 0.7, -2.4);
      scene.add(headBeam);

      const spotLeft = new THREE.SpotLight(0xffffff, 0.6, 20, Math.PI / 6, 0.5, 1);
      spotLeft.position.set(-3, 6, 3);
      spotLeft.target.position.set(0, 0.5, 0);
      scene.add(spotLeft);
      scene.add(spotLeft.target);

      const spotRight = new THREE.SpotLight(0xffffff, 0.6, 20, Math.PI / 6, 0.5, 1);
      spotRight.position.set(3, 6, -3);
      spotRight.target.position.set(0, 0.5, 0);
      scene.add(spotRight);
      scene.add(spotRight.target);

      this.lightObjects = { headBeam };
    },

    /* ── Transparent display cube ────────────────────────── */
    _buildCube() {
      const group = new THREE.Group();
      const size = 5.6;

      const glassMat = new THREE.MeshPhysicalMaterial({
        color: 0x88aacc,
        metalness: 0.08,
        roughness: 0.04,
        transparent: true,
        opacity: 0.07,
        side: THREE.DoubleSide,
        depthWrite: false,
        envMapIntensity: 1.2,
      });
      if ('transmission' in glassMat) {
        glassMat.transmission = 0.4;
        glassMat.thickness = 1.5;
        glassMat.ior = 1.5;
      }

      const boxGeo = new THREE.BoxGeometry(size, size, size);
      const cube = new THREE.Mesh(boxGeo, glassMat);
      cube.position.y = size / 2;
      group.add(cube);

      const edgeMat = new THREE.LineBasicMaterial({
        color: 0x6f7d8a, transparent: true, opacity: 0.22,
      });
      const edges = new THREE.EdgesGeometry(boxGeo);
      const lines = new THREE.LineSegments(edges, edgeMat);
      lines.position.y = size / 2;
      group.add(lines);

      const cornerMat = new THREE.MeshBasicMaterial({
        color: 0xe7332d, transparent: true, opacity: 0.12,
      });
      const cornerGeo = new THREE.BoxGeometry(0.015, size, 0.015);
      [[-1,-1],[1,-1],[-1,1],[1,1]].forEach(([sx,sz]) => {
        const pole = new THREE.Mesh(cornerGeo, cornerMat);
        pole.position.set(sx * size/2, size/2, sz * size/2);
        group.add(pole);
      });

      this.cubeGroup = group;
      this.scene.add(group);
    },

    /* ── Car loading (GLB only — no procedural fallback) ─── */
    _loadCar() {
      if (!root.THREE.GLTFLoader) {
        console.error('[AUTOHUB showcase] GLTFLoader not available');
        this._onCarLoadFailed('3D vehicle model loader is not available.');
        return;
      }

      const loader = new THREE.GLTFLoader();
      loader.load(
        MODEL_URL,
        (gltf) => this._onCarLoaded(gltf),
        (evt) => {
          if (evt.total && evt.total > 0 && this.callbacks.onProgress) {
            this.callbacks.onProgress(Math.round((evt.loaded / evt.total) * 100));
          }
        },
        (err) => {
          console.error('[AUTOHUB showcase] GLB load failed:', err);
          this._onCarLoadFailed(
            'Vehicle 3D model could not be loaded. ' +
            'Please ensure the model file exists at: ' + MODEL_URL
          );
        }
      );
    },

    _onCarLoadFailed(message) {
      if (this.callbacks.onError) {
        this.callbacks.onError(message);
      }
    },

    _onCarLoaded(gltf) {
      const model = gltf.scene || gltf.scenes[0];
      if (!model) {
        this._onCarLoadFailed('GLB file contains no scene data.');
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

      this.modelBoundingBox = {
        width: size.x * scale,
        height: size.y * scale,
        depth: size.z * scale,
      };

      this._recalcPresetsFromModel();

      const emissives = [];
      this._paintMaterial = null;
      this._lightMats = [];
      this._lightBaseIntensity = 1;
      this._glassMats = [];
      this._emissiveBaseIntensities = new Map();
      this.variants = [];
      this.activeVariant = 0;
      gltf.animations = gltf.animations || [];

      this._parseVariants(gltf);
      model.traverse((o) => {
        if (o.isMesh) {
          o.castShadow = true;
          o.receiveShadow = false;
          o.userData.originalVisible = (o.visible !== false);
          const m = Array.isArray(o.material) ? o.material : [o.material];
          m.forEach((mat) => {
            if (!mat) return;
            const n = (mat.name || '').toLowerCase();

            if (!this._paintMaterial && /(^|[\s(])paint|paint \d|body|exterior|coloured|colour|colored|color|bodywork|metal/i.test(n) &&
                !/glass|window|light|chrome|grille|badge|carbon|interior|seat|trim|rubber|tire|tyre|mirror|plate|disc|rim/i.test(n)) {
              this._paintMaterial = mat;
            }

            if (/light|headlight|taillight|brakelight|rearlight|lamp|signallight|turnsignal/i.test(n)) {
              this._lightMats.push(mat);
            }

            if (/glass|window|windshield|windscreen|rearwindow/i.test(n) &&
                (!this._glassMats.length || /glass/i.test(n))) {
              this._glassMats.push(mat);
            }

            if (mat.emissive && (mat.emissive.r + mat.emissive.g + mat.emissive.b > 0.01)) {
              const baseIntensity = mat.emissiveIntensity !== undefined ? mat.emissiveIntensity : 1;
              emissives.push({ mesh: o, material: mat, intensity: baseIntensity });
              this._emissiveBaseIntensities.set(mat, baseIntensity);
            }
          });
        }
      });

      if (!this._paintMaterial) {
        let best = null, bestMeta = -1;
        model.traverse((o) => {
          if (!o.isMesh) return;
          const m = Array.isArray(o.material) ? o.material[0] : o.material;
          if (m && m.isMeshStandardMaterial && m.metalness > bestMeta && m.emissiveIntensity <= 0.2 &&
              !/glass|window|light|chrome|interior|seat|rubber|tire/i.test((m.name || '').toLowerCase())) {
            bestMeta = m.metalness;
            best = m;
          }
        });
        this._paintMaterial = best;
      }

      this._detectDoorPivots();
      this._detectInterior();

      this.carModel = model;
      this.carGroup = new THREE.Group();
      this.carGroup.add(model);
      this.scene.add(this.carGroup);

      const animNames = (gltf.animations || []).map((a) => a.name || '(unnamed)');
      if (animNames.length) {
        console.log('[AUTOHUB showcase] Model animations:', animNames);
      }

      this.doorSupport = this.doorPivotSupport ||
        (gltf.animations || []).some((a) => /door|door_open|door_close/i.test(a.name || ''));

      const hasInteriorAnim = (gltf.animations || []).some((a) =>
        /interior|camera|seat/i.test(a.name || '')
      );
      this.interiorSupport = this.interiorSupport || hasInteriorAnim || this.interiorDetected;

      if (gltf.animations && gltf.animations.length) {
        this.carAnimations = gltf.animations;
        const mixer = new THREE.AnimationMixer(model);
        gltf.animations.forEach((clip) => {
          if (/door/i.test(clip.name || '')) {
            const action = mixer.clipAction(clip);
            action.clampWhenFinished = false;
            action.loop = THREE.LoopRepeat;
            this.doorAction = action;
          }
        });
        this.mixers.push(mixer);
      }

      this.lightObjects = this.lightObjects || {};
      this.lightObjects.emissives = emissives;

      this._finishCarReady();

      if (this.callbacks.onModelLoaded) {
        this.callbacks.onModelLoaded({
          doorSupport: this.doorSupport,
          hasInteriorAnim: hasInteriorAnim,
          interiorSupport: this.interiorSupport,
          paintMaterial: !!this._paintMaterial,
          variantCount: this.variants.length,
          animationCount: animNames.length,
          animationNames: animNames,
          dimensions: this.modelBoundingBox,
        });
      }
    },

    _parseVariants(gltf) {
      const variantsExt = gltf.extensions && gltf.extensions.KHR_materials_variants;
      if (!variantsExt || !variantsExt.variants) return;
      this.variants = variantsExt.variants.map((v) => v.name || ('Variant ' + this.variants.length));
      this._variantMapping = new Map();
      (gltf.meshes || []).forEach((mesh, mi) => {
        (mesh.primitives || []).forEach((prim, pi) => {
          const ext = prim.extensions && prim.extensions.KHR_materials_variants;
          if (!ext || !ext.mappings) return;
          const name = mesh.name || (mi + '/' + pi);
          this._variantMapping.set(name, ext.mappings);
        });
      });
    },

    _detectDoorPivots() {
      this.doorPivotSupport = false;
      this.doorNodes = [];
      this.doorPivots = [];
      this.carModel.traverse((o) => {
        if ((o.isGroup || o.isMesh) && o.children && o.children.length) {
          const n = (o.name || '');
          if (/^BodyDoor[LR]Color1$/.test(n) || /^Door[LR]Color1$/.test(n) || /door(left|right)/i.test(n)) {
            this.doorNodes.push(o);
          }
        }
      });
      if (this.doorNodes.length >= 2) {
        const L = this.doorNodes.find((o) => /[l]$/i.test(o.name || '') || /left/i.test(o.name || ''));
        const R = this.doorNodes.find((o) => /[r]$/i.test(o.name || '') || /right/i.test(o.name || ''));
        this.doorPivotSupport = !!(L && R);
        if (this.doorPivotSupport) {
          this._rigDoorPivot(L, true);
          this._rigDoorPivot(R, false);
          const piv = (o) => ({ pivot: o.userData.doorPivot, openAngle: o.userData.openAngle });
          this.doorPivots[0] = piv(L);
          this.doorPivots[1] = piv(R);
        }
      }
    },

    _rigDoorPivot(door) {
      const box = new THREE.Box3().setFromObject(door);
      const doorCenter = new THREE.Vector3();
      box.getCenter(doorCenter);
      const frontZ = doorCenter.z <= 0 ? box.min.z : box.max.z;
      const inboardX = doorCenter.z <= 0 ? box.max.x : box.min.x;
      const hingeWorld = new THREE.Vector3(inboardX, box.min.y + 0.0001, frontZ);
      door.updateWorldMatrix(true, false);
      const parent = door.parent;
      if (!parent) return;
      const hingeLocal = parent.worldToLocal(hingeWorld.clone());
      const pivot = new THREE.Group();
      pivot.name = (door.name || 'door') + '_pivot';
      pivot.position.copy(hingeLocal);
      door.position.sub(hingeLocal);
      parent.remove(door);
      pivot.add(door);
      parent.add(pivot);
      door.userData.doorPivot = pivot;
      door.userData.openAngle = doorCenter.z <= 0 ? 0.78 : -0.78;
    },

    _openDoor(group, open) {
      const pivot = group.userData.doorPivot;
      if (!pivot) return;
      const base = 0;
      const target = open ? (group.userData.openAngle || (this.doorsOpen ? 0.7 : -0.7)) : base;
      if (root.gsap) {
        root.gsap.killTweensOf(pivot.rotation);
        root.gsap.to(pivot.rotation, { y: target, duration: 1.0, ease: 'power3.inOut' });
      } else {
        pivot.rotation.y = target;
      }
    },

    _detectInterior() {
      this.interiorDetected = false;
      this.carModel.traverse((o) => {
        if (o.isMesh && /interior|dashboard|seat|steeringwheel|steering|console|pedal/i.test(o.name || '')) {
          this.interiorDetected = true;
          return;
        }
      });
    },

    _openDoor(group, open) {
      if (!this.doorPivotSupport || !group) return;
      const isL = /l/i.test(group.name || '');
      const hinge = isL ? this._doorHingeL : this._doorHingeR;
      if (!hinge) return;

      const world = this._doorWorldPos.get(isL ? 'L' : 'R');
      if (!world) return;

      const base = this.doorBaseRotation ? this.doorBaseRotation.get(group.name || '') : 0;
      const target = open ? (isL ? 0.72 : -0.72) : base;
      const start = group.rotation.y;
      const sign = isL ? 1 : -1;

      if (root.gsap) {
        root.gsap.killTweensOf(group.rotation);
        root.gsap.to(group.rotation, {
          y: target, duration: 0.9, ease: 'power3.inOut',
          onStart: () => {
            group.updateWorldMatrix(true, false);
            const c = group.parent;
            if (c) {
              const p = c.worldToLocal(world.clone());
              group.position.set(p.x, p.y, p.z);
              group.position.sub(new THREE.Vector3(hinge.x, 0, hinge.z).multiplyScalar(0));
            }
          },
        });
      } else {
        group.rotation.y = target;
      }
    },

    _recalcPresetsFromModel() {
      if (!this.modelBoundingBox) return;
      const bb = this.modelBoundingBox;
      const halfLen = bb.depth / 2;
      const halfW = bb.width / 2;
      const height = bb.height;
      const camDist34 = halfLen * 2.8;
      const camDistSide = halfW * 2.6;
      const camDistFront = halfLen * 2.5;
      const camHeight34 = height * 0.55;
      const camHeightFront = height * 0.42;
      const camHeightTop = height * 2.2;

      this.presets = {
        default:  { pos: [camDist34, camHeight34, camDist34], target: [0, height * 0.3, 0] },
        front:    { pos: [0, camHeightFront, camDistFront],   target: [0, height * 0.3, 0] },
        side:     { pos: [camDistSide, height * 0.4, 0],     target: [0, height * 0.3, 0] },
        rear:     { pos: [0, camHeightFront, -camDistFront],  target: [0, height * 0.3, 0] },
        top:      { pos: [0, camHeightTop, 0.3],             target: [0, 0, 0] },
        wheel:    { pos: [halfW * 1.5, height * 0.18, halfLen * 1.25], target: [halfW * 0.6, height * 0.18, 0] },
        headlight:{ pos: [-halfLen * 0.9, height * 0.34, halfW * 1.8], target: [-halfLen * 0.55, height * 0.28, 0] },
        interior: { pos: [0, height * 0.7, halfLen * 0.55],  target: [0, height * 0.5, 0] },
      };

      if (this.controls) {
        this.controls.minDistance = Math.max(1.5, halfLen * 0.8);
        this.controls.maxDistance = Math.max(10, halfLen * 4);
        this.controls.target.set(0, height * 0.3, 0);
        this.controls.update();
      }
    },

    _finishCarReady() {
      if (this.callbacks.onReady) this.callbacks.onReady();
      const g = this.carGroup;
      if (!g) return;

      const defaultP = this.presets.default.pos;
      const defaultT = this.presets.default.target;

      if (this.reducedMotion || !root.gsap) {
        g.position.set(0, 0, 0);
        g.scale.set(1, 1, 1);
        this.camera.position.set(...defaultP);
        if (this.controls) {
          this.controls.target.set(...defaultT);
          this.controls.update();
        }
        this.entryDone = true;
        this._playAmbient();
      } else {
        g.position.set(-3.2, 0.4, 1.6);
        g.rotation.y = -0.18;
        g.scale.set(0.94, 0.94, 0.94);
        root.gsap.to(g.position, {
          x: 0, y: 0, z: 0, duration: 1.5, ease: 'power3.out',
          onComplete: () => {
            this._playAmbient();
            this.entryDone = true;
            this.camera.position.set(...defaultP);
            if (this.controls) {
              this.controls.target.set(...defaultT);
              this.controls.update();
            }
            this.callbacks.onEntered && this.callbacks.onEntered();
          },
        });
        root.gsap.to(g.rotation, { y: 0, duration: 1.6, ease: 'power3.out' });
        root.gsap.to(g.scale, { x: 1, y: 1, z: 1, duration: 1.5, ease: 'power3.out' });
      }
    },

    _playAmbient() {
      if (this.reducedMotion) return;
      this._hoverTween = setInterval(() => {
        if (this.carGroup && this.entryDone) {
          this.carGroup.position.y = Math.sin(Date.now() / 900) * 0.01;
        }
      }, 0);
    },

    /* ── Public API (exposed to orchestrator) ────────────── */
    showHint() {},
    setBodyColor(hex) {
      if (!this.carModel) return false;
      const candidates = [];
      this.carModel.traverse((o) => {
        if (!o.isMesh) return;
        const mats = Array.isArray(o.material) ? o.material : [o.material];
        mats.forEach((mat) => {
          if (!mat || candidates.indexOf(mat) !== -1) return;
          const n = (mat.name || '').toLowerCase();
          if (/paint \d|colou?red|body|bodywork/.test(n) && !/glass|light|mirror|plate|interior light/.test(n)) {
            candidates.push(mat);
          }
        });
      });
      if (candidates.length) {
        this._paintMaterials = candidates;
        candidates.forEach((mat) => mat.color.setHex(hex));
        return true;
      }
      if (this._paintMaterial) {
        this._paintMaterial.color.setHex(hex);
        return true;
      }
      return false;
    },

    setLights(on) {
      this.lightsOn = on;
      const lo = this.lightObjects;
      if (!lo) return;
      if (lo.headBeam) lo.headBeam.intensity = on ? 2.8 : 0;

      if (this._lightMats) {
        this._lightMats.forEach((mat) => {
          if (mat && mat.emissiveIntensity !== undefined) {
            const base = this._emissiveBaseIntensities.get(mat) || 1;
            mat.emissiveIntensity = on ? base * 2.8 : base;
          }
        });
      }

      if (lo.emissives) {
        lo.emissives.forEach((e) => {
          const base = e.intensity || 1;
          const target = on ? base + 1.4 : base;
          if (e.material && e.material.emissiveIntensity !== undefined) {
            e.material.emissiveIntensity = target;
          }
        });
      }

      if (this._glassMats) {
        this._glassMats.forEach((mat) => {
          if (mat && mat.emissive) {
            if (on) {
              mat.emissive.setHex(0xffeedd);
              mat.emissiveIntensity = 0.15;
            } else {
              mat.emissive.setHex(0x000000);
              mat.emissiveIntensity = 0;
            }
          }
        });
      }
    },

    setHazard(on) { this.hazardOn = on; },
    setBrake(on) { this.brakeOn = on; },

    openDoors() {
      if (!this.doorSupport) return false;

      if (this.doorAction) {
        this.doorAction.reset();
        this.doorAction.setLoop(THREE.LoopOnce);
        this.doorAction.clampWhenFinished = true;
        this.doorAction.paused = false;
        this.doorAction.timeScale = 1;
        this.doorAction.play();
        this.doorsOpen = true;
        return true;
      }

      if (this.doorPivotSupport) {
        this.doorNodes.forEach((g) => this._openDoor(g, true));
        this.doorsOpen = true;
        return true;
      }

      return false;
    },

    closeDoors() {
      if (!this.doorSupport) return false;

      if (this.doorAction) {
        this.doorAction.paused = false;
        this.doorAction.timeScale = -1;
        this.doorAction.play();
        this.doorsOpen = false;
        return true;
      }

      if (this.doorPivotSupport) {
        this.doorNodes.forEach((g) => this._openDoor(g, false));
        this.doorsOpen = false;
        return true;
      }

      return false;
    },

    hasDoorSupport() { return this.doorSupport; },
    hasLightsSupport() { return true; },
    hasColorSupport() { return this.variants.length > 0 || !!this._paintMaterial; },
    hasInteriorSupport() { return this.interiorSupport; },

    getVariantNames() { return this.variants.slice(); },

    setVariant(index) {
      if (!this._paintMaterial) return false;
      const idx = ((index % this.variants.length || 0) + this.variants.length) % this.variants.length;
      this.activeVariant = idx;
      const variantColors = [0x9b1b1b, 0xf0e9e0, 0x2a2c2e];
      const color = variantColors[idx] !== undefined ? variantColors[idx] : this.bodyColor;
      this._paintMaterial.color.setHex(color);
      return true;
    },

    setAutoRotate(on) {
      this.autoRotate = on;
    },

    goToPreset(name) {
      const preset = this.presets[name] || this.presets.default;
      this._stopAutoRotate();
      if (root.gsap) {
        const dur = this.reducedMotion ? 0.001 : 1.2;
        root.gsap.to(this.camera.position, {
          x: preset.pos[0], y: preset.pos[1], z: preset.pos[2],
          duration: dur, ease: 'power3.inOut',
          onUpdate: () => this.controls && this.controls.update(),
        });
        if (this.controls) {
          root.gsap.to(this.controls.target, {
            x: preset.target[0], y: preset.target[1], z: preset.target[2],
            duration: dur, ease: 'power3.inOut',
            onComplete: () => this._scheduleAutoRotate(),
          });
        }
      } else {
        this.camera.position.set(...preset.pos);
        if (this.controls) { this.controls.target.set(...preset.target); this.controls.update(); }
      }
    },

    _stopAutoRotate() {
      if (this.controls) this.controls.autoRotate = false;
      if (this.autoRotateTimer) { clearTimeout(this.autoRotateTimer); this.autoRotateTimer = null; }
    },

    _scheduleAutoRotate() {
      if (this.reducedMotion) return;
      if (this.autoRotateTimer) clearTimeout(this.autoRotateTimer);
      this.autoRotateTimer = setTimeout(() => {
        if (this.controls && this.autoRotate && this.entryDone) {
          this.controls.autoRotate = true;
          this.controls.autoRotateSpeed = 0.5;
        }
      }, 4000);
    },

    _resize() {
      const canvas = this.renderer.domElement;
      const w = canvas.clientWidth || canvas.parentElement.clientWidth;
      const h = canvas.clientHeight || canvas.parentElement.clientHeight;
      if (!w || !h) return;
      this.camera.aspect = w / h;
      this.camera.updateProjectionMatrix();
      this.renderer.setSize(w, h);
    },

    _animate() {
      this.rafId = requestAnimationFrame(() => this._animate());
      const dt = this.clock.getDelta();
      const t = this.clock.elapsedTime;

      if (this.controls) {
        this.controls.update();
      }

      this.mixers.forEach((m) => m.update(dt));

      if (this.hazardOn) {
        const blink = (Math.floor(t * 1.8) % 2 === 0) ? 1 : 0.12;
        const apply = (mat, base) => {
          if (mat && mat.emissiveIntensity !== undefined) {
            mat.emissiveIntensity = this.lightsOn ? Math.max(blink * 2.4, base) : blink * (base || 1);
          }
        };
        (this._lightMats || []).forEach((mat) => apply(mat, this._lightBaseIntensity || 1));
        (this.lightObjects && this.lightObjects.emissives || []).forEach((e) => apply(e.material, e.intensity || 1));
      }

      if (this.brakeOn) {
        (this._lightMats || []).forEach((mat) => {
          if (mat && mat.emissiveIntensity !== undefined) {
            const base = this._emissiveBaseIntensities.get(mat) || 1;
            mat.emissiveIntensity = Math.max(
              this.lightsOn ? 3.0 : 2.0,
              mat.emissive.r > 0.1 ? 3.5 : base
            );
          }
        });
      }

      this.renderer.render(this.scene, this.camera);
    },

    /* ── Cleanup / dispose ───────────────────────────────── */
    dispose() {
      if (this.rafId) cancelAnimationFrame(this.rafId);
      if (this.autoRotateTimer) clearTimeout(this.autoRotateTimer);
      if (this._hoverTween) clearInterval(this._hoverTween);
      if (this._onResize) root.removeEventListener('resize', this._onResize);

      if (this.controls) { this.controls.dispose(); this.controls = null; }

      if (this.scene) {
        this.scene.traverse((o) => {
          if (o.geometry) o.geometry.dispose();
          if (o.material) {
            const mats = Array.isArray(o.material) ? o.material : [o.material];
            mats.forEach((m) => {
              if (m.map) m.map.dispose();
              if (m.emissiveMap) m.emissiveMap.dispose();
              if (m.normalMap) m.normalMap.dispose();
              if (m.metalnessMap) m.metalnessMap.dispose();
              if (m.roughnessMap) m.roughnessMap.dispose();
              m.dispose && m.dispose();
            });
          }
        });
      }
      this.mixers = [];
      this._emissiveBaseIntensities = new Map();
      if (this.renderer) { this.renderer.dispose(); this.renderer.forceContextLoss && this.renderer.forceContextLoss(); this.renderer = null; }
      this.scene = null;
      this.camera = null;
    },
  };

  root.AUTOHUB = root.AUTOHUB || {};
  root.AUTOHUB.scene = SceneModule;
})(window);
