using System.Collections.Generic;
using UnityEngine;

namespace TechnicalAssignment.CoinChange.Core
{
    /// <summary>
    /// Combination-count DP implementation of the Coin Change problem.
    ///
    /// PROBLEM STATEMENT:
    /// Given coin denominations and a target amount, find HOW MANY DISTINCT
    /// WAYS you can combine coins (with unlimited reuse) to reach the amount.
    /// Order does NOT matter: [1,5] and [5,1] count as ONE way.
    ///
    /// ALGORITHM: Unbounded Knapsack / Coin Change 2
    ///
    /// dp[i] = number of ways to make exactly amount i
    ///
    /// RECURRENCE:
    /// dp[0] = 1  (base case: one way to make 0 — use nothing)
    /// For each coin c:
    ///   For amt = c to amount:
    ///     dp[amt] += dp[amt - c]
    ///
    /// CRITICAL LOOP ORDER (most asked interview question):
    /// Outer = coins, Inner = amounts
    /// → Guarantees COMBINATIONS (each set counted once)
    /// Swapping gives PERMUTATIONS (each ordering counted separately)
    ///
    /// TIME:  O(n × amount) where n = number of coin denominations
    /// SPACE: O(amount) — single 1D array, not 2D table
    /// </summary>
    public class CoinChangeService : ICoinChangeService
    {
        /// <summary>
        /// Returns number of ways to make 'amount' using coins in 'notes'.
        ///
        /// INTERVIEW WALKTHROUGH:
        /// notes=[1,10], amount=20
        ///
        /// Init: dp[0]=1, dp[1..20]=0
        ///
        /// Coin=1:
        ///   amt=1:  dp[1]  += dp[0]  → 1
        ///   amt=2:  dp[2]  += dp[1]  → 1
        ///   ...
        ///   amt=20: dp[20] += dp[19] → 1
        ///   (only one way using just 1s: twenty 1s)
        ///
        /// Coin=10:
        ///   amt=10: dp[10] += dp[0]  → 1+1=2  (ten 1s, or one 10)
        ///   amt=11: dp[11] += dp[1]  → 1+1=2
        ///   ...
        ///   amt=20: dp[20] += dp[10] → 1+2=3
        ///   (twenty 1s | ten 1s + one 10 | two 10s)
        ///
        /// Answer: dp[20] = 3 ✅
        /// </summary>
        public int GetChange(int[] notes, int amount)
        {
            // ── Edge Case Handling ──────────────────────────────────────────

            if (notes == null || notes.Length == 0)
            {
                Debug.LogWarning("[CoinChangeService] Notes array is null or empty.");
                return -1;
            }

            if (amount < 0)
            {
                Debug.LogWarning("[CoinChangeService] Amount cannot be negative.");
                return -1;
            }

            // Validate denominations
            foreach (int note in notes)
            {
                if (note <= 0)
                {
                    Debug.LogWarning($"[CoinChangeService] Invalid denomination: {note}. All must be > 0.");
                    return -1;
                }
            }

            // Base case: exactly one way to make 0 (use no coins)
            if (amount == 0)
                return 1;

            // ── Core DP ────────────────────────────────────────────────────

            return CountWays(notes, amount);
        }

        /// <summary>
        /// The pure DP computation — no Unity dependency, fully unit-testable.
        ///
        /// WHY long type for dp:
        /// For large inputs, number of ways can exceed int.MaxValue (2.1 billion).
        /// Example: notes=[1,2,3], amount=1000 → astronomically large number.
        /// We clamp to int.MaxValue on return to maintain interface contract.
        /// In production you'd return long or BigInteger.
        /// </summary>
        private int CountWays(int[] notes, int amount)
        {
            // Use long to prevent overflow on large inputs
            long[] dp = new long[amount + 1];

            // Base case: one way to make amount 0
            dp[0] = 1;

            // OUTER LOOP = coins (this is what makes it combinations, not permutations)
            foreach (int coin in notes)
            {
                // Skip coins larger than our target (optimization)
                if (coin > amount) continue;

                // INNER LOOP = amounts, from coin value up to target
                for (int amt = coin; amt <= amount; amt++) // Example: 4 -> amount // notes(coin) -> 1, 2
                {
                    // KEY INSIGHT:
                    // dp[amt - coin] = ways to make the REMAINDER after using this coin.
                    // Adding it to dp[amt] means: "all previous ways, PLUS this coin."
                    // Since we're iterating amounts forward, dp[amt - coin] is already
                    // computed for the CURRENT coin — so we're allowing unlimited reuse.
                    dp[amt] += dp[amt - coin];

                    // Overflow guard: cap at int.MaxValue
                    if (dp[amt] > int.MaxValue)
                    {
                        dp[amt] = int.MaxValue;
                    }
                }
            }

            return (int)dp[amount];
        }

        /// <summary>
        /// Returns all unique combinations that sum to 'amount'.
        ///
        /// IMPLEMENTATION: Recursive backtracking with pruning.
        /// WHY not DP here: DP counts ways efficiently, but to ENUMERATE
        /// all combinations you need backtracking — different algorithm.
        ///
        /// WARNING: Only call for small amounts (<= 50 recommended).
        /// Number of combinations grows exponentially.
        /// </summary>
        public List<int[]> GetChangeBreakdown(int[] notes, int amount)
        {
            var results = new List<int[]>();

            if (notes == null || notes.Length == 0 || amount <= 0)
                return results;

            // Sort ascending — ensures we build combinations in order,
            // which naturally avoids duplicates (e.g., [1,5] vs [5,1])
            System.Array.Sort(notes);

            var current = new List<int>();
            FindCombinations(notes, amount, 0, current, results);

            return results;
        }

        /// <summary>
        /// Recursive backtracking to find all combinations.
        ///
        /// startIndex: we only pick coins at or after this index.
        /// WHY: Prevents [1,5] and [5,1] from both being added.
        /// By always moving forward through the coins array,
        /// we only generate each combination ONCE.
        /// </summary>
        private void FindCombinations(
            int[] notes,
            int remaining,
            int startIndex,
            List<int> current,
            List<int[]> results)
        {
            // Base case: found a valid combination
            if (remaining == 0)
            {
                results.Add(current.ToArray());
                return;
            }

            // Safety cap: don't enumerate more than 1000 combinations in UI
            if (results.Count >= 1000) return;

            for (int i = startIndex; i < notes.Length; i++)
            {
                int coin = notes[i];

                // Prune: if this coin is too large, all subsequent coins
                // (array is sorted) will also be too large — stop early
                if (coin > remaining) break;

                current.Add(coin);

                // Pass i (not i+1) because coins can be reused (unbounded)
                FindCombinations(notes, remaining - coin, i, current, results);

                current.RemoveAt(current.Count - 1);
            }
        }
    }
}