
(function () {
    'use strict';

    /* ---------- 1) Table cells ko header ka label do ---------- */
    function labelTables() {
        document.querySelectorAll('.custom-card table').forEach(function (table) {
            var headRow = table.querySelector('thead tr:last-child');
            if (!headRow) return;

            var labels = Array.prototype.map.call(headRow.children, function (th) {
                return (th.textContent || '').trim();
            });

            table.querySelectorAll('tbody tr').forEach(function (tr) {
                var col = 0;
                Array.prototype.forEach.call(tr.children, function (td) {
                    var span = parseInt(td.getAttribute('colspan') || '1', 10);

                    // Empty-state row (colspan > 1): label nahi chahiye
                    if (span > 1) {
                        td.classList.add('td-empty');
                        col += span;
                        return;
                    }

                    var label = labels[col] || '';
                    col += 1;

                    if (/^actions?$/i.test(label)) {
                        td.classList.add('td-actions');   // label nahi, buttons poori width
                    } else if (label && !td.hasAttribute('data-label')) {
                        td.setAttribute('data-label', label);
                    }
                });
            });
        });
    }

    /* ---------- 2) Sidebar active link ---------- */
    function markActiveNav() {
        var links = document.querySelectorAll('.dashboard-sidebar .nav-link[href]');
        var path = location.pathname.toLowerCase().replace(/\/+$/, '') || '/';
        var best = null, bestLen = -1;

        links.forEach(function (a) {
            var href = (a.getAttribute('href') || '').split('?')[0].toLowerCase().replace(/\/+$/, '') || '/';
            if (href === path || path.indexOf(href + '/') === 0) {
                if (href.length > bestLen) { best = a; bestLen = href.length; }
            }
        });

        if (best) {
            best.classList.add('active');
            best.setAttribute('aria-current', 'page');
        }
    }

    function init() {
        labelTables();
        markActiveNav();
    }

    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', init);
    } else {
        init();
    }
})();