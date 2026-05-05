namespace TechnicalAssignment.Common.Interfaces
{
    /// <summary>
    /// Contract for any system that requires explicit initialization.
    /// WHY: Separates construction from initialization (S in SOLID).
    /// Systems can be created but initialized lazily or in order.
    /// </summary>
    public interface IInitializable
    {
        void Initialize();
        bool IsInitialized { get; }
    }
}