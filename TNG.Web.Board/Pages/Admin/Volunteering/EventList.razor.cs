using Blazored.Modal;
using Blazored.Modal.Services;
using Google.Apis.Calendar.v3.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.EntityFrameworkCore;
using TNG.Web.Board.Data;
using TNG.Web.Board.Pages.Admin.Volunteering.Modals;
using TNG.Web.Board.Pages.Events.Modals;
using TNG.Web.Board.Services;
using TNG.Web.Board.Utilities;

namespace TNG.Web.Board.Pages.Admin.Volunteering
{
    public partial class EventList
    {
        const int DaysCalenderPreOffset = -2;
        const int MonthsCalendayPostOffset = 2;
#nullable disable
        [Inject]
        private ApplicationDbContext context { get; set; }
        [Inject]
        public GoogleServices Google { get; set; }
        [Inject]
        public IConfiguration Configuration { get; set; }
        [Inject]
        private NavigationManager navigation { get; set; }
        [Inject]
        private AuthUtilities auth { get; set; }
        [CascadingParameter]
        private IModalService Modal { get; set; }
#nullable enable

        protected override async Task OnInitializedAsync()
        {
            CalendarEvents ??= (await Google.GetEvents(CalendarStartDate, CalendarEndDate))
            .OrderBy(e => e.Start.DateTime);
        }

        private async Task UpdateCalendarEvents()
        {
            CalendarEvents = (await Google.GetEvents(CalendarStartDate, CalendarEndDate))
            .OrderBy(e => e.Start.DateTime);
        }


        private DateTime CalendarStartDate { get; set; } = DateTime.Now.AddDays(DaysCalenderPreOffset);
        private DateTime CalendarEndDate { get; set; } = DateTime.Now.AddMonths(MonthsCalendayPostOffset);

        private IEnumerable<Event>? CalendarEvents { get; set; }

        private void ShowVolunteerModal(string eventId)
        {
            var parameters = new ModalParameters()
                .Add(nameof(VolunteerSelectionModal.eventId), eventId);
            var options = new ModalOptions()
            {
                Class = "blazored-modal size-large"
            };
            Modal.Show<VolunteerSelectionModal>("Volunteer Selection", parameters, options);
        }
    }
}
