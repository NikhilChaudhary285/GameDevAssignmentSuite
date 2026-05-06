using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using TechnicalAssignment.Common;
using UnityEngine;

namespace TechnicalAssignment.ImageDownloader.Cache
{
    /// <summary>
    /// Two-tier cache: Memory (Dictionary) + Disk (PNG files).
    ///
    /// DISK LAYOUT:
    ///   {persistentDataPath}/ImageCache/
    ///       {hash}.png              raw image bytes
    ///       cache_index.json        master metadata index
    ///
    /// URL → FILENAME:
    ///   MD5 hash of URL → ensures valid filename from any URL string.
    ///   "https://example.com/img.png" → "a1b2c3d4e5f6...".png
    ///
    /// THREAD SAFETY:
    ///   Memory cache: locked (multiple coroutines may write simultaneously)
    ///   Disk cache: Unity main thread only (Texture2D requires main thread)
    /// </summary>
    public class CacheManager : ICacheManager
    {
        // ── Config ─────────────────────────────────────────────────────────
        private const string CACHE_FOLDER = "ImageCache";
        private const string INDEX_FILE_NAME = "cache_index.json";

        // ── Memory Cache ───────────────────────────────────────────────────
        private readonly Dictionary<string, Texture2D> _memoryCache
            = new Dictionary<string, Texture2D>();
        private readonly object _memoryCacheLock = new object();
        private bool _memoryEnabled;

        // ── Disk Cache ─────────────────────────────────────────────────────
        private readonly string _cacheDirectory;
        private readonly string _indexFilePath;
        private CacheIndex _index;

        // ── Constructor ────────────────────────────────────────────────────
        public CacheManager(bool enableMemoryCache)
        {
            _memoryEnabled = enableMemoryCache;
            _cacheDirectory = Path.Combine(Application.persistentDataPath, CACHE_FOLDER);
            _indexFilePath = Path.Combine(_cacheDirectory, INDEX_FILE_NAME);

            EnsureCacheDirectory();
            LoadIndex();

            Debug.Log($"[CacheManager] Init. Dir: {_cacheDirectory} | Memory: {_memoryEnabled}");
        }

        // ── ICacheManager: Memory ──────────────────────────────────────────

        public Texture2D GetFromMemory(string url)
        {
            if (!_memoryEnabled) return null;

            lock (_memoryCacheLock)
            {
                _memoryCache.TryGetValue(url, out Texture2D tex);
                return tex;
            }
        }

        public void StoreInMemory(string url, Texture2D texture)
        {
            if (!_memoryEnabled || texture == null) return;

            lock (_memoryCacheLock)
            {
                _memoryCache[url] = texture;
            }
        }

        public bool ExistsInMemory(string url)
        {
            if (!_memoryEnabled) return false;
            lock (_memoryCacheLock)
            {
                return _memoryCache.ContainsKey(url);
            }
        }

        // ── ICacheManager: Disk ────────────────────────────────────────────

        public OperationResult<Texture2D> LoadFromDisk(string url)
        {
            CacheEntryMetadata entry = FindEntry(url);

            if (entry == null)
                return OperationResult<Texture2D>.Failure("Not in disk cache.");

            if (entry.IsExpired)
            {
                DeleteEntry(entry);
                return OperationResult<Texture2D>.Failure("Cache entry expired.");
            }

            string filePath = Path.Combine(_cacheDirectory, entry.CacheFileName);

            if (!File.Exists(filePath))
            {
                DeleteEntry(entry);
                return OperationResult<Texture2D>.Failure("Cache file missing.");
            }

            try
            {
                byte[] bytes = File.ReadAllBytes(filePath);

                // WHY TextureFormat.RGBA32 and not DXT:
                // DXT doesn't support alpha on all mobile platforms.
                // RGBA32 supports full alpha transparency (task requirement).
                Texture2D tex = new Texture2D(2, 2, TextureFormat.RGBA32, false);
                tex.name = $"Cached_{GetHash(url).Substring(0, 8)}";

                if (!tex.LoadImage(bytes))   // LoadImage auto-resizes from PNG
                {
                    UnityEngine.Object.Destroy(tex);
                    return OperationResult<Texture2D>.Failure("Failed to decode image.");
                }

                tex.Apply();
                Debug.Log($"[CacheManager] Disk HIT: {url.Substring(0, Mathf.Min(60, url.Length))}");

                // Promote to memory cache for next access
                StoreInMemory(url, tex);

                return OperationResult<Texture2D>.Success(tex);
            }
            catch (Exception ex)
            {
                return OperationResult<Texture2D>.Failure($"Disk read error: {ex.Message}", ex);
            }
        }

