using System;
using System.Collections;
using UnityEngine;

namespace TechnicalAssignment.ScrollMenu.Core
{
    /// <summary>
    /// Navigation logic for the swipe-snap scroll menu.
    /// 
    /// RESPONSIBILITIES (Single Responsibility):
    /// - Track current page index
    /// - Respond to swipe delta (drag)
    /// - Snap to nearest card on release
    /// - Fire OnPageChanged events
    /// 
    /// NOT responsible for:
    /// - Moving RectTransforms (that's ScrollMenuView)
    /// - Rendering cards (that's LevelCard)
    /// - Showing dots (that's PaginationDots)
    /// 
    /// PATTERN: This is a pure state machine.
    /// View reacts to events — controller never touches UI directly.
    /// </summary>
    public class ScrollMenuController : IScrollMenuController
    {
        // ── State ──────────────────────────────────────────────────────────
        public int CurrentIndex { get; private set; }
        public int TotalCards { get; private set; }
        public bool IsAnimating { get; private set; }

        public event Action<int> OnPageChanged;

        // ── Config ─────────────────────────────────────────────────────────
        // How far user must drag (fraction of card width) to trigger a page change
        private const float SWIPE_THRESHOLD = 0.2f;
        // Minimum velocity (pixels/sec) to trigger page change regardless of distance
        private const float VELOCITY_THRESHOLD = 500f;

        // ── Runtime ────────────────────────────────────────────────────────
        private float _cardWidth;
        private float _accumulatedDrag;

        public ScrollMenuController(int totalCards, float cardWidth)
        {
            TotalCards = totalCards;
            _cardWidth = cardWidth;
            CurrentIndex = 0;
        }

        // ── Public API ─────────────────────────────────────────────────────

        public void GoToNext()
        {
            if (IsAnimating) return;
            GoToIndex(CurrentIndex + 1);
        }

        public void GoPrevious()
        {
            if (IsAnimating) return;
            GoToIndex(CurrentIndex - 1);
        }

        public void GoToIndex(int index)
        {
            if (IsAnimating) return;

            // Clamp to valid range
            int clamped = Mathf.Clamp(index, 0, TotalCards - 1);
            if (clamped == CurrentIndex && index == clamped) return;

            CurrentIndex = clamped;
            IsAnimating = true;

            OnPageChanged?.Invoke(CurrentIndex);
        }

        /// <summary>
        /// Called every frame while user is dragging.
        /// deltaX: pixels moved this frame (positive = dragged right = go left)
        /// </summary>
        public void OnDragDelta(float deltaX)
        {
            _accumulatedDrag += deltaX;
        }

        /// <summary>
        /// Called when user releases finger/mouse.
        /// velocity: pixels per second at release moment.
        /// Positive velocity = moving right (going to previous card).
        /// Negative velocity = moving left (going to next card).
        /// </summary>
        public void OnDragEnd(float velocity)
        {
            float dragFraction = _accumulatedDrag / _cardWidth;
            bool fastSwipe = Mathf.Abs(velocity) > VELOCITY_THRESHOLD;

            int targetIndex = CurrentIndex;

            if (fastSwipe)
            {
                // Fast swipe — direction determines page
                targetIndex = velocity > 0
                    ? CurrentIndex - 1   // swiped right → go to previous
                    : CurrentIndex + 1;  // swiped left  → go to next
            }
            else if (Mathf.Abs(dragFraction) > SWIPE_THRESHOLD)
            {
                // Slow drag past threshold — direction determines page
                targetIndex = _accumulatedDrag > 0
                    ? CurrentIndex - 1
                    : CurrentIndex + 1;
            }
            // else: didn't drag far enough → snap back to current

            _accumulatedDrag = 0f;
            IsAnimating = false; // allow GoToIndex to proceed

            GoToIndex(targetIndex);
        }

        /// <summary>
        /// Called by View after animation completes.
        /// </summary>
        public void OnAnimationComplete()
        {
            IsAnimating = false;
        }
    }
}