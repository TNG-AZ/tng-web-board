using Blazored.Modal;
using Blazored.Modal.Services;
using Blazored.TextEditor;
using Google.Apis.Calendar.v3.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using Microsoft.JSInterop;
using System.IO.Compression;
using System.Linq.Expressions;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using TNG.Web.Board.Data;
using TNG.Web.Board.Data.DTOs;
using TNG.Web.Board.Pages.Membership;
using TNG.Web.Board.Services;
using TNG.Web.Board.Utilities;

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
        [Inject]
        private IJSRuntime js { get; set; }
        [CascadingParameter] 
        private IModalService Modal { get; set; }
#nullable enable

        private Event? _event { get; set; }
        private Event? CalendarEvent
            => _event ??= google.GetEvent(eventId!)?.Result;

        private List<EventRsvp>? _eventRsvp { get; set; }
        private IEnumerable<EventRsvp> Rsvps
            => _eventRsvp ??= context.EventRsvps
                .Include(e => e.Member)
                .Include(e => e.Member.Suspensions)
                .Include(e => e.Member.Orientations)
                .Include(e => e.Member.Payments)
                .Include(e => e.Member.Invoices)
                .Where(e => e.EventId == eventId).ToList();

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

        private IList<Signature> _signatures { get; set; }
        private IEnumerable<Signature> Signatures
            => _signatures ??= context.Signatures
                .Include(s => s.Member)
                .Where(s => s.EventId == eventId).ToList();

        private HashSet<Guid> ExpandedPlusOnes = new();

        private bool ToggleExpand(Guid rsvpId)
            => ExpandedPlusOnes.Contains(rsvpId)
                ? ExpandedPlusOnes.Remove(rsvpId)
                : ExpandedPlusOnes.Add(rsvpId);

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

            if (((!(member.Payments?.Any() ?? false)) || (member.Payments!.Max(p => p.PaidOn) < DateTime.Now.AddYears(-1))) && member.MemberType == MemberType.Member)
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

        private enum EmailListEnum
        {
            All,
            GoodStanding,
            Paid
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
                    EmailListEnum.Paid => Rsvps
                        .Where(r => (r.Paid ?? false) || r.Member.Invoices.Any(i => i.EventId == eventId && i.PaidOnDate != null))
                        .Select(r => r.Member),
                    EmailListEnum.GoodStanding => Rsvps.Select(r => r.Member).Where(m => GetMembershipIssues(m).Status == null),
                    _ => Enumerable.Empty<Member>()
                };

                if (members.Any())
                {
                    if (!await js.InvokeAsync<bool>("confirm", "Confirm sending email?"))
                    {
                        return;
                    }

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

        private async Task ShowInvoiceModal(Member invoiceMember)
        {
            var parameters = new ModalParameters()
                .Add(nameof(EventInvoice.InvoiceMember), invoiceMember)
                .Add(nameof(EventInvoice.CalendarEvent), CalendarEvent)
                .Add(nameof(EventInvoice.Fees), EventFees ?? new() { MembershipDues = 12, GuestEntry = 10, MemberEntry = 8});
            var options = new ModalOptions()
            {
                Class = "blazored-modal size-large"
            };
            var modal = Modal.Show<EventInvoice>("Create Invoice", parameters, options);
            var result = await modal.Result;
            if (result.Confirmed)
            {
                _eventRsvp = null;
                StateHasChanged();
            }
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
            try
            {
                shouldRender = false;
                var fees = new EventFees()
                {
                    EventId = eventId,
                    MemberEntry = 8,
                    GuestEntry = 10,
                    MembershipDues = 12
                };
                await context.EventsFees.AddAsync(fees);
                await context.SaveChangesAsync();

                _eventFees = fees;
            }
            finally
            {
                shouldRender = true;
            }
        }

        private async Task GetSignature(Guid sigId)
        {
            var signature = Signatures.FirstOrDefault(s => s.Id == sigId);
            if (signature is not null)
            {
                using var streamRef = new DotNetStreamReference(stream: new MemoryStream(signature.SignedForm));

                await js.InvokeVoidAsync("downloadFileFromStream", $"liabilityForm-{eventId}-{signature!.Member.SceneName}.pdf", streamRef);
            }
        }

        private async Task GetSignaturesZip()
        {
            using (var compressedFileStream = new MemoryStream())
            {
                //Create an archive and store the stream in memory.
                using (var zipArchive = new ZipArchive(compressedFileStream, ZipArchiveMode.Create, false))
                {
                    var sigsBySceneName = Signatures.GroupBy(s => s.SceneName.Trim().ToLower());
                    foreach (var sigGroup in sigsBySceneName)
                    {
                        var num = 1;
                        var rgx = new Regex("[^a-zA-Z0-9]");
                        var sceneName = rgx.Replace(sigGroup.Key, "");
                        foreach(var sig in sigGroup)
                        {
                            //Create a zip entry for each attachment
                            var zipEntry = zipArchive.CreateEntry($"liabilityForm-{eventId}-{sceneName}-{num}.pdf");

                            //Get the stream of the attachment
                            using (var originalFileStream = new MemoryStream(sig.SignedForm))
                            using (var zipEntryStream = zipEntry.Open())
                            {
                                //Copy the attachment stream to the zip entry stream
                                originalFileStream.CopyTo(zipEntryStream);
                            }
                            num++;
                        }
                        
                    }
                }

                using var streamRef = new DotNetStreamReference(stream: new MemoryStream(compressedFileStream.ToArray()));
                await js.InvokeVoidAsync("downloadFileFromStream", $"liabilityForms-{eventId}.zip", streamRef);
            }
        }

        private string? NewRSVPEmail { get; set; }

        private async Task AddManualRsvp()
        {
            if (string.IsNullOrEmpty(NewRSVPEmail))
            {
                await js.InvokeVoidAsync("alert", "please specify a valid user email");
                return;
            }

            var member = await context.Members
                .FirstOrDefaultAsync(m => EF.Functions.Like(m.EmailAddress, NewRSVPEmail));

            if (member is null)
            {
                await js.InvokeVoidAsync("alert", "member not found with specified email");
                return;
            }

            if (Rsvps.Select(r => r.MemberId).Contains(member.Id))
            {
                await js.InvokeVoidAsync("alert", "Member has already RSVPed");
                return;
            }

            await context.EventRsvps.AddAsync(new() { MemberId = member.Id, EventId = eventId});
            await context.SaveChangesAsync();
            _eventRsvp = null;
            StateHasChanged();
        }

        private enum Ordering
        {
            Chronological,
            Alphabetical
        }

        private enum Filtering
        {
            Everyone,
            Paid,
            PaidAndNotHere
        }

        private Filtering RsvpFiltering { get; set; } = Filtering.Everyone;
        private Ordering RsvpOrdering { get; set; } = Ordering.Chronological;

        private void UpdateFiltering(Filtering filtering)
        {
            RsvpFiltering = filtering;
            _filteredRsvp = null;
        }

        private void UpdateOrdering(Ordering ordering)
        {
            RsvpOrdering = ordering;
            _filteredRsvp = null;
        }

        private List<EventRsvp>? _filteredRsvp { get; set; }
        private IEnumerable<EventRsvp> FilteredRsvps
        {
            get
            {
                if (_filteredRsvp == null)
                {
                    var filtered = Rsvps
                        .Where(r =>
                            RsvpFiltering == Filtering.Everyone
                            || (RsvpFiltering == Filtering.Paid
                                && ((r.Paid??false) || r.Member.Invoices.Any(i => i.EventId == eventId && i.PaidOnDate != null)))
                            || (RsvpFiltering == Filtering.PaidAndNotHere
                                && ((r.Paid ?? false) || r.Member.Invoices.Any(i => i.EventId == eventId && i.PaidOnDate != null))
                                && !(r.Attended??false)))
                        ;
                    _filteredRsvp = (RsvpOrdering switch
                    {
                        Ordering.Alphabetical => filtered.OrderBy(r => r.Member.SceneName),
                        Ordering.Chronological => filtered.OrderBy(r => r.AddedDate),
                        _ => filtered.OrderBy(r => r.AddedDate)
                    }).ToList();
                }
                return _filteredRsvp;
            }
        }

        private async Task UpdateMembership(Member member)
        {
            var startTime = CalendarEvent.Start.DateTime.ToAZTime();
            var oneYearAgo = DateTime.Now.ToAZTime().AddYears(-1);

            if (member.Payments.Any(p => p.PaidOn >= oneYearAgo)
                || member.Orientations.Any(o => o.DateReceived >= oneYearAgo))
            {
                if (!(await js.InvokeAsync<bool>("confirm", "This member has orientation/dues paid in the last 12 months. Continue?")))
                {
                    return;
                }
            }
            if (member.MemberType == MemberType.Member)
            {
                await context.MemberDuesPayments.AddAsync(new()
                {
                    MemberId = member.Id,
                    PaidOn = startTime!.Value
                });
            }
            
            await context.MemberOrientations.AddAsync(new()
            {
                MemberId = member.Id,
                DateReceived = startTime!.Value,
            });

            await context.SaveChangesAsync();
            StateHasChanged();
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

        private bool shouldRender = true;
        protected override bool ShouldRender()
        {
            return shouldRender;
        }
    }
}
