using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Linq;
using System.Linq.Expressions;
using TNG.Web.Board.Data;
using TNG.Web.Board.Data.DTOs;

namespace TNG.Web.Board.Pages.Membership
{
    public enum SuspendedStatusEnum
    {
        All,
        Yes,
        No
    }

    public enum DateStatusEnum
    {
        Good,
        Warning,
        Danger
    }

    [Authorize(Roles = "Boardmember")]
    public partial class ViewMembers
    {
#nullable disable
        [Inject]
        private ApplicationDbContext context { get; set; }
#nullable enable

        private string? SceneNameFilter { get; set; }
        private string? LegalNameFilter { get; set; }
        private string? EmailFilter { get; set; }
        private SuspendedStatusEnum? SuspendedStatusFilter { get; set; }
        private MemberType? MemberTypeFilter { get; set; }

        private static DateStatusEnum GetDateStatus(DateTime? date)
        {
            if (date == null || date < DateTime.Now.AddYears(-1))
                return DateStatusEnum.Danger;
            if (date < DateTime.Now.AddMonths(-11))
                return DateStatusEnum.Warning;
            return DateStatusEnum.Good;
        }

        private static DateStatusEnum GetBirthdayStatus(DateTime date)
        {
            if (date < DateTime.Now.AddYears(-40) || date > DateTime.Now.AddYears(-18))
                return DateStatusEnum.Danger;
            if (date < DateTime.Now.AddYears(-39).AddMonths(-11))
                return DateStatusEnum.Warning;
            return DateStatusEnum.Good;

        }

        private static DateStatusEnum GetDMTrainingStatus(DateTime date)
        {
            if (date < DateTime.Now.AddYears(-1))
                return DateStatusEnum.Warning;
            return DateStatusEnum.Good;
        }

        private static readonly Dictionary<DateStatusEnum, string> DateStatusClasses 
            = new() {
                { DateStatusEnum.Danger, "table-danger" },
                { DateStatusEnum.Warning, "table-warning" },
                { DateStatusEnum.Good, string.Empty }
            };

        private static string GetDMTrainingClass(DateTime date)
            => DateStatusClasses[GetDMTrainingStatus(date)];

        private static string GetDateClass(DateTime? date)
            => DateStatusClasses[GetDateStatus(date)];

        private static string GetBirthdayClass(DateTime date)
            => DateStatusClasses[GetBirthdayStatus(date)];

        private static readonly Expression<Func<MembershipSuspension, bool>> IsActiveSuspension
            = (m) => m.EndDate == null || m.EndDate >= DateTime.Now;

        private static string SuspensionDisplay(MembershipSuspension? suspension)
        {
            if (suspension != null)
                return suspension.EndDate.HasValue
                ? $"Suspended until {suspension.EndDate.Value:MM/dd/yyy}"
                : "Blacklisted";
            return string.Empty;
        }

        private IEnumerable<Member> GetFilteredMembers()
        {
            var nameIsDiscord = long.TryParse(SceneNameFilter, out var discordId);
            return context.Members
            .Include(m => m.Suspensions)
            .Include(m => m.Notes)
            .Include(m => m.Payments)
            .Include(m => m.Orientations)
            .Include(m => m.MemberDiscords)
            .Where(m =>
                (
                    string.IsNullOrEmpty(SceneNameFilter) 
                    || EF.Functions.Like(m.SceneName, $"%{SceneNameFilter}%")
                    || (nameIsDiscord && m.MemberDiscords.Any(d => d.DiscordId == discordId))
                )
                && (
                    string.IsNullOrEmpty(LegalNameFilter) 
                    || EF.Functions.Like(m.LegalName, $"%{LegalNameFilter}%")
                )
                && (string.IsNullOrEmpty(EmailFilter) || EF.Functions.Like(m.EmailAddress, $"%{EmailFilter}%"))
                && (!SuspendedStatusFilter.HasValue || SuspendedStatusFilter.Value == SuspendedStatusEnum.All
                    || (SuspendedStatusFilter.Value == SuspendedStatusEnum.No && (m.Suspensions == null || !m.Suspensions.AsQueryable().Any(IsActiveSuspension)))
                    || (SuspendedStatusFilter.Value == SuspendedStatusEnum.Yes && m.Suspensions != null && m.Suspensions.AsQueryable().Any(IsActiveSuspension)))
                && (!MemberTypeFilter.HasValue || m.MemberType == MemberTypeFilter)
            );
        }

        private void OnSuspendedFilterChange(ChangeEventArgs e)
            => SuspendedStatusFilter = Enum.Parse<SuspendedStatusEnum>(e.Value!.ToString()!);
        private void OnMemberTypeFilterChange(ChangeEventArgs e)
            => MemberTypeFilter = string.IsNullOrEmpty(e.Value.ToString()) ? null : Enum.Parse<MemberType>(e.Value!.ToString()!);

    }
}
