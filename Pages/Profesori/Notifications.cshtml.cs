using Learn_Earn.Data;
using Learn_Earn.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Learn_Earn.Pages.Profesori
{
    public class NotificationsModel : PageModel
    {
        private readonly ApplicationDbContext _db;
        private readonly UserManager<IdentityUser> _userManager;

        public NotificationsModel(ApplicationDbContext db, UserManager<IdentityUser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        public List<ProfessorNotification> Notifications { get; set; } = new List<ProfessorNotification>();

        public async Task OnGetAsync()
        {
            var userId = _userManager.GetUserId(User);
            if (string.IsNullOrEmpty(userId)) return;

            Notifications = await _db.ProfessorNotifications
                .Where(n => (n.ProfessorUserId == null || n.ProfessorUserId == userId) && !n.IsHandled)
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();
        }
    }
}
