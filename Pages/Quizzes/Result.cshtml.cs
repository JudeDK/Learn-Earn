using Microsoft.AspNetCore.Mvc.RazorPages;
using Learn_Earn.Data;
using Learn_Earn.Models;
using Microsoft.AspNetCore.Mvc;

namespace Learn_Earn.Pages.Quizzes
{
    public class ResultModel : PageModel
    {
        private readonly ApplicationDbContext _db;

        public ResultModel(ApplicationDbContext db)
        {
            _db = db;
        }

        public QuizAttempt Attempt { get; set; }

        public void OnGet(int attemptId)
        {
            Attempt = _db.QuizAttempts.Find(attemptId);
        }
    }
}
