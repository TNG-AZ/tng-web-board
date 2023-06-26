using Blazored.Modal;
using Blazored.Modal.Services;
using Blazored.TextEditor;
using Google.Apis.Calendar.v3.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using System.Runtime.InteropServices;
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
        [CascadingParameter] 
        private IModalService Modal { get; set; }
#nullable enable

        private Event? _event { get; set; }
        private Event CalendarEvent
            => _event ??= google.Calendar.Events.Get(configuration["CalendarId"], eventId).Execute();

        private List<EventRsvp>? _eventRsvp { get; set; }
        private IEnumerable<EventRsvp> Rsvps
            => _eventRsvp ??= context.EventRsvps
                .Include(e => e.Member)
                .Include(e => e.Member.Suspensions)
                .Include(e => e.Member.Orientations)
                .Include(e => e.Member.Payments)
                .Include(e => e.Member.Invoices)
                .Where(e => e.EventId == eventId).ToList();

        private EventFees? _eventFees { get; set; }
        private EventFees? EventFees
            => _eventFees ??= context.EventsFees.FirstOrDefault(f => f.EventId == eventId);

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

        private static MembershipIssues GetMembershipIssues(Member member)
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

        private enum EmailListEnum
        {
            All,
            GoodStanding,
            ApprovedAndPaid
        }

        BlazoredTextEditor QuillHtml;

        private EmailListEnum? EmailList { get; set; }

        private string? EmailSubject { get; set; }

        private void EmailListOnChange(ChangeEventArgs args)
        {
            EmailList = Enum.TryParse<EmailListEnum>(args.Value?.ToString(), out var value) ? value : null;
        }

        private async Task SendEmailToList()
        {
            if (EmailList is not null && Rsvps.Any())
            {
                var members = EmailList switch
                {
                    EmailListEnum.All => Rsvps.Select(r => r.Member),
                    EmailListEnum.ApprovedAndPaid => Rsvps.Where(r => (r.Approved ?? false) && (r.Paid ?? false)).Select(r => r.Member),
                    EmailListEnum.GoodStanding => Rsvps.Select(r => r.Member).Where(m => GetMembershipIssues(m).Status == null),
                    _ => Enumerable.Empty<Member>()
                };

                if (members.Any())
                {
                    var emails = members.Select(m => m.EmailAddress);
                    var content = await QuillHtml.GetHTML();
                    await google.EmailListAsync(emails, EmailSubject ?? "New message about an event you are attending", content);

                    EmailList = null;
                    EmailSubject = null;
                    await QuillHtml.LoadHTMLContent("Message has been sent");
                    StateHasChanged();
                }
            }
        }

        private void ShowInvoiceModal(Member invoiceMember)
        {
            var parameters = new ModalParameters()
                .Add(nameof(EventInvoice.InvoiceMember), invoiceMember)
                .Add(nameof(EventInvoice.CalendarEvent), CalendarEvent)
                .Add(nameof(EventInvoice.Fees), EventFees ?? new() { MembershipDues = 12, GuestEntry = 10, MemberEntry = 8});
            var options = new ModalOptions()
            {
                Class = "blazored-modal size-large"
            };
            Modal.Show<EventInvoice>("Create Invoice", parameters, options);
        }

        private void ShowNotesModal(EventRsvp rsvp)
        {
            var parameters = new ModalParameters()
                .Add(nameof(RSVPNotes.Rsvp), rsvp);
            var options = new ModalOptions()
            {
                Class = "blazored-modal size-large"
            };
            Modal.Show<RSVPNotes>("Add Notes", parameters, options);
        }

        private async Task CreateDefaultFees()
        {
            var fees = new EventFees()
            {
                EventId = eventId,
                MemberEntry = 8,
                GuestEntry = 10,
                MembershipDues = 12
            };
            await context.EventsFees.AddAsync(fees);
            await context.SaveChangesAsync();
        }
    }
}
