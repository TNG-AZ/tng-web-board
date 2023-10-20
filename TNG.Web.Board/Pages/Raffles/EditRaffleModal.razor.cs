using Blazored.Modal;
using Blazored.Modal.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore;
using Polly;
using TNG.Web.Board.Data;
using TNG.Web.Board.Data.DTOs;
using TNG.Web.Board.Data.Migrations;

namespace TNG.Web.Board.Pages.Raffles
{
    public partial class EditRaffleModal
    {
#nullable disable
        [Parameter]
        public Raffle Raffle { get; set; }
        [Parameter]
        public bool Insert { get; set; }

        [Inject]
        private ApplicationDbContext context { get; set; }
        [CascadingParameter]
        private BlazoredModalInstance Modal { get; set; }
#nullable enable
        private async Task SaveRaffle()
        {
            if (string.IsNullOrWhiteSpace(Raffle.ImageUrl))
                Raffle.ImageUrl = null;
            if (string.IsNullOrWhiteSpace(Raffle.FundraiserCause))
                Raffle.FundraiserCause =  null;

            context.Attach(Raffle);
            context.Entry(Raffle).State = Insert ? EntityState.Added : EntityState.Modified;

            await context.SaveChangesAsync();
            await Modal.CloseAsync(ModalResult.Ok());
        }
    }
}
