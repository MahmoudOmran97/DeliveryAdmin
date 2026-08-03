// Tawseela Admin - Service Worker
// Minimal SW: makes the site installable (PWA) and caches the app shell
// (logo/icons/css/js) so the UI still loads instantly on a flaky connection.
// Page data (orders, dashboard, etc.) is NOT cached - always fetched fresh.

const CACHE_NAME = 'tawseela-admin-shell-v1';
const SHELL_ASSETS = [
  '/css/admin.css',
  '/js/admin.js',
  '/images/logo.png',
  '/images/icons/icon-192.png',
  '/images/icons/icon-512.png',
  '/favicon.ico',
  '/manifest.json'
];

self.addEventListener('install', (event) => {
  event.waitUntil(
    caches.open(CACHE_NAME)
      .then((cache) => cache.addAll(SHELL_ASSETS))
      .then(() => self.skipWaiting())
  );
});

self.addEventListener('activate', (event) => {
  event.waitUntil(
    caches.keys().then((keys) =>
      Promise.all(keys.filter((k) => k !== CACHE_NAME).map((k) => caches.delete(k)))
    ).then(() => self.clients.claim())
  );
});

self.addEventListener('fetch', (event) => {
  const req = event.request;

  // Only handle GET requests for our own static shell assets.
  if (req.method !== 'GET') return;

  const url = new URL(req.url);
  const isShellAsset = SHELL_ASSETS.some((path) => url.pathname === path);
  if (!isShellAsset) return; // let everything else (pages/API) hit the network normally

  event.respondWith(
    caches.match(req).then((cached) => {
      const fetchPromise = fetch(req).then((res) => {
        if (res && res.status === 200) {
          const clone = res.clone();
          caches.open(CACHE_NAME).then((cache) => cache.put(req, clone));
        }
        return res;
      }).catch(() => cached);
      return cached || fetchPromise;
    })
  );
});
