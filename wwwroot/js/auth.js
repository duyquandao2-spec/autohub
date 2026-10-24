(function () {
    var root = document.querySelector('[data-auth-menu]');
    if (!root) return;
    var toggle = root.querySelector('[data-account-toggle]');
    var dd = root.querySelector('[data-account-dd]');
    if (!toggle || !dd) return;

    function close() { dd.hidden = true; toggle.setAttribute('aria-expanded', 'false'); }
    function open() { dd.hidden = false; toggle.setAttribute('aria-expanded', 'true'); }

    toggle.addEventListener('click', function (e) {
        e.stopPropagation();
        dd.hidden ? open() : close();
    });
    document.addEventListener('click', function (e) { if (!root.contains(e.target)) close(); });
    document.addEventListener('keydown', function (e) { if (e.key === 'Escape') close(); });
})();
