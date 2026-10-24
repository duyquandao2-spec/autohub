/* Press Signal — Monolith Velocity telemetry press bay */
(function () {
  var section = document.getElementById('press');
  if (!section) return;

  var API_URL = '/api/automotive-news?limit=3&lang=vi';

  var MOCK_DATA = [
    {
      id: 'mock-01',
      sourceName: 'ACEA',
      title: 'Electrication crosses the twenty percent threshold.',
      excerpt: 'ACEA recorded 1,220,890 new battery-electric vehicles in the EU in the first half of 2026 \u2014 a clear signal for the new race among manufacturers.',
      articleUrl: 'https://www.acea.auto/pc-registrations/new-car-registrations-5-7-in-h1-2026-battery-electric-20-7-market-share/',
      imageUrl: '',
      publishedAt: '2026-07-23T00:00:00Z',
      category: 'market',
      featuredMetric: { value: '20.7%', label: 'TH\u1EC6 PH\u1EA6N BEV T\u1EA0I EU \u00B7 N\u1EE2A \u0110\u1EA6U 2026', verified: true }
    },
    {
      id: 'mock-02',
      sourceName: 'Reuters',
      title: 'C\u1EA7u EV t\u0103ng t\u1ED1c, \u00E1p l\u1EF1c c\u1EA1nh tranh thi\u1EC7 ph\u1EA7n c\u0169ng t\u0103ng theo.',
      articleUrl: 'https://www.reuters.com/world/china/ev-demand-powers-europe-car-market-may-chinese-rivals-expand-share-2026-06-23/',
      imageUrl: '',
      publishedAt: '2026-06-22T00:00:00Z',
      category: 'brand'
    },
    {
      id: 'mock-03',
      sourceName: 'Car and Driver',
      title: 'B\u1EA3n \u0111\u1ED3 EV s\u1EAFp ra m\u1EAFt h\u00E9 l\u1ED9p xe hi\u1EC7u n\u0103ng k\u1EBF ti\u1EBFp.',
      articleUrl: 'https://www.caranddriver.com/news/g29994375/future-electric-cars-trucks/',
      imageUrl: '',
      publishedAt: '2026-07-30T00:00:00Z',
      category: 'technology'
    }
  ];

  function isDev() {
    try {
      if (typeof window !== 'undefined' && window.location) {
        var h = window.location.hostname;
        if (h === 'localhost' || h === '127.0.0.1' || h === '0.0.0.0' || h.endsWith('.local')) return true;
      }
    } catch (_) {}
    return false;
  }

  function fmtDate(iso) {
    if (!iso) return '';
    try {
      var d = new Date(iso);
      var dd = String(d.getDate()).padStart(2, '0');
      var mm = String(d.getMonth() + 1).padStart(2, '0');
      var yy = String(d.getFullYear()).slice(-2);
      return dd + '.' + mm + '.' + yy;
    } catch (_) { return ''; }
  }

  function esc(s) {
    if (!s) return '';
    var d = document.createElement('div');
    d.textContent = s;
    return d.innerHTML;
  }

  /* ArrowUpRight SVG inline */
  var ARROW_SVG = '<svg xmlns="http://www.w3.org/2000/svg" width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M7 17L17 7"/><path d="M7 7h10v10"/></svg>';
  var ARROW_SVG_SM = '<svg xmlns="http://www.w3.org/2000/svg" width="12" height="12" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"><path d="M7 17L17 7"/><path d="M7 7h10v10"/></svg>';

  /* ═══════════════════════════════════════════════════════════
     Lead Card — matches reference exactly
     ═══════════════════════════════════════════════════════════ */
  function leadCard(item) {
    var hasImg = !!(item.imageUrl && item.imageUrl.length);
    var imgTag = hasImg
      ? '<img src="' + esc(item.imageUrl) + '" alt="' + esc(item.title) + '" loading="lazy" />'
      : '';

    var metricHtml = '';
    if (item.featuredMetric && item.featuredMetric.verified) {
      metricHtml =
        '<div class="ps-lead__metric">'
        + '<strong class="ps-lead__metric-val">' + esc(item.featuredMetric.value) + '</strong>'
        + '<span class="ps-lead__metric-lbl">' + esc(item.featuredMetric.label) + '</span>'
        + '</div>';
    }

    var excerptHtml = item.excerpt
      ? '<p class="ps-lead__excerpt">' + esc(item.excerpt) + '</p>'
      : '';

    return '<a class="ps-lead" href="' + esc(item.articleUrl) + '" target="_blank" rel="noopener noreferrer" aria-label="' + esc(item.title) + '">'
      + '<div class="ps-lead__bg">' + imgTag + '</div>'
      + '<div class="ps-lead__grad"></div>'
      + '<div class="ps-lead__grid" aria-hidden="true"></div>'
      + '<div class="ps-lead__top">'
      +   '<span class="ps-lead__source">' + esc(item.sourceName) + '</span>'
      +   '<span class="ps-lead__date">' + fmtDate(item.publishedAt) + ' \u2197</span>'
      + '</div>'
      + metricHtml
      + '<h3 class="ps-lead__title">' + esc(item.title) + '</h3>'
      + excerptHtml
      + '<span class="ps-lead__cta">DOC BAO CAO ' + ARROW_SVG + '</span>'
      + '</a>';
  }

  /* ═══════════════════════════════════════════════════════════
     Sub Card — 3-col grid: [number | content | arrow]
     ═══════════════════════════════════════════════════════════ */
  function subCard(item, idx) {
    var num = '0' + (idx + 1);
    var hasImg = !!(item.imageUrl && item.imageUrl.length);
    var imgTag = hasImg
      ? '<img src="' + esc(item.imageUrl) + '" alt="' + esc(item.title) + '" loading="lazy" />'
      : '';

    return '<a class="ps-card" href="' + esc(item.articleUrl) + '" target="_blank" rel="noopener noreferrer" aria-label="' + esc(item.title) + '">'
      + '<div class="ps-card__bg">' + imgTag + '</div>'
      + '<div class="ps-card__grad"></div>'
      + '<div class="ps-card__row">'
      +   '<div class="ps-card__idx">'
      +     '<span class="ps-card__num">' + num + '</span>'
      +     '<span class="ps-card__dash" aria-hidden="true"></span>'
      +   '</div>'
      +   '<div class="ps-card__content">'
      +     '<p class="ps-card__meta">' + esc(item.sourceName) + ' \u00B7 ' + fmtDate(item.publishedAt) + '</p>'
      +     '<h3 class="ps-card__title">' + esc(item.title) + '</h3>'
      +   '</div>'
      +   '<span class="ps-card__arrow" aria-hidden="true">' + ARROW_SVG_SM + '</span>'
      + '</div>'
      + '</a>';
  }

  /* ═══════════════════════════════════════════════════════════
      Ticker — inside right column, pulsing red dot
      ═══════════════════════════════════════════════════════════ */
  var VERIFIED_SOURCES = ['AUTODAILY', 'CARTIMES', 'CAR AND DRIVER'];

  function ticker(sources) {
    var seen = [];
    sources.forEach(function (s) { if (seen.indexOf(s) === -1) seen.push(s); });
    var list = seen.length ? seen : VERIFIED_SOURCES;
    VERIFIED_SOURCES.forEach(function (s) { if (list.indexOf(s) === -1) list.push(s); });
    list = list.slice(0, 5);

    return '<div class="ps-ticker">'
      + '<span class="ps-ticker__left"><i class="ps-ticker__dot" aria-hidden="true"></i>VERIFIED SOURCES</span>'
      + '<span class="ps-ticker__right">' + list.map(esc).join(' \u00B7 ') + '</span>'
      + '</div>';
  }

  /* ═══════════════════════════════════════════════════════════
     Full render
     ═══════════════════════════════════════════════════════════ */
  function render(data) {
    if (!data || !data.length) return emptyState();
    var lead = data[0];
    var sides = data.slice(1, 3);
    var srcs = data.map(function (d) { return d.sourceName; });

    return '<div class="ps-grid">'
      + leadCard(lead)
      + '<div class="ps-col-right">'
      + sides.map(function (item, i) { return subCard(item, i + 1); }).join('')
      + ticker(srcs)
      + '</div></div>';
  }

  function skeleton() {
    return '<div class="ps-skel">'
      + '<div class="ps-skel__lead"><div class="ps-skel__shimmer"></div></div>'
      + '<div class="ps-col-right">'
      + '<div class="ps-skel__card"><div class="ps-skel__shimmer"></div></div>'
      + '<div class="ps-skel__card"><div class="ps-skel__shimmer"></div></div>'
      + '<div class="ps-skel__ticker"></div>'
      + '</div></div>';
  }

  function emptyState() {
    return '<div class="ps-empty">'
      + '<div class="ps-empty__icon">\u25CB</div>'
      + '<p class="ps-empty__text">Dang cho tin hieu moi tu duong dua.</p>'
      + '</div>';
  }

  function renderMock(reason) {
    console.warn('[PressSignal] Mock data:', reason);
    return render(MOCK_DATA);
  }

  async function load() {
    var el = section.querySelector('[data-press-content]');
    if (!el) return;
    el.innerHTML = skeleton();

    try {
      var res = await fetch(API_URL, { headers: { Accept: 'application/json' } });
      if (!res.ok) { el.innerHTML = renderMock('HTTP ' + res.status); return; }

      var payload = await res.json();
      if (!payload.data || !payload.data.length) {
        el.innerHTML = isDev() ? renderMock('empty') : render(payload.data);
        return;
      }
      el.innerHTML = render(payload.data);
    } catch (err) {
      el.innerHTML = isDev() ? renderMock(err.message) : emptyState();
    }
  }

  if (document.readyState === 'loading') {
    document.addEventListener('DOMContentLoaded', load);
  } else {
    load();
  }
})();
