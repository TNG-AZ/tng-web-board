using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using TNG.Web.Board.Data;
using TNG.Web.Board.Data.DTOs;

namespace TNG.Web.Board.Pages.Membership
{
    public partial class MemberDetails
    {
        [Parameter]
        public Guid? memberId { get; set; }


#nullable disable
        [Inject]
        private ApplicationDbContext context { get; set; }

        [Inject]
        private NavigationManager navigation { get; set; }
#nullable enable

        private Member GetMember()
            => context.Members
            .Include(m => m.Suspensions)
            .Include(m => m.Notes)
            .Include(m => m.Payments)
            .Include(m => m.Orientations)
            .FirstOrDefault(m => m.Id == memberId) ?? new();

        private Member? _member { get; set; }
        private Member Member
        {
            get
            { return _member ??= GetMember(); }
        }

        private bool AddDuesPaidToggle { get; set; } = false;
        private DateTime? NewDuesPaid { get; set; }

        private bool AddOrientationDateToggle { get; set; } = false;
        private DateTime? NewOrientationAttended { get; set; }

        private bool AddSuspensionToggle { get; set; } = false;
        private DateTime? NewSuspensionStartDate { get; set; }
        private DateTime? NewSuspensionEndDate { get; set; }
        private string? NewSuspensionReason { get; set; }

        private bool AddNoteToggle { get; set; } = false;
        private string? NewNote { get; set; }
        private bool ViewNotesToggle { get; set; } = false;

        private async void UpdateMember()
        {
            if (memberId.HasValue)
                context.Update(Member);
            else
                context.Add(Member);
            await context.SaveChangesAsync();

            if (NewDuesPaid.HasValue)
            {
                context.Add(new MembershipPayment { MemberId = Member.Id, PaidOn = NewDuesPaid.Value });
                await context.SaveChangesAsync();
            }
            if (NewOrientationAttended.HasValue)
            {
                context.Add(new MembershipOrientation { MemberId = Member.Id, DateReceived = NewOrientationAttended.Value });
                await context.SaveChangesAsync();
            }
            if (NewSuspensionStartDate.HasValue)
            {
                context.Add(new MembershipSuspension { MemberId = Member.Id, StartDate = NewSuspensionStartDate.Value, EndDate = NewSuspensionEndDate, Reason = NewSuspensionReason });
                await context.SaveChangesAsync();
            }
            if (!string.IsNullOrEmpty(NewNote))
            {
                context.Add(new MembershipNote { MemberId = Member.Id, Note = NewNote });
                await context.SaveChangesAsync();
            }

            navigation.NavigateTo("/members/");
        }

        private void MembershipTypeChange(ChangeEventArgs e)
        {
            var membershipType = (MemberType)int.Parse(e.Value!.ToString()!);
            Member.MemberType = membershipType;
        }

        private static string SuspensionDisplay(MembershipSuspension? suspension)
        {
            if (suspension != null)
                return suspension.EndDate.HasValue
                ? $"Suspended until {suspension.EndDate.Value:MM/dd/yyy}"
                : "Blacklisted";
            return "In good standing";
        }
    }
}
