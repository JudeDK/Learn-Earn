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
        private readonly ILogger<DetailsModel> _logger;

        public DetailsModel(ApplicationDbContext db, UserManager<IdentityUser> userManager, ILogger<DetailsModel> logger)
        {
            _db = db;
            _userManager = userManager;
            _logger = logger;
        }

        public Lesson Lesson { get; set; }
        public LessonProgress? Progress { get; set; }
        // Professor review context
        public ProfessorNotification? NotificationForProfessor { get; set; }
        public Learn_Earn.Models.Quiz? QuizForLesson { get; set; }
        public List<QuizQuestion> QuizQuestions { get; set; } = new List<QuizQuestion>();
        public IdentityUser? StudentForReview { get; set; }
        public QuizAttempt? UserAttempt { get; set; }


        public async Task<IActionResult> OnGetAsync(int id, int? notificationId)
        {
            try
            {
                Lesson = await _db.Lessons.FirstOrDefaultAsync(l => l.Id == id);
                if (Lesson == null) return NotFound();

                var userId = _userManager.GetUserId(User);
                if (!string.IsNullOrEmpty(userId))
                {
                    Progress = await _db.LessonProgresses.FirstOrDefaultAsync(p => p.LessonId == id && p.UserId == userId);
                }

                // Load a quiz for this lesson.
                // Professors/admins may see any quiz; students only see quizzes that are not targeted or targeted to them.
                var currentUserId = _userManager.GetUserId(User);
                if (User.IsInRole("Profesor") || User.IsInRole("Admin"))
                {
                    QuizForLesson = await _db.Quizzes.FirstOrDefaultAsync(q => q.LessonId == id);
                }
                else
                {
                    QuizForLesson = await _db.Quizzes.FirstOrDefaultAsync(q => q.LessonId == id && (q.TargetUserId == null || q.TargetUserId == currentUserId));
                }
                if (QuizForLesson != null)
                {
                    QuizQuestions = await _db.QuizQuestions.Where(q => q.QuizId == QuizForLesson.Id).ToListAsync();
                    // If a logged-in user, load their validated attempt for display
                    if (!string.IsNullOrEmpty(userId))
                    {
                        UserAttempt = await _db.QuizAttempts
                            .Where(a => a.QuizId == QuizForLesson.Id && a.UserId == userId && a.IsValidated == true)
                            .OrderByDescending(a => a.AttemptedAt)
                            .FirstOrDefaultAsync();
                        _logger.LogInformation("Loaded UserAttempt for user {UserId}, lesson {LessonId}: AttemptId={AttemptId}, Note='{Note}'", userId, id, UserAttempt?.Id, UserAttempt?.ValidationNote);
                    }
                }

                // If a professor opens this lesson with a notificationId, load the review context
                if (notificationId.HasValue && (User.IsInRole("Profesor") || User.IsInRole("Admin")))
                {
                    NotificationForProfessor = await _db.ProfessorNotifications.FirstOrDefaultAsync(n => n.Id == notificationId.Value && n.LessonId == id);
                    if (NotificationForProfessor != null)
                    {
                        StudentForReview = await _userManager.FindByIdAsync(NotificationForProfessor.StudentUserId);
                        // QuizForLesson/QuizQuestions already loaded above
                    }
                }

                return Page();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading lesson details for id {LessonId} (notification {NotificationId})", id, notificationId);
                // show a friendly error message on the page
                ViewData["ErrorMessage"] = "A intervenit o eroare la încărcarea paginii. Verifică jurnalul serverului pentru detalii.";
                return Page();
            }
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

            // Prepare a friendly toast message about XP gain
            TempData["XpToastMessage"] = $"You earned {xpToAward} XP for completing this lesson.";
            TempData["XpToastKind"] = "lesson";

            return RedirectToPage(new { id });
        }

        public class ProfessorValidationInput
        {
            public int NotificationId { get; set; }
            public int Score { get; set; }
            public int Total { get; set; }
            public bool Passed { get; set; }
            public string? Note { get; set; }
        }

        [BindProperty]
        public ProfessorValidationInput ValidationInput { get; set; }

        public async Task<IActionResult> OnPostValidateAttemptAsync()
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId)) return Challenge();
            if (!(User.IsInRole("Profesor") || User.IsInRole("Admin"))) return Forbid();

            var notif = await _db.ProfessorNotifications.FirstOrDefaultAsync(n => n.Id == ValidationInput.NotificationId);
            if (notif == null) return NotFound();

            var quiz = await _db.Quizzes.FirstOrDefaultAsync(q => q.LessonId == notif.LessonId);
            if (quiz == null)
            {
                notif.IsHandled = true;
                _db.ProfessorNotifications.Update(notif);
                await _db.SaveChangesAsync();
                return RedirectToPage(new { id = notif.LessonId });
            }

            var attempt = await _db.QuizAttempts.FirstOrDefaultAsync(a => a.QuizId == quiz.Id && a.UserId == notif.StudentUserId);
            if (attempt == null)
            {
                attempt = new QuizAttempt
                {
                    QuizId = quiz.Id,
                    UserId = notif.StudentUserId,
                    Score = ValidationInput.Score,
                    Total = ValidationInput.Total,
                    AttemptedAt = DateTime.UtcNow,
                    IsValidated = true,
                    ValidatedByUserId = userId,
                    Passed = ValidationInput.Passed,
                    ValidationNote = ValidationInput.Note
                };
                _db.QuizAttempts.Add(attempt);
            }
            else
            {
                attempt.Score = ValidationInput.Score;
                attempt.Total = ValidationInput.Total;
                attempt.AttemptedAt = DateTime.UtcNow;
                attempt.IsValidated = true;
                attempt.ValidatedByUserId = userId;
                attempt.Passed = ValidationInput.Passed;
                attempt.ValidationNote = ValidationInput.Note;
                _db.QuizAttempts.Update(attempt);
            }

            notif.IsHandled = true;
            _db.ProfessorNotifications.Update(notif);

            if (ValidationInput.Passed)
            {
                // Award first lesson badge (existing behaviour)
                var hasFirst = await _db.Badges.AnyAsync(b => b.UserId == notif.StudentUserId && b.Name == "First Lesson");
                if (!hasFirst)
                {
                    _db.Badges.Add(new Badge { UserId = notif.StudentUserId, Name = "First Lesson" });
                }

                // Additionally, manage quiz completion counts and quiz streaks when professor validates as passed
                var userIdOfStudent = notif.StudentUserId;
                var today = DateTime.UtcNow.Date;

                // Add today's quiz completion record if missing
                var existingDay = await _db.UserQuizDailyCompletions.FirstOrDefaultAsync(d => d.UserId == userIdOfStudent && d.Date == today);
                if (existingDay == null)
                {
                    _db.UserQuizDailyCompletions.Add(new UserQuizDailyCompletion { UserId = userIdOfStudent, Date = today });
                }

                // Compute passed quiz count (validated) and award cumulative badges
                var passedCount = await _db.QuizAttempts.CountAsync(a => a.UserId == userIdOfStudent && a.IsValidated == true && a.Passed == true);
                // If this validate action creates the first record for this quiz attempt (attempt may be newly created above), include it
                // (We already set attempt.IsValidated = true above before saving.)

                var countThresholds = new int[] { 1, 5, 50, 100 };
                foreach (var t in countThresholds)
                {
                    if (passedCount >= t)
                    {
                        var name = t == 1 ? "First Quiz" : $"{t} Quizzes Completed";
                        var has = await _db.Badges.AnyAsync(b => b.UserId == userIdOfStudent && b.Name == name);
                        if (!has)
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
                        var badgeName = $"Quiz Streak {t} days";
                        var has = await _db.Badges.AnyAsync(b => b.UserId == userIdOfStudent && b.Name == badgeName);
                        if (!has)
                        {
                            _db.Badges.Add(new Badge { UserId = userIdOfStudent, Name = badgeName });
                        }
                    }
                }
            }

            await _db.SaveChangesAsync();

            return RedirectToPage(new { id = notif.LessonId });
        }
    }
}
