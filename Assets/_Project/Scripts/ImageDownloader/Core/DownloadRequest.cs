using System;
using System.Collections.Generic;
using UnityEngine;

namespace TechnicalAssignment.ImageDownloader.Core
{
    /// <summary>
    /// Represents a single image download request.
    ///
    /// WHY a class not a struct:
    /// Multiple WebImage components may request the same URL simultaneously.
    /// We store all their callbacks in one request and notify all at once.
    /// This prevents downloading the same image 5 times if 5 WebImages
    /// all request it before the first one completes.
    ///
    /// PATTERN: This is the "Coalescing Requests" pattern.
    /// Same URL → same DownloadRequest → all callers notified together.
    /// </summary>
    public class DownloadRequest
    {
        // ── Identity ───────────────────────────────────────────────────────
        public readonly string Url;
        public readonly string RequestId;   // unique ID for logging

        // ── Timing ────────────────────────────────────────────────────────
        public readonly float EnqueueTime;  // Time.realtimeSinceStartup when created
        public float StartTime { get; private set; }   // when download actually started
        public bool HasStarted => StartTime > 0f;

        // ── State ──────────────────────────────────────────────────────────
        public DownloadStatus Status { get; private set; } = DownloadStatus.Queued;

        // ── Callbacks ─────────────────────────────────────────────────────
        // Multiple WebImages may await the same URL
        private readonly List<Action<Texture2D, string>> _callbacks
            = new List<Action<Texture2D, string>>();

        // ── Constructor ────────────────────────────────────────────────────
        public DownloadRequest(string url)
        {
            Url = url ?? throw new ArgumentNullException(nameof(url));
            RequestId = Guid.NewGuid().ToString("N").Substring(0, 8).ToUpper();
            EnqueueTime = UnityEngine.Time.realtimeSinceStartup;
        }

        // ── Public API ─────────────────────────────────────────────────────

        public void AddCallback(Action<Texture2D, string> callback)
        {
            if (callback != null && !_callbacks.Contains(callback))
                _callbacks.Add(callback);
        }

        public void MarkStarted()
        {
            Status = DownloadStatus.Downloading;
            StartTime = UnityEngine.Time.realtimeSinceStartup;
        }

        public void NotifySuccess(Texture2D texture)
        {
            Status = DownloadStatus.Completed;
            foreach (var cb in _callbacks)
            {
                try { cb?.Invoke(texture, null); }
                catch (Exception ex)
                { Debug.LogError($"[DownloadRequest] Callback error for {Url}: {ex.Message}"); }
            }
            _callbacks.Clear();
        }

        public void NotifyFailure(string errorMessage)
        {
            Status = DownloadStatus.Failed;
            foreach (var cb in _callbacks)
            {
                try { cb?.Invoke(null, errorMessage); }
                catch (Exception ex)
                { Debug.LogError($"[DownloadRequest] Callback error for {Url}: {ex.Message}"); }
            }
            _callbacks.Clear();
        }

        /// <summary>
        /// How long this request has been waiting in queue (seconds).
        /// </summary>
        public float WaitTime => UnityEngine.Time.realtimeSinceStartup - EnqueueTime;

        public override string ToString() =>
            $"[Request {RequestId}] {Status} | URL: {Url} | Wait: {WaitTime:F1}s";
    }

    public enum DownloadStatus
    {
        Queued,
        Downloading,
        Completed,
        Failed
    }
}