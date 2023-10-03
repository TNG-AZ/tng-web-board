using Blazored.Modal;
using Blazored.Modal.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using Polly;
using TNG.Web.Board.Data;
using TNG.Web.Board.Data.DTOs;
using TNG.Web.Board.Data.Migrations;

namespace TNG.Web.Board.Pages.Membership
{
    public partial class EditMemberModal
    {
#nullable disable
        [Parameter]
        public Member UserMember { get; set; }

        [Inject]
        private ApplicationDbContext context { get; set; }
        [CascadingParameter]
        private BlazoredModalInstance Modal { get; set; }
#nullable enable
        private string TogglePrivateButtonText
            => UserMember.PrivateProfile
                ? "Make Profile Public"
                : "Make Profile Private";

        private async Task TogglePrivate()
        {
            UserMember.PrivateProfile = !UserMember.PrivateProfile;
            await context.SaveChangesAsync();
        }

        private async Task SaveUser()
        {
            
            if (string.IsNullOrWhiteSpace(UserMember!.SceneName))
                return;
            if (string.IsNullOrEmpty(UserMember!.ProfileUrl?.Trim()))
                UserMember.ProfileUrl = null;
            if (string.IsNullOrEmpty(UserMember!.AboutMe?.Trim()))
                UserMember.AboutMe = null;
            if (UserMember!.ProfileUrl is not null && context.Members.Any(m => m.Id != UserMember.Id && EF.Functions.Like(m.ProfileUrl, UserMember.ProfileUrl)))
                return;
            if (!string.IsNullOrEmpty(PronounText) && !PronounText.Equals(UserMember.Pronouns))
                UserMember.Pronouns = PronounText;

            context.Attach(UserMember);
            context.Entry(UserMember).State= EntityState.Modified;

            await context.SaveChangesAsync();
            await Modal.CloseAsync(ModalResult.Ok());
        }

        private char PronounOption { get; set; }
        private string CustomPronounText { get; set; } = string.Empty;
        private string PronounText
            => PronounOption switch
            {
                'h' => "He/Him/His",
                's' => "She/Her/Hers",
                't' => "They/Them/Theirs",
                _ => CustomPronounText
            };
    }
}
