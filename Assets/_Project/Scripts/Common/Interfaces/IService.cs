namespace TechnicalAssignment.Common.Interfaces
{
    /// <summary>
    /// Marker interface for all service-layer classes.
    /// WHY: Allows generic service locator patterns and 
    /// clear architectural intent — this is a SERVICE, not a MonoBehaviour controller.
    /// </summary>
    public interface IService : IInitializable, IDisposableService
    {

    }
}