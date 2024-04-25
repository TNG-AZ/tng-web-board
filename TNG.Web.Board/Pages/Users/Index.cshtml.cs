using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using TNG.Web.Board.Data;

namespace TNG.Web.Board.Pages.Users
{
    public partial class ListUsers : PageModel
    {
       public readonly UserManager<IdentityUser> _userManager;

        public ListUsers(
            UserManager<IdentityUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<string> GetRoles(IdentityUser user)
            => string.Join(", ", await _userManager.GetRolesAsync(user));

        public async Task<IActionResult> OnPostDeleteAsync(string userId)
        {
            var user = await _userManager.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user is not null)
            {
                var rolesForUser = await _userManager.GetRolesAsync(user);
                Task.WaitAll(rolesForUser.Select(r => _userManager.RemoveFromRoleAsync(user, r)).ToArray());

                await _userManager.DeleteAsync(user);
            }
            return Redirect("/users");
        }

        public async Task<IActionResult> OnPostRoleChangeAsync(string userId, string role)
        {
            var user = await _userManager.Users.FirstOrDefaultAsync(u => u.Id == userId);
            if (user is not null)
            {
                var rolesForUser = await _userManager.GetRolesAsync(user);
                if ((rolesForUser?.Any(r => r.Equals(role, StringComparison.OrdinalIgnoreCase)) ?? false))
                {
                    await _userManager.RemoveFromRoleAsync(user, role);
                }
                else
                {
                    await _userManager.AddToRoleAsync(user, role);
                }
            }
            return Redirect("/users");
        }

        public async Task<IActionResult> OnPostToggleAdminAsync(string userId)
            => await OnPostRoleChangeAsync(userId, "Administrator");
        public async Task<IActionResult> OnPostToggleBoardAsync(string userId)
           => await OnPostRoleChangeAsync(userId, "Boardmember");
        public async Task<IActionResult> OnPostToggleAmbassadorAsync(string userId)
           => await OnPostRoleChangeAsync(userId, "Ambassador");
    }
}
