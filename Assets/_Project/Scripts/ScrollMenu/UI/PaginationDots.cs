using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace TechnicalAssignment.ScrollMenu.UI
{
    /// <summary>
    /// Displays pagination dots below the scroll menu.
    /// Active dot = current page. Inactive = other pages.
    ///
    /// SINGLE RESPONSIBILITY: Only manages dot visuals.
    /// Receives page change events — never controls navigation.
    /// </summary>
    public class PaginationDots : MonoBehaviour
    {
        [Header("── Dot Settings ───────────────────")]
        [SerializeField] private GameObject _dotPrefab;
        [SerializeField] private Color _activeColor = Color.white;
        [SerializeField] private Color _inactiveColor = new Color(1, 1, 1, 0.35f);
        [SerializeField] private float _activeDotSize = 16f;
        [SerializeField] private float _inactiveDotSize = 10f;

        private List<Image> _dots = new List<Image>();
        private List<RectTransform> _dotRects = new List<RectTransform>();
        private int _currentIndex;

        // ── Public API ─────────────────────────────────────────────────────

        public void Initialize(int count)
        {
            // Clear existing
            foreach (Transform child in transform)
                Destroy(child.gameObject);

            _dots.Clear();
            _dotRects.Clear();

            for (int i = 0; i < count; i++)
            {
                GameObject dot = Instantiate(_dotPrefab, transform);
                dot.name = $"Dot_{i}";

                Image img = dot.GetComponent<Image>();
                RectTransform rt = dot.GetComponent<RectTransform>();

                if (img != null) _dots.Add(img);
                if (rt != null) _dotRects.Add(rt);
            }

            SetActiveDot(0);
        }

        public void SetActiveDot(int index)
        {
            _currentIndex = index;

            for (int i = 0; i < _dots.Count; i++)
            {
                bool isActive = (i == index);

                _dots[i].color = isActive ? _activeColor : _inactiveColor;

                float size = isActive ? _activeDotSize : _inactiveDotSize;
                _dotRects[i].sizeDelta = new Vector2(size, size);
            }
        }
    }
}