using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Learn_Earn.Data;
using Learn_Earn.Models;

namespace Learn_Earn.Pages.Quizzes
{
    public class AddQuestionModel : PageModel
    {
        private readonly ApplicationDbContext _db;

        public AddQuestionModel(ApplicationDbContext db)
        {
            _db = db;
        }

        [BindProperty]
        public int QuizId { get; set; }

        [BindProperty]
        public string QuestionText { get; set; }

        [BindProperty]
        public string Options { get; set; }

        [BindProperty]
        public int CorrectIndex { get; set; }

        public void OnGet(int quizId)
        {
            QuizId = quizId;
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid) return Page();

            var q = new QuizQuestion
            {
                QuizId = QuizId,
                QuestionText = QuestionText,
                Options = Options,
                CorrectIndex = CorrectIndex
            };
            _db.QuizQuestions.Add(q);
            await _db.SaveChangesAsync();

            return RedirectToPage("AddQuestion", new { quizId = QuizId });
        }
    }
}
