using Learn_Earn.Models;
using System.Collections.Generic;

namespace Learn_Earn.Services
{
    public static class LevelService
    {
        // Level thresholds: index = level, value = minimum XP required for that level
        // Level 0 -> 0 XP, Level 1 -> 25 XP, Level 2 -> 75 XP, etc.
        private static readonly int[] Thresholds = new int[] { 0, 25, 75, 150, 300, 500, 800, 1200, 1700, 2300, 3000 };

        public static int GetLevel(int totalXp)
        {
            int level = 0;
            for (int i = 0; i < Thresholds.Length; i++)
            {
                if (totalXp >= Thresholds[i]) level = i;
                else break;
            }
            return level;
        }

        public static int GetNextLevelXp(int totalXp)
        {
            for (int i = 0; i < Thresholds.Length; i++)
            {
                if (totalXp < Thresholds[i]) return Thresholds[i];
            }
            // If beyond known thresholds, scale linearly (next +500)
            return Thresholds[^1] + 500;
        }

        public static int GetCurrentLevelMinXp(int totalXp)
        {
            int level = GetLevel(totalXp);
            if (level < 0) return 0;
            if (level >= Thresholds.Length) return Thresholds[^1];
            return Thresholds[level];
        }

        // Determine XP awarded for completing a lesson. Use lesson difficulty when available.
        public static int GetXpForLesson(Lesson? lesson)
        {
            if (lesson == null) return 10;
            var diff = (lesson.Difficulty ?? string.Empty).ToLowerInvariant();
            return diff switch
            {
                "easy" => 10,
                "medium" => 25,
                "hard" => 50,
                _ => 20,
            };
        }
    }
}
