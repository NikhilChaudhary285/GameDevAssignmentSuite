namespace TechnicalAssignment.CoinChange.Core
{
    /// <summary>
    /// Contract for coin change computation.
    /// 
    /// INTERVIEW NOTE:
    /// We keep this interface unchanged from the original design.
    /// The METHOD SIGNATURE is the same: GetChange(int[] notes, int amount)
    /// Only the IMPLEMENTATION changes — this is the Open/Closed Principle:
    /// "Open for extension, closed for modification."
    /// The UI and TestRunner never needed to change because they
    /// depend on THIS interface, not the concrete class.
    /// </summary>
    public interface ICoinChangeService
    {
        /// <summary>
        /// Returns the NUMBER OF WAYS to make 'amount'
        /// using unlimited coins from 'notes[]'.
        /// Order does NOT matter (combinations, not permutations).
        /// Returns 0 if amount cannot be made.
        /// Returns 1 if amount is 0 (one way: use nothing).
        /// Returns -1 for invalid inputs.
        /// </summary>
        int GetChange(int[] notes, int amount);

        /// <summary>
        /// Returns all unique combinations that sum to amount.
        /// Used for UI breakdown display.
        /// WARNING: Can be large for big inputs — use with caution.
        /// </summary>
        System.Collections.Generic.List<int[]> GetChangeBreakdown(int[] notes, int amount);
    }
}