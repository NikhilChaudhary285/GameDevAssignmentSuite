using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TechnicalAssignment.ScrollMenu.UI
{
    /// <summary>
    /// Handles the scale animation of all cards based on their
    /// distance from the center card.
    ///
    /// PATTERN: Strategy — the animation strategy (scale by distance)
    /// is isolated here. Swap this class to get different animation
    /// styles (fade, rotate, etc.) without touching other classes.
    ///
    /// CENTER card  → scale 1.0  (full size)
    /// SIDE cards   → scale 0.85 (shrunk)
    /// Cards animate smoothly via Lerp every frame.
    /// </summary>
    public class CardScaleAnimator : MonoBehaviour
    {
        [Header("── Scale Settings ─────────────────")]
        [SerializeField] private float _centerScale = 1.0f;
        [SerializeField] private float _sideScale = 0.85f;
        [SerializeField] private float _animSpeed = 10f;   // Lerp speed

        [Header("── Alpha Settings ─────────────────")]
        [SerializeField] private float _centerAlpha = 1.0f;
        [SerializeField] private float _sideAlpha = 0.65f;

        // All managed cards
        private List<RectTransform> _cardTransforms = new List<RectTransform>();
        private List<CanvasGroup> _cardGroups = new List<CanvasGroup>();
        private List<LevelCard> _levelCards = new List<LevelCard>();

        private int _currentCenterIndex = 0;
        private bool _initialized = false;

        // ── Public API ─────────────────────────────────────────────────────

        public void Initialize(List<LevelCard> cards)
        {
            _cardTransforms.Clear();
            _cardGroups.Clear();
            _levelCards.Clear();

            foreach (var card in cards)
            {
                _cardTransforms.Add(card.GetComponent<RectTransform>());

                // Add CanvasGroup if missing (needed for alpha)
                var cg = card.GetComponent<CanvasGroup>();
                if (cg == null) cg = card.gameObject.AddComponent<CanvasGroup>();
                _cardGroups.Add(cg);

                _levelCards.Add(card);
            }

            _initialized = true;

            // Apply initial state immediately (no lerp)
            ApplyScalesImmediate(0);
        }

        /// <summary>
        /// Called by ScrollMenuView when page changes.
        /// Triggers smooth animated transition.
        /// </summary>
        public void SetCenterIndex(int index)
        {
            if (!_initialized) return;

            // Update card centered state
            for (int i = 0; i < _levelCards.Count; i++)
                _levelCards[i].SetCentered(i == index);

            _currentCenterIndex = index;
        }

        // ── Unity ──────────────────────────────────────────────────────────

        private void Update()
        {
            if (!_initialized || _cardTransforms.Count == 0) return;
            AnimateCards();
        }

        // ── Private ────────────────────────────────────────────────────────

        private void AnimateCards()
        {
            for (int i = 0; i < _cardTransforms.Count; i++)
            {
                float targetScale = (i == _currentCenterIndex) ? _centerScale : _sideScale;
                float targetAlpha = (i == _currentCenterIndex) ? _centerAlpha : _sideAlpha;

                // Smooth lerp toward target
                float currentScale = _cardTransforms[i].localScale.x;
                float newScale = Mathf.Lerp(currentScale, targetScale, Time.deltaTime * _animSpeed);
                _cardTransforms[i].localScale = Vector3.one * newScale;

                // Alpha lerp
                float currentAlpha = _cardGroups[i].alpha;
                _cardGroups[i].alpha = Mathf.Lerp(currentAlpha, targetAlpha, Time.deltaTime * _animSpeed);
            }
        }

        private void ApplyScalesImmediate(int centerIndex)
        {
            for (int i = 0; i < _cardTransforms.Count; i++)
            {
                float scale = (i == centerIndex) ? _centerScale : _sideScale;
                float alpha = (i == centerIndex) ? _centerAlpha : _sideAlpha;

                _cardTransforms[i].localScale = Vector3.one * scale;
                _cardGroups[i].alpha = alpha;
                _levelCards[i].SetCentered(i == centerIndex);
            }
        }
    }
}