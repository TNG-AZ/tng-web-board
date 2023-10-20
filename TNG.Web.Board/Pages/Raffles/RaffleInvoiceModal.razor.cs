using Blazored.Modal;
using Blazored.Modal.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Square.Models;
using System.Runtime.CompilerServices;
using TNG.Web.Board.Data;
using TNG.Web.Board.Data.DTOs;
using TNG.Web.Board.Services;
using TNG.Web.Board.Utilities;

namespace TNG.Web.Board.Pages.Raffles
{
    public partial class RaffleInvoiceModal
    {
#nullable disable
        [Parameter]
        public Member InvoiceMember { get; set; }
        [Parameter]
        public Raffle Raffle { get; set; }

        [Inject]
        public IConfiguration Configuration { get; set; }
        [Inject]
        private ApplicationDbContext context { get; set; }
        [Inject]
        private SquareService square { get; set; }
        [Inject]
        private IJSRuntime js { get; set; }
        [CascadingParameter]
        BlazoredModalInstance BlazoredModal { get; set; }
#nullable enable

        private int IndividualTicketCount = 0;
        private int FiveTicketBundleCount = 0;
        private decimal AdditionalDonation = 0;
        private bool PrivateDonation = false;

        private const string TicketText = "Raffle Ticket";
        private const string BundleText = "Raffle Ticket x5 Bundle";
        private const string DonationText = "Donation";

        private async Task<IList<OrderLineItem>> GenerateLineItems()
        {
            var lineItems = new List<OrderLineItem>();
            if (IndividualTicketCount > 0)
            {
                lineItems.Add(await square.CreateLineItem(TicketText, IndividualTicketCount, Raffle.RaffleEntryCostCents, Configuration["SquareItems:RaffleTicket"]));
            }
            if (FiveTicketBundleCount > 0)
            {
                lineItems.Add(await square.CreateLineItem(BundleText, FiveTicketBundleCount, Raffle.RaffleEntryCostCents * 4, Configuration["SquareItems:RaffleTicketBundle"]));
            }
            if (AdditionalDonation > 0)
            {
                lineItems.Add(await square.CreateLineItem("Donation", 1, (long)(AdditionalDonation * 100)));
            }
            return lineItems;
        }


        private async Task SubmitInvoce()
        {
            var lineItems = await GenerateLineItems();
            if (!lineItems.Any())
            {
                await js.InvokeVoidAsync("alert", "must submit at least one line item");
                return;
            }
            try
            {
                var entry = new RaffleEntry()
                {
                    RaffleId = Raffle.RaffleId,
                    MemberId = InvoiceMember.Id,
                    EntryQuanity = Math.Max(IndividualTicketCount, 0) + (Math.Max(FiveTicketBundleCount, 0) * 5),
                    PrivateDonation = PrivateDonation
                };
                await context.RaffleEntries.AddAsync(entry);
                await context.SaveChangesAsync();

                await square.CreateInvoice(
                    InvoiceMember.EmailAddress,
                    lineItems,
                    Raffle.DrawingDate,
                    raffleEntryId: entry.RaffleEntryId);

                await BlazoredModal.CloseAsync(ModalResult.Ok());
            }
            catch (Exception ex) { }
        }
    }
}
