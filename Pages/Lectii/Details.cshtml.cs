using Learn_Earn.Data;
using Learn_Earn.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Learn_Earn.Pages.Lectii
{
    public class DetailsModel : PageModel
    {
        private readonly ApplicationDbContext _db;

        public DetailsModel(ApplicationDbContext db)
        {
            _db = db;
        }

        public Lesson Lesson { get; set; }

        public async Task<IActionResult> OnGetAsync(int id)
        {
            Lesson = await _db.Lessons.FirstOrDefaultAsync(l => l.Id == id);
            if (Lesson == null) return NotFound();
            return Page();
        }
    }
}
