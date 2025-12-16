using Learn_Earn.Data;
using Learn_Earn.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Learn_Earn.Pages.Student
{
    public class MyProgressModel : PageModel
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ILogger<MyProgressModel> _logger;

        public MyProgressModel(ApplicationDbContext db, UserManager<IdentityUser> userManager, ILogger<MyProgressModel> logger)
        {
            _db = db;
            _userManager = userManager;
            _logger = logger;
        }

        public List<(Lesson Lesson, LessonProgress Progress)> CompletedLessons { get; set; } = new();
        public List<QuizAttempt> QuizAttempts { get; set; } = new();
        public Dictionary<int, string> QuizTitles { get; set; } = new();
        public Dictionary<int, int?> QuizLessonIds { get; set; } = new();
        public List<Badge> Badges { get; set; } = new();

        public async Task<IActionResult> OnGetAsync()
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId)) return Challenge();

            var progresses = await _db.LessonProgresses.Where(p => p.UserId == userId && p.CompletedSections > 0).ToListAsync();
            var lessonIds = progresses.Select(p => p.LessonId).Distinct().ToList();
            var lessons = await _db.Lessons.Where(l => lessonIds.Contains(l.Id)).ToListAsync();

            CompletedLessons = progresses.Select(p =>
            {
                var lesson = lessons.FirstOrDefault(l => l.Id == p.LessonId)!;
                return (Lesson: lesson, Progress: p);
            }).OrderByDescending(x => x.Progress.UpdatedAt).ToList();

            QuizAttempts = await _db.QuizAttempts.Where(a => a.UserId == userId).OrderByDescending(a => a.AttemptedAt).ToListAsync();

            var quizIdsForAttempts = QuizAttempts.Select(a => a.QuizId).Distinct().ToList();
            var quizzes = await _db.Quizzes.Where(q => quizIdsForAttempts.Contains(q.Id)).ToListAsync();
            QuizTitles = quizzes.ToDictionary(q => q.Id, q => q.Title ?? ("Quiz " + q.Id));
            QuizLessonIds = quizzes.ToDictionary(q => q.Id, q => q.LessonId);

            Badges = await _db.Badges.Where(b => b.UserId == userId).OrderByDescending(b => b.AwardedAt).ToListAsync();
            _logger.LogInformation("Loaded MyProgress for user {UserId}: {Lessons} completed, {Attempts} attempts, {Badges} badges", userId, CompletedLessons.Count, QuizAttempts.Count, Badges.Count);

            return Page();
        }
    }
}
