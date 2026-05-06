using System;
using TechnicalAssignment.Common.Patterns;
using TechnicalAssignment.ImageDownloader.Cache;
using TechnicalAssignment.ImageDownloader.Handler;
using TechnicalAssignment.ImageDownloader.Queue;
using UnityEngine;

namespace TechnicalAssignment.ImageDownloader.Core
{
    /// <summary>
    /// Singleton orchestrator for the entire image download system.
    ///
    /// PATTERN: Singleton (inherits SingletonMonoBehaviour<T>)
    /// WHY Singleton here (and not elsewhere):
    /// The download system must be globally accessible, persist across
    /// scene loads, and maintain a single queue + cache.
    /// Two ImageDownloaders would duplicate downloads and split the cache.
    /// This is one of the FEW legitimate Singleton use cases.
    ///
    /// FLOW for RequestImage(url, callback):
    ///   1. Check memory cache → hit? return immediately (0ms)
    ///   2. Check disk cache   → hit? load PNG, return (~5ms)
    ///   3. Enqueue download   → DownloadQueue manages concurrency
    ///      → RequestHandler executes HTTP download
    ///      → On complete: cache saves, callbacks fire
    ///
    /// ALL DECISIONS MADE HERE — sub-systems are pure workers.
    /// </summary>
    public class ImageDownloader : SingletonMonoBehaviour<ImageDownloader>,
                                   IImageDownloader
    {
        // ── Inspector Config ───────────────────────────────────────────────
        [Header("── Concurrency ─────────────────────")]
        [SerializeField, Range(1, 6)]
        private int _maxConcurrentDownloads = 3;

        [SerializeField]
        private float _overrideWaitSeconds = 10f;

        [Header("── Cache ────────────────────────────")]
        [SerializeField]
        private bool _enableMemoryCache = true;

        [SerializeField]
        private int _cacheDurationDays = 7;

        [Header("── Debug ────────────────────────────")]
        [SerializeField]
        private bool _verboseLogging = false;

        // ── Sub-systems ────────────────────────────────────────────────────
        private ICacheManager _cacheManager;
        private DownloadQueue _downloadQueue;
        private RequestHandler _requestHandler;

        private bool _initialized;

        // ── IImageDownloader ───────────────────────────────────────────────
        public int ActiveDownloads => _downloadQueue?.ActiveCount ?? 0;
        public int PendingDownloads => _downloadQueue?.PendingCount ?? 0;

        // ── Lifecycle ──────────────────────────────────────────────────────

        protected override void OnAwake()
        {
            Initialize();
        }

        private void Initialize()
        {
            if (_initialized) return;

            // Build sub-systems (Composition Root)
            _cacheManager = new CacheManager(enableMemoryCache: _enableMemoryCache);

            _downloadQueue = new DownloadQueue(
                maxConcurrent: _maxConcurrentDownloads,
                overrideWaitSeconds: _overrideWaitSeconds
            );

            _requestHandler = new RequestHandler(
                cacheManager: _cacheManager,
                cacheExpiry: TimeSpan.FromDays(_cacheDurationDays),
                coroutineRunner: this   // RequestHandler needs a MonoBehaviour to start coroutines
            );

            // Wire events
            _downloadQueue.OnRequestReady += _requestHandler.Execute;
            _downloadQueue.OnRequestAborted += OnRequestAborted;
            _requestHandler.OnComplete += _downloadQueue.OnDownloadComplete;

            // Purge old cache entries on startup
            _cacheManager.PurgeExpiredEntries();

            _initialized = true;
            Debug.Log($"[ImageDownloader] Initialized. MaxConcurrent={_maxConcurrentDownloads} " +
                      $"| Memory={_enableMemoryCache} | CacheDays={_cacheDurationDays}");
        }

        private void Update()
        {
            // Tick the queue every frame to check override conditions
            _downloadQueue?.Tick();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            Cleanup();
        }

        protected override void OnApplicationQuit()
        {
            base.OnApplicationQuit();
            Cleanup();
        }

        private void Cleanup()
        {
            if (_downloadQueue != null)
            {
                _downloadQueue.OnRequestReady -= _requestHandler.Execute;
                _downloadQueue.OnRequestAborted -= OnRequestAborted;
            }
            if (_requestHandler != null)
                _requestHandler.OnComplete -= _downloadQueue.OnDownloadComplete;
        }

        // ── IImageDownloader Implementation ────────────────────────────────

        /// <summary>
        /// Main entry point. Called by WebImage or any system needing an image.
        ///
        /// INTERVIEW EXPLANATION:
        /// "We check memory first (dictionary lookup, O(1), instant),
        ///  then disk (file I/O, ~5ms), then network (variable latency).
        ///  This three-tier cache ensures we never re-download an image
        ///  the user has already seen within the past 7 days."
        /// </summary>
        public void RequestImage(string url, Action<Texture2D, string> callback)
        {
            if (string.IsNullOrEmpty(url))
            {
                callback?.Invoke(null, "URL is null or empty.");
                return;
            }

            if (callback == null)
            {
                Debug.LogWarning($"[ImageDownloader] RequestImage called with null callback for: {url}");
                return;
            }

            EnsureInitialized();

            // ── Tier 1: Memory Cache ───────────────────────────────────────
            Texture2D memoryTexture = _cacheManager.GetFromMemory(url);
            if (memoryTexture != null)
            {
                if (_verboseLogging)
                    Debug.Log($"[ImageDownloader] Memory HIT: {url.Substring(0, Mathf.Min(50, url.Length))}");
                callback.Invoke(memoryTexture, null);
                return;
            }

            // ── Tier 2: Disk Cache ─────────────────────────────────────────
            if (_cacheManager.ExistsOnDisk(url))
            {
                var diskResult = _cacheManager.LoadFromDisk(url);
                if (diskResult.IsSuccess)
                {
                    callback.Invoke(diskResult.Value, null);
                    return;
                }
                // Disk load failed (corrupt file?) → fall through to network
                Debug.LogWarning($"[ImageDownloader] Disk load failed: {diskResult.ErrorMessage}");
            }

            // ── Tier 3: Network Download ───────────────────────────────────
            _downloadQueue.Enqueue(url, callback);
        }

        public void CancelRequest(string url, Action<Texture2D, string> callback)
        {
            // In a full implementation, we'd remove the specific callback
            // from the DownloadRequest's callback list.
            // For now: the WebImage component handles this by checking
            // if it's still alive when callback fires (see WebImage.cs).
            Debug.Log($"[ImageDownloader] Cancel requested for: {url}");
        }

        public void ClearAllCaches()
        {
            _cacheManager?.ClearAll();
            Debug.Log("[ImageDownloader] All caches cleared.");
        }

        // ── Private ────────────────────────────────────────────────────────

        private void OnRequestAborted(DownloadRequest request)
        {
            Debug.LogWarning($"[ImageDownloader] Request aborted (override): {request.Url}");
        }

        private void EnsureInitialized()
        {
            if (!_initialized) Initialize();
        }
    }
}