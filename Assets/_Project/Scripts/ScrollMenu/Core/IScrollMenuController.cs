using System;

namespace TechnicalAssignment.ScrollMenu.Core
{
    /// <summary>
    /// Contract for the scroll menu navigation logic.
    /// WHY: UI buttons, keyboard, swipe — all call THIS interface.
    /// Swapping the navigation engine (e.g., infinite scroll vs. finite)
    /// requires zero changes to LevelCard or PaginationDots.
    /// </summary>
    public interface IScrollMenuController
    {
        int CurrentIndex { get; }
        int TotalCards { get; }
        bool IsAnimating { get; }

        void GoToNext();
        void GoPrevious();
        void GoToIndex(int index);
        void OnDragDelta(float deltaX);
        void OnDragEnd(float velocity);
        void OnAnimationComplete();

        event Action<int> OnPageChanged;   // fires with new index
    }
}