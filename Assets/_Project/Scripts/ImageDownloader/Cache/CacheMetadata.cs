using System;
using System.Collections.Generic;

namespace TechnicalAssignment.ImageDownloader.Cache
{
    /// <summary>
    /// Metadata stored alongside cached images on disk.
    ///
    /// WHY a sidecar file:
    /// Texture2D files (PNG) have no place to store expiry dates.
    /// We write a separate JSON file next to each PNG that records
    /// when it was cached. On load, we check this JSON first.
    ///
    /// Disk layout:
    ///   <persistentDataPath>/ImageCache/
    ///       abc123def.png          ← the image
    ///       abc123def.meta.json    ← this metadata
    /// </summary>
    [Serializable]
    public class CacheEntryMetadata
    {
        public string OriginalUrl;
        public string CacheFileName;
        public long CachedAtTicks;   // DateTime.UtcNow.Ticks when saved
        public long ExpiryTicks;     // CachedAt + expiry duration

        public DateTime CachedAt => new DateTime(CachedAtTicks, DateTimeKind.Utc);
        public DateTime ExpiresAt => new DateTime(ExpiryTicks, DateTimeKind.Utc);
        public bool IsExpired => DateTime.UtcNow > ExpiresAt;
    }

    /// <summary>
    /// Master index of all cached files.
    /// Stored as one JSON file: ImageCache/cache_index.json
    ///
    /// WHY a master index:
    /// Instead of scanning the directory for .meta.json files on every load,
    /// we keep one index file. Faster startup, easier expiry sweeping.
    /// </summary>
    [Serializable]
    public class CacheIndex
    {
        public List<CacheEntryMetadata> Entries = new List<CacheEntryMetadata>();
    }
}