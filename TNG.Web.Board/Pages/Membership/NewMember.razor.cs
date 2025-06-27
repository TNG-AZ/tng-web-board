using Ixnas.AltchaNet;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.JSInterop;
using System.ComponentModel.DataAnnotations;
using System.Security.Policy;
using System.Text.Encodings.Web;
using System.Text;
using TNG.Web.Board.Data;
using TNG.Web.Board.Data.DTOs;
using TNG.Web.Board.Services;
using TNG.Web.Board.Utilities;

namespace TNG.Web.Board.Pages.Membership
{
    public class NewMemberForm
    {
        [Required]
        [IsToday(ErrorMessage = "Must be set to today in AZ time")]
        public DateTime? TodaysDate { get; set; }
        [Required]
        public string? LegalName { get; set; }
        [Required]
        public string? SceneName { get; set; }
        [Required]
        [IsOfAge(ErrorMessage = "Must be at least 18 years of age to join TNG")]
        public DateTime? Birthday { get; set; }
        [Required]
        [EmailAddress]
        public string? Email { get; set; }
        [Required]
        public MemberType MemberType { get; set; }
        [Required]
        public string Altcha { get; set; }
        public bool NewUser { get; set; } = true;
        public string Password { get; set;}
    }

    public partial class NewMember
    {
#nullable disable
        [Inject]

        private IUserStore<IdentityUser> userStore {  get; set; }
        private IUserEmailStore<IdentityUser> _es { get; set; }
        private IUserEmailStore<IdentityUser> emailStore { get { return _es ??= (IUserEmailStore<IdentityUser>)userStore; }}
        [Inject]

        private UserManager<IdentityUser> userManager { get; set; }
        [Inject]

        private IEmailSender emailSender { get; set; }
        [Inject]
        private SignInManager<IdentityUser> signInManager { get; set; }
        [Inject]
        private ApplicationDbContext context { get; set; }

        [Inject]
        private NavigationManager navigation { get; set; }
        [Inject]
        private AuthUtilities auth { get; set; }
        [Inject]
        private IJSRuntime jsRuntime { get; set; }
        [Inject]
        AltchaPageService altcha { get; set; }
#nullable enable

        protected override async Task OnInitializedAsync()
        {
            UserEmail = await auth.GetEmail();
            EmailRegistered = context.Members.Any(m => EF.Functions.Like(m.EmailAddress, UserEmail));
            if (!string.IsNullOrEmpty(UserEmail))
                formModel.Email = UserEmail;
        }

        private string UserEmail { get; set; }
        private bool EmailRegistered { get; set; }

        private NewMemberForm formModel = new();
        private string ErrorMessage { get; set; } = string.Empty;

        protected async void SubmitNewMemberForm()
        {
            if (string.IsNullOrEmpty(formModel.Altcha) || !await altcha.Validate(formModel.Altcha))
            {
                ErrorMessage = "Invalid Altcha";
                await jsRuntime.InvokeVoidAsync("scrollToTop");
                return;
            }
            if (formModel.Email is not null && context.Members.Any(m => EF.Functions.Like(m.EmailAddress, formModel.Email)))
            {
                ErrorMessage = "Membership form already submitted for this user";
                await jsRuntime.InvokeVoidAsync("scrollToTop");
                return;
            }
            try
            {
                await context.Members.AddAsync(new Member()
                {
                    LegalName = formModel.LegalName,
                    SceneName = formModel.SceneName,
                    Birthday = formModel.Birthday!.Value!,
                    EmailAddress = formModel.Email,
                    MemberType = formModel.MemberType,
                });
                await context.SaveChangesAsync();
            }
            finally
            {
                if (formModel.NewUser)
                    await CreateLogin();
                else
                    navigation.NavigateTo("/");
            }
        }

        protected void MembershipTypeChange(ChangeEventArgs e)
        {
            var membershipType = (MemberType)int.Parse(e.Value!.ToString()!);
            formModel.MemberType = membershipType;
        }

        private async void SetAltcha(ChangeEventArgs e)
        {
            if (bool.TryParse(e.Value.ToString(), out var validated) && validated)
            {
                formModel.Altcha = await jsRuntime.InvokeAsync<string>("getAltcha");
            }
        }

        public async Task CreateLogin()
        {

            var emailExists = await userManager.FindByEmailAsync(formModel.Email);
            if (emailExists != null)
            {
                if (userManager.Options.SignIn.RequireConfirmedAccount)
                {
                    await jsRuntime.InvokeVoidAsync("alert", "Check your email for a confirmation code");
                }
                navigation.NavigateTo($"/");
                return;
            }


            var user = Activator.CreateInstance<IdentityUser>();

            await userStore.SetUserNameAsync(user, formModel.Email, CancellationToken.None);
            await emailStore.SetEmailAsync(user, formModel.Email, CancellationToken.None);
            var result = await userManager.CreateAsync(user, formModel.Password);

            if (result.Succeeded)
            {

                var userId = await userManager.GetUserIdAsync(user);
                var code = await userManager.GenerateEmailConfirmationTokenAsync(user);
                code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));

                var confirmationLink = $"{navigation.BaseUri}Identity/Account/ConfirmEmail?userId={user.Id}&code={code}";

                await emailSender.SendEmailAsync(formModel.Email, "Confirm your email",
                    $"Please confirm your account by <a href='{HtmlEncoder.Default.Encode(confirmationLink)}'>clicking here</a>.");

                if (userManager.Options.SignIn.RequireConfirmedAccount)
                {
                    await jsRuntime.InvokeVoidAsync("alert", "Check your email for a confirmation code");
                    navigation.NavigateTo($"/");
                }
                else
                {
                    await signInManager.SignInAsync(user, isPersistent: false);
                    navigation.NavigateTo("/");
                }
            }
            else
            {
                ErrorMessage = "Invalid Login";
                await jsRuntime.InvokeVoidAsync("scrollToTop");
            }
        }
    }
}
