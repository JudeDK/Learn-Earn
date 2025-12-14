using Microsoft.AspNetCore.Mvc.RazorPages;
using Learn_Earn.Data;
using Learn_Earn.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Learn_Earn.Services;
using System;

namespace Learn_Earn.Pages.Profile
{
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<IdentityUser> _userManager;

        public IndexModel(ApplicationDbContext db, UserManager<IdentityUser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        public UserXp? Xp { get; set; }
        public List<Badge> Badges { get; set; } = new List<Badge>();

        // Computed level information
        public int Level { get; set; }
        public int NextLevelXp { get; set; }
        public int XpToNext { get; set; }
        public int LevelMinXp { get; set; }
        public int ProgressPercent { get; set; }

        public async Task OnGetAsync()
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId)) return;

            Xp = await _db.UserXps.FirstOrDefaultAsync(x => x.UserId == userId);
            Badges = await _db.Badges.Where(b => b.UserId == userId).ToListAsync();

            var total = Xp?.TotalXp ?? 0;
            Level = LevelService.GetLevel(total);
            NextLevelXp = LevelService.GetNextLevelXp(total);
            LevelMinXp = LevelService.GetCurrentLevelMinXp(total);
            XpToNext = NextLevelXp - total;

            var range = NextLevelXp - LevelMinXp;
            if (range <= 0) ProgressPercent = 100;
            else
            {
                var progressed = total - LevelMinXp;
                ProgressPercent = (int)Math.Round(Math.Max(0, Math.Min(100, (double)progressed / range * 100)));
            }
        }
    }
}
