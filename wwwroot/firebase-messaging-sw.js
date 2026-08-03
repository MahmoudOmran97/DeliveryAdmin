// ─────────────────────────────────────────────────────────────────────────
// Firebase Messaging Service Worker — بيتشغّل في الخلفية حتى لو التاب
// مقفول أو الـ PWA مش فاتحة، وبيعرض إشعار نظام (OS notification) عادي.
// السيرفر بيبعت data-only messages (زي الموبايل بالظبط)، فمفيش عرض تلقائي
// من فايربيز نفسها — لازم نبني الإشعار يدوي من payload.data هنا.
// ─────────────────────────────────────────────────────────────────────────
importScripts('https://www.gstatic.com/firebasejs/10.13.2/firebase-app-compat.js');
importScripts('https://www.gstatic.com/firebasejs/10.13.2/firebase-messaging-compat.js');
// القيم دي بتتحط وقت الـ build/serve من الـ endpoint تحت (مش hardcoded هنا)
// عشان نفس السيرفس ووركر يشتغل في أي بيئة (dev/staging/prod) من غير ما تتعدل يدوي
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
