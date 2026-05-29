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
})();
