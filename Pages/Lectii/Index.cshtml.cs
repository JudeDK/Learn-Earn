using Learn_Earn.Data;
using Learn_Earn.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace Learn_Earn.Pages.Lectii
{
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _db;

        public IndexModel(ApplicationDbContext db)
        {
            _db = db;
        }

        public IList<Lesson> Lessons { get; set; } = new List<Lesson>();

        [BindProperty(SupportsGet = true)]
        public string? Search { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? Language { get; set; }

        [BindProperty(SupportsGet = true)]
        public string? Difficulty { get; set; }

        [BindProperty(SupportsGet = true)]
        public int PageNumber { get; set; } = 1;

        [BindProperty(SupportsGet = true)]
        public string? Professor { get; set; }

        public int PageSize { get; set; } = 10;

        public int TotalPages { get; set; }

        // Distinct filter options loaded from DB
        public List<string> Languages { get; set; } = new List<string>();
        public List<string> Difficulties { get; set; } = new List<string>();
        public List<string> Professors { get; set; } = new List<string>();

        public async Task OnGetAsync()
        {
            var query = _db.Lessons.AsQueryable();

            // Prepare filter dropdown values
            Languages = await _db.Lessons.Select(l => l.Language).Where(s => !string.IsNullOrEmpty(s)).Select(s => s!).Distinct().ToListAsync();
            Difficulties = await _db.Lessons.Select(l => l.Difficulty).Where(s => !string.IsNullOrEmpty(s)).Select(s => s!).Distinct().ToListAsync();
            Professors = await _db.Lessons.Select(l => l.CreatedByEmail).Where(s => !string.IsNullOrEmpty(s)).Select(s => s!).Distinct().ToListAsync();

            // Search: case-insensitive and token-friendly — check concatenated Title + Language
            if (!string.IsNullOrWhiteSpace(Search))
            {
                var s = Search.Trim().ToLower();
                query = query.Where(l => (l.Title + " " + l.Language).ToLower().Contains(s));
            }

            if (!string.IsNullOrWhiteSpace(Language))
            {
                query = query.Where(l => l.Language == Language);
            }
            if (!string.IsNullOrWhiteSpace(Difficulty))
            {
                query = query.Where(l => l.Difficulty == Difficulty);
            }
            if (!string.IsNullOrWhiteSpace(Professor))
            {
                query = query.Where(l => l.CreatedByEmail == Professor);
            }

            // default sorting: most recent first
            query = query.OrderByDescending(l => l.CreatedAt);

            var totalCount = await query.CountAsync();
            TotalPages = (int)Math.Ceiling(totalCount / (double)PageSize);

            if (PageNumber < 1) PageNumber = 1;
            if (PageNumber > TotalPages && TotalPages > 0) PageNumber = TotalPages;

            Lessons = await query
                .Skip((PageNumber - 1) * PageSize)
                .Take(PageSize)
                .ToListAsync();
        }
    }
}
