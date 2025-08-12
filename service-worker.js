// Caution: Be sure to write to the correct file
self.importScripts('./service-worker-assets.js');
self.addEventListener('install', event => event.waitUntil(onInstall(event)));

async function onInstall(event) {
    console.info('Service worker: Install');
    
    // Get all assets from manifest except undefined or invalid URLs
    const assetsRequests = self.assetsManifest.assets
        .filter(asset => asset.url && asset.url !== 'undefined' && !asset.url.includes('404.html'))
        .map(asset => new Request(asset.url, { integrity: asset.hash, cache: 'no-cache' }));

    const cache = await caches.open('offline-cache');
    
    // Cache assets individually to handle failures gracefully
    for (const request of assetsRequests) {
        try {
            await cache.add(request);
        } catch (error) {
            console.warn('Failed to cache:', request.url, error);
        }
    }
}

self.addEventListener('fetch', event => event.respondWith(onFetch(event)));

async function onFetch(event) {
    let cachedResponse = null;
    if (event.request.method === 'GET') {
        try {
            cachedResponse = await caches.match(event.request);
        } catch (error) {
            console.warn('Cache match failed:', error);
        }
    }

    return cachedResponse || fetch(event.request);
}
/* Manifest version: r0mm1xyg */
