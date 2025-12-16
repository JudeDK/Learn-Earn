using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Learn_Earn.Data;
using Learn_Earn.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;

namespace Learn_Earn.Pages.Quizzes
{
    public class TakeModel : PageModel
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ILogger<TakeModel> _logger;

        public TakeModel(ApplicationDbContext db, UserManager<IdentityUser> userManager, ILogger<TakeModel> logger)
        {
            _db = db;
            _userManager = userManager;
            _logger = logger;
        }

        public Quiz Quiz { get; set; }
        public List<QuizQuestion> Questions { get; set; }

        [BindProperty]
        public Dictionary<string, int> Answers { get; set; } = new Dictionary<string, int>();

        [BindProperty]
        public IFormFile? StudentSubmission { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            Quiz = await _db.Quizzes.FindAsync(id);
            if (Quiz == null) return NotFound();
            // If quiz is targeted, only allow the target student (or professors/admins) to access it
            var currentUserId = _userManager.GetUserId(User);
            if (!string.IsNullOrEmpty(Quiz.TargetUserId) && !(User.IsInRole("Profesor") || User.IsInRole("Admin") || currentUserId == Quiz.TargetUserId))
            {
                return Forbid();
            }
            Questions = await _db.QuizQuestions.Where(q => q.QuizId == id).ToListAsync();
            // Require that student completed the lesson before taking quiz
            var userId = _userManager.GetUserId(User);
            if (!string.IsNullOrEmpty(userId) && !(User.IsInRole("Profesor") || User.IsInRole("Admin")))
            {
                if (Quiz.LessonId.HasValue)
                {
                    var prog = await _db.LessonProgresses.FirstOrDefaultAsync(p => p.LessonId == Quiz.LessonId.Value && p.UserId == userId);
                    if (prog == null || prog.CompletedSections == 0)
                    {
                        // redirect back to lesson details with a message
                        return RedirectToPage("/Lectii/Details", new { id = Quiz.LessonId.Value });
                    }
                }
            }

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int id)
        {
            try
            {
                Quiz = await _db.Quizzes.FindAsync(id);
                if (Quiz == null) return NotFound();
                // Prevent submissions by users who are not the target (unless professor/admin)
                var currentUserId = _userManager.GetUserId(User) ?? "";
                if (!string.IsNullOrEmpty(Quiz.TargetUserId) && !(User.IsInRole("Profesor") || User.IsInRole("Admin") || currentUserId == Quiz.TargetUserId))
                {
                    return Forbid();
                }
                Questions = await _db.QuizQuestions.Where(q => q.QuizId == id).ToListAsync();

                int total = Questions.Count;
                int score = 0;

                var userId = _userManager.GetUserId(User) ?? "anonymous";
                // enforce completed lesson before submission when not a professor
                if (!string.IsNullOrEmpty(userId) && !(User.IsInRole("Profesor") || User.IsInRole("Admin")))
                {
                    if (Quiz.LessonId.HasValue)
                    {
                        var prog = await _db.LessonProgresses.FirstOrDefaultAsync(p => p.LessonId == Quiz.LessonId.Value && p.UserId == userId);
                        if (prog == null || prog.CompletedSections == 0)
                        {
                            return Forbid();
                        }
                    }
                }

                if (!string.IsNullOrEmpty(Quiz?.AttachmentPath))
                {
                    // File-based quiz: save student submission and create an unvalidated attempt for professor to grade
                    if (StudentSubmission != null && StudentSubmission.Length > 0)
                    {
                        var uploads = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
                        if (!Directory.Exists(uploads)) Directory.CreateDirectory(uploads);
                        var guidName = Guid.NewGuid().ToString() + Path.GetExtension(StudentSubmission.FileName);
                        var filePath = Path.Combine(uploads, guidName);
                        using (var fs = System.IO.File.Create(filePath))
                        {
                            await StudentSubmission.CopyToAsync(fs);
                        }

                        var attemptFile = new QuizAttempt
                        {
                            QuizId = id,
                            UserId = userId,
                            Score = 0,
                            Total = 0,
                            StudentSubmissionFileName = StudentSubmission.FileName,
                            StudentSubmissionPath = "/uploads/" + guidName,
                            StudentSubmissionContentType = StudentSubmission.ContentType,
                            IsValidated = false
                        };
                        _db.QuizAttempts.Add(attemptFile);
                        _logger.LogInformation("Saved student submission for user {UserId} at path {Path} (quiz {QuizId})", userId, attemptFile.StudentSubmissionPath, id);
                        // Do not award XP here; professor will validate and award badges/xp
                        await _db.SaveChangesAsync();
                        _logger.LogInformation("Created QuizAttempt Id {AttemptId}", attemptFile.Id);
                        return RedirectToPage("/Quizzes/Result", new { attemptId = attemptFile.Id });
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, "Trebuie să încarci un fișier de răspuns pentru acest quiz.");
                        return Page();
                    }
                }
                else
                {
                    foreach (var q in Questions)
                    {
                        if (Answers != null && Answers.TryGetValue(q.Id.ToString(), out var sel))
                        {
                            if (sel == q.CorrectIndex) score++;
                        }
                    }
                }
                var attempt = new QuizAttempt
                {
                    QuizId = id,
                    UserId = userId,
                    Score = score,
                    Total = total
                };
                // If student attached a file for a non-file quiz, save it and include its metadata on the attempt
                if (StudentSubmission != null && StudentSubmission.Length > 0)
                {
                    var uploads = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads");
                    if (!Directory.Exists(uploads)) Directory.CreateDirectory(uploads);
                    var guidName = Guid.NewGuid().ToString() + Path.GetExtension(StudentSubmission.FileName);
                    var filePath = Path.Combine(uploads, guidName);
                    using (var fs = System.IO.File.Create(filePath))
                    {
                        await StudentSubmission.CopyToAsync(fs);
                    }
                    attempt.StudentSubmissionFileName = StudentSubmission.FileName;
                    attempt.StudentSubmissionPath = "/uploads/" + guidName;
                    attempt.StudentSubmissionContentType = StudentSubmission.ContentType;
                          _logger.LogInformation("Saved optional student submission for user {UserId} at path {Path} (quiz {QuizId})", userId, attempt.StudentSubmissionPath, id);
                }

                // Save attempt but leave it unvalidated so professor can grade later
                attempt.IsValidated = false;
                _db.QuizAttempts.Add(attempt);
                await _db.SaveChangesAsync();

                // Redirect back to lesson details with a friendly TempData message
                if (Quiz.LessonId.HasValue)
                {
                    TempData["QuizSubmittedMessage"] = "Quiz trimis. Nota va fi afișată în curând în secțiunea lecției.";
                    return RedirectToPage("/Lectii/Details", new { id = Quiz.LessonId.Value });
                }

                return RedirectToPage("/Quizzes/Result", new { attemptId = attempt.Id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error submitting quiz {QuizId} by user", id);
                ViewData["Error"] = ex.ToString();
                ModelState.AddModelError(string.Empty, "A intervenit o eroare la trimiterea quiz-ului. Verifică jurnalul serverului pentru detalii.");
                return Page();
            }
        }
    }
}
