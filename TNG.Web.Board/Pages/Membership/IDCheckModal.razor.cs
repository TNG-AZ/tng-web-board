using Blazored.Modal;
using Microsoft.AspNetCore.Components;
using TNG.Web.Board.Data.DTOs;
using TNG.Web.Board.Data;
using Blazored.Modal.Services;
using Microsoft.EntityFrameworkCore;

namespace TNG.Web.Board.Pages.Membership
{
    public partial class IDCheckModal
    {
#nullable disable
        [Parameter]
        public Member UserMember { get; set; }

        [Inject]
        private ApplicationDbContext context { get; set; }
        [CascadingParameter]
        private BlazoredModalInstance Modal { get; set; }
#nullable enable

        private async Task SaveUser()
        {
            context.Attach(UserMember);
            context.Entry(UserMember).State = EntityState.Modified;

            await context.SaveChangesAsync();
            await Modal.CloseAsync(ModalResult.Ok());
        }
    }
}
