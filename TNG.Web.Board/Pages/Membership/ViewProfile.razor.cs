using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using TNG.Web.Board.Data.DTOs;
using TNG.Web.Board.Data;
using TNG.Web.Board.Utilities;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Authorization;

namespace TNG.Web.Board.Pages.Membership
{
    [Authorize]
    public partial class ViewProfile
    {
        [Parameter]
        public Guid? memberId { get; set; }


#nullable disable
        [Inject]
        private ApplicationDbContext context { get; set; }
        [Inject]
        private AuthUtilities auth { get; set; }

        [Inject]
        private NavigationManager navigation { get; set; }
#nullable enable

        private Member? GetMember()
            => context.Members?
            .Include(m => m.MemberFetishes)
            .ThenInclude(mf => mf.Fetish)
            .FirstOrDefault(m => m.Id == memberId);

        private Member? _viewMember { get; set; }
        private Member? ViewMember
        {
            get
            {
                if (memberId is null && (_viewMember ??= GetUserMember()) is not null)
                    return _viewMember;
                if (memberId is not null && (_viewMember ??= GetMember()) is not null)
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
            return context.Members?.Include(m => m.MemberFetishes).ThenInclude(mf => mf.Fetish).FirstOrDefault(m => EF.Functions.Like(m.EmailAddress, name));
        }

        private bool EnableEdit
            => ViewMember?.Id == UserMember?.Id;

        private bool EditSceneNameToggle;

        private string? NewSceneName { get; set; }

        private async Task SubmitSceneName()
        {
            if (!string.IsNullOrEmpty(NewSceneName))
            {
                UserMember!.SceneName = NewSceneName;
                context.Update(UserMember);
                await context.SaveChangesAsync();

                EditSceneNameToggle = false;

                StateHasChanged();
            }
        }

        private List<Fetish>? _fetishes { get; set; }
        private List<Fetish> Fetishes
            => _fetishes ??= context.Fetishes.ToList();

        private EditContext editContext = new(new object());

        private string? NewFetishName { get; set; }
        private FetishRoleEnum? NewFetishRole { get; set; }
        private bool? NewFetishWillingToTeach { get; set; }

        private void NewFetishRoleChange(ChangeEventArgs e)
        {
            if (string.IsNullOrEmpty(e.Value?.ToString()))
                NewFetishRole = null;
            else
                NewFetishRole = int.TryParse(e.Value.ToString(), out var roleId) ? (FetishRoleEnum)roleId : null;
        }
        private async Task AddFetishToMember()
        {
            if (!string.IsNullOrEmpty(NewFetishName)
                && !context.MembersFetishes.Where(mf => mf.MemberId == UserMember!.Id).Select(mf => mf.Fetish).Any(f => EF.Functions.Like(f.Name, NewFetishName)))
            {
                var fetish = await context.Fetishes.FirstOrDefaultAsync(f => EF.Functions.Like(f.Name, NewFetishName));
                if (fetish is null)
                {
                    fetish = context.Add(new Fetish { Name = NewFetishName }).Entity;
                    await context.SaveChangesAsync();
                }
                context.Add(new MemberFetish { MemberId = UserMember!.Id, FetishId = fetish.Id, Role = NewFetishRole, WillingToTeach = NewFetishWillingToTeach });
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

        private string TogglePrivateButtonText
            => ViewMember.PrivateProfile
                ? "Make Profile Public"
                : "Make Profile Private";

        private async Task TogglePrivate()
        {
            ViewMember.PrivateProfile = !ViewMember.PrivateProfile;
            await context.SaveChangesAsync();
        }
    }
}
