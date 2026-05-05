using System;

namespace TechnicalAssignment.Common.Interfaces
{
    /// <summary>
    /// Contract for services that hold resources and must clean up.
    /// WHY: Unity's OnDestroy is unreliable for service cleanup order.
    /// Explicit dispose gives us control.
    /// </summary>
    public interface IDisposableService : IDisposable
    {
        bool IsDisposed { get; }
    }
}