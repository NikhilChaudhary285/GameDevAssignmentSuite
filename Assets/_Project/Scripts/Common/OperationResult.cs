using System;

namespace TechnicalAssignment.Common
{
    /// <summary>
    /// Generic result wrapper that carries either a success value or an error.
    /// WHY: Avoids throwing exceptions for expected failures (network errors, cache misses).
    /// Exceptions are for EXCEPTIONAL cases. A failed download is expected behavior.
    /// Pattern: Railway-oriented programming / Result monad.
    /// </summary>
    public readonly struct OperationResult<T>
    {
        public readonly T Value;
        public readonly string ErrorMessage;
        public readonly bool IsSuccess;
        public readonly Exception Exception;

        private OperationResult(T value)
        {
            Value = value;
            IsSuccess = true;
            ErrorMessage = null;
            Exception = null;
        }

        private OperationResult(string errorMessage, Exception exception = null)
        {
            Value = default;
            IsSuccess = false;
            ErrorMessage = errorMessage;
            Exception = exception;
        }

        public static OperationResult<T> Success(T value) => new OperationResult<T>(value);

        public static OperationResult<T> Failure(string errorMessage, Exception ex = null)
            => new OperationResult<T>(errorMessage, ex);

        public override string ToString() =>
            IsSuccess ? $"Success({Value})" : $"Failure({ErrorMessage})";
    }
}