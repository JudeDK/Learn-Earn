using Learn_Earn.Data;
using Learn_Earn.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Learn_Earn.Pages.Lectii
{
    [Authorize(Roles = "Profesor,Admin")]
    public class DeleteModel : PageModel
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<IdentityUser> _userManager;

        public DeleteModel(ApplicationDbContext db, UserManager<IdentityUser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        [BindProperty]
        public Lesson Lesson { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            Lesson = await _db.Lessons.FindAsync(id);
            if (Lesson == null) return NotFound();

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Forbid();
            if (Lesson.CreatedByUserId != user.Id && !User.IsInRole("Admin")) return Forbid();

            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int id)
        {
            var existing = await _db.Lessons.FindAsync(id);
            if (existing == null) return NotFound();

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Forbid();
            if (existing.CreatedByUserId != user.Id && !User.IsInRole("Admin")) return Forbid();

            _db.Lessons.Remove(existing);
            await _db.SaveChangesAsync();
            // If an admin deleted the lesson, redirect back to the global list.
            if (User.IsInRole("Admin"))
            {
                return RedirectToPage("/Lectii/Index");
            }

            return RedirectToPage("/Profesori/MyLectii");
        }
    }
}
