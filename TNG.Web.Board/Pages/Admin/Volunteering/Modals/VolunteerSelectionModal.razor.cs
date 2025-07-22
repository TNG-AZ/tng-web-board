using Blazored.Modal.Services;
using Google.Apis.Calendar.v3.Data;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using Microsoft.JSInterop;
using System.Linq;
using System.Web;
using TNG.Web.Board.Data;
using TNG.Web.Board.Data.DTOs;
using TNG.Web.Board.Services;
using TNG.Web.Board.Utilities;

namespace TNG.Web.Board.Pages.Admin.Volunteering.Modals
{
    public partial class VolunteerSelectionModal
    {
        [Parameter]
        public string eventId { get; set; }

        [Parameter]
        public bool editMode { get; set; } = true;

#nullable disable
        [Inject]
        private ApplicationDbContext context { get; set; }
        [Inject]
        public GoogleServices Google { get; set; }
        [Inject]
        public AuthUtilities auth { get; set; }
        [Inject]
        public NavigationManager nav { get; set; }
        [Inject]
        private IJSRuntime jsRuntime { get; set; }
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
                .ThenInclude(s => s.Member)
                .ThenInclude(m => m.Suspensions)
                .Where(s => s.EventId == eventId)
                .OrderBy(s => s.Priority)
                .ToListAsync();

            foreach (var s in Slots)
            {
                AddToggle[s.Id] = false;
            }

            var email = await auth.GetEmail();

            RSVPMembers = await context.EventRsvps
                .Include(r => r.Member)
                .Include(r => r.Member.VolunteerRoles)
                .Include(r => r.Member.Suspensions)
                .Where(r => r.EventId == eventId)
                .Select(r => r.Member)
                .ToListAsync();

            if (!(Slots?.Any() ?? false) || CalendarEvent == null)
                nav.NavigateTo("/events");
        }

        private Event CalendarEvent { get; set; }
        private IEnumerable<VolunteerEventSlot> Slots { get; set; }

        private Dictionary<int, bool> AddToggle { get; set; } = new Dictionary<int, bool>();

        private void ToggleAdd(int id)
        {
            AddToggle[id] = !AddToggle[id];
            StateHasChanged();
        }

        private IEnumerable<Member> RSVPMembers { get; set; }

        private enum IssueLevel
        {
            Okay,
            Warning,
            Danger
        }

        private class MemberIssues
        {
            public List<string> Issues { get; set; } = new();
            public IssueLevel Level { get; set; }
        }

        private MemberIssues GetIssues(VolunteerSlotMember member)
        {
            var result = new MemberIssues()
            {
                Level = IssueLevel.Okay
            };
            var roleId = member.Slot.Position.RequiredRoleId;
            if (roleId != null)
            {
                if (!member.Member.VolunteerRoles.Any(r => r.RoleId == roleId))
                {
                    result.Issues.Add("Does not have needed role");
                    result.Level = IssueLevel.Warning;
                }
            }
            if (!member.Member.HasAttendedSocial)
            {
                result.Issues.Add("Has not attended a social");
                result.Level = IssueLevel.Warning;
            }
            if (member.Member.Suspensions.Any(s => s.EndDate == null || s.EndDate > DateTime.Now))
            {
                result.Issues.Add("Member is suspended or banned");
                result.Level = IssueLevel.Danger;
            }
            return result;
        }

        private async Task ToggleApproval(VolunteerSlotMember m)
        {
            var e = context.Attach(m);
            if (m.Approval ?? false)
                m.Approval = false;
            else
                m.Approval = true;
            e.State = EntityState.Modified;
            await context.SaveChangesAsync();
            StateHasChanged();
        }

        private async Task AddToSlot(VolunteerEventSlot slot, Member member)
        {
            if (slot.SlotMembers.Any(m => m.Member.Id == member.Id))
            {
                Alert("Already volunteered");
                return;
            }
            var e = await context.AddAsync(new VolunteerSlotMember()
            {
                SlotId = slot.Id,
                MemberId = member.Id
            });
            await context.SaveChangesAsync();
            slot.SlotMembers.Add(e.Entity);
            StateHasChanged();
        }

        private void Alert(string alert)
            => jsRuntime.InvokeVoidAsync("alert", alert);
    }
}
