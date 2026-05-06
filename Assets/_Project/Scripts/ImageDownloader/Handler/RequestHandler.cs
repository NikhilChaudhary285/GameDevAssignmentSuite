using System;
using System.Collections;
using TechnicalAssignment.ImageDownloader.Cache;
using TechnicalAssignment.ImageDownloader.Core;
using UnityEngine;
using UnityEngine.Networking;

namespace TechnicalAssignment.ImageDownloader.Handler
{
    /// <summary>
    /// Executes the actual HTTP download for one DownloadRequest.
    ///
    /// SINGLE RESPONSIBILITY:
    /// - Make the web request
    /// - Decode bytes into Texture2D with alpha support
    /// - Save result to disk cache
    /// - Notify the DownloadRequest (which notifies all callbacks)
    ///
    /// WHY a coroutine and not async/await:
    /// UnityWebRequest is NOT thread-safe. It must run on the main thread
    /// via coroutine. Using Task.Run() with UnityWebRequest causes crashes.
    /// Coroutines are Unity's sanctioned async pattern for web requests.
    ///
    /// ALPHA SUPPORT:
    /// We create Texture2D with TextureFormat.RGBA32 (not RGB24).
    /// LoadImage() respects PNG alpha channels automatically.
    /// This satisfies the "support alpha images" requirement.
    /// </summary>
    public class RequestHandler
    {
        private readonly ICacheManager _cacheManager;
        private readonly TimeSpan _cacheExpiry;
        private readonly MonoBehaviour _coroutineRunner; // needed to start coroutines

        public event Action<DownloadRequest> OnComplete; // success or failure

        public RequestHandler(
            ICacheManager cacheManager,
            TimeSpan cacheExpiry,
            MonoBehaviour coroutineRunner)
        {
            _cacheManager = cacheManager;
            _cacheExpiry = cacheExpiry;
            _coroutineRunner = coroutineRunner;
        }

        /// <summary>
        /// Starts the download coroutine for the given request.
        /// </summary>
        public void Execute(DownloadRequest request)
        {
            _coroutineRunner.StartCoroutine(DownloadCoroutine(request));
        }

        private IEnumerator DownloadCoroutine(DownloadRequest request)
        {
            Debug.Log($"[RequestHandler] Starting download: {request.RequestId} → {request.Url}");

            // ── Step 1: Build web request ──────────────────────────────────
            using (UnityWebRequest webRequest = UnityWebRequest.Get(request.Url))
            {
                // Set timeout — belt and suspenders with our override system
                webRequest.timeout = 30;

                // Set headers for better server compatibility
                webRequest.SetRequestHeader("User-Agent", "UnityImageDownloader/1.0");

                // ── Step 2: Send request ───────────────────────────────────
                yield return webRequest.SendWebRequest();

                // ── Step 3: Check for errors ───────────────────────────────
                if (webRequest.result != UnityWebRequest.Result.Success)
                {
                    string error = $"HTTP Error [{webRequest.responseCode}]: {webRequest.error}";
                    Debug.LogWarning($"[RequestHandler] FAILED {request.RequestId}: {error}");
                    request.NotifyFailure(error);
                    OnComplete?.Invoke(request);
                    yield break;
                }

                // ── Step 4: Get raw bytes ──────────────────────────────────
                byte[] rawBytes = webRequest.downloadHandler.data;

                if (rawBytes == null || rawBytes.Length == 0)
                {
                    request.NotifyFailure("Downloaded 0 bytes — empty response.");
                    OnComplete?.Invoke(request);
                    yield break;
                }

                // ── Step 5: Decode to Texture2D ────────────────────────────
                // WHY TextureFormat.RGBA32:
                // - Supports full alpha channel (PNG transparency)
                // - Works on all mobile platforms (iOS, Android)
                // - RGB24 would silently drop alpha channel
                Texture2D texture = new Texture2D(2, 2, TextureFormat.RGBA32, mipChain: false);
                texture.name = $"Downloaded_{request.RequestId}";

                // LoadImage auto-resizes texture to match PNG dimensions
                bool decoded = texture.LoadImage(rawBytes, markNonReadable: false);

                if (!decoded)
                {
                    UnityEngine.Object.Destroy(texture);
                    request.NotifyFailure("Failed to decode image bytes into Texture2D.");
                    OnComplete?.Invoke(request);
                    yield break;
                }

                // Apply — uploads texture data to GPU
                texture.Apply(updateMipmaps: false, makeNoLongerReadable: false);

                // ── Step 6: Save to disk cache ─────────────────────────────
                // WHY EncodeToPNG and not raw bytes:
                // Raw bytes from UnityWebRequest may be JPEG.
                // We always normalize to PNG to ensure alpha is preserved.
                byte[] pngBytes = texture.EncodeToPNG();
                var saveResult = _cacheManager.SaveToDisk(request.Url, pngBytes, _cacheExpiry);

                if (!saveResult.IsSuccess)
                    Debug.LogWarning($"[RequestHandler] Disk save failed: {saveResult.ErrorMessage}");

                // ── Step 7: Store in memory cache ──────────────────────────
                _cacheManager.StoreInMemory(request.Url, texture);

                // ── Step 8: Notify all waiting callbacks ───────────────────
                Debug.Log($"[RequestHandler] SUCCESS {request.RequestId}: " +
                          $"{texture.width}x{texture.height} | {rawBytes.Length / 1024f:F1}KB");

                request.NotifySuccess(texture);
                OnComplete?.Invoke(request);
            }
        }
    }
}