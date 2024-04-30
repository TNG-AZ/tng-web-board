using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using Microsoft.JSInterop;
using System.ComponentModel.DataAnnotations;
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
    }

    public partial class NewMember
    {
#nullable disable
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

        private NewMemberForm formModel = new();
        private string ErrorMessage { get; set; } = string.Empty;

        protected async void SubmitNewMemberForm()
        {
            if (!await altcha.Validate(formModel.Altcha))
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
    }
}
