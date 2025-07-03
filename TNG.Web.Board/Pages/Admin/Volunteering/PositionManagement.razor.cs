using Microsoft.AspNetCore.Components;
using TNG.Web.Board.Data.DTOs;
using TNG.Web.Board.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;
using static System.Reflection.Metadata.BlobBuilder;

namespace TNG.Web.Board.Pages.Admin.Volunteering
{
    [Authorize(Roles = "Boardmember")]
    public partial class PositionManagement
    {
        [Inject]
        public ApplicationDbContext context { get; set; }

        protected override async Task OnInitializedAsync()
        {
            Positions = await context.VolunteerPositions.ToListAsync();
            Roles = await context.VolunteerPositionRoles.ToListAsync();
        }


        private IEnumerable<VolunteerPositionRole> Roles { get; set; }

        private IList<VolunteerPosition> Positions { get; set; }

        private string positionName { get; set; }

        private async void CreatePosition()
        {
            var pos = await context.AddAsync(new VolunteerPosition()
            {
                Name = positionName
            });
            await context.SaveChangesAsync();

            Positions.Add(pos.Entity);
            StateHasChanged();
        }

        protected void ChangeRole(ChangeEventArgs e, VolunteerPosition pos)
        {
            pos.RequiredRoleId = int.TryParse(e.Value!.ToString(), out var id) ? id : 0;
        }

        private async void SyncPositions()
        {
            foreach (var i in Enumerable.Range(0, Positions.Count))
            {
                var position = Positions[i];
                if (position.Id == 0)
                {
                    var s = await context.AddAsync(position);
                    position = s.Entity;
                }
                else
                {
                    var a = context.Attach(position);
                    a.State = EntityState.Modified;
                }
            }
            await context.SaveChangesAsync();
        }
    }
}
