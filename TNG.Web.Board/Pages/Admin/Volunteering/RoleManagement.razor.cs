using Microsoft.AspNetCore.Components;
using TNG.Web.Board.Data.DTOs;
using TNG.Web.Board.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace TNG.Web.Board.Pages.Admin.Volunteering
{
    [Authorize(Roles = "Boardmember")]
    public partial class RoleManagement
    {
        [Inject]
        public ApplicationDbContext context { get; set; }

        protected override async Task OnInitializedAsync()
        {
           Roles = await context.VolunteerPositionRoles.ToListAsync();
        }

        private IList<VolunteerPositionRole> Roles { get; set; }

        private string roleName { get; set; }

        private async void CreateRole()
        {
            var pos = await context.AddAsync(new VolunteerPositionRole()
            {
                Name = roleName
            });
            await context.SaveChangesAsync();

            Roles.Add(pos.Entity);
            StateHasChanged();
        }
    }
}
