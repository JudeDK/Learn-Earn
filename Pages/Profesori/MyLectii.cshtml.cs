using Learn_Earn.Data;
using Learn_Earn.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Learn_Earn.Pages.Profesori
{
    [Authorize(Roles = "Profesor,Admin")]
    public class MyLectiiModel : PageModel
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<IdentityUser> _userManager;

        public MyLectiiModel(ApplicationDbContext db, UserManager<IdentityUser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        public IList<Lesson> Lessons { get; set; } = new List<Lesson>();

        public async Task OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return;
            Lessons = await _db.Lessons.Where(l => l.CreatedByUserId == user.Id).OrderByDescending(l => l.CreatedAt).ToListAsync();
        }
    }
}
