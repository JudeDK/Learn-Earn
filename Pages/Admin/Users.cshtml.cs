using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Learn_Earn.Pages.Admin
{
    [Authorize(Roles = "Admin")]
    public class UsersModel : PageModel
    {
        private readonly UserManager<IdentityUser> _userManager;

        public UsersModel(UserManager<IdentityUser> userManager)
        {
            _userManager = userManager;
        }

        public record UserRow(string Id, string Email, string Roles);

        public List<UserRow> Users { get; set; } = new List<UserRow>();

        public async Task OnGetAsync()
        {
            var all = _userManager.Users.ToList();
            foreach (var u in all)
            {
                var roles = await _userManager.GetRolesAsync(u);
                Users.Add(new UserRow(u.Id, u.Email ?? "", string.Join(", ", roles)));
            }
        }

        public async Task<IActionResult> OnPostSetRoleAsync(string id, string role)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return NotFound();

            var current = await _userManager.GetRolesAsync(user);
            // remove all custom roles except keep data
            foreach (var r in current)
            {
                await _userManager.RemoveFromRoleAsync(user, r);
            }
            // assign requested role (and ensure user also in 'User' base role)
            if (!string.IsNullOrEmpty(role) && role != "User")
            {
                await _userManager.AddToRoleAsync(user, role);
            }
            // always ensure base User role
            if (!await _userManager.IsInRoleAsync(user, "User"))
            {
                await _userManager.AddToRoleAsync(user, "User");
            }

            return RedirectToPage();
        }
    }
}
