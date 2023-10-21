using Blazored.Modal;
using Blazored.Modal.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.JSInterop;
using Square.Models;
using TNG.Web.Board.Data;
using TNG.Web.Board.Data.DTOs;
using TNG.Web.Board.Data.Migrations;
using TNG.Web.Board.Pages.Membership;
using TNG.Web.Board.Utilities;

namespace TNG.Web.Board.Pages.Raffles
{
    public partial class List
    {

#nullable disable
        [Inject]
        private ApplicationDbContext context { get; set; }
        [Inject]
        private AuthUtilities auth { get; set; }
        [Inject]
        private NavigationManager navigation { get; set; }
        [Inject]
        private IJSRuntime js { get; set; }
        [CascadingParameter]
        private IModalService Modal { get; set; }
#nullable enable

        private List<Raffle>? _raffles { get; set; }
        private List<Raffle>? Raffles 
            => _raffles ?? context.Raffles
            .Include(r => r.Winner)
            .Include(r => r.Entries)
            .ThenInclude(e => e.Member)
            .Where(r => r.DrawingDate >= DateTime.UtcNow.AddDays(-15))?.ToList();

        private async void ShowEditRaffleModal(Raffle raffle, bool insert = false)
        {
            var parameters = new ModalParameters()
                .Add(nameof(EditRaffleModal.Raffle), raffle)
                .Add(nameof(EditRaffleModal.Insert), insert);
            var options = new ModalOptions()
            {
                Class = "blazored-modal size-large"
            };
            var modal = Modal.Show<EditRaffleModal>("Edit Raffle", parameters, options);
            var response = await modal.Result;
            if (response.Confirmed)
            {
                StateHasChanged();
            }
        }

        private async void ShowRaffleInvoiceModal(Raffle raffle)
        {
            try
            {
                shouldRender = false;
                var invoiceMemberEmail = auth.GetIdentity().Result?.Name;
                if (string.IsNullOrWhiteSpace(invoiceMemberEmail))
                {
                    navigation.NavigateTo("/Identity/Account/Login", true);
                }
                var invoiceMember = await context.Members.FirstOrDefaultAsync(m => EF.Functions.Like(m.EmailAddress, invoiceMemberEmail));
                if (invoiceMember == null)
                {
                    navigation.NavigateTo("/members/new", true);
                }
                var parameters = new ModalParameters()
                    .Add(nameof(RaffleInvoiceModal.Raffle), raffle)
                    .Add(nameof(RaffleInvoiceModal.InvoiceMember), invoiceMember);
                var options = new ModalOptions()
                {
                    Class = "blazored-modal size-large"
                };
                var modal = Modal.Show<RaffleInvoiceModal>("Buy Raffle Tickets", parameters, options);
                var response = await modal.Result;
                if (response.Confirmed)
                {
                    await js.InvokeVoidAsync("alert", "Please check your email for a donation invoice from Square.");
                    shouldRender = true;
                    StateHasChanged();
                }
            }
            finally
            {
                shouldRender = true;
            }
        }

        private async void ShowDrawRaffleModal(Raffle raffle)
        {
            var parameters = new ModalParameters()
                .Add(nameof(DrawRaffleWinnerModal.Raffle), raffle);
            var options = new ModalOptions()
            {
                Class = "blazored-modal size-large"
            };
            var modal = Modal.Show<DrawRaffleWinnerModal>("Draw Winner", parameters, options);
            var response = await modal.Result;
            if (response.Confirmed)
            {
                StateHasChanged();
            }
        }

        private bool shouldRender = true;

        protected override bool ShouldRender()
        {
            return shouldRender;
        }
    }
}
