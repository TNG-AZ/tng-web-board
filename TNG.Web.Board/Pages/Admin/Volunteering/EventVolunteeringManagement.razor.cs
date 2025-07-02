using Google.Apis.Calendar.v3.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using TNG.Web.Board.Data;
using TNG.Web.Board.Data.DTOs;
using TNG.Web.Board.Services;

namespace TNG.Web.Board.Pages.Admin.Volunteering
{
    [Authorize(Roles = "Boardmember")]
    public partial class EventVolunteeringManagement
    {
        [Parameter]
        public string? eventId { get; set; }


#nullable disable
        [Inject]
        private ApplicationDbContext context { get; set; }
        [Inject]
        public GoogleServices Google { get; set; }
        [Inject]
        public IConfiguration Configuration { get; set; }
        [Inject]
        private NavigationManager navigation { get; set; }
#nullable disable

        protected override async Task OnInitializedAsync()
        {
            Positions = await context.VolunteerPositions.ToListAsync();
            Slots = await context.VolunteerEventSlots
                .Include(s => s.Position)
                .Include(s => s.SlotMembers)
                .Where(s => s.EventId == eventId)
                .ToListAsync();
        }

        private Event? _calendarEvent { get; set; }
        private Event? CalendarEvent
        {
            get
            {
                if (_calendarEvent is not null)
                    return _calendarEvent;
                if (string.IsNullOrEmpty(eventId))
                {
                    navigation.NavigateTo("/calendar/");
                    return default;
                }
                return _calendarEvent ??= Google.GetEvent(eventId)?.Result;
            }
        }

        private IEnumerable<VolunteerPosition> Positions { get; set; }

        private int SelectedPositionId { get; set; }

        protected void PositionChange(ChangeEventArgs e)
        {
            SelectedPositionId = int.TryParse(e.Value!.ToString(), out var id) ? id : 0;
        }

        private IList<VolunteerEventSlot> Slots { get; set; }

        private async Task AddSlot(int positionId)
        {
            var pos = await context.VolunteerPositions.FirstOrDefaultAsync(p => p.Id  == positionId);
            if (pos != null)
                Slots.Add(new() { EventId = eventId, PositionId = positionId, Position = pos });
        }

        private async void SyncSlots()
        {
            foreach (var i in Enumerable.Range(0, Slots.Count))
            {
                var slot = Slots[i];
                if (slot.Id == 0)
                {
                    var s = await context.AddAsync(slot);
                    slot = s.Entity;
                }
                else
                {
                    var a = context.Attach(slot);
                    a.State = EntityState.Modified;
                } 
            }
            await context.SaveChangesAsync();

            Slots = Slots.OrderBy(s => s.Priority).ToList();
            StateHasChanged();
        }
    }
}
