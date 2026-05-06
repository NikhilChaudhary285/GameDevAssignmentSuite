using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TechnicalAssignment.ImageDownloader.UI
{
    using TechnicalAssignment.ImageDownloader.Core;

    /// <summary>
    /// Test scene UI for demonstrating and validating the ImageDownloader.
    /// Shows: live queue status, loaded images grid, cache controls.
    /// </summary>
    public class ImageDownloaderTestUI : MonoBehaviour
    {
        [Header("── Status Bar ─────────────────────")]
        [SerializeField] private TextMeshProUGUI _statusText;
        [SerializeField] private TextMeshProUGUI _queueText;

        [Header("── Image Grid ──────────────────────")]
        [SerializeField] private Transform _imageGridParent;
        [SerializeField] private GameObject _imageCardPrefab;  // Has Image + WebImage + Text

        [Header("── Controls ───────────────────────")]
        [SerializeField] private Button _loadAllButton;
        [SerializeField] private Button _clearCacheButton;
        [SerializeField] private Button _reloadButton;
        [SerializeField] private TMP_InputField _customUrlInput;
        [SerializeField] private Button _loadCustomButton;

        [Header("── Test URLs ───────────────────────")]
        [SerializeField] private bool _useTestUrls = true;

        // ── Sample URLs (publicly available placeholder images) ────────────
        // Using picsum.photos — reliable test image service with alpha support
        private readonly string[] _testImageUrls = new string[]
        {
            "https://picsum.photos/seed/alpha/400/400",
            "https://picsum.photos/seed/beta/400/400",
            "https://picsum.photos/seed/gamma/400/400",
            "https://picsum.photos/seed/delta/400/400",
            "https://picsum.photos/seed/epsilon/400/400",
            "https://picsum.photos/seed/zeta/400/400",
            "https://picsum.photos/seed/eta/400/400",
            "https://picsum.photos/seed/theta/400/400",
            "https://picsum.photos/seed/iota/400/400",
            "https://via.placeholder.com/400x400/FF6B6B/FFFFFF?text=FAIL_TEST",   // intentional failure test
        };

        private List<WebImage> _spawnedWebImages = new List<WebImage>();
        private Coroutine _statusCoroutine;

        // ── Lifecycle ──────────────────────────────────────────────────────

        private void Start()
        {
            WireButtons();
            _statusCoroutine = StartCoroutine(UpdateStatusLoop());
        }

        private void OnDestroy()
        {
            if (_statusCoroutine != null) StopCoroutine(_statusCoroutine);
            _loadAllButton?.onClick.RemoveAllListeners();
            _clearCacheButton?.onClick.RemoveAllListeners();
            _reloadButton?.onClick.RemoveAllListeners();
            _loadCustomButton?.onClick.RemoveAllListeners();
        }

        // ── Button Wiring ──────────────────────────────────────────────────

        private void WireButtons()
        {
            _loadAllButton?.onClick.AddListener(OnLoadAllClicked);
            _clearCacheButton?.onClick.AddListener(OnClearCacheClicked);
            _reloadButton?.onClick.AddListener(OnReloadClicked);
            _loadCustomButton?.onClick.AddListener(OnLoadCustomClicked);
        }

        // ── Button Handlers ────────────────────────────────────────────────

        private void OnLoadAllClicked()
        {
            ClearGrid();

            foreach (string url in _testImageUrls)
                SpawnImageCard(url);

            UpdateStatus("Loading all test images...");
        }

        private void OnClearCacheClicked()
        {
            ImageDownloader.Instance.ClearAllCaches();
            UpdateStatus("All caches cleared. Re-load to test fresh downloads.");
        }

        private void OnReloadClicked()
        {
            // Reload all currently shown images
            foreach (var webImg in _spawnedWebImages)
            {
                string url = webImg.CurrentUrl;
                webImg.Clear();
                webImg.SetUrl(url);
            }
            UpdateStatus("Reloading all images...");
        }

        private void OnLoadCustomClicked()
        {
            if (_customUrlInput == null) return;

            string url = _customUrlInput.text.Trim();
            if (string.IsNullOrEmpty(url))
            {
                UpdateStatus("Enter a URL first.");
                return;
            }

            SpawnImageCard(url);
            UpdateStatus($"Loading: {url}");
        }

        // ── Grid Management ────────────────────────────────────────────────

        private void SpawnImageCard(string url)
        {
            if (_imageCardPrefab == null || _imageGridParent == null) return;

            GameObject card = Instantiate(_imageCardPrefab, _imageGridParent);
            card.name = $"ImageCard_{_spawnedWebImages.Count}";

            // Get WebImage and set URL
            WebImage webImage = card.GetComponentInChildren<WebImage>();
            if (webImage != null)
            {
                webImage.SetUrl(url);
                _spawnedWebImages.Add(webImage);
            }

            // Get label and show truncated URL
            TextMeshProUGUI label = card.GetComponentInChildren<TextMeshProUGUI>();
            if (label != null)
            {
                string display = url.Length > 40 ? url.Substring(0, 37) + "..." : url;
                label.text = display;
            }
        }

        private void ClearGrid()
        {
            foreach (Transform child in _imageGridParent)
                Destroy(child.gameObject);
            _spawnedWebImages.Clear();
        }

        // ── Status Updates ─────────────────────────────────────────────────

        private IEnumerator UpdateStatusLoop()
        {
            while (true)
            {
                yield return new WaitForSeconds(0.3f);
                RefreshQueueDisplay();
            }
        }

        private void RefreshQueueDisplay()
        {
            if (ImageDownloader.Instance == null || _queueText == null) return;

            int active = ImageDownloader.Instance.ActiveDownloads;
            int pending = ImageDownloader.Instance.PendingDownloads;

            string activeColor = active > 0 ? "#6BCB77" : "#ADB5BD";
            string pendingColor = pending > 0 ? "#FFD93D" : "#ADB5BD";

            _queueText.text =
                $"Active: <color={activeColor}><b>{active}</b></color>/3  " +
                $"Queued: <color={pendingColor}><b>{pending}</b></color>";

            // Count loaded vs loading in grid
            int loaded = 0, loading = 0, failed = 0;
            foreach (var wi in _spawnedWebImages)
            {
                if (wi == null) continue;
                switch (wi.State)
                {
                    case WebImageState.Loaded: loaded++; break;
                    case WebImageState.Loading: loading++; break;
                    case WebImageState.Failed: failed++; break;
                }
            }

            if (_spawnedWebImages.Count > 0)
            {
                _queueText.text +=
                    $"\nImages: <color=#6BCB77>{loaded}</color> loaded | " +
                    $"<color=#FFD93D>{loading}</color> loading | " +
                    $"<color=#FF6B6B>{failed}</color> failed";
            }
        }

        private void UpdateStatus(string message)
        {
            if (_statusText != null)
                _statusText.text = message;
        }
    }
}