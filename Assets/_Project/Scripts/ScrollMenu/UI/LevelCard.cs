using TechnicalAssignment.ScrollMenu.Core;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TechnicalAssignment.ScrollMenu.UI
{
    /// <summary>
    /// Controls a single level card's appearance and state.
    /// 
    /// SINGLE RESPONSIBILITY: Render one card's visual state.
    /// Knows nothing about scrolling, snapping, or other cards.
    /// 
    /// WHY MonoBehaviour: It lives on a GameObject in the scene.
    /// All logic is still data-driven via Bind(LevelData).
    /// </summary>
    public class LevelCard : MonoBehaviour
    {
        [Header("── References ─────────────────────")]
        [SerializeField] private Image _cardBackground;
        [SerializeField] private TextMeshProUGUI _levelNumberText;
        [SerializeField] private TextMeshProUGUI _levelNameText;
        [SerializeField] private GameObject[] _starsFilled;    // 3 filled star objects
        [SerializeField] private GameObject[] _starsEmpty;     // 3 empty star objects
        [SerializeField] private GameObject _lockOverlay;
        [SerializeField] private Button _playButton;
        [SerializeField] private TextMeshProUGUI _playButtonText;

        // ── Runtime State ──────────────────────────────────────────────────
        public int CardIndex { get; private set; }
        public LevelData Data { get; private set; }
        private bool _isCentered;

        // ── Public API ─────────────────────────────────────────────────────

        /// <summary>
        /// Populate this card with level data.
        /// Called once after Instantiate by ScrollMenuView.
        /// </summary>
        public void Bind(LevelData data, int cardIndex)
        {
            Data = data;
            CardIndex = cardIndex;

            // Background color
            if (_cardBackground != null)
                _cardBackground.color = data.CardColor;

            // Level number
            if (_levelNumberText != null)
                _levelNumberText.text = data.LevelNumber.ToString("D2");

            // Level name
            if (_levelNameText != null)
                _levelNameText.text = data.LevelName;

            // Stars
            RefreshStars(data.StarsEarned);

            // Lock state
            if (_lockOverlay != null)
                _lockOverlay.SetActive(!data.IsUnlocked);

            // Play button
            if (_playButton != null)
            {
                _playButton.interactable = data.IsUnlocked;
                _playButton.onClick.RemoveAllListeners();
                _playButton.onClick.AddListener(OnPlayClicked);
            }

            if (_playButtonText != null)
                _playButtonText.text = data.IsUnlocked ? "PLAY" : "LOCKED";
        }

        /// <summary>
        /// Called by CardScaleAnimator when this card becomes/leaves center.
        /// </summary>
        public void SetCentered(bool isCentered)
        {
            _isCentered = isCentered;

            // Play button only interactable when card is centered and unlocked
            if (_playButton != null && Data != null)
                _playButton.interactable = isCentered && Data.IsUnlocked;
        }

        // ── Private ────────────────────────────────────────────────────────

        private void RefreshStars(int earned)
        {
            if (_starsFilled == null || _starsEmpty == null) return;

            for (int i = 0; i < 3; i++)
            {
                bool hasStar = i < earned;
                if (i < _starsFilled.Length && _starsFilled[i] != null)
                    _starsFilled[i].SetActive(hasStar);
                if (i < _starsEmpty.Length && _starsEmpty[i] != null)
                    _starsEmpty[i].SetActive(!hasStar);
            }
        }

        private void OnPlayClicked()
        {
            if (Data == null || !Data.IsUnlocked) return;
            Debug.Log($"[LevelCard] Play clicked: Level {Data.LevelNumber} — {Data.LevelName}");
            // In production: SceneManager.LoadScene($"Level_{Data.LevelNumber}");
        }

        private void OnDestroy()
        {
            if (_playButton != null)
                _playButton.onClick.RemoveAllListeners();
        }
    }
}