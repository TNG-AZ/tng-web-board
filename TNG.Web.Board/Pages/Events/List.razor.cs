using Google.Apis.Calendar.v3.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using System.Dynamic;
using System.Runtime.CompilerServices;
using TNG.Web.Board.Data;
using TNG.Web.Board.Data.DTOs;
using TNG.Web.Board.Services;
using TNG.Web.Board.Utilities;

namespace TNG.Web.Board.Pages.Events
{
    [Authorize]
    public partial class List
    {
#nullable disable
        [Inject]
        private ApplicationDbContext context { get; set; }
        [Inject]
        public GoogleCalendar Google { get; set; }
        [Inject]
        public IConfiguration Configuration { get; set; }
        [Inject]
        private NavigationManager navigation { get; set; }
        [Inject]
        private AuthUtilities auth { get; set; }
#nullable enable

        private Member _member { get; set; }
        private Member? Member

        {
            get
            {
                if ((_member ??= GetMember()) is not null)
                {
                    return _member;
                }
                navigation.NavigateTo("/members/new");
                return null;
            }
        }

        private Member? GetMember()
            => context.Members.Include(m=>m.Events).FirstOrDefault(m => EF.Functions.Like(m.EmailAddress, auth.GetIdentity().Result.Name ?? string.Empty)) ;

        private string CalendarId
            => Configuration["CalendarId"] ?? throw new ArgumentNullException(nameof(CalendarId));

        private Google.Apis.Calendar.v3.Data.Events GetEvents()
            => Google.Calendar.Events.List(CalendarId).Execute();

        private IEnumerable<Event> GetUpcomingEvents()
            => GetEvents().Items
            .Where(e => e.Start?.DateTime is not null && e.Start.DateTime >= DateTime.Now.AddDays(-2) && e.Start.DateTime < DateTime.Now.AddMonths(1))
            .OrderBy(e => e.Start.DateTime);

        private async void RsvpDelete(string eventId)
        {
            var rsvp = await context.EventRsvps.FirstOrDefaultAsync(r => r.MemberId == Member!.Id && eventId == r.EventId);
            if (rsvp is null)
                return;

            context.EventRsvps.Remove(rsvp);
            await context.SaveChangesAsync();
            StateHasChanged();
        }

        private async void RsvpGoing(string eventId)
        {
            var rsvp = await context.EventRsvps.FirstOrDefaultAsync(r => r.MemberId == Member!.Id && eventId == r.EventId)
                ?? new() { EventId = eventId, MemberId = Member!.Id, Status = EventRsvpStatus.Going};
            if (rsvp.Id == default)
                context.Add(rsvp);
            else
                rsvp.Status = EventRsvpStatus.Going;

            await context.SaveChangesAsync();
            StateHasChanged();
        }

        private async void RsvpMaybeGoing(string eventId)
        {
            var rsvp = await context.EventRsvps.FirstOrDefaultAsync(r => r.MemberId == Member!.Id && eventId == r.EventId)
                ?? new() { EventId = eventId, MemberId = Member!.Id, Status = EventRsvpStatus.MaybeGoing };
            if (rsvp.Id == default)
                context.Add(rsvp);
            else
                rsvp.Status = EventRsvpStatus.MaybeGoing;

            await context.SaveChangesAsync();
            StateHasChanged();
        }
    }
}
