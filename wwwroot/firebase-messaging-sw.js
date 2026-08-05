// ─────────────────────────────────────────────────────────────────────────
// ⚠️ FIX: ده بقى الـ Service Worker الوحيد المسجل للموقع (scope '/').
// كان فيه service-worker.js تاني بيتسجل على نفس الـ scope عشان الـ PWA
// caching بس — تسجيل سكريبتين مختلفين على نفس الـ scope بيخلي المتصفح
// يستبدل الأول بالتاني، فالـ push subscription اللي اتعمل على أساس
// firebase-messaging-sw.js كان بيبقى معلّق على SW مش موجود فعليًا،
// ونتيجة كده الإشعار ما كانش بيوصل أبدًا لما التاب/الـ PWA مقفولة
// (كان بيبان شغال بس وانت فاتح لإن ده جاي من SignalR مش من الـ push أصلاً).
// الحل: ندمج كل حاجة (الـ caching + استقبال الـ push في الخلفية) في
// ملف واحد ونسجله مرة واحدة بس.
// ─────────────────────────────────────────────────────────────────────────

// ── 1) PWA shell caching (منقول من service-worker.js القديم) ─────────────
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

// ── 2) Firebase Messaging — بيتشغّل في الخلفية حتى لو التاب مقفول أو
// الـ PWA مش فاتحة، وبيعرض إشعار نظام (OS notification) عادي.
// السيرفر بيبعت data-only messages (زي الموبايل بالظبط)، فمفيش عرض تلقائي
// من فايربيز نفسها — لازم نبني الإشعار يدوي من payload.data هنا.
importScripts('https://www.gstatic.com/firebasejs/10.13.2/firebase-app-compat.js');
importScripts('https://www.gstatic.com/firebasejs/10.13.2/firebase-messaging-compat.js');
// القيم دي بتتحط وقت الـ build/serve من الـ endpoint تحت (مش hardcoded هنا)
// عشان نفس السيرفس ووركر يشتغل في أي بيئة (dev/staging/prod) من غير ما تتعدل يدوي.
// لو FirebaseWeb مش متظبطة في appsettings.json، الـ endpoint بيرجع
// self.firebaseConfig = null وبس (بدون أي خطأ)، فالبلوك تحت بيتخطى بأمان.
importScripts('/firebase-config.js');

if (self.firebaseConfig && self.firebaseConfig.apiKey) {
    firebase.initializeApp(self.firebaseConfig);
    var messaging = firebase.messaging();

    messaging.onBackgroundMessage(function (payload) {
        var data = payload.data || {};
        var title = data.title || 'Tawseela';
        var body = data.body || '';
        var url = data.orderId
            ? (data.type === 'PrescriptionRequest' ? '/Pharmacy' : '/MyStore/Orders')
            : '/';

        return self.registration.showNotification(title, {
            body: body,
            icon: '/images/icons/icon-192.png',
            badge: '/images/icons/icon-192.png',
            data: { url: url },
            requireInteraction: true
        });
    });
}

self.addEventListener('notificationclick', function (event) {
    event.notification.close();
    var url = (event.notification.data && event.notification.data.url) || '/';
    event.waitUntil(clients.openWindow(url));
});
