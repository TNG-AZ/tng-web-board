using Blazored.Modal;
using Blazored.Modal.Services;
using Google.Apis.Calendar.v3.Data;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.EntityFrameworkCore;
using System;
using TNG.Web.Board.Data;
using TNG.Web.Board.Data.DTOs;
using TNG.Web.Board.Services;
using TNG.Web.Board.Utilities;
using System.Web;
using Humanizer;

namespace TNG.Web.Board.Pages.Events
{
    public partial class Index
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
        [Inject]
        private AuthUtilities auth { get; set; }
        [Inject]
        private AuthenticationStateProvider authStateProvider { get; set; }
        [CascadingParameter]
        private IModalService Modal { get; set; }
#nullable enable

        private Member? _member { get; set; }
        private Member? Member
            => _member ??= GetMember();

        private Member? GetMember()
        {
            var name = auth.GetIdentity().Result?.Name ?? string.Empty;
            return context.Members.Include(m => m.Events).FirstOrDefault(m => EF.Functions.Like(m.EmailAddress, name));
        }

        private string CalendarId
            => Configuration["CalendarId"] ?? throw new ArgumentNullException(nameof(CalendarId));

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
                return _calendarEvent ??= Google.Calendar.Events.Get(CalendarId, eventId).Execute();
            }
        }

        private async Task RsvpDelete(string eventId)
        {
            try
            {
                shouldRender = false;
                if (Member is null)
                {
                    if (string.IsNullOrEmpty(auth.GetIdentity().Result?.Name))
                        navigation.NavigateTo("/Identity/Account/Login", true);
                    else
                        navigation.NavigateTo("/members/new");
                    return;
                }
                try
                {
                    var rsvp = await context.EventRsvps.FirstOrDefaultAsync(r => r.MemberId == Member!.Id && eventId == r.EventId);
                    if (rsvp is null)
                        return;

                    context.EventRsvps.Remove(rsvp);
                    await context.SaveChangesAsync();
                }
                catch { }
                StateHasChanged();
            }
            finally
            {
                shouldRender = true;
            }
        }

        private async Task RsvpChange(string eventId, EventRsvpStatus status)
        {
            try
            {
                shouldRender = false;
                if (Member is null)
                {
                    if (string.IsNullOrEmpty(auth.GetIdentity().Result?.Name))
                        navigation.NavigateTo("/Identity/Account/Login", true);
                    else
                        navigation.NavigateTo("/members/new");
                    return;
                }
                try
                {
                    var rsvp = await context.EventRsvps.FirstOrDefaultAsync(r => r.MemberId == Member!.Id && eventId == r.EventId)
                        ?? new() { EventId = eventId, MemberId = Member!.Id, Status = status };
                    if (rsvp.Id == default)
                        context.Add(rsvp);
                    else
                        rsvp.Status = status;

                    await context.SaveChangesAsync();
                }
                catch { }
                StateHasChanged();
            }
            finally
            {
                shouldRender = true;
            }
        }

        private async Task RsvpGoing(string eventId)
            => await RsvpChange(eventId, EventRsvpStatus.Going);

        private async Task RsvpMaybeGoing(string eventId)
            => await RsvpChange(eventId, EventRsvpStatus.MaybeGoing);

        private string GetRsvpMemberList(string eventId, EventRsvpStatus status, bool isBoardMember)
        {
            var viewableMemberIds = new List<Guid>();
            if (Member is not null)
            {
                viewableMemberIds.Add(Member.Id);
            }
            var members = context.EventRsvps?.AsEnumerable().Where(e =>
                    e.EventId == eventId && e.Status == status
                    && (viewableMemberIds.Contains(e.MemberId) || !e.Member.PrivateProfile || isBoardMember))
                .Select(e =>
                {
                    var profileId = e.Member.ProfileUrl ?? e.Member.Id.ToString();
                    var profileName = HttpUtility.HtmlEncode(e.Member.SceneName);
                    return $"<a href='/members/view/{profileId}'><span class='badge badge-pill badge-primary'><i class='bi bi-person'></i>{profileName}</span></a>";
                });
            return string.Join(" ", members ?? Enumerable.Empty<string>()) ?? string.Empty;
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

        private bool shouldRender = true;

        protected override bool ShouldRender()
        {
            return shouldRender;
        }
    }
}
