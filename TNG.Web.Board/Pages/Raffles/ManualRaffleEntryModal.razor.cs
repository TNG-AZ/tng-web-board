using Blazored.Modal;
using Blazored.Modal.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using TNG.Web.Board.Data;
using TNG.Web.Board.Data.DTOs;

namespace TNG.Web.Board.Pages.Raffles
{
    public partial class ManualRaffleEntryModal
    {
#nullable disable
        [Parameter]
        public Raffle Raffle { get; set; }

        [Inject]
        private ApplicationDbContext context { get; set; }
        [CascadingParameter]
        private BlazoredModalInstance Modal { get; set; }
#nullable enable
        private int EntryCount { get; set; }
        private string EntryEmail { get; set; }
        private async Task AddEntry()
        {
            if (EntryCount == 0 || string.IsNullOrWhiteSpace(EntryEmail))
            {
                return;
            }
            var member = await context.Members.FirstOrDefaultAsync(m => EF.Functions.Like(m.EmailAddress, EntryEmail));
            if (member == null)
            {
                return;
            }

            await context.RaffleEntries.AddAsync(new()
            {
                RaffleId = Raffle.RaffleId,
                MemberId = member.Id,
                EntryQuanity = EntryCount,
                EntryDate = DateTime.UtcNow,
                PaidOnDate = DateTime.UtcNow
            });

            await context.SaveChangesAsync();
            await Modal.CloseAsync(ModalResult.Ok());
        }
    }
}
