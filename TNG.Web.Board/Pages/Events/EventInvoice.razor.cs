using Blazored.Modal;
using Blazored.Modal.Services;
using Microsoft.AspNetCore.Components;
using Square.Models;
using System.Runtime.CompilerServices;
using TNG.Web.Board.Data;
using TNG.Web.Board.Data.DTOs;
using TNG.Web.Board.Services;
using TNG.Web.Board.Utilities;

namespace TNG.Web.Board.Pages.Events
{
    public partial class EventInvoice
    {
#nullable disable
        [Parameter]
        public Member InvoiceMember { get; set; }
        [Parameter]
        public Google.Apis.Calendar.v3.Data.Event CalendarEvent { get; set; }
        [Parameter]
        public EventFees Fees { get; set; }
        [Inject]
        public IConfiguration Configuration { get; set; }
        [Inject]
        private ApplicationDbContext context { get; set; }
        [Inject]
        private SquareService square { get; set; }
        [CascadingParameter]
        BlazoredModalInstance BlazoredModal { get; set; }
#nullable enable

        private int MembershipDuesCount = 0;
        private int PartyEntryMemberCount = 0;
        private int PartyEntryGuestCount = 0;
        private DateTime DueDate { get; set; } = DateTime.Now.ToAZTime().AddDays(1);

        private const string DuesText = "Memership Dues";
        private const string EntryMemberText = "Party Entry - Member";
        private const string EntryGuestText = "Party Entry - Guest";

        private async Task<IList<OrderLineItem>> GenerateLineItems()
        {
            var lineItems = new List<OrderLineItem>();
            if (Math.Max(MembershipDuesCount, 0) > 0) 
            {
                lineItems.Add(await square.CreateLineItem(DuesText, MembershipDuesCount, (long)Fees.MembershipDues * 100, Configuration["SquareItems:MembershipDues"]));
            }
            if (Math.Max(PartyEntryMemberCount, 0) > 0)
            {
                lineItems.Add(await square.CreateLineItem(EntryMemberText, PartyEntryMemberCount, (long)Fees.MemberEntry * 100, Configuration["SquareItems:PartyMember"]));
            }
            if (Math.Max(PartyEntryGuestCount, 0) > 0)
            {
                lineItems.Add(await square.CreateLineItem(EntryGuestText, PartyEntryGuestCount, (long)Fees.GuestEntry * 100, Configuration["SquareItems:PartyGuest"]));
            }
            return lineItems;
        }


        private async Task SubmitInvoce()
        {
            var lineItems = await GenerateLineItems();
            if (!lineItems.Any())
            {
                return;
            }
            try
            {
                var invoiceRef = new Data.DTOs.EventInvoice() { EventId = CalendarEvent.Id, MemberId = InvoiceMember.Id };
                await context.EventsInvoices.AddAsync(invoiceRef);
                await context.SaveChangesAsync();

                await square.CreateInvoice(
                    InvoiceMember.EmailAddress,
                    lineItems,
                    DueDate,
                    invoiceId: invoiceRef.Id);

                await BlazoredModal.CloseAsync(ModalResult.Ok());
            }
            catch (Exception ex) { }
        }
    }
}
