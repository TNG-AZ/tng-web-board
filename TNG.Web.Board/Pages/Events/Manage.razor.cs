using Google.Apis.Calendar.v3.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using TNG.Web.Board.Data;
using TNG.Web.Board.Data.DTOs;
using TNG.Web.Board.Services;

namespace TNG.Web.Board.Pages.Events
{
    [Authorize(Roles = "Boardmember")]
    public partial class Manage
    {
        [Parameter]
        public string? eventId { get; set; }

#nullable disable
        [Inject]
        private ApplicationDbContext context { get; set; }
        [Inject]
        private GoogleServices google { get; set; }
        [Inject]
        private IConfiguration configuration { get; set; }
#nullable enable

        private Event? _event { get; set; }
        private Event CalendarEvent
            => _event ??= google.Calendar.Events.Get(configuration["CalendarId"], eventId).Execute();

        private IEnumerable<EventRsvp> Rsvps
            => context.EventRsvps
                .Include(e => e.Member)
                .Include(e => e.Member.Suspensions)
                .Include(e => e.Member.Orientations)
                .Include(e => e.Member.Payments)
                .Where(e => e.EventId == eventId);

        private enum IssuesStatus
        {
            Warning,
            Danger
        }

        private class MembershipIssues
        {
            public IssuesStatus? Status { get; set; }
            public List<string> Issues { get; set; } = new();
        }

        private MembershipIssues GetMembershipIssues(Member member)
        {
            var status = new MembershipIssues();

            //DANGER
            if ((member.Suspensions?.Any() ?? false) && (member.Suspensions.Max(s => s.EndDate ?? DateTime.MaxValue) > DateTime.Now))
                status.Issues.Add("Member is suspended or blacklisted.");

            if (member.MemberType == MemberType.Member
                && (member.Birthday < DateTime.Now.AddYears(-40) || member.Birthday > DateTime.Now.AddYears(-18)))
                status.Issues.Add("Member is not age elibigle.");

            if (status.Issues.Any())
                status.Status = IssuesStatus.Danger;


            //Warning
            if (!member.HasAttendedSocial)
                status.Issues.Add("Member has not attended a social or educational.");

            if ((!(member.Payments?.Any() ?? false)) || (member.Payments!.Max(p => p.PaidOn) < DateTime.Now.AddYears(-1)))
                status.Issues.Add("Member has not paid dues in the last year.");

            if ((!(member.Orientations?.Any() ?? false)) || (member.Orientations!.Max(o => o.DateReceived) < DateTime.Now.AddYears(-1)))
                status.Issues.Add("Member has not attended orientation in the last year.");

            if (status.Issues.Any())
                status.Status ??= IssuesStatus.Warning;

            return status;
        }

        private static string GetRsvpStatus(EventRsvp rsvp, MembershipIssues issues)
            => (rsvp, issues) switch
            {
                ({ Attended: true }, _) => "table-primary",
                (_, { Status: IssuesStatus.Danger }) => "table-danger",
                (_, { Status: IssuesStatus.Warning }) => "table-warning",
                ({ Paid: false }, _) => "table-warning",
                ({ Approved: false }, _) => "table-warning",
                ({ Member.ReceivedProofOfCovid19Vaccination: false}, _) => "table-warning",
                _ => "table-success"
            };

        private async Task TogglePaid(EventRsvp rsvp)
        {
            rsvp.Paid = !(rsvp?.Paid ?? false);
            await context.SaveChangesAsync();
            StateHasChanged();
        }
        private async Task ToggleAttended(EventRsvp rsvp)
        {
            rsvp.Attended = !(rsvp?.Attended ?? false);
            await context.SaveChangesAsync();
            StateHasChanged();
        }
        private async Task ToggleApproved(EventRsvp rsvp)
        {
            rsvp.Approved = !(rsvp?.Approved ?? false);
            await context.SaveChangesAsync();
            StateHasChanged();
        }
    }
}
