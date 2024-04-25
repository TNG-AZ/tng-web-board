using Microsoft.AspNetCore.Http.HttpResults;
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
                Task.WaitAll(rolesForUser.Select(r => _userManager.RemoveFromRoleAsync(user, r)).ToArray());

                await _userManager.DeleteAsync(user);
            }
            return new OkResult();
        }
    }
}
