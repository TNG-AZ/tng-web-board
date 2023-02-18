using Google.Apis.Calendar.v3.Data;
using Microsoft.AspNetCore.Components;
using TNG.Web.Board.Services;

namespace TNG.Web.Board.Pages.Events
{
    public partial class List
    {
#nullable disable
        [Inject]
        public GoogleCalendar Google { get; set; }
        [Inject]
        public IConfiguration Configuration { get; set; }
#nullable enable

        private string CalendarId
            => Configuration["CalendarId"] ?? throw new ArgumentNullException(nameof(CalendarId));

        private Google.Apis.Calendar.v3.Data.Events GetEvents()
            => Google.Calendar.Events.List(CalendarId).Execute();

        private IEnumerable<Event> GetUpcomingEvents()
            => GetEvents().Items
            .Where(e => e.Start?.DateTime is not null && e.Start.DateTime >= DateTime.Now.AddDays(-2) && e.Start.DateTime < DateTime.Now.AddMonths(1))
            .OrderBy(e => e.Start.DateTime);
    }
}
