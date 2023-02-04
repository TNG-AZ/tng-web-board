using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
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

    public partial class ViewMembers
    {
#nullable disable
        [Inject]
        private ApplicationDbContext context { get; set; }
#nullable enable

        private string? SceneNameFilter { get; set; }
        private string? LegalNameFilter { get; set; }
        private string? EmailFilter { get; set; }
        private SuspendedStatusEnum? SuspendedStatus { get; set; }

        private static readonly Expression<Func<MembershipSuspensions, bool>> IsActiveSuspension
            = (m) => m.EndDate == null || m.EndDate >= DateTime.Now;

        private static string SuspensionDisplay(Member member)
        {
            var suspension = member?.Suspensions?
                .OrderByDescending(s => s.EndDate ?? DateTime.MaxValue)
                .FirstOrDefault(s => !s.EndDate.HasValue || s.EndDate >= DateTime.Now);
            if (suspension != null)
                return suspension.EndDate.HasValue
                ? $"Suspended until {suspension.EndDate.Value:MM/dd/yyy}"
                : "Blacklisted";
            return string.Empty;
        }

        private IEnumerable<Member> GetFilteredMembers()
            => context.Members
            .Where(m =>
                (string.IsNullOrEmpty(SceneNameFilter) || EF.Functions.Like(m.SceneName, $"%{SceneNameFilter}%"))
                && (string.IsNullOrEmpty(LegalNameFilter) || EF.Functions.Like(m.LegalName, $"%{LegalNameFilter}%"))
                && (string.IsNullOrEmpty(EmailFilter) || EF.Functions.Like(m.EmailAddress, $"%{EmailFilter}%"))
                && (!SuspendedStatus.HasValue || SuspendedStatus.Value == SuspendedStatusEnum.All
                    || (SuspendedStatus.Value == SuspendedStatusEnum.No && !m.Suspensions.Any(IsActiveSuspension))
                    || (SuspendedStatus.Value == SuspendedStatusEnum.Yes && m.Suspensions.Any(IsActiveSuspension))));
    }
}
