using Learn_Earn.Data;
using Learn_Earn.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Learn_Earn.Pages.Profesori
{
    public class ToGradeModel : PageModel
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IWebHostEnvironment _env;
        private readonly ILogger<ToGradeModel> _logger;

        public ToGradeModel(ApplicationDbContext db, UserManager<IdentityUser> userManager, IWebHostEnvironment env, ILogger<ToGradeModel> logger)
        {
            _db = db;
            _userManager = userManager;
            _env = env;
            _logger = logger;
        }

        public class PendingDto
        {
            public QuizAttempt Attempt { get; set; } = null!;
            public string StudentName { get; set; } = "";
            public string StudentEmail { get; set; } = "";
            public string QuizTitle { get; set; } = "";
            public int? LessonId { get; set; }
        }

        public List<PendingDto> PendingAttempts { get; set; } = new List<PendingDto>();

        public async Task<IActionResult> OnGetAsync()
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId)) return Challenge();
            if (!(User.IsInRole("Profesor") || User.IsInRole("Admin"))) return Forbid();

            // Load all unvalidated attempts and include quiz & lesson info
            var attempts = await _db.QuizAttempts
                .Where(a => !a.IsValidated)
                .ToListAsync();

            var quizIds = attempts.Select(a => a.QuizId).Distinct().ToList();
            var quizzes = await _db.Quizzes.Where(q => quizIds.Contains(q.Id)).ToListAsync();
            var lessonIds = quizzes.Where(q => q.LessonId.HasValue).Select(q => q.LessonId!.Value).Distinct().ToList();
            var lessons = await _db.Lessons.Where(l => lessonIds.Contains(l.Id)).ToListAsync();

            // Find professor notifications for these lessons (to see assignments targeted to this professor)
            var notifications = await _db.ProfessorNotifications.Where(n => lessonIds.Contains(n.LessonId)).ToListAsync();

            // Filter attempts to those relevant for this professor: either the lesson was created by them, or there's a notification for that lesson assigned to them (or generic)
            var relevant = attempts.Where(a =>
            {
                var quiz = quizzes.FirstOrDefault(q => q.Id == a.QuizId);
                var lesson = quiz?.LessonId != null ? lessons.FirstOrDefault(l => l.Id == quiz!.LessonId) : null;
                if (lesson == null) return false;
                if (lesson.CreatedByUserId == userId) return true;
                var hasNotif = notifications.Any(n => n.LessonId == lesson.Id && (!string.IsNullOrEmpty(n.ProfessorUserId) ? n.ProfessorUserId == userId : true) && !n.IsHandled);
                return hasNotif;
            }).ToList();

            var userIds = relevant.Select(x => x.UserId).Distinct().ToList();
            var users = await _db.Users.Where(u => userIds.Contains(u.Id)).ToListAsync();

            PendingAttempts = relevant.Select(a =>
            {
                var quiz = quizzes.FirstOrDefault(q => q.Id == a.QuizId);
                var lesson = quiz?.LessonId != null ? lessons.FirstOrDefault(l => l.Id == quiz!.LessonId) : null;
                return new PendingDto
                {
                    Attempt = a,
                    StudentName = users.FirstOrDefault(u => u.Id == a.UserId)?.UserName ?? a.UserId,
                    StudentEmail = users.FirstOrDefault(u => u.Id == a.UserId)?.Email ?? "",
                    QuizTitle = quiz?.Title ?? "",
                    LessonId = lesson?.Id
                };
            }).ToList();

            return Page();
        }

        public class GradeInput
        {
            public int AttemptId { get; set; }
            public int Score { get; set; }
            public int Total { get; set; }
            public bool Passed { get; set; }
            public string? Note { get; set; }
        }

        [BindProperty]
        public GradeInput Input { get; set; } = new GradeInput();

        public async Task<IActionResult> OnPostGradeAsync()
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId)) return Challenge();
            if (!(User.IsInRole("Profesor") || User.IsInRole("Admin"))) return Forbid();

            var attempt = await _db.QuizAttempts.FirstOrDefaultAsync(a => a.Id == Input.AttemptId);
            if (attempt == null) return NotFound();

            // Ensure professor owns the lesson for this quiz
            var quiz = await _db.Quizzes.FirstOrDefaultAsync(q => q.Id == attempt.QuizId);
            var lesson = quiz?.LessonId != null ? await _db.Lessons.FirstOrDefaultAsync(l => l.Id == quiz.LessonId) : null;
            if (lesson == null) return Forbid();

            // Allow grading if:
            //  - the current user is the creator of the lesson, OR
            //  - there is a professor notification for this lesson assigned to this user (or generic) and not yet handled
            var hasNotif = await _db.ProfessorNotifications.AnyAsync(n =>
                n.LessonId == lesson.Id &&
                (!string.IsNullOrEmpty(n.ProfessorUserId) ? n.ProfessorUserId == userId : true) &&
                !n.IsHandled);

            if (lesson.CreatedByUserId != userId && !hasNotif)
            {
                return Forbid();
            }

            // Preserve previous validated/passed state to compute increments
            var wasPreviouslyPassed = attempt.IsValidated && attempt.Passed == true;

            attempt.Score = Input.Score;
            attempt.Total = Input.Total;
            attempt.AttemptedAt = DateTime.UtcNow;
            attempt.IsValidated = true;
            attempt.ValidatedByUserId = userId;
            attempt.Passed = Input.Passed;
            attempt.ValidationNote = Input.Note;

            _db.QuizAttempts.Update(attempt);
            _logger.LogInformation("Professor {ProfessorId} graded attempt {AttemptId}: Score={Score}/{Total} Passed={Passed} Note='{Note}'", userId, attempt.Id, attempt.Score, attempt.Total, attempt.Passed, attempt.ValidationNote);

            // If passed (newly), award XP and simple XP badge
            var newlyPassed = (!wasPreviouslyPassed && Input.Passed);
            if (Input.Passed)
            {
                int xpToAdd = attempt.Score * 10;
                var ux = await _db.UserXps.FirstOrDefaultAsync(x => x.UserId == attempt.UserId);
                if (ux == null)
                {
                    ux = new UserXp { UserId = attempt.UserId, TotalXp = xpToAdd };
                    _db.UserXps.Add(ux);
                }
                else
                {
                    ux.TotalXp += xpToAdd;
                    _db.UserXps.Update(ux);
                }

                // Simple badge awarding for XP
                if ((await _db.UserXps.FirstOrDefaultAsync(x => x.UserId == attempt.UserId))?.TotalXp >= 50)
                {
                    var has = await _db.Badges.FirstOrDefaultAsync(b => b.UserId == attempt.UserId && b.Name == "50 XP");
                    if (has == null)
                    {
                        _db.Badges.Add(new Badge { UserId = attempt.UserId, Name = "50 XP" });
                    }
                }
            }

            // Manage quiz completion counts & streaks only when this is a newly passed validation
            if (newlyPassed)
            {
                var userIdOfStudent = attempt.UserId;

                // Add today's quiz completion record if missing
                var today = DateTime.UtcNow.Date;
                var existingDay = await _db.UserQuizDailyCompletions.FirstOrDefaultAsync(d => d.UserId == userIdOfStudent && d.Date == today);
                if (existingDay == null)
                {
                    _db.UserQuizDailyCompletions.Add(new UserQuizDailyCompletion { UserId = userIdOfStudent, Date = today });
                }

                // Compute total passed quiz attempts (including this one)
                var existingPassedCount = await _db.QuizAttempts.CountAsync(a => a.UserId == userIdOfStudent && a.IsValidated == true && a.Passed == true);
                var passedCount = existingPassedCount; // existingPassedCount already includes earlier validated rows; if this attempt was not previously counted, include it
                if (!wasPreviouslyPassed && Input.Passed) passedCount = existingPassedCount + 1;

                // Award cumulative badges: 1,5,50,100
                var countThresholds = new int[] { 1, 5, 50, 100 };
                foreach (var t in countThresholds)
                {
                    if (passedCount >= t)
                    {
                        var name = t == 1 ? "First Quiz" : $"{t} Quizzes Completed";
                        var has = await _db.Badges.FirstOrDefaultAsync(b => b.UserId == userIdOfStudent && b.Name == name);
                        if (has == null)
                        {
                            _db.Badges.Add(new Badge { UserId = userIdOfStudent, Name = name });
                        }
                    }
                }

                // Compute quiz streak: consecutive days with at least one passed quiz
                int streak = 0;
                var checkDate = today;
                while (true)
                {
                    var found = await _db.UserQuizDailyCompletions.AnyAsync(d => d.UserId == userIdOfStudent && d.Date == checkDate);
                    if (!found) break;
                    streak++;
                    checkDate = checkDate.AddDays(-1);
                }

                var streakThresholds = new int[] { 1, 7, 30, 60 };
                foreach (var t in streakThresholds)
                {
                    if (streak >= t)
                    {
                        var name = $"Quiz Streak {t} days";
                        var has = await _db.Badges.FirstOrDefaultAsync(b => b.UserId == userIdOfStudent && b.Name == name);
                        if (has == null)
                        {
                            _db.Badges.Add(new Badge { UserId = userIdOfStudent, Name = name });
                        }
                    }
                }
            }

            await _db.SaveChangesAsync();

            return RedirectToPage();
        }

        public async Task<IActionResult> OnGetDownloadAsync(int attemptId)
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId)) return Challenge();
            if (!(User.IsInRole("Profesor") || User.IsInRole("Admin"))) return Forbid();

            var attempt = await _db.QuizAttempts.FirstOrDefaultAsync(a => a.Id == attemptId);
            if (attempt == null || string.IsNullOrEmpty(attempt.StudentSubmissionPath)) return NotFound();

            var quiz = await _db.Quizzes.FirstOrDefaultAsync(q => q.Id == attempt.QuizId);
            var lesson = quiz?.LessonId != null ? await _db.Lessons.FirstOrDefaultAsync(l => l.Id == quiz.LessonId) : null;
            if (lesson == null || lesson.CreatedByUserId != userId) return Forbid();

            // Ensure path points under wwwroot/uploads
            var rel = attempt.StudentSubmissionPath.TrimStart('/', '\\');
            var full = Path.Combine(_env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"), rel.Replace('/', Path.DirectorySeparatorChar));
            if (!System.IO.File.Exists(full)) return NotFound();

            var contentType = attempt.StudentSubmissionContentType ?? "application/octet-stream";
            var fileName = attempt.StudentSubmissionFileName ?? Path.GetFileName(full);
            var fs = System.IO.File.OpenRead(full);
            return File(fs, contentType, fileName);
        }
    }
}
