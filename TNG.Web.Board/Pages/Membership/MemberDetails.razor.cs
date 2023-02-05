using Microsoft.AspNetCore.Components;
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
            => context.Members.FirstOrDefault(m => m.Id == memberId) ?? new();

        private Member? _member { get; set; }
        private Member Member
        {
            get
            { return _member ??= GetMember(); }
        }

        private async void UpdateMember()
        {
            if (memberId.HasValue)
                context.Update(Member);
            else
                context.Add(Member);
            await context.SaveChangesAsync();
            memberId = Member.Id;
            navigation.NavigateTo("/members/");
        }

        private void MembershipTypeChange(ChangeEventArgs e)
        {
            var membershipType = (MemberType)int.Parse(e.Value!.ToString()!);
            Member.MemberType = membershipType;
        }
    }
}
