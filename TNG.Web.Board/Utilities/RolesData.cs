using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TNG.Web.Board.Data;

namespace TNG.Web.Board.Utilities
{
    public static class RolesData
    {
        private static readonly string[] Roles = Enum.GetNames<RolesEnum>();

        public static async Task SeedRoles(IServiceProvider serviceProvider)
        {
            using var serviceScope = serviceProvider.GetRequiredService<IServiceScopeFactory>().CreateScope();
            var roleManager = serviceScope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            foreach (var role in Roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }
        }
    }
}
