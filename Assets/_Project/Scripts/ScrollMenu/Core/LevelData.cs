using System;
using UnityEngine;

namespace TechnicalAssignment.ScrollMenu.Core
{
    /// <summary>
    /// Pure data model for a single level card.
    /// WHY: Data separated from View (MVC pattern).
    /// Changing card visual design never touches this class.
    /// </summary>
    [Serializable]
    public class LevelData
    {
        public int LevelNumber;
        public string LevelName;
        public int StarsEarned;   // 0–3
        public int MaxStars;      // always 3
        public bool IsUnlocked;
        public Color CardColor;

        public LevelData(int number, string name, int stars, bool unlocked, Color color)
        {
            LevelNumber = number;
            LevelName = name;
            StarsEarned = Mathf.Clamp(stars, 0, 3);
            MaxStars = 3;
            IsUnlocked = unlocked;
            CardColor = color;
        }
    }
}