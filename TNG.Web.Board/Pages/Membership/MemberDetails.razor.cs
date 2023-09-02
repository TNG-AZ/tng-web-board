using Azure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using Microsoft.JSInterop;
using TNG.Web.Board.Data;
using TNG.Web.Board.Data.DTOs;

namespace TNG.Web.Board.Pages.Membership
{
    [Authorize(Roles = "Boardmember")]
    public partial class MemberDetails
    {
        [Parameter]
        public Guid? memberId { get; set; }


#nullable disable
        [Inject]
        private ApplicationDbContext context { get; set; }

        [Inject]
        private NavigationManager navigation { get; set; }
        [Inject]
        private IJSRuntime JsRuntime { get; set; }
#nullable enable

        private Member GetMember()
            => context.Members?
            .Include(m => m.Suspensions)
            .Include(m => m.Notes)
            .ThenInclude(n => n.NoteTags)
            .ThenInclude(n => n.Tag)
            .Include(m => m.Payments)
            .Include(m => m.Orientations)
            .Include(m => m.MemberDiscords)
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
        private string? NewNoteTags { get; set; }
        private bool ViewNotesToggle { get; set; } = false;

        private string? NotesTagsFilter { get; set; }

        private static IEnumerable<string> GetTags(string tags)
            => tags.ToLower().Split(',').Select(t => t.Trim()).Where(t => !string.IsNullOrEmpty(t)).Distinct();

        private IEnumerable<MembershipNote> GetFilteredNotes()
        {
            var filterTags = Enumerable.Empty<string>();
            if (!string.IsNullOrEmpty(NotesTagsFilter))
            {
                filterTags = GetTags(NotesTagsFilter);
            }
            return Member.Notes.Where(n => !filterTags.Any() || (n.NoteTags?.Any(t => filterTags.Contains(t.Tag.Name.ToLower())) ?? false));
        }

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

        private async Task<MembershipNote> AddNote(string note)
        {
            if (!string.IsNullOrEmpty(note))
            {
                var noteEntity = context.MemberNotes.Add(new MembershipNote
                {
                    MemberId = Member.Id,
                    Note = note
                }).Entity;
                await context.SaveChangesAsync();

                return noteEntity;
            }
            return await Task.FromResult<MembershipNote>(default);
        }

        private async void AddMemberNote()
        {
            if (!string.IsNullOrEmpty(NewNote))
            {
                var note = await AddNote(NewNote);
                NewNote = string.Empty;

                if (!string.IsNullOrEmpty(NewNoteTags))
                {
                    var tags = GetTags(NewNoteTags);

                    var newTags = tags.Where(t => !(context.Tags?.Select(t => t.Name).Any(n => EF.Functions.Like(n, t)) ?? false));
                    context.AddRange(newTags.Select(t => new Tag { Name = t }));
                    await context.SaveChangesAsync();

                    var tagEntities = context.Tags.Where(e => tags.Contains(e.Name.ToLower()));
                    context.AddRange(tagEntities.Select(t => new NoteTag { TagId = t.Id, NoteId = note.Id }));
                    await context.SaveChangesAsync();
                }
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

        private async void DeleteMember()
        {
            bool confirmed = await JsRuntime.InvokeAsync<bool>("confirm", "Are you sure?");
            if (confirmed)
            {
                context.MemberOrientations.RemoveRange(Member.Orientations);
                context.MemberDuesPayments.RemoveRange(Member.Payments);
                context.MemberNotes.RemoveRange(Member.Notes);
                context.MemberSuspensions.RemoveRange(Member.Suspensions);
                context.EventRsvpPlusOnes.RemoveRange(Member.RsvpPlusOnesAsGuest);
                context.EventRsvpPlusOnes.RemoveRange(Member.RsvpPlusOnesAsPrimary);
                context.Members.Remove(Member);
                await context.SaveChangesAsync();
                await JsRuntime.InvokeVoidAsync("alert", "Successfully deleted");
                navigation.NavigateTo("/members/");
            }
        }

        private long? NewDiscordId { get; set; }

        private async Task AddDiscordId()
        {
            if (!NewDiscordId.HasValue)
                return;
            try
            {
                context.Add(new MemberDiscordIntegration() {  MemberId = Member.Id, DiscordId = NewDiscordId.Value });
                await context.SaveChangesAsync();
                NewDiscordId = null;
                StateHasChanged();
            }
            finally
            {
            }
        }

        private async Task UnlinkDiscord(Guid linkId)
        {
            context.Remove(context.MembersDiscordIntegrations.First(d => d.Id == linkId));
            await context.SaveChangesAsync();
            StateHasChanged();
        }
    }
}
