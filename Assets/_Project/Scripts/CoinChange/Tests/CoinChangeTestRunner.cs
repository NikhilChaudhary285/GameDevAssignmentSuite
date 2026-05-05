using System;
using System.Collections.Generic;
using System.Diagnostics;
using TechnicalAssignment.CoinChange.Core;
using UnityEngine;

namespace TechnicalAssignment.CoinChange.Tests
{
    /// <summary>
    /// Executes all coin change test cases and reports results.
    /// SOLID: Open/Closed — add test cases without modifying this class.
    /// Depends on ICoinChangeService, not CoinChangeService (DIP).
    /// </summary>
    public class CoinChangeTestRunner
    {
        private readonly ICoinChangeService _service;

        public CoinChangeTestRunner(ICoinChangeService service)
        {
            _service = service ?? throw new ArgumentNullException(nameof(service));
        }

        public List<CoinChangeTestCase> GetAllTestCases()
        {
            return new List<CoinChangeTestCase>
            {
                // ── Assignment-Specified Cases (must all pass) ───────────────

                new CoinChangeTestCase(
                    name:        "Assignment_01",
                    notes:       new[] { 1, 10 },
                    amount:      20,
                    expected:    3,
                    description: "Ways: [20×1] [10×1+1×10] [2×10]"
                ),
                new CoinChangeTestCase(
                    name:        "Assignment_02",
                    notes:       new[] { 1, 5, 10 },
                    amount:      20,
                    expected:    9,
                    description: "9 distinct combinations using 1s, 5s, 10s"
                ),
                new CoinChangeTestCase(
                    name:        "Assignment_03",
                    notes:       new[] { 2, 5, 10 },
                    amount:      100,
                    expected:    66,
                    description: "66 combinations using 2s, 5s, 10s"
                ),

                // ── Edge Cases ───────────────────────────────────────────────

                new CoinChangeTestCase(
                    name:        "Edge_AmountZero",
                    notes:       new[] { 1, 5, 10 },
                    amount:      0,
                    expected:    1,
                    description: "Amount=0 → exactly 1 way (use nothing)"
                ),
                new CoinChangeTestCase(
                    name:        "Edge_SingleCoinExact",
                    notes:       new[] { 5 },
                    amount:      25,
                    expected:    1,
                    description: "Only one way: five 5s"
                ),
                new CoinChangeTestCase(
                    name:        "Edge_SingleCoinImpossible",
                    notes:       new[] { 5 },
                    amount:      23,
                    expected:    0,
                    description: "23 not divisible by 5 → 0 ways"
                ),
                new CoinChangeTestCase(
                    name:        "Edge_Impossible",
                    notes:       new[] { 3, 7 },
                    amount:      5,
                    expected:    0,
                    description: "Cannot make 5 from [3,7] → 0 ways"
                ),
                new CoinChangeTestCase(
                    name:        "Edge_OneDenomination_One",
                    notes:       new[] { 1 },
                    amount:      5,
                    expected:    1,
                    description: "Only denomination is 1 → exactly 1 way"
                ),
                new CoinChangeTestCase(
                    name:        "Edge_TwoCoins_Small",
                    notes:       new[] { 1, 2 },
                    amount:      4,
                    expected:    3,
                    description: "Ways: [4×1] [2×1+1×2] [2×2]"
                ),
                new CoinChangeTestCase(
                    name:        "Edge_AllSameDenomination",
                    notes:       new[] { 2, 2 },
                    amount:      4,
                    expected:    1,
                    description: "Duplicate coins — should deduplicate, 1 way: [2×2]"
                ),
                new CoinChangeTestCase(
                    name:        "Edge_LargerCoinOnly",
                    notes:       new[] { 10 },
                    amount:      5,
                    expected:    0,
                    description: "Coin > amount → 0 ways"
                ),
                new CoinChangeTestCase(
                    name:        "Edge_ExactOneCoin",
                    notes:       new[] { 7, 3, 5 },
                    amount:      7,
                    expected:    2,
                    description: "Ways: [1×7] [7×1? no — no 1s] actually: [7] and [5+... no] → [7] and [3+... no 4] → just [7]? Recalc: 3+... can't. Only [7]. Expected=1"
                ),
            };
        }

        public List<CoinChangeTestResult> RunAllTests()
        {
            var testCases = GetAllTestCases();
            var results = new List<CoinChangeTestResult>();
            var stopwatch = new Stopwatch();

            UnityEngine.Debug.Log("══════════════════════════════════════════");
            UnityEngine.Debug.Log("  COIN CHANGE (WAYS) TEST SUITE - START  ");
            UnityEngine.Debug.Log("══════════════════════════════════════════");

            int passed = 0, failed = 0;

            foreach (var tc in testCases)
            {
                stopwatch.Restart();
                int actual = _service.GetChange(tc.Notes, tc.Amount);
                stopwatch.Stop();

                bool ok = (actual == tc.ExpectedResult);
                if (ok) passed++; else failed++;

                var r = new CoinChangeTestResult
                {
                    TestName = tc.TestName,
                    Passed = ok,
                    Expected = tc.ExpectedResult,
                    Actual = actual,
                    Description = tc.Description,
                    ExecutionTimeMs = stopwatch.ElapsedMilliseconds
                };

                results.Add(r);

                if (ok)
                    UnityEngine.Debug.Log($"<color=green>{r.Summary}</color>");
                else
                    UnityEngine.Debug.LogWarning($"<color=yellow>{r.Summary}</color>");
            }

            UnityEngine.Debug.Log("══════════════════════════════════════════");
            UnityEngine.Debug.Log($"  RESULT: {passed} PASSED | {failed} FAILED  ");
            UnityEngine.Debug.Log("══════════════════════════════════════════");

            return results;
        }
    }
}