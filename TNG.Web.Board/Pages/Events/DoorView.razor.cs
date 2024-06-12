using Blazored.Modal;
using Blazored.Modal.Services;
using Google.Apis.Calendar.v3.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using Microsoft.JSInterop;
using TNG.Web.Board.Data;
using TNG.Web.Board.Data.DTOs;
using TNG.Web.Board.Pages.Membership;
using TNG.Web.Board.Services;

namespace TNG.Web.Board.Pages.Events
{
    [Authorize(Roles = "Boardmember,Ambassador")]
    public partial class DoorView
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
        [Inject]
        private IJSRuntime js { get; set; }
        [CascadingParameter]
        private IModalService Modal { get; set; }
#nullable enable

        private Event? _event { get; set; }
        private Event? CalendarEvent
            => _event ??= google.GetEvent(configuration["CalendarId"]!, eventId!)?.Result;

        private List<EventRsvp>? _eventRsvp { get; set; }
        private IEnumerable<EventRsvp> Rsvps
            => _eventRsvp ??= context.EventRsvps
                .Include(e => e.Member)
                .Include(e => e.Member.Suspensions)
                .Include(e => e.Member.Orientations)
                .Include(e => e.Member.Payments)
                .Include(e => e.Member.Invoices)
                .Where(e => e.EventId == eventId 
                    && (e.Approved ?? false) 
                    && ((e.Paid ?? false) || e.Member.Invoices.Any(p => p.EventId == eventId && p.PaidOnDate != null))
                )
                .OrderBy(e => e.Member.SceneName).ToList();

        private IEnumerable<EventRsvp> CheckedInRsvps
            => Rsvps.Where(r => r.Attended ?? false);
        private IEnumerable<EventRsvp> ToCheckInRsvps
            => Rsvps.Where(r => !(r.Attended ?? false));

        private IEnumerable<EventRsvp> GetAllRsvps()
        {
            foreach (var r in ToCheckInRsvps)
            {
                yield return r;
            }
            foreach(var r in CheckedInRsvps)
            {
                yield return r;
            }
        }

        private IEnumerable<EventRsvpPlusOne>? _plusOnes { get; set; }
        private IEnumerable<EventRsvpPlusOne> PlusOnes
            => _plusOnes ??= context.EventRsvpPlusOnes
                .Include(p => p.Member)
                .Include(p => p.PlusOne)
                .Include(p => p.PlusOne.Suspensions)
                .Include(p => p.PlusOne.Orientations)
                .Include(p => p.PlusOne.Payments)
                .Where(p => p.EventId == eventId
                    && Rsvps.Select(r => r.MemberId).Contains(p.MemberId)
                ).ToList();

        private EventFees? _eventFees { get; set; }
        private EventFees? EventFees
            => _eventFees ??= context.EventsFees.FirstOrDefault(f => f.EventId == eventId);

        private IList<Signature>? _signatures { get; set; }
        private IEnumerable<Signature> Signatures
            => _signatures ??= context.Signatures
                .Include(s => s.Member)
                .Where(s => s.EventId == eventId).ToList();

        private HashSet<Guid> ExpandedPlusOnes = new();

        private bool ToggleExpand(Guid rsvpId)
            => ExpandedPlusOnes.Contains(rsvpId)
                ? ExpandedPlusOnes.Remove(rsvpId)
                : ExpandedPlusOnes.Add(rsvpId);

        private async Task GetSignature(Guid sigId)
        {
            var signature = Signatures.FirstOrDefault(s => s.Id == sigId);
            if (signature is not null)
            {
                using var streamRef = new DotNetStreamReference(stream: new MemoryStream(signature.SignedForm));

                await js.InvokeVoidAsync("downloadFileFromStream", $"liabilityForm-{eventId}-{signature!.Member.SceneName}.pdf", streamRef);
            }
        }

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

            if ((!(member.Orientations?.Any() ?? false)) || (member.Orientations!.Max(o => o.DateReceived) < DateTime.Now.AddYears(-1)))
                status.Issues.Add("Member has not attended orientation in the last year.");

            if (status.Issues.Any())
                status.Status ??= IssuesStatus.Warning;

            return status;
        }

        private async Task ToggleAttended(EventRsvp rsvp)
        {
            rsvp.Attended = !(rsvp?.Attended ?? false);
            await context.SaveChangesAsync();
            _eventRsvp = null;
            StateHasChanged();
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

        private async Task ShowProfileEditModal(Member userMember)
        {
            var parameters = new ModalParameters()
                .Add(nameof(IDCheckModal.UserMember), userMember);
            var options = new ModalOptions()
            {
                Class = "blazored-modal size-large"
            };
            var modal = Modal.Show<IDCheckModal>("Check ID", parameters, options);
            var response = await modal.Result;
        }

        private string GetCheckIdText(Member member)
            => (member.MemberType != MemberType.Member || (member.Payments?.Any() ?? false)) && (member.Orientations?.Any() ?? false)
                ? "Check ID"
                : "!!!REQUIRED - Check ID!!!";

        private static string GetRsvpStatus(EventRsvp rsvp, MembershipIssues issues)
            => (rsvp, issues) switch
            {
                ({ Attended: true }, _) => "table-primary",
                (_, { Status: IssuesStatus.Danger }) => "table-danger",
                (_, { Status: IssuesStatus.Warning }) => "table-warning",
                _ => "table-success"
            };
    }
}
