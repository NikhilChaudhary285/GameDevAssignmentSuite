using System;

namespace TechnicalAssignment.CoinChange.Tests
{
    /// <summary>
    /// Data container for a single test case.
    /// WHY separate from runner: Single Responsibility.
    /// Adding 50 new test cases = zero changes to runner logic.
    /// </summary>
    [Serializable]
    public class CoinChangeTestCase
    {
        public string TestName;
        public int[] Notes;
        public int Amount;
        public int ExpectedResult;
        public string Description;

        public CoinChangeTestCase(
            string name,
            int[] notes,
            int amount,
            int expected,
            string description = "")
        {
            TestName = name;
            Notes = notes;
            Amount = amount;
            ExpectedResult = expected;
            Description = description;
        }
    }

    [Serializable]
    public class CoinChangeTestResult
    {
        public string TestName;
        public bool Passed;
        public int Expected;
        public int Actual;
        public string Description;
        public long ExecutionTimeMs;

        public string Summary =>
            $"[{(Passed ? "PASS" : "FAIL")}] {TestName}\n" +
            $"  Expected: {Expected} ways | Got: {Actual} ways\n" +
            $"  {Description}\n" +
            $"  Time: {ExecutionTimeMs}ms";
    }
}