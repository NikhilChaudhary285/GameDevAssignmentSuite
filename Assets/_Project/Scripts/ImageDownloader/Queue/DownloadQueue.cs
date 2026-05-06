using System;
using System.Collections.Generic;
using TechnicalAssignment.ImageDownloader.Core;
using UnityEngine;

namespace TechnicalAssignment.ImageDownloader.Queue
{
    /// <summary>
    /// Manages concurrent download slots and the waiting queue.
    ///
    /// RULES:
    /// 1. Max 3 downloads active simultaneously (configurable)
    /// 2. New requests join the queue if all slots are full
    /// 3. If a request waits > 10 seconds → override: force a slot open
    ///    (abort the oldest active download and replace with this one)
    /// 4. Same URL requested twice → coalesced into one request
    ///    (both callbacks fire when the single download completes)
    ///
    /// PATTERN: This is a bounded semaphore queue with priority override.
    /// The 10-second override is the "priority lane" for long-waiting requests.
    ///
    /// WHY max 3 concurrent:
    /// HTTP/1.1 browsers historically limit to 6 connections per host.
    /// Mobile networks perform better with fewer parallel connections.
    /// 3 is a good balance: fast enough, doesn't saturate bandwidth.
    /// </summary>
    public class DownloadQueue
    {
        // ── Config ─────────────────────────────────────────────────────────
        private readonly int _maxConcurrent;
        private readonly float _overrideWaitSeconds;

        // ── State ──────────────────────────────────────────────────────────
        // Active: currently downloading
        private readonly List<DownloadRequest> _activeDownloads = new List<DownloadRequest>();
        // Pending: waiting for a slot
        private readonly LinkedList<DownloadRequest> _pendingQueue = new LinkedList<DownloadRequest>();
        // All requests by URL (for coalescing)
        private readonly Dictionary<string, DownloadRequest> _allRequests
            = new Dictionary<string, DownloadRequest>();

        // Fired when a request is ready to start downloading
        public event Action<DownloadRequest> OnRequestReady;
        // Fired when a request is forcibly aborted (override scenario)
        public event Action<DownloadRequest> OnRequestAborted;

        // ── Constructor ────────────────────────────────────────────────────
        public DownloadQueue(int maxConcurrent = 3, float overrideWaitSeconds = 10f)
        {
            _maxConcurrent = maxConcurrent;
            _overrideWaitSeconds = overrideWaitSeconds;
        }

        // ── Public API ─────────────────────────────────────────────────────

        /// <summary>
        /// Enqueue a download request.
        ///
        /// COALESCING: If the URL is already active/pending,
        /// we add the callback to the existing request and return it.
        /// No duplicate download is started.
        /// </summary>
        public DownloadRequest Enqueue(string url, Action<Texture2D, string> callback)
        {
            // Coalesce: check if request for this URL already exists
            if (_allRequests.TryGetValue(url, out DownloadRequest existing))
            {
                existing.AddCallback(callback);
                Debug.Log($"[DownloadQueue] Coalesced request for: {url.Substring(0, Mathf.Min(50, url.Length))}");
                return existing;
            }

            // Create new request
            var request = new DownloadRequest(url);
            request.AddCallback(callback);
            _allRequests[url] = request;

            // Try to start immediately if slot available
            if (_activeDownloads.Count < _maxConcurrent)
            {
                StartRequest(request);
            }
            else
            {
                _pendingQueue.AddLast(request);
                Debug.Log($"[DownloadQueue] Queued ({_pendingQueue.Count} waiting): " +
                          $"{url.Substring(0, Mathf.Min(50, url.Length))}");
            }

            return request;
        }

        /// <summary>
        /// Call every frame (from ImageDownloader.Update).
        /// Handles: slot freeing, override checks, promoting queued requests.
        /// </summary>
        public void Tick()
        {
            CheckOverrideConditions();
            TryPromotePending();
        }

        /// <summary>
        /// Called when a download completes (success or failure).
        /// Frees the slot and promotes the next pending request.
        /// </summary>
        public void OnDownloadComplete(DownloadRequest request)
        {
            _activeDownloads.Remove(request);
            _allRequests.Remove(request.Url);

            Debug.Log($"[DownloadQueue] Completed: {request} | " +
                      $"Active: {_activeDownloads.Count}/{_maxConcurrent} | " +
                      $"Pending: {_pendingQueue.Count}");

            TryPromotePending();
        }

        // ── Accessors ──────────────────────────────────────────────────────
        public int ActiveCount => _activeDownloads.Count;
        public int PendingCount => _pendingQueue.Count;

        // ── Private ────────────────────────────────────────────────────────

        private void StartRequest(DownloadRequest request)
        {
            _activeDownloads.Add(request);
            request.MarkStarted();
            OnRequestReady?.Invoke(request);

            Debug.Log($"[DownloadQueue] Starting: {request} | " +
                      $"Active: {_activeDownloads.Count}/{_maxConcurrent}");
        }

        /// <summary>
        /// 10-Second Override Logic:
        ///
        /// If the OLDEST pending request has waited > 10 seconds:
        ///   - If a slot is free → promote normally
        ///   - If no slot free → abort the OLDEST active download
        ///     and give its slot to the long-waiting request
        ///
        /// WHY abort oldest active:
        /// The oldest active has already had its chance. The waiting request
        /// has been patient for 10 seconds — it gets priority.
        /// Aborted request is notified with failure so its WebImage
        /// can show a fallback immediately.
        /// </summary>
        private void CheckOverrideConditions()
        {
            if (_pendingQueue.Count == 0) return;

            var oldest = _pendingQueue.First?.Value;
            if (oldest == null) return;

            if (oldest.WaitTime < _overrideWaitSeconds) return;

            Debug.LogWarning($"[DownloadQueue] OVERRIDE: Request waited {oldest.WaitTime:F1}s > {_overrideWaitSeconds}s. Forcing slot.");

            if (_activeDownloads.Count < _maxConcurrent)
            {
                // Slot opened up between ticks — promote normally
                _pendingQueue.RemoveFirst();
                StartRequest(oldest);
                return;
            }

            // No free slot — abort oldest active download
            if (_activeDownloads.Count > 0)
            {
                DownloadRequest toAbort = _activeDownloads[0]; // oldest = first added
                _activeDownloads.Remove(toAbort);
                _allRequests.Remove(toAbort.Url);

                Debug.LogWarning($"[DownloadQueue] Aborting {toAbort.RequestId} to free slot for override.");
                OnRequestAborted?.Invoke(toAbort);
                toAbort.NotifyFailure("Download aborted: override by higher-priority request.");

                // Now give the freed slot to the waiting request
                _pendingQueue.RemoveFirst();
                StartRequest(oldest);
            }
        }

        /// <summary>
        /// Promotes pending requests to active slots if capacity allows.
        /// Called after a download completes or an override frees a slot.
        /// </summary>
        private void TryPromotePending()
        {
            while (_pendingQueue.Count > 0 &&
                   _activeDownloads.Count < _maxConcurrent)
            {
                var next = _pendingQueue.First.Value;
                _pendingQueue.RemoveFirst();
                StartRequest(next);
            }
        }
    }
}