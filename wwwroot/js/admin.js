(function () {
    const root = document.documentElement;
    const key = 'deliveryadmin-theme';
    const saved = localStorage.getItem(key) || 'dark';
    root.setAttribute('data-theme', saved);
    updateThemeIcon(saved);

    document.getElementById('themeToggle')?.addEventListener('click', function () {
        const next = root.getAttribute('data-theme') === 'light' ? 'dark' : 'light';
        root.setAttribute('data-theme', next);
        localStorage.setItem(key, next);
        updateThemeIcon(next);
    });

    function updateThemeIcon(theme) {
        const btn = document.getElementById('themeToggle');
        if (!btn) return;
        btn.textContent = theme === 'light' ? '☀️' : '🌙';
    }

    // Mobile / tablet off-canvas sidebar
    const sidebar = document.getElementById('sidebar');
    const overlay = document.getElementById('sidebarOverlay');
    const openBtn = document.getElementById('sidebarOpen');
    const closeBtn = document.getElementById('sidebarClose');

    function openSidebar() {
        sidebar?.classList.add('open');
        overlay?.classList.add('open');
    }
    function closeSidebar() {
        sidebar?.classList.remove('open');
        overlay?.classList.remove('open');
    }

    openBtn?.addEventListener('click', openSidebar);
    closeBtn?.addEventListener('click', closeSidebar);
    overlay?.addEventListener('click', closeSidebar);

    // Close the drawer automatically when a nav link is tapped (mobile)
    document.querySelectorAll('.nav-item').forEach(function (link) {
        link.addEventListener('click', closeSidebar);
    });

    // Tag every table cell with its column header text (data-label),
    // so CSS can turn rows into stacked cards on phone screens.
    document.querySelectorAll('.table-wrap table').forEach(function (table) {
        var headers = Array.from(table.querySelectorAll('thead th')).map(function (th) {
            return th.textContent.trim();
        });
        if (!headers.length) return;
        table.querySelectorAll('tbody tr, tfoot tr').forEach(function (row) {
            var cells = row.querySelectorAll('td');
            if (cells.length === 1 && cells[0].hasAttribute('colspan')) return; // empty-state row
            cells.forEach(function (td, i) {
                if (headers[i]) td.setAttribute('data-label', headers[i]);
            });
        });
    });
    // Generic live search: any input with .table-search-input filters the
    // rows of the nearest table in the same .card as you type (no reload).
    document.querySelectorAll('.table-search-input').forEach(function (input) {
        input.addEventListener('input', function () {
            var card = input.closest('.card');
            var table = card ? card.querySelector('table') : null;
            if (!table) return;
            var term = input.value.trim().toLowerCase();
            table.querySelectorAll('tbody tr').forEach(function (row) {
                if (row.querySelector('td[colspan]')) return; // leave empty-state row alone
                var text = row.textContent.toLowerCase();
                row.style.display = !term || text.indexOf(term) !== -1 ? '' : 'none';
            });
        });
    });

    // ── زرار "ارجع لفوق" — بيظهر بعد سكرول معين لتحت وبيرجعك لأول الصفحة ──
    // ملحوظة: اللي بيعمل سكرول فعليًا هو .main (مش الـ window)، فبنراقب سكرول
    // .main نفسها مش الصفحة عمومًا.
    var scrollContainer = document.querySelector('.main');
    var scrollBtn = document.createElement('button');
    scrollBtn.type = 'button';
    scrollBtn.className = 'scroll-top-btn';
    scrollBtn.setAttribute('aria-label', 'Scroll to top');
    scrollBtn.innerHTML = '↑';
    document.body.appendChild(scrollBtn);

    // لو زرار "تواصل مع الدعم" ظاهر (صاحب المحل)، بنرفع زرار الرجوع لفوق شوية
    // عشان الاتنين ميتلخبطوش على بعض في نفس الركن
    if (document.getElementById('supportFloatBtn')) {
        scrollBtn.style.bottom = '92px';
    }

    var scrollThreshold = 300;
    function toggleScrollBtn() {
        var y = window.scrollY || (scrollContainer ? scrollContainer.scrollTop : 0);
        if (y > scrollThreshold) {
            scrollBtn.classList.add('visible');
        } else {
            scrollBtn.classList.remove('visible');
        }
    }
    window.addEventListener('scroll', toggleScrollBtn, { passive: true });
    if (scrollContainer) {
        scrollContainer.addEventListener('scroll', toggleScrollBtn, { passive: true });
    }
    toggleScrollBtn();

    scrollBtn.addEventListener('click', function () {
        window.scrollTo({ top: 0, behavior: 'smooth' });
        if (scrollContainer && scrollContainer.scrollTop > 0) {
            scrollContainer.scrollTo({ top: 0, behavior: 'smooth' });
        }
    });
})();