        public OperationResult<bool> SaveToDisk(string url, byte[] pngBytes, TimeSpan expiry)
        {
            if (pngBytes == null || pngBytes.Length == 0)
                return OperationResult<bool>.Failure("Empty bytes — nothing to save.");

            try
            {
                string hash = GetHash(url);
                string fileName = $"{hash}.png";
                string filePath = Path.Combine(_cacheDirectory, fileName);

                File.WriteAllBytes(filePath, pngBytes);

                // Update index
                // Remove old entry for this URL if exists
                _index.Entries.RemoveAll(e => e.OriginalUrl == url);

                var metadata = new CacheEntryMetadata
                {
                    OriginalUrl = url,
                    CacheFileName = fileName,
                    CachedAtTicks = DateTime.UtcNow.Ticks,
                    ExpiryTicks = (DateTime.UtcNow + expiry).Ticks
                };

                _index.Entries.Add(metadata);
                SaveIndex();

                Debug.Log($"[CacheManager] Saved to disk: {fileName} | Expires: {metadata.ExpiresAt:yyyy-MM-dd}");
                return OperationResult<bool>.Success(true);
            }
            catch (Exception ex)
            {
                return OperationResult<bool>.Failure($"Disk write error: {ex.Message}", ex);
            }
        }

        public bool ExistsOnDisk(string url)
        {
            CacheEntryMetadata entry = FindEntry(url);
            if (entry == null) return false;
            if (entry.IsExpired) { DeleteEntry(entry); return false; }

            string filePath = Path.Combine(_cacheDirectory, entry.CacheFileName);
            return File.Exists(filePath);
        }

        // ── ICacheManager: Maintenance ─────────────────────────────────────

        public void PurgeExpiredEntries()
        {
            int removed = 0;
            var expired = new List<CacheEntryMetadata>();

            foreach (var entry in _index.Entries)
            {
                if (!entry.IsExpired) continue;

                string filePath = Path.Combine(_cacheDirectory, entry.CacheFileName);
                if (File.Exists(filePath))
                {
                    File.Delete(filePath);
                    removed++;
                }
                expired.Add(entry);
            }

            foreach (var e in expired)
                _index.Entries.Remove(e);

            if (removed > 0)
            {
                SaveIndex();
                Debug.Log($"[CacheManager] Purged {removed} expired cache entries.");
            }
        }

        public void ClearAll()
        {
            // Memory
            lock (_memoryCacheLock)
                _memoryCache.Clear();

            // Disk
            if (Directory.Exists(_cacheDirectory))
            {
                // Delete all PNGs
                foreach (string file in Directory.GetFiles(_cacheDirectory, "*.png"))
                    File.Delete(file);
            }

            _index.Entries.Clear();
            SaveIndex();

            Debug.Log("[CacheManager] All caches cleared.");
        }

        // ── Private Helpers ────────────────────────────────────────────────

        private void EnsureCacheDirectory()
        {
            if (!Directory.Exists(_cacheDirectory))
            {
                Directory.CreateDirectory(_cacheDirectory);
                Debug.Log($"[CacheManager] Created cache directory: {_cacheDirectory}");
            }
        }

        private void LoadIndex()
        {
            if (!File.Exists(_indexFilePath))
            {
                _index = new CacheIndex();
                return;
            }

            try
            {
                string json = File.ReadAllText(_indexFilePath);
                _index = JsonUtility.FromJson<CacheIndex>(json) ?? new CacheIndex();
                Debug.Log($"[CacheManager] Loaded index: {_index.Entries.Count} entries.");
            }
            catch
            {
                Debug.LogWarning("[CacheManager] Index corrupt — resetting.");
                _index = new CacheIndex();
            }
        }

        private void SaveIndex()
        {
            try
            {
                string json = JsonUtility.ToJson(_index, prettyPrint: true);
                File.WriteAllText(_indexFilePath, json);
            }
            catch (Exception ex)
            {
                Debug.LogError($"[CacheManager] Failed to save index: {ex.Message}");
            }
        }

        private CacheEntryMetadata FindEntry(string url)
        {
            return _index.Entries.Find(e => e.OriginalUrl == url);
        }

        private void DeleteEntry(CacheEntryMetadata entry)
        {
            _index.Entries.Remove(entry);
            SaveIndex();
        }

        /// <summary>
        /// Converts URL to a safe, fixed-length filename using MD5 hash.
        /// "https://example.com/long/path/to/image.png?v=123"
        ///  → "a1b2c3d4e5f6a1b2c3d4e5f6a1b2c3d4"
        /// </summary>
        private string GetHash(string input)
        {
            using (MD5 md5 = MD5.Create())
            {
                byte[] bytes = md5.ComputeHash(Encoding.UTF8.GetBytes(input));
                StringBuilder sb = new StringBuilder();
                foreach (byte b in bytes)
                    sb.Append(b.ToString("x2"));
                return sb.ToString();
            }
        }
    }
}