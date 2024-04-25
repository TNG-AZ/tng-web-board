using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace TNG.Web.Board.Pages.Users
{
    public class ManageModel : PageModel
    {
        public readonly UserManager<IdentityUser> _userManager;

        public ManageModel(
            UserManager<IdentityUser> userManager)
        {
            _userManager = userManager;
        }

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostDeleteAsync()
        {
            var users = _userManager.Users.Where(u => !u.EmailConfirmed);
            foreach (var user in users)
            {
                var rolesForUser = await _userManager.GetRolesAsync(user);
                if (rolesForUser?.Any() ?? false)
                {
                    //do not delete if has roles
                    continue;
                }

                await _userManager.DeleteAsync(user);
            }
            return new OkResult();
        }
    }
}
