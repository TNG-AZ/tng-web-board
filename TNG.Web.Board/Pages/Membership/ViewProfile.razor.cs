﻿using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using TNG.Web.Board.Data.DTOs;
using TNG.Web.Board.Data;
using TNG.Web.Board.Utilities;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Authorization;
using TNG.Web.Board.Services;
using Square.Models;
using System.Configuration;
using Microsoft.JSInterop;
using Blazored.Modal;
using Blazored.Modal.Services;

namespace TNG.Web.Board.Pages.Membership
{
    [Authorize]
    public partial class ViewProfile
    {
        [Parameter]
        public string? profileUrl { get; set; }

        private Guid? _memberId { get; set; }
        private Guid? MemberId
            => _memberId ??= Guid.TryParse(profileUrl, out var tmp) ? tmp : null;


#nullable disable
        [Inject]
        private ApplicationDbContext context { get; set; }
        [Inject]
        private AuthUtilities auth { get; set; }

        [Inject]
        private NavigationManager navigation { get; set; }
        [Inject]
        private SquareService square { get; set; }
        [Inject]
        private IJSRuntime js { get; set; }
        [Inject]
        public GoogleServices Google { get; set; }
        [Inject]
        public IConfiguration Configuration { get; set; }
        [CascadingParameter]
        private IModalService Modal { get; set; }
#nullable enable

        private Member? GetMember()
            => context.Members?
                .Include(m => m.MemberFetishes)
                .ThenInclude(mf => mf.Fetish)
                .Include(m => m.Events)
                .Include(m => m.Orientations)
                .Include(m => m.Payments)
                .FirstOrDefault(m => (!m.PrivateProfile || auth.IsBoardmember().Result) 
                    && (m.Id == MemberId || EF.Functions.Like(m.ProfileUrl, profileUrl)));

        private Member? _viewMember { get; set; }
        private Member? ViewMember
        {
            get
            {
                if (_viewMember is not null)
                    return _viewMember;
                if (profileUrl is null && (_viewMember ??= GetUserMember()) is not null)
                    return _viewMember;
                if (profileUrl is not null && (_viewMember ??= GetMember()) is not null)
                    return _viewMember;
                else
                {
                    navigation.NavigateTo("/");
                    return null;
                }
            }
        }

        private Member? _userMember { get; set; }
        private Member? UserMember
        {
            get
            {
                if ((_userMember ??= GetUserMember()) is not null)
                    return _userMember;
                else
                {
                    navigation.NavigateTo("/members/new");
                    return null;
                }
            }
        }

        private Member? GetUserMember()
        {
            var name = auth.GetIdentity().Result?.Name ?? string.Empty;
            return context.Members?
                .Include(m => m.MemberFetishes)
                .ThenInclude(mf => mf.Fetish)
                .Include(m => m.Events)
                .Include(m => m.Orientations)
                .Include(m => m.Payments)
                .FirstOrDefault(m => EF.Functions.Like(m.EmailAddress, name));
        }

        private async Task GenerateDuesInvoice()
        {
            var duesCents = 1200;
            var lineItems = new List<OrderLineItem>
            {
                new(quantity:"1", name:"Membership Dues", basePriceMoney:new(duesCents, "USD"))
            };
            var email = ViewMember!.EmailAddress;
            var dueDate = DateTime.Now.ToAZTime().AddDays(7);
            await square.CreateInvoice(email, lineItems, dueDate);
            await js.InvokeVoidAsync("alert", "Dues invoice has been sent");
        }

        private bool EnableEdit
            => ViewMember?.Id == UserMember?.Id;

        private bool EditToggle;

        private List<Fetish>? _fetishes { get; set; }
        private List<Fetish> Fetishes
            => _fetishes ??= context.Fetishes.ToList();

        private EditContext editContext = new(new object());

        private string? NewFetishName { get; set; }
        private FetishRoleEnum? NewFetishRole { get; set; }
        private bool? NewFetishWillingToTeach { get; set; }
        private bool? NewFetishHardLimit { get; set; }

        private void NewFetishRoleChange(ChangeEventArgs e)
        {
            if (string.IsNullOrEmpty(e.Value?.ToString()))
                NewFetishRole = null;
            else
                NewFetishRole = int.TryParse(e.Value.ToString(), out var roleId) ? (FetishRoleEnum)roleId : null;
        }
        private async Task AddFetishToMember(bool limit = false)
        {
            if (!string.IsNullOrEmpty(NewFetishName)
                && !context.MembersFetishes.Where(mf => mf.MemberId == UserMember!.Id).Select(mf => mf.Fetish).Any(f => EF.Functions.Like(f.Name, NewFetishName)))
            {
                var fetish = await context.Fetishes.FirstOrDefaultAsync(f => EF.Functions.Like(f.Name, NewFetishName));
                if (fetish is null)
                {
                    fetish = context.Add(new Fetish { Name = NewFetishName}).Entity;
                    await context.SaveChangesAsync();
                }
                context.Add(new MemberFetish { 
                    MemberId = UserMember!.Id, 
                    FetishId = fetish.Id, 
                    Role = NewFetishRole, 
                    WillingToTeach = NewFetishWillingToTeach, 
                    HardLimit = NewFetishHardLimit,
                    Limit = limit });
                await context.SaveChangesAsync();

                NewFetishName = string.Empty;
                NewFetishWillingToTeach = false;
                StateHasChanged();
            }
        }

        private async Task UnlinkFetish(Guid memberFetishId)
        {
            context.Remove(context.MembersFetishes.FirstOrDefault(mf => mf.Id == memberFetishId));
            await context.SaveChangesAsync();
            StateHasChanged();
        }

        private async Task ShowProfileEditModal()
        {
            var parameters = new ModalParameters()
                .Add(nameof(EditMemberModal.UserMember), UserMember);
            var options = new ModalOptions()
            {
                Class = "blazored-modal size-large"
            };
            var modal = Modal.Show<EditMemberModal>("Edit Profile", parameters, options);
            var response = await modal.Result;
            if (response.Confirmed) 
            {
                StateHasChanged();
            }
        }

        private class EventDetail
        {
            public string EventId { get; set; }
            public DateTime? EventDate { get; set; }
            public string EventName { get; set; }
            public EventRsvpStatus Status { get; set; }
        }

        private string CalendarId
            => Configuration["CalendarId"] ?? throw new ArgumentNullException(nameof(CalendarId));

        private string GetEventRsvps()
         => string.Join("</li><li>", ViewMember?.Events?.AsParallel()
                .OrderByDescending(r => r.AddedDate)
                .Take(10)
                .Select(r =>
                {
                    var eventData = Google.GetEvent(CalendarId, r.EventId);
                    return new EventDetail
                    {
                        EventId = r.EventId,
                        EventDate = eventData!.Start.DateTime.ToAZTime(),
                        EventName = eventData!.Summary,
                        Status = r.Status
                    };
                })
                .OrderByDescending(r => r.EventDate)
                .Select(r => $"{(r.Status == EventRsvpStatus.Going ? "Going" : "Maybe Going")} to <a href='/events/{r.EventId}'>{r.EventName}</a> on {r.EventDate.Value.Date:MMMM d, yyyy}")
                ?? Enumerable.Empty<string>());
    }
}
