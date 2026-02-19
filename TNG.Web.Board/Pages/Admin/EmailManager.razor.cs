using Blazored.Modal;
using Blazored.Modal.Services;
using Blazored.TextEditor;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using Microsoft.JSInterop;
using TNG.Web.Board.Data;
using TNG.Web.Board.Data.DTOs;
using TNG.Web.Board.Data.Helpers;
using TNG.Web.Board.Pages.Shared.Modals;
using TNG.Web.Board.Services;

namespace TNG.Web.Board.Pages.Admin
{
    [Authorize(Roles = "Boardmember")]
    public partial class EmailManager
    {

#nullable disable
        [Inject]
        private ApplicationDbContext context { get; set; }
        [Inject]
        private GoogleServices google { get; set; }
        [Inject]
        private IJSRuntime js { get; set; }
        [CascadingParameter]
        private IModalService Modal { get; set; }

        private BlazoredTextEditor QuillHtml;
#nullable enable

        private List<Member> _allMembers { get; set; }
        private List<Member> AllMembers
            => _allMembers ??= context.Members
                .Include(m => m.Orientations)
                .Include(m => m.Payments)
                .Include(m => m.Suspensions)
                .ToList();

        private IEnumerable<Member> FilteredMembers
            => AllMembers
                .Where(m => m.MemberType == MemberType.Member || (IncludeGuests && m.MemberType == MemberType.Guest) || (IncludeHonorary && m.MemberType == MemberType.Honorary))
                .Where(m => IncludeSuspended || !m.Suspensions.Any() || !m.Suspensions.Any(s => s.EndDate == null || s.EndDate > DateTime.Now))
                .Where(m => DateFilter == MembershipFilter.None
                    || ( DateFilter == MembershipFilter.Current 
                        && m.Orientations.Any() 
                        && m.Orientations.Max(o => o.DateReceived) >= DateTime.Now.AddYears(-1)
                        && m.Payments.Any()
                        && m.Payments.Max(p => p.PaidOn) >= DateTime.Now.AddYears(-1))
                    || ( DateFilter == MembershipFilter.SinceDate
                        && m.Orientations.Any()
                        && m.Orientations.Max(o => o.DateReceived) >= MemberSince.AddYears(-1)
                        && m.Payments.Any()
                        && m.Payments.Max(p => p.PaidOn) >= MemberSince.AddYears(-1)));

        private bool IncludeGuests { get; set; }
        private bool IncludeHonorary { get; set; }
        private bool IncludeSuspended { get; set; }
        private MembershipFilter DateFilter { get; set; } = MembershipFilter.None;
        private DateTime MemberSince { get; set; } = DateTime.Now;

        private enum MembershipFilter
        {
            None,
            Current, 
            SinceDate,
        }

        private string? EmailSubject { get; set; }

        private async Task SendEmail()
        {
            if (FilteredMembers.Any())
            {
                if (!await js.InvokeAsync<bool>("confirm", "Confirm sending email?"))
                {
                    return;
                }
                var emails = FilteredMembers.Select(m => m.EmailAddress);
                var content = await QuillHtml.GetHTML();
                await google.EmailListAsync(emails, EmailSubject ?? "A Message From TNG:AZ Board", content);

                EmailSubject = null;
                await QuillHtml.LoadHTMLContent("Message has been sent");
                StateHasChanged();
            }
        }

        private async Task OpenEmailScheduler()
        {
            var email = new ScheduledEmail()
            {
                Subject = EmailSubject,
                Body = await QuillHtml.GetHTML(),
                RecipientsCSV = string.Join(", ",FilteredMembers.Select(m => m.EmailAddress))
            };
            var parameters = new ModalParameters()
                .Add(nameof(QueueEmail.email), email);
            var options = new ModalOptions()
            {
                Class = "blazored-modal size-large"
            };
            var modal = Modal.Show<QueueEmail>("Queue Email", parameters, options);
            var response = await modal.Result;
            await js.InvokeVoidAsync("alert", response.Confirmed ? "Scheduled" : "Failed");

        }
    }
}
