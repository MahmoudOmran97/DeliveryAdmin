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
})();
