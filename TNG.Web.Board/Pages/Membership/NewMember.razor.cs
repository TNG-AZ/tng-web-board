using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using TNG.Web.Board.Data;
using TNG.Web.Board.Data.DTOs;
using TNG.Web.Board.Utilities;

namespace TNG.Web.Board.Pages.Membership
{

    public class IsToday : ValidationAttribute
    {
        public override bool IsValid(object? value)// Return a boolean value: true == IsValid, false != IsValid
        {
            DateTime d = Convert.ToDateTime(value);
            //date is equal to today in MST timezone
            return d.Date == DateTime.Now.ToAZTime().Date;

        }
    }

    public class IsOfAge : ValidationAttribute
    {
        public override bool IsValid(object? value)// Return a boolean value: true == IsValid, false != IsValid
        {
            DateTime d = Convert.ToDateTime(value);
            //date is ~18 years or more from today in MST timezone
            var eligibileAge = DateTime.Now.ToAZTime().AddYears(-18);

            return d.Date >= eligibileAge.Date;
        }
    }

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
    }

    public partial class NewMember
    {
#nullable disable
        [Inject]
        private ApplicationDbContext _context { get; set; }

        [Inject]
        private NavigationManager navigation { get; set; }
        [Inject]
        private AuthUtilities auth { get; set; }
#nullable enable

        private NewMemberForm formModel = new();
        private string ErrorMessage { get; set; } = string.Empty;

        protected async void SubmitNewMemberForm()
        {
            try
            {
                await _context.Members.AddAsync(new Member()
                {
                    LegalName = formModel.LegalName,
                    SceneName = formModel.SceneName,
                    Birthday = formModel.Birthday!.Value!,
                    EmailAddress = formModel.Email,
                    MemberType = formModel.MemberType,
                });
                await _context.SaveChangesAsync();
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
    }
}
