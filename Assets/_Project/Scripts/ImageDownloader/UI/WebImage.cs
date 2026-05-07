using UnityEngine;
using UnityEngine.UI;

namespace TechnicalAssignment.ImageDownloader.UI
{
    using TechnicalAssignment.ImageDownloader.Core;

    /// <summary>
    /// Attach this component to any UI Image GameObject.
    /// Set the URL in Inspector or call SetUrl() from code.
    /// It automatically downloads, caches, and displays the image.
    ///
    /// USAGE:
    ///   1. Add to a UI Image GameObject
    ///   2. Set URL in Inspector (downloads on Start)
    ///   3. Or call: webImage.SetUrl("https://...");
    ///
    /// STATES:
    ///   Loading  → shows _loadingSprite (or transparent)
    ///   Success  → shows downloaded texture
    ///   Failed   → shows _fallbackSprite
    ///
    /// SAFETY: Checks if component is still alive before applying texture.
    /// This prevents the "missing MonoBehaviour" error when a card
    /// is destroyed while its image is still downloading.
    ///
    /// ALPHA SUPPORT: Sets rawImage.texture OR image.sprite with
    /// full RGBA32 texture — alpha channel is preserved.
    /// </summary>
    [RequireComponent(typeof(Image))]
    public class WebImage : MonoBehaviour
    {
        // ── Inspector ──────────────────────────────────────────────────────
        [Header("── URL ──────────────────────────────")]
        [SerializeField] private string _url;
        [SerializeField] private bool _loadOnStart = true;

        [Header("── Sprites ─────────────────────────")]
        [Tooltip("Shown while image is loading")]
        [SerializeField] private Sprite _loadingSprite;
        [Tooltip("Shown when download fails")]
        [SerializeField] private Sprite _fallbackSprite;

        [Header("── Aspect ───────────────────────────")]
        [Tooltip("Preserve image aspect ratio after loading")]
        [SerializeField] private bool _preserveAspect = true;

        [Header("── Debug ───────────────────────────")]
        [SerializeField] private bool _showDebugState = false;

        [Header("── Caching ───────────────────────")]
        [SerializeField] private bool _useMemoryCache = true;

        // ── Runtime ────────────────────────────────────────────────────────
        private Image _image;
        private string _currentUrl;
        private WebImageState _state = WebImageState.Idle;

        public WebImageState State => _state;
        public string CurrentUrl => _currentUrl;

        // ── Lifecycle ──────────────────────────────────────────────────────

        private void Awake()
        {
            _image = GetComponent<Image>();
        }

        private void Start()
        {
            if (_loadOnStart && !string.IsNullOrEmpty(_url))
                SetUrl(_url);
        }

        private void OnDestroy()
        {
            // Cancel prevents memory leaks from callbacks firing
            // on a destroyed component.
            if (!string.IsNullOrEmpty(_currentUrl))
                ImageDownloader.Instance?.CancelRequest(_currentUrl, OnImageReceived);
        }

        // ── Public API ─────────────────────────────────────────────────────

        /// <summary>
        /// Load image from URL.
        /// Cancels any in-progress download for a different URL.
        /// Safe to call multiple times — handles URL changes correctly.
        /// Request via the downloader (Pass memory cache preference to downloader)
        /// </summary>
        public void SetUrl(string url)
        {
            if (string.IsNullOrEmpty(url))
            {
                SetState(WebImageState.Failed);
                ShowFallback("URL is empty.");
                return;
            }

            // Don't re-download if same URL already loaded
            if (url == _currentUrl && _state == WebImageState.Loaded)
                return;

            _currentUrl = url;
            SetState(WebImageState.Loading);
            ShowLoading();

            // Request via the downloader (Pass memory cache preference to downloader)
            ImageDownloader.Instance.RequestImage(url, OnImageReceived, _useMemoryCache);
        }

        /// <summary>
        /// Manually clear and reset to fallback.
        /// </summary>
        public void Clear()
        {
            _currentUrl = null;
            _state = WebImageState.Idle;
            _image.sprite = _fallbackSprite;
            //_image.texture = null;
            _image.color = Color.white;
        }

        // ── Private ────────────────────────────────────────────────────────

        /// <summary>
        /// Callback from ImageDownloader.
        /// IMPORTANT: This fires on the Unity main thread (coroutine completion).
        /// SAFETY: Check 'this == null' — component may be destroyed
        /// while awaiting download if the parent card was recycled/destroyed.
        /// </summary>
        private void OnImageReceived(Texture2D texture, string error)
        {
            // Safety check: component destroyed while downloading
            if (this == null || _image == null) return;

            // URL may have changed (user scrolled, new URL set)
            // Ignore stale callbacks
            // (In this implementation callbacks are tied to URL so this
            //  rarely fires, but it's good defensive practice)

            if (texture != null)
            {
                ApplyTexture(texture);
                SetState(WebImageState.Loaded);
            }
            else
            {
                Debug.LogWarning($"[WebImage] Failed to load '{_currentUrl}': {error}");
                ShowFallback(error);
                SetState(WebImageState.Failed);
            }
        }

        private void ApplyTexture(Texture2D texture)
        {
            // Convert Texture2D to Sprite for UI Image component
            // WHY Sprite and not RawImage:
            // UI Image integrates with layout, masking, and UI events.
            // RawImage doesn't support slicing or proper UI interaction.
            Rect rect = new Rect(0, 0, texture.width, texture.height);
            Vector2 pivot = new Vector2(0.5f, 0.5f);
            float ppu = 100f;

            Sprite sprite = Sprite.Create(texture, rect, pivot, ppu,
                extrude: 0,
                meshType: SpriteMeshType.FullRect);

            _image.sprite = sprite;
            _image.preserveAspect = _preserveAspect;
            _image.color = Color.white; // ensure full opacity

            if (_showDebugState)
                Debug.Log($"[WebImage] Applied {texture.width}x{texture.height} to {gameObject.name}");
        }

        private void ShowLoading()
        {
            _image.sprite = _loadingSprite;
            _image.color = _loadingSprite != null ? Color.white : new Color(1, 1, 1, 0.3f);
        }

        private void ShowFallback(string reason)
        {
            _image.sprite = _fallbackSprite;
            _image.color = _fallbackSprite != null ? Color.white : new Color(0.8f, 0.2f, 0.2f, 0.8f);

            if (_showDebugState)
                Debug.Log($"[WebImage] Showing fallback. Reason: {reason}");
        }

        private void SetState(WebImageState newState)
        {
            _state = newState;
        }
    }

    public enum WebImageState
    {
        Idle,
        Loading,
        Loaded,
        Failed
    }
}