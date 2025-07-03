using Blazored.Modal.Services;
using Google.Apis.Calendar.v3.Data;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using TNG.Web.Board.Data;
using TNG.Web.Board.Data.DTOs;
using TNG.Web.Board.Services;
using TNG.Web.Board.Utilities;

namespace TNG.Web.Board.Pages.Events.Modals
{
    public partial class VolunteerModal
    {
        [Parameter]
        public string eventId { get; set; }

#nullable disable
        [Inject]
        private ApplicationDbContext context { get; set; }
        [Inject]
        public GoogleServices Google { get; set; }
        [Inject]
        public AuthUtilities auth { get; set; }
        [Inject]
        public NavigationManager nav { get; set; }
        [CascadingParameter]
        private IModalService Modal { get; set; }
#nullable enable

        protected override async Task OnInitializedAsync()
        {
            CalendarEvent ??= await Google.GetEvent(eventId);

            Slots = await context.VolunteerEventSlots
                .Include(s => s.Position)
                .Include(s => s.Position.RequiredRole)
                .Include(s => s.SlotMembers)
                .Where(s => s.EventId == eventId)
                .OrderBy(s => s.Priority)
                .ToListAsync();

            var email = await auth.GetEmail();
            Member = await context.Members.Include(m => m.VolunteerRoles).FirstOrDefaultAsync(m => m.EmailAddress == email);

            if (Member == null || !(Slots?.Any() ?? false) || CalendarEvent == null)
                nav.NavigateTo("/events");
        }

        private Member Member { get; set; }

        private Event CalendarEvent { get; set; }
        private IEnumerable<VolunteerEventSlot> Slots { get; set; }

        private async Task Volunteer(VolunteerEventSlot slot)
        {
            var e = await context.AddAsync(new VolunteerSlotMember()
            {
                MemberId = Member.Id,
                SlotId = slot.Id
            });
            await context.SaveChangesAsync();
            slot.SlotMembers.Add(e.Entity);
            StateHasChanged();
        }

        private async Task RequestRole(VolunteerPositionRole role)
        {
            var e = await context.AddAsync(new VolunteerRoleMember()
            {
                MemberId = Member.Id,
                RoleId = role.Id
            });
            await context.SaveChangesAsync();
            Member.VolunteerRoles.Add(e.Entity);
            StateHasChanged();
        }

    }
}
