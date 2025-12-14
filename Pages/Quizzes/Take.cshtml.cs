using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Learn_Earn.Data;
using Learn_Earn.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

namespace Learn_Earn.Pages.Quizzes
{
    public class TakeModel : PageModel
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<IdentityUser> _userManager;

        public TakeModel(ApplicationDbContext db, UserManager<IdentityUser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        public Quiz Quiz { get; set; }
        public List<QuizQuestion> Questions { get; set; }

        [BindProperty]
        public Dictionary<int, int> Answers { get; set; } = new Dictionary<int, int>();

        public async Task<IActionResult> OnGetAsync(int id)
        {
            Quiz = await _db.Quizzes.FindAsync(id);
            if (Quiz == null) return NotFound();
            Questions = await _db.QuizQuestions.Where(q => q.QuizId == id).ToListAsync();
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int id)
        {
            Quiz = await _db.Quizzes.FindAsync(id);
            if (Quiz == null) return NotFound();
            Questions = await _db.QuizQuestions.Where(q => q.QuizId == id).ToListAsync();

            int total = Questions.Count;
            int score = 0;
            foreach (var q in Questions)
            {
                if (Answers != null && Answers.TryGetValue(q.Id, out var sel))
                {
                    if (sel == q.CorrectIndex) score++;
                }
            }

            var userId = _userManager.GetUserId(User) ?? "anonymous";
            var attempt = new QuizAttempt
            {
                QuizId = id,
                UserId = userId,
                Score = score,
                Total = total
            };
            _db.QuizAttempts.Add(attempt);

            // Award XP
            int xpToAdd = score * 10;
            var ux = _db.UserXps.FirstOrDefault(x => x.UserId == userId);
            if (ux == null)
            {
                ux = new UserXp { UserId = userId, TotalXp = xpToAdd };
                _db.UserXps.Add(ux);
            }
            else
            {
                ux.TotalXp += xpToAdd;
                _db.UserXps.Update(ux);
            }

            // Simple badge awarding: award "50 XP" badge when crossing 50 XP
            if (ux.TotalXp >= 50)
            {
                var has = _db.Badges.FirstOrDefault(b => b.UserId == userId && b.Name == "50 XP");
                if (has == null)
                {
                    _db.Badges.Add(new Badge { UserId = userId, Name = "50 XP" });
                }
            }

            await _db.SaveChangesAsync();

            return RedirectToPage("/Quizzes/Result", new { attemptId = attempt.Id });
        }
    }
}
