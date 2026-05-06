using System.Collections.Generic;
using UnityEngine;

namespace TechnicalAssignment.ScrollMenu.Core
{
    /// <summary>
    /// Provides level data to the scroll menu.
    /// WHY separate class: In production this would fetch from a server,
    /// save file, or ScriptableObject. The menu never cares WHERE data
    /// comes from — only that it receives List<LevelData>.
    /// This is the Dependency Inversion Principle in action.
    /// </summary>
    public class LevelDataProvider
    {
        public List<LevelData> GetLevels()
        {
            // In production: load from PlayerPrefs, JSON, or server.
            // For demo: hardcoded sample data.
            return new List<LevelData>
            {
                new LevelData(1,  "Forest Dawn",    3, true,  HexColor("#4CAF50")),
                new LevelData(2,  "River Crossing", 2, true,  HexColor("#2196F3")),
                new LevelData(3,  "Desert Storm",   1, true,  HexColor("#FF9800")),
                new LevelData(4,  "Ice Cavern",     3, true,  HexColor("#00BCD4")),
                new LevelData(5,  "Volcano Peak",   0, true,  HexColor("#F44336")),
                new LevelData(6,  "Sky Temple",     0, false, HexColor("#9C27B0")),
                new LevelData(7,  "Shadow Realm",   0, false, HexColor("#607D8B")),
                new LevelData(8,  "Crystal Cave",   0, false, HexColor("#009688")),
                new LevelData(9,  "Thunder Plains", 0, false, HexColor("#795548")),
                new LevelData(10, "Final Fortress",  0, false, HexColor("#E91E63")),
            };
        }

        private Color HexColor(string hex)
        {
            ColorUtility.TryParseHtmlString(hex, out Color c);
            return c;
        }
    }
}