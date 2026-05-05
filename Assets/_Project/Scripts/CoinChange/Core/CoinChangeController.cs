using System.Collections.Generic;
using TechnicalAssignment.CoinChange.Tests;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TechnicalAssignment.CoinChange.Core
{
    /// <summary>
    /// MonoBehaviour UI bridge.
    /// Pure C# service has ZERO Unity dependency.
    /// This class ONLY handles: input parsing, button wiring, display.
    /// WHY: If Unity changes its UI system, we replace ONLY this file.
    /// CoinChangeService remains untouched.
    /// </summary>
    public class CoinChangeController : MonoBehaviour
    {
        [Header("── Input ──────────────────────────")]
        [SerializeField] private TMP_InputField _notesInputField;
        [SerializeField] private TMP_InputField _amountInputField;

        [Header("── Output ─────────────────────────")]
        [SerializeField] private TextMeshProUGUI _resultText;
        [SerializeField] private TextMeshProUGUI _breakdownText;
        [SerializeField] private TextMeshProUGUI _testResultsText;

        [Header("── Buttons ────────────────────────")]
        [SerializeField] private Button _calculateButton;
        [SerializeField] private Button _runTestsButton;
        [SerializeField] private Button _clearButton;

        [Header("── Scroll ──────────────────────────")]
        [SerializeField] private ScrollRect _testResultsScroll;

        [Header("── Settings ────────────────────────")]
        [Tooltip("Show combination breakdown (disable for large amounts)")]
        [SerializeField] private bool _showBreakdown = true;
        [Tooltip("Max amount before breakdown is suppressed")]
        [SerializeField] private int _breakdownAmountLimit = 50;

        private ICoinChangeService _coinChangeService;
        private CoinChangeTestRunner _testRunner;

        // ── Lifecycle ───────────────────────────────────────────────────────

        private void Awake()
        {
            // Composition Root: build dependency graph here.
            // CoinChangeService is pure C# — no MonoBehaviour needed.
            _coinChangeService = new CoinChangeService();
            _testRunner = new CoinChangeTestRunner(_coinChangeService);
        }

        private void Start()
        {
            _calculateButton.onClick.AddListener(OnCalculateClicked);
            _runTestsButton.onClick.AddListener(OnRunTestsClicked);
            _clearButton.onClick.AddListener(OnClearClicked);

            // Default values matching assignment test cases
            _notesInputField.text = "1,5,10";
            _amountInputField.text = "20";

            SetInitialDisplay();
        }

        private void OnDestroy()
        {
            // Always clean up listeners to prevent memory leaks
            _calculateButton.onClick.RemoveAllListeners();
            _runTestsButton.onClick.RemoveAllListeners();
            _clearButton.onClick.RemoveAllListeners();
        }

        // ── Button Handlers ─────────────────────────────────────────────────

        private void OnCalculateClicked()
        {
            if (!TryParseInputs(out int[] notes, out int amount))
                return;

            int ways = _coinChangeService.GetChange(notes, amount);

            // Handle invalid input return
            if (ways == -1)
            {
                ShowError("Invalid input. Check denominations are positive integers.");
                return;
            }

            // Display result
            if (ways == 0)
            {
                _resultText.text =
                    $"<color=#FF6B6B><b>0 Ways</b></color>\n" +
                    $"<size=30>Cannot make <b>{amount}</b> with denominations " +
                    $"[{string.Join(", ", notes)}]</size>";
                _breakdownText.text = "";
            }
            else
            {
                _resultText.text =
                    $"<color=#6BCB77><b>{ways} Way{(ways == 1 ? "" : "s")}</b></color>\n" +
                    $"<size=30>to make <b>{amount}</b> using " +
                    $"[{string.Join(", ", notes)}]</size>";

                DisplayBreakdown(notes, amount);
            }
        }

        private void OnRunTestsClicked()
        {
            _testResultsText.text = "<color=#FFD93D>Running tests...</color>\n";

            List<CoinChangeTestResult> results = _testRunner.RunAllTests();

            var sb = new System.Text.StringBuilder();
            int passed = 0, failed = 0;

            foreach (var r in results)
            {
                if (r.Passed)
                {
                    passed++;
                    sb.AppendLine($"<color=#6BCB77>:) {r.TestName}</color>");
                    sb.AppendLine($"   {r.Expected} ways  ·  {r.Description}");
                }
                else
                {
                    failed++;
                    sb.AppendLine($"<color=#FF6B6B>:| {r.TestName}</color>");
                    sb.AppendLine($"   Expected <b>{r.Expected}</b> | Got <b>{r.Actual}</b>");
                    sb.AppendLine($"   <color=#ADB5BD>{r.Description}</color>");
                }
                sb.AppendLine();
            }

            sb.AppendLine("──────────────────────────────");
            sb.AppendLine(
                $"<b>PASSED: <color=#6BCB77>{passed}</color>  " +
                $"FAILED: <color=#FF6B6B>{failed}</color></b>"
            );

            _testResultsText.text = sb.ToString();

            // Scroll to top after populating
            Canvas.ForceUpdateCanvases();
            if (_testResultsScroll != null)
                _testResultsScroll.verticalNormalizedPosition = 1f;
        }

        private void OnClearClicked()
        {
            _notesInputField.text = "";
            _amountInputField.text = "";
            SetInitialDisplay();
        }

        // ── Helpers ─────────────────────────────────────────────────────────

        private void SetInitialDisplay()
        {
            _resultText.text = "<color=#ADB5BD>Enter denominations and amount,\nthen press Calculate.</color>";
            _breakdownText.text = "";
            _testResultsText.text = "<color=#ADB5BD>Press 'Run Tests' to validate all cases.</color>";
        }

        private void ShowError(string message)
        {
            _resultText.text = $"<color=#FF6B6B>:| {message}</color>";
            _breakdownText.text = "";
        }

        /// <summary>
        /// Shows combination breakdown only for small amounts.
        /// WHY limit: for large amounts, enumeration is computationally
        /// expensive and the list would be too long to be useful in UI.
        /// </summary>
        private void DisplayBreakdown(int[] notes, int amount)
        {
            if (!_showBreakdown || amount > _breakdownAmountLimit)
            {
                _breakdownText.text = amount > _breakdownAmountLimit
                    ? $"<color=#ADB5BD>(Breakdown hidden for amount > {_breakdownAmountLimit})</color>"
                    : "";
                return;
            }

            List<int[]> combinations = _coinChangeService.GetChangeBreakdown(notes, amount);

            if (combinations.Count == 0)
            {
                _breakdownText.text = "";
                return;
            }

            var sb = new System.Text.StringBuilder();
            sb.AppendLine("<color=#ADB5BD>Combinations:</color>");

            for (int i = 0; i < combinations.Count; i++)
            {
                int[] combo = combinations[i];
                // Group: [1,1,1,5,10] → "3×1 + 1×5 + 1×10"
                sb.AppendLine($"  {i + 1}. {FormatCombo(combo)}");
            }

            _breakdownText.text = sb.ToString();
            //Debug.Log(sb.ToString());
        }

        /// <summary>
        /// Formats [1,1,1,5,10] → "3×1 + 1×5 + 1×10"
        /// </summary>
        private string FormatCombo(int[] coins)
        {
            var counts = new Dictionary<int, int>();
            foreach (int c in coins)
            {
                if (!counts.ContainsKey(c)) counts[c] = 0;
                counts[c]++;
            }

            var parts = new List<string>();
            foreach (var kvp in counts)
                parts.Add($"{kvp.Value}×{kvp.Key}");

            return string.Join(" + ", parts);
        }

        /// <summary>
        /// Parses user input with full validation and clear error messages.
        /// Returns false and sets error display if input is invalid.
        /// </summary>
        private bool TryParseInputs(out int[] notes, out int amount)
        {
            notes = null;
            amount = 0;

            string notesText = _notesInputField.text.Trim();

            if (string.IsNullOrEmpty(notesText))
            {
                ShowError("Please enter coin denominations (e.g. 1,5,10)");
                return false;
            }

            string[] parts = notesText.Split(',');
            notes = new int[parts.Length];

            for (int i = 0; i < parts.Length; i++)
            {
                string part = parts[i].Trim();
                if (!int.TryParse(part, out notes[i]) || notes[i] <= 0)
                {
                    ShowError($"Invalid denomination: '{part}'. Must be a positive integer.");
                    return false;
                }
            }

            string amountText = _amountInputField.text.Trim();
            if (!int.TryParse(amountText, out amount) || amount < 0)
            {
                ShowError("Amount must be a non-negative integer.");
                return false;
            }

            return true;
        }
    }
}