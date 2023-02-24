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
        private readonly UserManager<IdentityUser> _userManager;

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
                if (rolesForUser?.Any() ?? false)
                {
                    foreach (var item in rolesForUser.ToList())
                    {
                        await _userManager.RemoveFromRoleAsync(user, item);
                    }
                }

                await _userManager.DeleteAsync(user);
            }
            return Redirect("/users");
        }
    }
}
