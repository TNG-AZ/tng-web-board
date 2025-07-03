using Google.Apis.Calendar.v3.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
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

            await GetCloneEvents();
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

        private async Task DeleteSlot(VolunteerEventSlot s)
        {
            context.RemoveRange(context.VolunteerSlotMembers.Where(m => m.SlotId == s.Id));
            await context.SaveChangesAsync();
            context.Remove(s);
            await context.SaveChangesAsync();
            Slots.Remove(s);
            StateHasChanged();
        }


        const int DaysCalenderPreOffset = -2;
        const int MonthsCalendayPostOffset = 2;

        private DateTime CloneStartDate { get; set; } = DateTime.Now.AddDays(DaysCalenderPreOffset);
        private DateTime CloneEndDate { get; set; } = DateTime.Now.AddMonths(MonthsCalendayPostOffset);

        private IEnumerable<Event> CloneEvents { get; set; } = Enumerable.Empty<Event>();

        private string CloneEventId { get; set; }

        private async Task GetCloneEvents()
        {
            var events = await Google.GetEvents(CloneStartDate, CloneEndDate);
            var cloneEvents = new List<Event>();
            foreach(var e in events)
            {
                if (await context.VolunteerEventSlots.AnyAsync(s => s.EventId == e.Id))
                    cloneEvents.Add(e);
            }
            CloneEvents = cloneEvents;
        }

        private async Task CloneEventSlots()
        {
            var slots = await context.VolunteerEventSlots.Where(s => s.EventId == CloneEventId).ToListAsync();
            foreach(var s in slots)
            {
                s.Id = 0;
                s.EventId = eventId;
                var e = await context.AddAsync(s);
                Slots.Add(e.Entity);
            }
            await context.SaveChangesAsync();
            StateHasChanged();
        }
    }
}
