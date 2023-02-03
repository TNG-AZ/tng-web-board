using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using TNG.Web.Board.Data;
using TNG.Web.Board.Data.DTOs;

namespace TNG.Web.Board.Pages.NewMember
{

    public class IsToday : ValidationAttribute
    {
        public override bool IsValid(object? value)// Return a boolean value: true == IsValid, false != IsValid
        {
            DateTime d = Convert.ToDateTime(value);
            //date is equal to today in -7GMT timezone
            return d.Date == TimeZoneInfo.ConvertTime(DateTime.Today, TimeZoneInfo.GetSystemTimeZones().First(tz => tz.BaseUtcOffset == TimeSpan.FromHours(-7))).Date;

        }
    }

    public class IsOfAge : ValidationAttribute
    {
        public override bool IsValid(object? value)// Return a boolean value: true == IsValid, false != IsValid
        {
            DateTime d = Convert.ToDateTime(value);
            //date is ~18 years or more from today in -7GMT timezone
            var today = TimeZoneInfo.ConvertTime(DateTime.Today, TimeZoneInfo.GetSystemTimeZones().First(tz => tz.BaseUtcOffset == TimeSpan.FromHours(-7)));
            var eligibileAge = today.AddYears(-18);

            if ((d.Year < eligibileAge.Year)
                || (d.Year == eligibileAge.Year && d.Month < eligibileAge.Month)
                || (d.Year == eligibileAge.Year && d.Month == eligibileAge.Month && d.Day <= eligibileAge.Day))
                return true;
            return false;

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

    public partial class Index
    {
        [Inject]
        private ApplicationDbContext _context { get; set; }

        private NewMemberForm formModel = new();

        protected async void SubmitNewMemberForm()
        {
            var a = 0;
        }

        protected void MembershipTypeChange(ChangeEventArgs e)
        {
            var membershipType = (MemberType)int.Parse(e.Value!.ToString()!);
            formModel.MemberType = membershipType;
        }
    }
}
