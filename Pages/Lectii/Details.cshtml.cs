using Learn_Earn.Data;
using Learn_Earn.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Learn_Earn.Services;

namespace Learn_Earn.Pages.Lectii
{
    public class DetailsModel : PageModel
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<IdentityUser> _userManager;

        public DetailsModel(ApplicationDbContext db, UserManager<IdentityUser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        public Lesson Lesson { get; set; }
        public LessonProgress? Progress { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            Lesson = await _db.Lessons.FirstOrDefaultAsync(l => l.Id == id);
            if (Lesson == null) return NotFound();

            var userId = _userManager.GetUserId(User);
            if (!string.IsNullOrEmpty(userId))
            {
                Progress = await _db.LessonProgresses.FirstOrDefaultAsync(p => p.LessonId == id && p.UserId == userId);
            }

            return Page();
        }

        public async Task<IActionResult> OnPostMarkSectionAsync(int id)
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId))
            {
                return Challenge();
            }
            // Check if this user had any completed sections before — used to award "First Lesson" badge
            var hadAnyProgressBefore = await _db.LessonProgresses.AnyAsync(p => p.UserId == userId && p.CompletedSections > 0);

            var progress = await _db.LessonProgresses.FirstOrDefaultAsync(p => p.LessonId == id && p.UserId == userId);
            if (progress == null)
            {
                // mark lesson as completed (one-time)
                progress = new LessonProgress { LessonId = id, UserId = userId, CompletedSections = 1 };
                _db.LessonProgresses.Add(progress);
            }
            else if (progress.CompletedSections == 0)
            {
                // if somehow present but not completed, set to completed
                progress.CompletedSections = 1;
                progress.UpdatedAt = DateTime.UtcNow;
                _db.LessonProgresses.Update(progress);
            }
            // otherwise already completed — idempotent: do nothing

            // Record daily completion (one per user per day)
            var today = DateTime.UtcNow.Date;
            var existingDaily = await _db.UserDailyCompletions.FirstOrDefaultAsync(d => d.UserId == userId && d.Date == today);
            if (existingDaily == null)
            {
                _db.UserDailyCompletions.Add(new UserDailyCompletion { UserId = userId, Date = today });
            }

            // Award XP for completing this lesson
            var lesson = await _db.Lessons.FirstOrDefaultAsync(l => l.Id == id);
            var xpToAward = LevelService.GetXpForLesson(lesson);
            var userXp = await _db.UserXps.FirstOrDefaultAsync(x => x.UserId == userId);
            if (userXp == null)
            {
                userXp = new UserXp { UserId = userId, TotalXp = xpToAward };
                _db.UserXps.Add(userXp);
            }
            else
            {
                userXp.TotalXp += xpToAward;
                _db.UserXps.Update(userXp);
            }

            // Create a notification for the professor to review/assign the quiz
            var professorId = lesson?.CreatedByUserId;
            _db.ProfessorNotifications.Add(new ProfessorNotification
            {
                LessonId = id,
                StudentUserId = userId,
                ProfessorUserId = professorId,
                IsHandled = false
            });

            await _db.SaveChangesAsync();

            return RedirectToPage(new { id = id });
        }
    }
}
