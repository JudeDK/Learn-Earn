using Learn_Earn.Data;
using Learn_Earn.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Learn_Earn.Pages.Profesori
{
    public class ReviewAttemptModel : PageModel
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<IdentityUser> _userManager;

        public ReviewAttemptModel(ApplicationDbContext db, UserManager<IdentityUser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        public ProfessorNotification? Notification { get; set; }
        public Lesson? Lesson { get; set; }
        public Learn_Earn.Models.Quiz? Quiz { get; set; }
        public IdentityUser? Student { get; set; }
        public int QuizQuestionCount { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId)) return Challenge();
            if (!(User.IsInRole("Profesor") || User.IsInRole("Admin"))) return Forbid();

            Notification = await _db.ProfessorNotifications.FirstOrDefaultAsync(n => n.Id == id);
            if (Notification == null) return NotFound();

            Lesson = await _db.Lessons.FirstOrDefaultAsync(l => l.Id == Notification.LessonId);
            Student = await _userManager.FindByIdAsync(Notification.StudentUserId);

            // find a quiz for this lesson
            Quiz = await _db.Quizzes.FirstOrDefaultAsync(q => q.LessonId == Notification.LessonId);
            if (Quiz != null)
            {
                QuizQuestionCount = await _db.QuizQuestions.CountAsync(q => q.QuizId == Quiz.Id);
            }

            return Page();
        }

        public class InputModel
        {
            public int NotificationId { get; set; }
            public int Score { get; set; }
            public int Total { get; set; }
            public bool Passed { get; set; }
            public string? Note { get; set; }
        }

        [BindProperty]
        public InputModel Input { get; set; }

        public async Task<IActionResult> OnPostAsync()
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId)) return Challenge();
            if (!(User.IsInRole("Profesor") || User.IsInRole("Admin"))) return Forbid();

            var note = await _db.ProfessorNotifications.FirstOrDefaultAsync(n => n.Id == Input.NotificationId);
            if (note == null) return NotFound();

            var notif = note;
            var lesson = await _db.Lessons.FirstOrDefaultAsync(l => l.Id == notif.LessonId);
            var quiz = await _db.Quizzes.FirstOrDefaultAsync(q => q.LessonId == notif.LessonId);

            if (quiz == null)
            {
                // allow professor to still mark handled even if no quiz exists
                notif.IsHandled = true;
                _db.ProfessorNotifications.Update(notif);
                await _db.SaveChangesAsync();
                return RedirectToPage("/Profesori/MyLectii");
            }

            // update or create a QuizAttempt for this student and quiz
            var attempt = await _db.QuizAttempts.FirstOrDefaultAsync(a => a.QuizId == quiz.Id && a.UserId == notif.StudentUserId);
            if (attempt == null)
            {
                attempt = new QuizAttempt
                {
                    QuizId = quiz.Id,
                    UserId = notif.StudentUserId,
                    Score = Input.Score,
                    Total = Input.Total,
                    AttemptedAt = DateTime.UtcNow,
                    IsValidated = true,
                    ValidatedByUserId = userId,
                    Passed = Input.Passed,
                    ValidationNote = Input.Note
                };
                _db.QuizAttempts.Add(attempt);
            }
            else
            {
                attempt.Score = Input.Score;
                attempt.Total = Input.Total;
                attempt.AttemptedAt = DateTime.UtcNow;
                attempt.IsValidated = true;
                attempt.ValidatedByUserId = userId;
                attempt.Passed = Input.Passed;
                attempt.ValidationNote = Input.Note;
                _db.QuizAttempts.Update(attempt);
            }

            // mark notification handled
            notif.IsHandled = true;
            _db.ProfessorNotifications.Update(notif);

            // If passed, award streak badges (and first-lesson) similarly to previous logic
            if (Input.Passed)
            {
                // First lesson badge
                var hasFirst = await _db.Badges.AnyAsync(b => b.UserId == notif.StudentUserId && b.Name == "First Lesson");
                if (!hasFirst)
                {
                    _db.Badges.Add(new Badge { UserId = notif.StudentUserId, Name = "First Lesson" });
                }

                // Compute consecutive-day streak
                int streak = 0;
                var today = DateTime.UtcNow.Date;
                var checkDate = today;
                while (true)
                {
                    var found = await _db.UserDailyCompletions.AnyAsync(d => d.UserId == notif.StudentUserId && d.Date == checkDate);
                    if (!found) break;
                    streak++;
                    checkDate = checkDate.AddDays(-1);
                }

                var streakThresholds = new int[] { 2, 3, 5, 7, 14, 30, 45, 60, 90, 120 };
                foreach (var t in streakThresholds)
                {
                    if (streak >= t)
                    {
                        var badgeName = $"Streak {t} days";
                        var has = await _db.Badges.AnyAsync(b => b.UserId == notif.StudentUserId && b.Name == badgeName);
                        if (!has)
                        {
                            _db.Badges.Add(new Badge { UserId = notif.StudentUserId, Name = badgeName });
                        }
                    }
                }
            }

            await _db.SaveChangesAsync();

            return RedirectToPage("/Profesori/MyLectii");
        }
    }
}
