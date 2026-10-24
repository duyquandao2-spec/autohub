/* ── AUTOHUB AI Chatbot — Context-Aware & Database Connected ── */
(function () {
  var root = document.querySelector('[data-chatbot]');
  if (!root) return;

  var win = root.querySelector('.chatbot__window');
  var messagesEl = root.querySelector('[data-chatbot-messages]');
  var actionsEl = root.querySelector('[data-chatbot-actions]');
  var input = root.querySelector('[data-chatbot-input]');
  var sendBtn = root.querySelector('[data-chatbot-send]');
  var closeBtn = root.querySelector('[data-chatbot-close]');

  if (!win || !messagesEl || !input || !sendBtn) return;

  var state = window.ApexChatState;
  var service = window.ApexChatService;
  var welcomeSent = false;
  var lastSentMessage = '';

  function uid() {
    return Date.now().toString(36) + Math.random().toString(36).slice(2, 6);
  }

  function timeNow() {
    var d = new Date();
    return d.getHours().toString().padStart(2, '0') + ':' + d.getMinutes().toString().padStart(2, '0');
  }

  function scrollToBottom() {
    messagesEl.scrollTop = messagesEl.scrollHeight;
  }

  function formatMarkdown(text) {
    if (!text) return '';
    // Escape HTML first
    var safe = text
      .replace(/&/g, '&amp;')
      .replace(/</g, '&lt;')
      .replace(/>/g, '&gt;');

    // Bold **text**
    safe = safe.replace(/\*\*(.*?)\*\*/g, '<strong>$1</strong>');
    // Italic *text*
    safe = safe.replace(/\*(.*?)\*/g, '<em>$1</em>');
    // Bullet points •
    safe = safe.replace(/^[•\-]\s*(.*)$/gm, '&bull; $1');
    // Line breaks
    safe = safe.replace(/\n/g, '<br/>');

    return safe;
  }

  function renderVehicleCards(vehicles) {
    var container = document.createElement('div');
    container.className = 'chatbot__vehicle-list';

    vehicles.forEach(function (v) {
      var card = document.createElement('div');
      card.className = 'chatbot__vehicle-card';

      var img = document.createElement('img');
      img.className = 'chatbot__vehicle-thumb';
      img.src = v.imagePath || v.ImagePath || '/Images/placeholder-car.jpg';
      img.alt = v.name || v.Name || 'Xe';

      var body = document.createElement('div');
      body.className = 'chatbot__vehicle-body';

      var meta = document.createElement('div');
      meta.className = 'chatbot__vehicle-meta';
      meta.textContent = (v.brand || v.Brand || '') + ' · ' + (v.category || v.Category || 'XE');

      var name = document.createElement('div');
      name.className = 'chatbot__vehicle-name';
      name.textContent = v.name || v.Name || '';

      var price = document.createElement('div');
      price.className = 'chatbot__vehicle-price';
      price.textContent = v.priceDisplay || v.PriceDisplay || 'Liên hệ';

      var specs = document.createElement('div');
      specs.className = 'chatbot__vehicle-specs';
      var hp = v.power || v.Power;
      var acc = v.acceleration || v.Acceleration;
      if (acc && acc !== '-') {
        specs.innerHTML += '<span>0-100: <strong>' + acc + '</strong></span>';
      }
      if (hp && hp !== '-') {
        specs.innerHTML += '<span>CS: <strong>' + hp + '</strong></span>';
      }

      var actions = document.createElement('div');
      actions.className = 'chatbot__vehicle-actions';

      var slug = v.slug || v.Slug || '';
      actions.innerHTML = `
        <a href="/Vehicles/${slug}" class="chatbot__card-btn chatbot__card-btn--primary">Chi tiết</a>
        <a href="/Finance?vehicle=${slug}" class="chatbot__card-btn">Trả góp</a>
        <a href="/TestDrive?vehicle=${slug}" class="chatbot__card-btn">Lái thử</a>
      `;

      body.appendChild(meta);
      body.appendChild(name);
      body.appendChild(price);
      if (specs.children.length > 0) body.appendChild(specs);
      body.appendChild(actions);

      card.appendChild(img);
      card.appendChild(body);
      container.appendChild(card);
    });

    return container;
  }

  function renderMessage(msg) {
    var el = document.createElement('div');
    el.className = 'chatbot__msg chatbot__msg--' + msg.role;

    var bubble = document.createElement('div');
    bubble.className = 'chatbot__bubble';

    if (msg.role === 'assistant') {
      bubble.innerHTML = formatMarkdown(msg.content);

      if (msg.vehicles && msg.vehicles.length > 0) {
        var cardsEl = renderVehicleCards(msg.vehicles);
        bubble.appendChild(cardsEl);
      }
    } else {
      bubble.textContent = msg.content;
    }

    el.appendChild(bubble);

    var time = document.createElement('span');
    time.className = 'chatbot__time';
    time.textContent = msg.timestamp;
    el.appendChild(time);

    messagesEl.appendChild(el);

    requestAnimationFrame(function () {
      requestAnimationFrame(function () {
        el.classList.add('is-visible');
      });
    });

    scrollToBottom();
    return el;
  }

  function renderActions(actions) {
    actionsEl.innerHTML = '';
    if (!actions || actions.length === 0) return;

    actions.forEach(function (a) {
      var btn = document.createElement('button');
      btn.className = 'chatbot__action-btn';
      btn.type = 'button';
      btn.textContent = a.label || a.Label;
      btn.dataset.action = a.action || a.Action;
      if (a.href || a.Href) btn.dataset.href = a.href || a.Href;
      actionsEl.appendChild(btn);
    });
  }

  function showLoading() {
    state.isLoading = true;
    var el = document.createElement('div');
    el.className = 'chatbot__msg chatbot__msg--assistant';
    el.dataset.loading = 'true';

    var loadingDiv = document.createElement('div');
    loadingDiv.className = 'chatbot__loading';

    var dotsSpan = document.createElement('span');
    dotsSpan.className = 'chatbot__dots';
    var dot1 = document.createElement('span');
    dot1.className = 'chatbot__dot';
    var dot2 = document.createElement('span');
    dot2.className = 'chatbot__dot';
    var dot3 = document.createElement('span');
    dot3.className = 'chatbot__dot';
    dotsSpan.appendChild(dot1);
    dotsSpan.appendChild(dot2);
    dotsSpan.appendChild(dot3);

    var textSpan = document.createElement('span');
    textSpan.textContent = 'APEX AI đang tra cứu dữ liệu xe...';

    loadingDiv.appendChild(dotsSpan);
    loadingDiv.appendChild(textSpan);
    el.appendChild(loadingDiv);
    messagesEl.appendChild(el);
    scrollToBottom();
  }

  function hideLoading() {
    state.isLoading = false;
    var el = messagesEl.querySelector('[data-loading="true"]');
    if (el) el.remove();
  }

  function showError(retryFn) {
    var el = document.createElement('div');
    el.className = 'chatbot__msg chatbot__msg--assistant';

    var errorDiv = document.createElement('div');
    errorDiv.className = 'chatbot__error';

    var errorText = document.createElement('span');
    errorText.textContent = 'Không thể kết nối máy chủ. ';
    errorDiv.appendChild(errorText);

    var retryBtn = document.createElement('button');
    retryBtn.type = 'button';
    retryBtn.className = 'chatbot__retry';
    retryBtn.textContent = 'Thử lại';
    retryBtn.addEventListener('click', function () {
      el.remove();
      retryFn();
    });

    errorDiv.appendChild(retryBtn);
    el.appendChild(errorDiv);
    messagesEl.appendChild(el);
    scrollToBottom();
  }

  function handleAction(action, btn) {
    if (action === 'navigate' && btn && btn.dataset.href) {
      window.location.href = btn.dataset.href;
      return;
    }

    if (action === 'query') {
      addAndRespond(btn ? btn.textContent.trim() : '');
      return;
    }

    switch (action) {
      case 'consultation':
        var consultToggle = document.querySelector('[data-contact-hub-toggle]');
        if (consultToggle) consultToggle.click();
        else window.location.href = '/Contact';
        break;
      case 'view-3d':
        var showcaseOverlay = document.getElementById('vsc-overlay');
        if (showcaseOverlay) {
          document.dispatchEvent(new CustomEvent('apex:showcase:open'));
        } else {
          window.location.href = '/Experience#exp-3d';
        }
        break;
      case 'view-experience':
        window.location.href = '/Experience';
        break;
      case 'view-all-vehicles':
        window.location.href = '/#machines';
        break;
      case 'filter-ev':
        window.location.href = '/?flag=ev#machines';
        break;
      case 'search-vehicle':
        addAndRespond('Gợi ý các dòng xe nổi bật');
        break;
      default:
        if (btn && btn.textContent) addAndRespond(btn.textContent.trim());
        break;
    }
  }

  function addAndRespond(text) {
    if (!text || !text.trim() || state.isLoading) return;

    var userMsg = { id: uid(), role: 'user', content: text.trim(), timestamp: timeNow() };
    state.messages.push(userMsg);
    renderMessage(userMsg);

    lastSentMessage = text.trim();
    input.value = '';
    state.inputValue = '';
    updateSendBtn();
    autoResize();
    actionsEl.innerHTML = '';

    showLoading();

    service.sendMessage(lastSentMessage).then(function (response) {
      hideLoading();
      var assistantMsg = {
        id: uid(),
        role: 'assistant',
        content: response.content,
        vehicles: response.vehicles || [],
        timestamp: timeNow()
      };
      state.messages.push(assistantMsg);
      renderMessage(assistantMsg);
      renderActions(response.actions);
    }).catch(function () {
      hideLoading();
      showError(function () {
        addAndRespond(lastSentMessage);
      });
    });
  }

  function updateSendBtn() {
    sendBtn.disabled = !input.value.trim();
  }

  function autoResize() {
    input.style.height = 'auto';
    input.style.height = Math.min(input.scrollHeight, 120) + 'px';
  }

  input.addEventListener('input', function () {
    state.inputValue = input.value;
    updateSendBtn();
    autoResize();
  });

  input.addEventListener('keydown', function (e) {
    if (e.key === 'Enter' && !e.shiftKey) {
      e.preventDefault();
      if (input.value.trim()) addAndRespond(input.value);
    }
  });

  sendBtn.addEventListener('click', function () {
    if (input.value.trim()) addAndRespond(input.value);
  });

  actionsEl.addEventListener('click', function (e) {
    var btn = e.target.closest('.chatbot__action-btn');
    if (!btn) return;
    handleAction(btn.dataset.action, btn);
  });

  function sendWelcome() {
    if (welcomeSent) return;
    welcomeSent = true;

    var msg1 = {
      id: uid(),
      role: 'assistant',
      content: 'Xin chào! Tôi là **APEX AI** — trợ lý tư vấn showroom AUTOHUB.',
      timestamp: timeNow()
    };
    state.messages.push(msg1);
    renderMessage(msg1);

    setTimeout(function () {
      var msg2 = {
        id: uid(),
        role: 'assistant',
        content: 'Tôi có thể hỗ trợ bạn tìm xe theo ngân sách (VD: *tầm 2 tỷ*, *dưới 3 tỷ*), tìm xe SUV/Sedan 7 chỗ, xe điện EV hoặc tính toán trả góp ngân hàng.',
        timestamp: timeNow()
      };
      state.messages.push(msg2);
      renderMessage(msg2);

      setTimeout(function () {
        renderActions([
          { label: 'Xe dưới 2 tỷ', action: 'query' },
          { label: 'Xe SUV gia đình', action: 'query' },
          { label: 'Xe điện EV', action: 'filter-ev' },
          { label: 'Dự toán trả góp', action: 'navigate', href: '/Finance' },
          { label: 'Xem xe 3D', action: 'view-3d' }
        ]);
      }, 180);
    }, 220);
  }

  function openChat() {
    state.isOpen = true;
    root.classList.add('is-open');
    input.focus();
    sendWelcome();
  }

  function closeChat() {
    state.isOpen = false;
    root.classList.remove('is-open');
  }

  function toggleChat() {
    if (state.isOpen) {
      closeChat();
    } else {
      openChat();
    }
  }

  document.addEventListener('apex:chatbot:toggle', toggleChat);
  closeBtn.addEventListener('click', closeChat);

  document.addEventListener('keydown', function (e) {
    if (e.key === 'Escape' && state.isOpen) closeChat();
  });

  document.addEventListener('pointerdown', function (e) {
    if (state.isOpen && !win.contains(e.target) && !e.target.closest('[data-contact-hub]')) {
      closeChat();
    }
  });
})();
