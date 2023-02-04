using Microsoft.AspNetCore.Components;
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
        private ApplicationDbContext _context { get; set; }
#nullable enable

        private string? SceneNameFilter { get; set; }
        private string? LegalNameFilter { get; set; }
        private string? EmailFilter { get; set; }
        private SuspendedStatusEnum? SuspendedStatus { get; set; }

        private Expression<Func<MembershipSuspensions, bool>> IsActiveSuspension
            = (m) => m.EndDate == null || m.EndDate >= DateTime.Now;

        private IEnumerable<Member> GetFilteredMembers()
            => _context.Members
            .Where(m =>
                (string.IsNullOrEmpty(SceneNameFilter) || m.SceneName.Contains(SceneNameFilter, StringComparison.OrdinalIgnoreCase))
                && (string.IsNullOrEmpty(LegalNameFilter) || m.LegalName.Contains(LegalNameFilter, StringComparison.OrdinalIgnoreCase))
                && (string.IsNullOrEmpty(EmailFilter) || m.EmailAddress.Contains(EmailFilter, StringComparison.OrdinalIgnoreCase))
                && (!SuspendedStatus.HasValue || SuspendedStatus.Value == SuspendedStatusEnum.All
                    || (SuspendedStatus.Value == SuspendedStatusEnum.No && !m.Suspensions.Any(IsActiveSuspension))
                    || (SuspendedStatus.Value == SuspendedStatusEnum.Yes && m.Suspensions.Any(IsActiveSuspension))
            ));
    }
}
