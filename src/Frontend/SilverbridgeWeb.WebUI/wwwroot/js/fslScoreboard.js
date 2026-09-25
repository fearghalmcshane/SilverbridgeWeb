'use strict';

  var instances = new WeakMap();

  var API = 'https://uingnhvsgijsbfanrsry.supabase.co';
  var KEY = 'sb_publishable_d99Fz4rogMS79Q0AMve1oA_xwNCpKn2';

  // Everything renders inside a shadow root, so this widget cannot affect
  // the rest of your page and your stylesheet cannot affect it.
  var CSS = [
    // The host element lives in the club's document, so their rules beat a
    // plain :host. !important is what stops a theme's div{border;background}
    // drawing a box around the widget.
    ':host{all:initial!important;display:block!important}',
    // Inheritable properties cross the shadow boundary from the host element,
    // so a club stylesheet with *{font-family:x!important} would reach in.
    // Setting them on a node INSIDE the shadow tree blocks that.
    '.w{max-width:620px;margin:0 auto;display:flex;flex-direction:column;gap:10px;overflow:hidden;',
      'font-family:"Arial Narrow","Helvetica Neue",Helvetica,Arial,sans-serif;',
      'font-style:normal;font-weight:400;line-height:normal;letter-spacing:normal;',
      'text-transform:none;text-align:left;color:#131211;direction:ltr}',
    '.w *{box-sizing:border-box;margin:0;padding:0;font-family:inherit}',
    '.sp{background:#fff;border:1px solid #DFDCD7;border-radius:10px;padding:12px 18px 14px;',
      'display:flex;flex-direction:column;align-items:center;gap:7px;text-align:center}',
    '.sp-e{font-size:11px;font-weight:700;letter-spacing:.14em;text-transform:uppercase;color:#7C776F}',
    '.sp-n{font-size:clamp(18px,5vw,30px);font-weight:700;letter-spacing:.02em;color:#141414;',
      'line-height:1.1;overflow-wrap:anywhere;max-width:100%}',
    '.sp-l{display:block;max-width:100%;max-height:70px;width:auto}',
    'a.sp-a{text-decoration:none;display:block}',
    '.b{background:#131211;border-radius:12px;padding:16px 20px 12px;display:flex;flex-direction:column}',
    '.bar{display:flex;align-items:center;gap:14px;padding-bottom:10px;border-bottom:1px solid rgba(255,255,255,.16)}',
    '.st{display:flex;align-items:center;gap:8px;font-size:clamp(11px,2.6vw,14px);font-weight:700;',
      'letter-spacing:.1em;text-transform:uppercase;color:#fff;white-space:nowrap}',
    '.dot{width:10px;height:10px;border-radius:50%;background:#7C776F;flex:none}',
    '.bar[data-s="live"] .dot{background:#3FCF55;animation:p 2s ease-in-out infinite}',
    '.bar[data-s="finished"] .dot{background:#F79A28}',
    '.bar[data-s="offline"] .dot{background:#B4463F}',
    '@keyframes p{0%,100%{opacity:1}50%{opacity:.3}}',
    '.sep{width:2px;height:20px;background:#F79A28;flex:none}',
    '.clk{font-size:clamp(26px,8vw,46px);font-weight:700;color:#F79A28;',
      'font-variant-numeric:tabular-nums;line-height:.9;margin:0 auto}',
    '.rows{display:flex;flex-direction:column}',
    '.r{display:flex;align-items:center;justify-content:space-between;gap:16px;padding:14px 0}',
    '.r+.r{border-top:2px solid #C97D1E}',
    '.nm{font-size:clamp(20px,6.4vw,38px);font-weight:700;color:#fff;text-transform:uppercase;',
      'line-height:1;min-width:0;overflow:hidden;text-overflow:ellipsis;white-space:nowrap}',
    '.sc{font-size:clamp(26px,8.4vw,50px);font-weight:700;color:#F79A28;',
      'font-variant-numeric:tabular-nums;line-height:.9;flex:none}',
    '.pw{display:flex;align-items:center;gap:14px;padding-top:12px;border-top:1px solid rgba(255,255,255,.16)}',
    '.pw-r{flex:1;height:2px;background:#C97D1E}',
    '.pw-a{display:flex;align-items:center;gap:9px;text-decoration:none;font-size:11px;font-weight:700;',
      'letter-spacing:.14em;text-transform:uppercase;color:#fff;white-space:nowrap}',
    '.pw-m{display:block;height:28px;width:auto;background:#fff;border-radius:3px;padding:3px 5px}',
    '.b[data-stale="true"] .rows,.b[data-stale="true"] .clk{opacity:.4}',
    '.msg{padding:30px 0;text-align:center;font-size:14px;font-weight:700;letter-spacing:.14em;',
      'text-transform:uppercase;color:#7C776F}',
    '[hidden]{display:none!important}',
    '@media(prefers-reduced-motion:reduce){.dot{animation:none!important}}'
  ].join('');

  // FSL Scoreboards logo, embedded so there is no image to host.
  // Wrapped in a link back to fslscoreboards.com.
  var LOGO =
    '<img class="pw-m" alt="FSL Scoreboards" '+
      'src="data:image/png;base64,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">';

  var HTML =
    '<div class="w">' +
      '<div class="sp" data-el="sponsor" hidden>' +
        '<span class="sp-e">Scoreboard sponsor</span><div data-el="sponsorSlot"></div>' +
      '</div>' +
      '<div class="b" data-el="board" data-stale="false">' +
        '<div class="bar" data-el="bar" data-s="idle">' +
          '<span class="st"><span class="dot"></span><span data-el="state">Connecting</span></span>' +
          '<span class="sep"></span>' +
          '<span class="clk" data-el="clock">00:00</span>' +
          '<span class="sep"></span>' +
        '</div>' +
        '<div class="msg" data-el="msg">Waiting for board</div>' +
        '<div class="rows" data-el="rows" hidden aria-live="polite" aria-atomic="true">' +
          '<div class="r"><span class="nm" data-el="homeName">HOME</span>' +
            '<span class="sc" data-el="homeScore">0-00</span></div>' +
          '<div class="r"><span class="nm" data-el="awayName">AWAY</span>' +
            '<span class="sc" data-el="awayScore">0-00</span></div>' +
        '</div>' +
        '<div class="pw"><span class="pw-r"></span>' +
          '<a class="pw-a" href="https://www.fslscoreboards.com" target="_blank" rel="noopener noreferrer">' +
            '<span>Powered by</span>' + LOGO + '</a>' +
          '<span class="pw-r"></span>' +
        '</div>' +
      '</div>' +
    '</div>';

  export function initialize(host, boardId) {
    if (instances.has(host)) return; // never initialise the same element twice
    host.__fsl = true;

    var board = (boardId || host.getAttribute('data-board') || '')
                  .toUpperCase().replace(/[^A-Z0-9_-]/g, '').slice(0, 32);
    var RENAMED = { 'FC012CD7FDC4': 'ARDBOESB' };
    if (RENAMED[board]) board = RENAMED[board];
    if (!board) { host.textContent = 'FSL scoreboard: data-board is missing.'; return; }

    var root = host.attachShadow ? host.attachShadow({ mode: 'open' }) : host;
    var style = document.createElement('style');
    style.textContent = CSS;
    root.appendChild(style);
    var wrap = document.createElement('div');
    wrap.innerHTML = HTML;
    root.appendChild(wrap);

    var el = {};
    Array.prototype.forEach.call(root.querySelectorAll('[data-el]'), function (n) {
      el[n.getAttribute('data-el')] = n;
    });

    var clockBase = 0, running = false, syncedAt = 0;
    var backoff = 2000, last = null, prev = { h: null, a: null };

    function pad(n) { return String(n).padStart(2, '0'); }
    function score(g, p) { return g + '-' + pad(p); }
    function clock(s) {
      s = Math.max(0, Math.floor(s));
      return pad(Math.floor(s / 60)) + ':' + pad(s % 60);
    }
    function state(s, t) { el.bar.setAttribute('data-s', s); el.state.textContent = t; }

    // Values arrive from the board. Cap them so an absurd length can never
    // push the club's page sideways.
    function cap(v, n) {
      if (v === null || v === undefined) return '';
      return String(v).slice(0, n);
    }
    function num(v) {
      var n = parseInt(v, 10);
      return isFinite(n) && n >= 0 && n < 1000 ? n : 0;
    }

    // A sponsor link or logo comes from the database and lands in href/src,
    // so only real web URLs are let through.
    function safe(u, allowData) {
      if (!u) return null;
      var v = String(u).trim();
      if (/^https?:\/\//i.test(v)) return v;
      if (allowData && /^data:image\/(png|jpe?g|gif|svg\+xml|webp);base64,/i.test(v)) return v;
      return null;
    }

    function sponsor(text, logoRaw, urlRaw) {
      var logo = safe(logoRaw, true), url = safe(urlRaw, false);
      if (!text && !logo) { el.sponsor.hidden = true; return; }
      el.sponsor.hidden = false;
      var inner;
      if (logo) {
        inner = document.createElement('img');
        inner.className = 'sp-l';
        inner.src = logo;
        inner.alt = text || 'Scoreboard sponsor';
        inner.loading = 'lazy';
        inner.onerror = function () { sponsor(text, null, urlRaw); };
      } else {
        inner = document.createElement('div');
        inner.className = 'sp-n';
        inner.textContent = text;
      }
      var node = inner;
      if (url) {
        var a = document.createElement('a');
        a.className = 'sp-a';
        a.href = url; a.target = '_blank'; a.rel = 'noopener noreferrer';
        a.appendChild(inner);
        node = a;
      }
      el.sponsorSlot.textContent = '';
      el.sponsorSlot.appendChild(node);
    }

    function render(row, message) {
      if (!row) {
        last = null;
        el.rows.hidden = true;
        el.sponsor.hidden = true;
        el.msg.hidden = false;
        el.msg.textContent = message || 'Waiting for board';
        el.clock.textContent = '00:00';
        state('idle', 'Offline');
        return;
      }
      last = row;
      el.msg.hidden = true;
      el.rows.hidden = false;

      sponsor(cap(row.sponsor_text, 60), row.sponsor_logo_url, row.sponsor_url);

      el.homeName.textContent = cap(row.home_name, 40) || 'HOME';
      el.awayName.textContent = cap(row.away_name, 40) || 'AWAY';

      var h = score(num(row.home_goals), num(row.home_points));
      var a = score(num(row.away_goals), num(row.away_points));
      prev.h = h; prev.a = a;
      el.homeScore.textContent = h;
      el.awayScore.textContent = a;

      clockBase = num(row.clock_seconds);
      running = !!row.clock_running && row.status === 'live';
      syncedAt = Date.now();
      tick();

      var offline = row.online === false;
      el.board.setAttribute('data-stale', offline ? 'true' : 'false');
      if (offline) state('offline', 'Board offline');
      else if (row.status === 'live') state('live', 'Live');
      else if (row.status === 'finished') state('finished', 'Full time');
      else state('idle', 'Not started');
    }

    function tick() {
      var s = clockBase;
      if (running) s += (Date.now() - syncedAt) / 1000;
      el.clock.textContent = clock(s);
    }

    function poll() {
      fetch(API + '/rest/v1/live_scores?mac=eq.' + encodeURIComponent(board) + '&select=*', {
        headers: { apikey: KEY, Authorization: 'Bearer ' + KEY },
        cache: 'no-store'
      })
        .then(function (r) { if (!r.ok) throw new Error(r.status); return r.json(); })
        .then(function (rows) {
          if (disposed) return;
          backoff = 2000;
          render(rows && rows.length ? rows[0] : null, 'No board registered');
        })
        .catch(function () {
          if (disposed) return;
          if (last) { el.board.setAttribute('data-stale', 'true'); state('offline', 'Reconnecting'); }
          else { render(null, 'Cannot reach scoreboard'); }
          backoff = Math.min(backoff * 2, 30000);
        })
        .then(function () { if (!disposed) schedule(); });
    }

    function schedule() {
      if (!document.body.contains(host)) { cleanup(); return; }
      // A hidden tab doesn't need a score. Saves requests and phone battery.
      var wait = document.hidden ? 15000 : backoff;
      timer = setTimeout(poll, wait);
    }

    var timer = null;
    var disposed = false;
    var ticker = setInterval(tick, 250);

    function onVisibilityChange() {
      if (!document.hidden) { clearTimeout(timer); backoff = 2000; poll(); }
    }

    function cleanup() {
      if (disposed) return;
      disposed = true;
      clearTimeout(timer);
      clearInterval(ticker);
      document.removeEventListener('visibilitychange', onVisibilityChange);
      instances.delete(host);
      delete host.__fsl;
    }

    document.addEventListener('visibilitychange', onVisibilityChange);
    instances.set(host, cleanup);
    poll();
  }

  export function dispose(host) {
    var cleanup = instances.get(host);
    if (cleanup) cleanup();
  }
