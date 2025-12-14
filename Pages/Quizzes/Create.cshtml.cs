using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Learn_Earn.Data;
using Learn_Earn.Models;
using Microsoft.AspNetCore.Identity;

namespace Learn_Earn.Pages.Quizzes
{
    public class CreateModel : PageModel
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<IdentityUser> _userManager;

        public CreateModel(ApplicationDbContext db, UserManager<IdentityUser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        [BindProperty]
        public Quiz Quiz { get; set; }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid) return Page();

            var userId = _userManager.GetUserId(User);
            Quiz.CreatedByUserId = userId;
            _db.Quizzes.Add(Quiz);
            await _db.SaveChangesAsync();

            return RedirectToPage("AddQuestion", new { quizId = Quiz.Id });
        }
    }
}
