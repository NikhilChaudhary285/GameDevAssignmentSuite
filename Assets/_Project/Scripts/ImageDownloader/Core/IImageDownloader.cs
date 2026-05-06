using System;
using UnityEngine;

namespace TechnicalAssignment.ImageDownloader.Core
{
    /// <summary>
    /// Public contract for the image download system.
    ///
    /// WHY: WebImage, test scenes, and any future system
    /// depend on THIS interface, never on the concrete singleton.
    /// This makes the system mockable and unit-testable.
    ///
    /// DIP (Dependency Inversion Principle):
    /// High-level modules (WebImage) depend on abstraction (this interface).
    /// Low-level modules (ImageDownloader singleton) implement this interface.
    /// </summary>
    public interface IImageDownloader
    {
        /// <summary>
        /// Request an image from URL.
        /// Callback fires with (Texture2D texture, string error).
        /// If texture is null → error occurred, use fallback.
        /// If error is null → success.
        ///
        /// Thread: Callback always fires on Unity main thread.
        /// </summary>
        void RequestImage(string url, Action<Texture2D, string> callback);

        /// <summary>
        /// Cancel all pending callbacks for a URL.
        /// Call when the requesting component is destroyed mid-download.
        /// </summary>
        void CancelRequest(string url, Action<Texture2D, string> callback);

        /// <summary>
        /// Force clear all caches. Useful for testing or low-memory situations.
        /// </summary>
        void ClearAllCaches();

        // Status accessors for debug UI
        int ActiveDownloads { get; }
        int PendingDownloads { get; }
    }
}