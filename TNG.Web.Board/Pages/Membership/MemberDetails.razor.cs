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

        private MembershipPayment? Dues => Member?.Payments?.OrderByDescending(p => p.PaidOn).FirstOrDefault();
        private MembershipOrientation? Orientation => Member?.Orientations?.OrderByDescending(o => o.DateReceived).FirstOrDefault();

        private DateTime? PaidOnDate => Dues?.PaidOn;
        private DateTime? OrientationOnDate => Orientation?.DateReceived;
        private MembershipSuspension? ActiveSuspension => Member?.Suspensions?.OrderByDescending(s => s.EndDate ?? DateTime.MaxValue).FirstOrDefault(s => !s.EndDate.HasValue || s.EndDate >= DateTime.Now);


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
            try
            {
                if (memberId.HasValue)
                    context.Update(Member);
                else
                    context.Add(Member);

                if (NewDuesPaid.HasValue)
                {
                    context.Add(new MembershipPayment { MemberId = Member.Id, PaidOn = NewDuesPaid.Value });
                }
                if (NewOrientationAttended.HasValue)
                {
                    context.Add(new MembershipOrientation { MemberId = Member.Id, DateReceived = NewOrientationAttended.Value });
                }
            }
            finally
            {
                await context.SaveChangesAsync();
                navigation.NavigateTo("/members/");
            }
        }

        private void MembershipTypeChange(ChangeEventArgs e)
        {
            var membershipType = (MemberType)int.Parse(e.Value!.ToString()!);
            Member.MemberType = membershipType;
        }

        private string SuspensionDisplay()
        {
            if (ActiveSuspension != null)
                return ActiveSuspension.EndDate.HasValue
                ? $"Suspended until {ActiveSuspension.EndDate.Value:MM/dd/yyy}"
                : "Blacklisted";
            return "In good standing";
        }

        private async void AddSuspension()
        {
            if (NewSuspensionStartDate.HasValue)
            {
                var note = (NewSuspensionEndDate.HasValue
                        ? $"Member suspended from {NewSuspensionStartDate.Value:MM/dd/yyyy} until {NewSuspensionEndDate.Value:MM/dd/yyyy}"
                        : $"Member is blacklisted starting {NewSuspensionStartDate.Value:MM/dd/yyyy}")
                        + (!string.IsNullOrEmpty(NewSuspensionReason) ? $" Reason - {NewSuspensionReason}" : string.Empty);
                await AddNote(note);

                context.Add(new MembershipSuspension { MemberId = Member.Id, StartDate = NewSuspensionStartDate.Value, EndDate = NewSuspensionEndDate, Reason = NewSuspensionReason });
                await context.SaveChangesAsync();

                NewSuspensionStartDate = null;
                NewSuspensionEndDate = null;
                NewSuspensionReason = null;

                AddSuspensionToggle = false;

                StateHasChanged();
            }
        }

        private async void DeleteSuspension()
        {
            await AddNote((ActiveSuspension!.EndDate.HasValue ? "Suspension" : "Blacklist") + " removed from member");

            context.Remove(ActiveSuspension);

            await context.SaveChangesAsync();

            StateHasChanged();
        }

        private async void EndSuspension()
        {
            await AddNote((ActiveSuspension!.EndDate.HasValue ? "Suspension" : "Blacklist") + " ended early");

            ActiveSuspension.EndDate = DateTime.Now;

            await context.SaveChangesAsync();

            StateHasChanged();
        }

        private async Task AddNote(string note)
        {
            if (!string.IsNullOrEmpty(note))
            {
                context.Add(new MembershipNote
                {
                    MemberId = Member.Id,
                    Note = note
                });
                await context.SaveChangesAsync();
            }
        }

        private async void AddMemberNote()
        {
            if (!string.IsNullOrEmpty(NewNote))
            {
                await AddNote(NewNote);
                NewNote = string.Empty;
                StateHasChanged();
            }
        }

        private async void DeleteDues()
        {
            if (Dues != null)
            {
                await AddNote($"Removed record of dues paid, previously {Dues.PaidOn:MM/dd/yyyy}");
                context.Remove(Dues);
                await context.SaveChangesAsync();
                StateHasChanged();
            }
        }

        private async void DeleteOrientation()
        {
            if (Orientation != null)
            {
                await AddNote($"Removed record of dues paid, previously {Orientation.DateReceived:MM/dd/yyyy}");
                context.Remove(Orientation);
                await context.SaveChangesAsync();
                StateHasChanged();
            }
        }
    }
}
