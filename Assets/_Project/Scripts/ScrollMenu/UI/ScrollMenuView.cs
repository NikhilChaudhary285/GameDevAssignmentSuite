using System.Collections;
using System.Collections.Generic;
using TechnicalAssignment.ScrollMenu.Core;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace TechnicalAssignment.ScrollMenu.UI
{
    /// <summary>
    /// Main orchestrator for the scroll menu scene.
    ///
    /// RESPONSIBILITIES:
    /// - Spawn card prefabs from LevelData
    /// - Wire controller events to visual updates
    /// - Handle drag input (IBeginDragHandler, IDragHandler, IEndDragHandler)
    /// - Move the content RectTransform to the correct position
    /// - Delegate animations to CardScaleAnimator
    /// - Delegate dots to PaginationDots
    ///
    /// PATTERN: This is the VIEW in MVC.
    /// Controller holds state. View renders state.
    /// Neither knows about the other's internals.
    ///
    /// ASPECT RATIO SUPPORT (9:16 to 3:4):
    /// Card width = 65% of screen width, capped at 500px.
    /// Spacing    = 15% of screen width.
    /// This ensures cards always fit proportionally.
    /// </summary>
    public class ScrollMenuView : MonoBehaviour,
        IBeginDragHandler, IDragHandler, IEndDragHandler
    {
        // ── Inspector References ───────────────────────────────────────────

        [Header("── Layout ──────────────────────────")]
        [SerializeField] private RectTransform _viewport;
        [SerializeField] private RectTransform _content;
        [SerializeField] private LevelCard _cardPrefab;

        [Header("── Navigation ─────────────────────")]
        [SerializeField] private Button _prevButton;
        [SerializeField] private Button _nextButton;

        [Header("── Sub-Components ─────────────────")]
        [SerializeField] private PaginationDots _paginationDots;
        [SerializeField] private CardScaleAnimator _scaleAnimator;

        [Header("── Card Sizing ────────────────────")]
        [Tooltip("Card width as fraction of screen width")]
        [SerializeField] private float _cardWidthFraction = 0.65f;
        [Tooltip("Gap between cards as fraction of screen width")]
        [SerializeField] private float _cardSpacingFraction = 0.15f;
        [Tooltip("Snap animation duration in seconds")]
        [SerializeField] private float _snapDuration = 0.3f;

        // ── Runtime ────────────────────────────────────────────────────────
        private IScrollMenuController _controller;
        private LevelDataProvider _dataProvider;
        private List<LevelCard> _cards = new List<LevelCard>();

        private float _cardWidth;
        private float _cardSpacing;
        private float _cardStep;     // cardWidth + spacing = one page step

        private Vector2 _dragStartPosition;
        private float _dragVelocity;
        private float _lastDragX;
        private float _lastDragTime;

        private bool _isSnapping;
        private Coroutine _snapCoroutine;

        // ── Unity Lifecycle ────────────────────────────────────────────────

        private void Awake()
        {
            _dataProvider = new LevelDataProvider();
        }

        private void Start()
        {
            BuildMenu();
        }

        private void OnDestroy()
        {
            if (_controller != null)
                _controller.OnPageChanged -= OnPageChanged;

            if (_prevButton != null) _prevButton.onClick.RemoveAllListeners();
            if (_nextButton != null) _nextButton.onClick.RemoveAllListeners();
        }

        // ── Build ──────────────────────────────────────────────────────────

        private void BuildMenu()
        {
            // 1. Calculate card dimensions from screen size
            CalculateCardDimensions();

            // 2. Get level data
            List<LevelData> levels = _dataProvider.GetLevels();

            // 3. Create controller with calculated card width
            _controller = new ScrollMenuController(levels.Count, _cardWidth);
            _controller.OnPageChanged += OnPageChanged;

            // 4. Spawn cards
            SpawnCards(levels);

            // 5. Size the content rect
            SizeContentRect(levels.Count);

            // 6. Initialize sub-components
            _scaleAnimator.Initialize(_cards);
            _paginationDots.Initialize(levels.Count);

            // 7. Wire buttons
            if (_prevButton != null)
                _prevButton.onClick.AddListener(() => _controller.GoPrevious());
            if (_nextButton != null)
                _nextButton.onClick.AddListener(() => _controller.GoToNext());

            // 8. Snap to first card immediately
            SnapToIndex(0, instant: true);
            UpdateButtonStates(0);
        }

        private void CalculateCardDimensions()
        {
            // Use viewport width (safe for all aspect ratios)
            float screenWidth = _viewport.rect.width;

            // Fallback if rect not ready yet (first frame timing)
            if (screenWidth < 1f)
                screenWidth = Screen.width;

            _cardWidth = Mathf.Min(screenWidth * _cardWidthFraction, 500f);
            _cardSpacing = screenWidth * _cardSpacingFraction;
            _cardStep = _cardWidth + _cardSpacing;

            Debug.Log($"[ScrollMenuView] ScreenW={screenWidth:F0} " +
                      $"CardW={_cardWidth:F0} Spacing={_cardSpacing:F0} Step={_cardStep:F0}");
        }

        private void SpawnCards(List<LevelData> levels)
        {
            // Clear existing cards
            foreach (Transform child in _content)
                Destroy(child.gameObject);
            _cards.Clear();

            for (int i = 0; i < levels.Count; i++)
            {
                LevelCard card = Instantiate(_cardPrefab, _content);
                card.name = $"Card_{i:D2}_{levels[i].LevelName}";

                // Size the card
                RectTransform rt = card.GetComponent<RectTransform>();
                rt.sizeDelta = new Vector2(_cardWidth, _cardWidth * 1.3f); // portrait aspect

                // Position: each card is offset by _cardStep
                // Cards are centered on content's pivot
                rt.anchoredPosition = new Vector2(i * _cardStep, 0f);

                card.Bind(levels[i], i);
                _cards.Add(card);
            }
        }

        private void SizeContentRect(int count)
        {
            // Content width = (n-1) card steps + one card width
            // WHY (n-1): first card starts at 0, last card starts at (n-1)*step
            float totalWidth = (count - 1) * _cardStep + _cardWidth;
            _content.sizeDelta = new Vector2(totalWidth, _content.sizeDelta.y);
        }

        // ── Drag Input ─────────────────────────────────────────────────────

        public void OnBeginDrag(PointerEventData eventData)
        {
            // Stop any running snap coroutine when user starts dragging
            if (_snapCoroutine != null)
            {
                StopCoroutine(_snapCoroutine);
                _snapCoroutine = null;
                _isSnapping = false;
            }

            _dragStartPosition = eventData.position;
            _lastDragX = eventData.position.x;
            _lastDragTime = Time.time;
            _dragVelocity = 0f;
        }

        public void OnDrag(PointerEventData eventData)
        {
            // Calculate delta from last frame
            float deltaX = eventData.position.x - _lastDragX;

            // Calculate instantaneous velocity
            float dt = Time.time - _lastDragTime;
            if (dt > 0f)
                _dragVelocity = deltaX / dt;

            _lastDragX = eventData.position.x;
            _lastDragTime = Time.time;

            // Tell controller about drag
            _controller.OnDragDelta(deltaX);

            // Move content visually while dragging (raw follow)
            Vector2 pos = _content.anchoredPosition;
            pos.x += deltaX;
            _content.anchoredPosition = pos;
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            // Let controller decide final page
            _controller.OnDragEnd(_dragVelocity);
        }

        // ── Event Handlers ─────────────────────────────────────────────────

        /// <summary>
        /// Fired by controller whenever the page changes.
        /// This drives ALL visual updates — single source of truth.
        /// </summary>
        private void OnPageChanged(int index)
        {
            SnapToIndex(index, instant: false);
            _paginationDots.SetActiveDot(index);
            _scaleAnimator.SetCenterIndex(index);
            UpdateButtonStates(index);
        }

        // ── Snap Animation ─────────────────────────────────────────────────

        private void SnapToIndex(int index, bool instant)
        {
            // Target X: content moves LEFT as index increases
            // Card[0] at X=0 → content.anchoredPosition.x = 0
            // Card[1] at X=step → content moves -step so card[1] is centered
            float targetX = -index * _cardStep;

            // Center offset: shift content so card appears in viewport center
            float viewportCenter = _viewport.rect.width * 0.5f;
            float cardCenter = _cardWidth * 0.5f;
            targetX += viewportCenter - cardCenter;

            if (instant)
            {
                _content.anchoredPosition = new Vector2(targetX, _content.anchoredPosition.y);
                _scaleAnimator.SetCenterIndex(index);
                _paginationDots.SetActiveDot(index);
                return;
            }

            if (_snapCoroutine != null)
                StopCoroutine(_snapCoroutine);

            _snapCoroutine = StartCoroutine(AnimateSnap(targetX));
        }

        private IEnumerator AnimateSnap(float targetX)
        {
            _isSnapping = true;

            float startX = _content.anchoredPosition.x;
            float elapsed = 0f;

            while (elapsed < _snapDuration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / _snapDuration;

                // Ease-out cubic: feels snappy but smooth
                t = 1f - Mathf.Pow(1f - t, 3f);

                float newX = Mathf.Lerp(startX, targetX, t);
                _content.anchoredPosition = new Vector2(newX, _content.anchoredPosition.y);

                yield return null;
            }

            // Ensure exact final position
            _content.anchoredPosition = new Vector2(targetX, _content.anchoredPosition.y);

            _isSnapping = false;
            _snapCoroutine = null;
        }

        // ── UI State ───────────────────────────────────────────────────────

        private void UpdateButtonStates(int index)
        {
            if (_prevButton != null)
                _prevButton.interactable = index > 0;

            if (_nextButton != null)
                _nextButton.interactable = index < _controller.TotalCards - 1;
        }
    }
}