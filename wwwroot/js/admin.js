(function () {
    var burger = document.querySelector('[data-admin-burger]');
    var side = document.querySelector('[data-admin-side]');
    var backdrop = document.querySelector('[data-admin-backdrop]');
    if (burger && side) {
        burger.addEventListener('click', function () { side.classList.toggle('is-open'); });
        if (backdrop) backdrop.addEventListener('click', function () { side.classList.remove('is-open'); });
    }
    var confirmers = Array.prototype.slice.call(document.querySelectorAll('[data-confirm]'));
    confirmers.forEach(function (el) {
        el.addEventListener('click', function (e) {
            var msg = el.getAttribute('data-confirm') || 'Bạn có chắc chắn?';
            if (!window.confirm(msg)) e.preventDefault();
        });
    });
})();
