using Learn_Earn.Data;
using Learn_Earn.Models;
using Learn_Earn.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Learn_Earn.Pages.Admin
{
    [Authorize(Roles = "Admin")]
    public class AwardLevelBadgesModel : PageModel
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<IdentityUser> _userManager;

        public AwardLevelBadgesModel(ApplicationDbContext db, UserManager<IdentityUser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        public List<string> Report { get; set; } = new List<string>();

        public async Task<IActionResult> OnGetAsync()
        {
            var adminUserId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(adminUserId)) return Challenge();

            // Define thresholds used across the app
            var levelThresholds = new int[] { 1, 2, 3, 5, 7, 10, 15, 25, 50, 100 };
            var quizCountThresholds = new int[] { 1, 5, 10, 25, 50, 100, 250, 500, 1000, 2000 };
            var quizStreakThresholds = new int[] { 1, 3, 7, 14, 21, 30, 45, 60, 90, 120 };
            var lessonStreakThresholds = new int[] { 2, 3, 5, 7, 14, 30, 45, 60, 90, 120 };

            // Collect all users from Users table (ensure admins without XP are included)
            var allUsers = await _db.Users.Select(u => u.Id).ToListAsync();
            int totalAdded = 0;

            foreach (var uid in allUsers)
            {
                var existingBadges = await _db.Badges.Where(b => b.UserId == uid).Select(b => b.Name).ToListAsync();
                var existingSet = new HashSet<string>(existingBadges);
                var addedForUser = new List<string>();

                // LEVEL badges
                var userXp = await _db.UserXps.FirstOrDefaultAsync(x => x.UserId == uid);
                if (userXp != null)
                {
                    var level = LevelService.GetLevel(userXp.TotalXp);
                    foreach (var t in levelThresholds)
                    {
                        if (level >= t)
                        {
                            var name = $"Level {t}";
                            if (!existingSet.Contains(name))
                            {
                                _db.Badges.Add(new Badge { UserId = uid, Name = name });
                                existingSet.Add(name);
                                addedForUser.Add(name);
                                totalAdded++;
                            }
                        }
                    }
                }

                // FIRST LESSON badge
                var hadAnyLessonProgress = await _db.LessonProgresses.AnyAsync(p => p.UserId == uid && p.CompletedSections > 0);
                if (hadAnyLessonProgress)
                {
                    var name = "First Lesson";
                    if (!existingSet.Contains(name))
                    {
                        _db.Badges.Add(new Badge { UserId = uid, Name = name });
                        existingSet.Add(name);
                        addedForUser.Add(name);
                        totalAdded++;
                    }
                }

                // QUIZ counts (passed validated attempts)
                var passedCount = await _db.QuizAttempts.CountAsync(a => a.UserId == uid && a.IsValidated == true && a.Passed == true);
                foreach (var t in quizCountThresholds)
                {
                    if (passedCount >= t)
                    {
                        var name = t == 1 ? "First Quiz" : $"{t} Quizzes Completed";
                        if (!existingSet.Contains(name))
                        {
                            _db.Badges.Add(new Badge { UserId = uid, Name = name });
                            existingSet.Add(name);
                            addedForUser.Add(name);
                            totalAdded++;
                        }
                    }
                }

                // QUIZ streaks: compute longest consecutive run from UserQuizDailyCompletions
                var quizDates = await _db.UserQuizDailyCompletions.Where(d => d.UserId == uid).Select(d => d.Date).Distinct().ToListAsync();
                var quizLongest = GetLongestConsecutiveDays(quizDates);
                foreach (var t in quizStreakThresholds)
                {
                    if (quizLongest >= t)
                    {
                        var name = $"Quiz Streak {t} days";
                        if (!existingSet.Contains(name))
                        {
                            _db.Badges.Add(new Badge { UserId = uid, Name = name });
                            existingSet.Add(name);
                            addedForUser.Add(name);
                            totalAdded++;
                        }
                    }
                }

                // LESSON streaks: compute longest consecutive run from UserDailyCompletions
                var lessonDates = await _db.UserDailyCompletions.Where(d => d.UserId == uid).Select(d => d.Date).Distinct().ToListAsync();
                var lessonLongest = GetLongestConsecutiveDays(lessonDates);
                foreach (var t in lessonStreakThresholds)
                {
                    if (lessonLongest >= t)
                    {
                        var name = $"Streak {t} days";
                        if (!existingSet.Contains(name))
                        {
                            _db.Badges.Add(new Badge { UserId = uid, Name = name });
                            existingSet.Add(name);
                            addedForUser.Add(name);
                            totalAdded++;
                        }
                    }
                }

                if (addedForUser.Any())
                {
                    Report.Add($"User {uid}: added {string.Join(", ", addedForUser)}");
                }
            }

            if (totalAdded > 0)
            {
                await _db.SaveChangesAsync();
            }

            Report.Insert(0, $"Total badges added: {totalAdded}");
            return Page();
        }

        private static int GetLongestConsecutiveDays(List<DateTime> dates)
        {
            if (dates == null || dates.Count == 0) return 0;
            var sorted = dates.Select(d => d.Date).Distinct().OrderBy(d => d).ToList();
            int longest = 1;
            int current = 1;
            for (int i = 1; i < sorted.Count; i++)
            {
                if (sorted[i] == sorted[i - 1].AddDays(1))
                {
                    current++;
                    if (current > longest) longest = current;
                }
                else current = 1;
            }
            return longest;
        }
    }
}
