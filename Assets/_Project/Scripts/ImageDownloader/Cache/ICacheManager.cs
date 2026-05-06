using System;
using UnityEngine;
using TechnicalAssignment.Common;

namespace TechnicalAssignment.ImageDownloader.Cache
{
    /// <summary>
    /// Contract for the two-tier cache system.
    ///
    /// WHY an interface:
    /// - Unit testable with mock caches
    /// - Swap disk cache for cloud cache without changing ImageDownloader
    /// - Each method has one clear job
    ///
    /// TIER 1: Memory cache — Dictionary<url, Texture2D>
    ///         Pro: Instant retrieval.
    ///         Con: Lost on scene change (unless DontDestroyOnLoad).
    ///
    /// TIER 2: Disk cache — PNG files in persistentDataPath
    ///         Pro: Survives app restart.
    ///         Con: File I/O cost on first load (then promoted to memory).
    /// </summary>
    public interface ICacheManager
    {
        // ── Memory Cache ───────────────────────────────────────────────────

        /// <summary>Returns texture from memory cache. Null if not cached.</summary>
        Texture2D GetFromMemory(string url);

        /// <summary>Stores texture in memory cache.</summary>
        void StoreInMemory(string url, Texture2D texture);

        /// <summary>True if URL exists in memory cache.</summary>
        bool ExistsInMemory(string url);

        // ── Disk Cache ─────────────────────────────────────────────────────

        /// <summary>
        /// Loads texture from disk cache.
        /// Returns OperationResult with texture on success.
        /// Returns failure if not cached, expired, or corrupt.
        /// </summary>
        OperationResult<Texture2D> LoadFromDisk(string url);

        /// <summary>Saves texture bytes to disk with expiry metadata.</summary>
        OperationResult<bool> SaveToDisk(string url, byte[] pngBytes, TimeSpan expiry);

        /// <summary>True if URL has a valid (non-expired) disk entry.</summary>
        bool ExistsOnDisk(string url);

        // ── Maintenance ────────────────────────────────────────────────────

        /// <summary>Deletes all expired entries from disk.</summary>
        void PurgeExpiredEntries();

        /// <summary>Clears all memory AND disk cache.</summary>
        void ClearAll();
    }
}