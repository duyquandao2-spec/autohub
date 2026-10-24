/* ═══════════════════════════════════════════════════════════════
   AUTOHUB Vehicle Showcase — Engine Audio (Web Audio API synth)
   Realistic layered automotive engine: firing-order harmonic
   stack (V8 cadence) + exhaust rumble + mechanical/intake noise.
   No audio assets required; everything is synthesized.
   ═══════════════════════════════════════════════════════════════ */
(function (root) {
  'use strict';

  const AudioEngine = {
    ctx: null,
    master: null,
    engineOn: false,
    muted: false,
    volume: 0.45,

    // RPM model (0..6800)
    rpm: 0,
    targetRpm: 0,
    revHeld: false,
    started: false,

    // synth node groups
    harmonics: [],      // { osc, gain } — firing-order harmonics
    harmBus: null,      // sum bus for harmonics + sub (chug-modulated)
    subGain: null,      // deep half-order rumble
    subOsc: null,
    exhaust: null,      // { src, filter, gain }
    mech: null,         // { src, filter, gain }
    flutter: null,      // sub-audible "chug" LFO → drives harmBus amplitude
    flutterAtt: null,

    // engine params
    cylinders: 8,       // V8 (BMW M8)
    redline: 6800,
    idle: 800,
    _noiseBuffer: null,

    // Must be called from a user gesture (AudioContext resume)
    ensure() {
      if (!this.ctx) {
        const AC = root.AudioContext || root.webkitAudioContext;
        if (!AC) return false;
        this.ctx = new AC();

        this.master = this.ctx.createGain();
        this.master.gain.value = this.muted ? 0 : this.volume;
        this.master.connect(this.ctx.destination);

        this._createNoiseBuffer();
        this._buildSynth();
        this._tick();
      }
      if (this.ctx.state === 'suspended') this.ctx.resume();
      return true;
    },

    _createNoiseBuffer() {
      const len = this.ctx.sampleRate * 2;
      const buf = this.ctx.createBuffer(1, len, this.ctx.sampleRate);
      const data = buf.getChannelData(0);
      for (let i = 0; i < len; i++) data[i] = Math.random() * 2 - 1;
      this._noiseBuffer = buf;
    },

    _harmSources(harm) {
      // helper: create one harmonic tone (sawtooth at F; we tune freq live)
      const osc = this.ctx.createOscillator();
      osc.type = 'sawtooth';
      const g = this.ctx.createGain();
      g.gain.value = 0;
      osc.connect(g);
      g.connect(this.harmBus);
      osc.start();
      this.harmonics.push({ osc, gain: g, harm });
    },

    _buildSynth() {
      if (!this.ctx) return;
      const ctx = this.ctx;

      // ── Harmonic bus (sum of firing harmonics + sub, chug-modulated) ──
      this.harmBus = ctx.createGain();
      this.harmBus.gain.value = 1.0;
      this.harmBus.connect(this.master);

      // ── Firing-order harmonic stack (V8: fires 4x per rev) ──
      // Spectral weights approximate a car exhaust note (strong low
      // fundamentals, harmonics rolling off ~1/n).
      const weights = [
        { harm: 0.5, w: 0.55 },  // half-order sub rumble
        { harm: 1.0, w: 1.0  },  // firing fundamental
        { harm: 1.5, w: 0.70 },  // half-order
        { harm: 2.0, w: 0.55 },  // 2nd
        { harm: 2.5, w: 0.42 },
        { harm: 3.0, w: 0.33 },  // 3rd
        { harm: 4.0, w: 0.20 },
        { harm: 5.0, w: 0.13 },
        { harm: 6.0, w: 0.09 },
        { harm: 8.0, w: 0.05 },
      ];
      weights.forEach((h) => this._harmSources(h));

      // ── Deep sub (pure sine for the bass body) ──
      this.subOsc = ctx.createOscillator();
      this.subOsc.type = 'sine';
      this.subGain = ctx.createGain();
      this.subGain.gain.value = 0;
      this.subOsc.connect(this.subGain);
      this.subGain.connect(this.harmBus);
      this.subOsc.start();

      // ── Idle "chug" LFO → amplitude-modulates the harmonic bus ──
      // A triangle LFO output (bipolar ±1) scaled by flutterAtt and added
      // onto harmBus.gain, giving an engine-lope pulsing feel.
      this.flutter = ctx.createOscillator();
      this.flutter.type = 'triangle';
      this.flutter.frequency.value = 5;
      this.flutterAtt = ctx.createGain();
      this.flutterAtt.gain.value = 0.15;
      this.flutter.connect(this.flutterAtt);
      this.flutterAtt.connect(this.harmBus.gain);
      this.flutter.start();

      // ── Exhaust rumble (noise → lowpass, cutoff tracks RPM) ──
      const exSrc = ctx.createBufferSource();
      exSrc.buffer = this._noiseBuffer;
      exSrc.loop = true;
      const exFilter = ctx.createBiquadFilter();
      exFilter.type = 'lowpass';
      exFilter.frequency.value = 220;
      exFilter.Q.value = 0.7;
      const exGain = ctx.createGain();
      exGain.gain.value = 0;
      exSrc.connect(exFilter); exFilter.connect(exGain); exGain.connect(this.master);
      exSrc.start();
      this.exhaust = { src: exSrc, filter: exFilter, gain: exGain };

      // ── Mechanical / intake noise (bandpass) ──
      const mechSrc = ctx.createBufferSource();
      mechSrc.buffer = this._noiseBuffer;
      mechSrc.loop = true;
      mechSrc.playbackRate.value = 0.7;
      const mechFilter = ctx.createBiquadFilter();
      mechFilter.type = 'bandpass';
      mechFilter.frequency.value = 900;
      mechFilter.Q.value = 1.1;
      const mechGain = ctx.createGain();
      mechGain.gain.value = 0;
      mechSrc.connect(mechFilter); mechFilter.connect(mechGain); mechGain.connect(this.master);
      mechSrc.start();
      this.mech = { src: mechSrc, filter: mechFilter, gain: mechGain };
    },

    startEngine() {
      if (!this.ensure()) return false;
      if (this.engineOn) { this.stopEngine(); return false; }
      this.engineOn = true;
      this.rpm = this.idle;
      this.targetRpm = this.idle;
      return true;
    },

    stopEngine() {
      this.engineOn = false;
      this.revHeld = false;
      this.rpm = 0;
      this.targetRpm = 0;
    },

    setRev(held) {
      this.revHeld = held;
      if (held) this.targetRpm = this.redline;
      else this.targetRpm = this.engineOn ? this.idle : 0;
    },

    setMuted(m) {
      this.muted = m;
      if (this.master && this.ctx) {
        this.master.gain.setTargetAtTime(m ? 0 : this.volume, this.ctx.currentTime, 0.05);
      }
    },

    getMuted() { return this.muted; },

    setVolume(v) {
      this.volume = v;
      if (this.master && this.ctx && !this.muted) {
        this.master.gain.setTargetAtTime(v, this.ctx.currentTime, 0.05);
      }
    },

    shutdown() {
      this.engineOn = false;
      this.revHeld = false;
      this.rpm = 0;
      this.targetRpm = 0;
      if (this.ctx && this.master) {
        try { this.master.gain.setTargetAtTime(0, this.ctx.currentTime, 0.12); } catch (e) {}
      }
    },

    resume() {
      if (!this.ctx) return;
      if (this.ctx.state === 'suspended') this.ctx.resume();
      if (this.master) this.master.gain.setTargetAtTime(this.muted ? 0 : this.volume, this.ctx.currentTime, 0.05);
    },

    getRpm() { return this.rpm; },

    _weight(harm, norm) {
      // base rolloff ~1/harm; higher harmonics gain presence as RPM climbs
      const base = 1 / Math.max(harm, 0.5);
      const boost = norm * 0.55 * Math.min(1, harm / 3);
      return base * (1 + boost);
    },

    _tick() {
      if (!this.ctx || !this.master) return;

      // Smooth RPM toward target
      const diff = this.targetRpm - this.rpm;
      const rate = this.revHeld ? 0.16 : 0.07; // fast rise, slower fall
      this.rpm += diff * rate;
      if (Math.abs(this.rpm - this.targetRpm) < 2) this.rpm = this.targetRpm;

      const running = this.engineOn && !this.muted;
      const rpm = Math.max(this.rpm, running ? this.idle * 0.5 : 0);
      const t = this.ctx.currentTime;

      // V8 firing frequency (4 fires per revolution for 8 cyl, 4-stroke)
      const fc = (rpm / 60) * (this.cylinders / 2);
      const norm = (rpm - this.idle) / (this.redline - this.idle); // 0..1
      const runningLevel = running ? 1 : 0;

      // Per-harmonic emulation
      this.harmonics.forEach((h) => {
        const f = fc * h.harm;
        h.osc.frequency.setTargetAtTime(Math.max(f, 8), t, 0.02);
        const g = runningLevel * h.w * this._weight(h.harm, norm);
        h.gain.gain.setTargetAtTime(g, t, 0.04);
      });

      // Deep sub
      this.subOsc.frequency.setTargetAtTime(Math.max(fc * 0.5, 14), t, 0.03);
      this.subGain.gain.setTargetAtTime(runningLevel * 0.5, t, 0.06);

      // Exhaust rumble
      const exCut = 180 + fc * 3 + norm * 500;
      this.exhaust.filter.frequency.setTargetAtTime(Math.min(exCut, 9000), t, 0.05);
      this.exhaust.gain.gain.setTargetAtTime(runningLevel * (0.16 + norm * 0.28), t, 0.06);

      // Mechanical / intake noise
      const mechCut = 700 + norm * 2600 + (this.revHeld ? 900 : 0);
      this.mech.filter.frequency.setTargetAtTime(Math.min(mechCut, 6000), t, 0.06);
      this.mech.src.playbackRate.setTargetAtTime(0.6 + norm * 0.9, t, 0.1);
      const mechAmt = runningLevel * (0.05 + norm * 0.20) * (this.revHeld ? 1.5 : 1);
      this.mech.gain.gain.setTargetAtTime(mechAmt, t, 0.05);

      // Chug modulation (idle lope) — strongest at idle, fades as RPM rises
      if (running) {
        const flFr = Math.max(fc * 0.5, 4);
        this.flutter.frequency.setTargetAtTime(flFr, t, 0.05);
        const chugDepth = (0.22 * Math.max(0, 1 - norm * 2.4) + 0.05) * runningLevel;
        this.flutterAtt.gain.setTargetAtTime(chugDepth, t, 0.04);
      } else {
        this.flutterAtt.gain.setTargetAtTime(0, t, 0.02);
      }

      requestAnimationFrame(() => this._tick());
    },
  };

  root.AUTOHUB = root.AUTOHUB || {};
  root.AUTOHUB.audio = AudioEngine;
})(window);
