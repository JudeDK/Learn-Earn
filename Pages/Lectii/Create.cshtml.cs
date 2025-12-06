using Learn_Earn.Data;
using Learn_Earn.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Learn_Earn.Pages.Lectii
{
    [Authorize(Roles = "Profesor,Admin")]
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
        public Lesson Lesson { get; set; }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var user = await _userManager.GetUserAsync(User);
            Lesson.CreatedByUserId = user?.Id;
            Lesson.CreatedByEmail = user?.Email;
            Lesson.CreatedAt = DateTime.UtcNow;

            _db.Lessons.Add(Lesson);
            await _db.SaveChangesAsync();

            return RedirectToPage("/Lectii/Index");
        }
    }
}
