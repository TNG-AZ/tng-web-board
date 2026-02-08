using Blazored.Modal;
using Blazored.Modal.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Square;
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
        [Inject]
        private IJSRuntime js { get; set; }
        [CascadingParameter]
        BlazoredModalInstance BlazoredModal { get; set; }
#nullable enable

        private int MembershipDuesCount = 0;
        private int PartyEntryMemberCount = 0;
        private int PartyEntryGuestCount = 0;
        private int Discount_VolunteerHalfCount = 0;
        private int Discount_VolunteerFullCount = 0;
        private DateTime? _dueDate { get; set; }
        private DateTime DueDate
        {
            get
            {
                return _dueDate ??= CalendarEvent.Start.DateTime.ToAZTime() ?? DateTime.Now.ToAZTime().AddDays(1);
            }
            set
            {
                _dueDate = value;
            }
        }

        private const string DuesText = "Memership Dues";
        private const string EntryMemberText = "Party Entry - Member";
        private const string EntryGuestText = "Party Entry - Guest";

        private async Task<IList<OrderLineItem>> GenerateLineItems()
        {
            var lineItems = new List<OrderLineItem>();
            if (MembershipDuesCount > 0) 
            {
                lineItems.Add(await square.CreateLineItem(DuesText, MembershipDuesCount, (long)Fees.MembershipDues * 100, Configuration["SquareItems:MembershipDues"]));
            }
            if (PartyEntryMemberCount > 0)
            {
                if (Math.Max(Discount_VolunteerHalfCount, 0) + Math.Max(Discount_VolunteerFullCount, 0) > 0)
                {
                    if (Discount_VolunteerFullCount > 0)
                    {
                        var discount = await square.GetOrCreateDiscount(Configuration["SquareItems:Discount_VolunteerFull"]);
                        if (discount == null)
                        {
                            throw new Exception("No entity defined for Discount_VonunteerFull");
                        }
                        lineItems.Add(await square.CreateLineItem(
                            EntryMemberText,
                            Discount_VolunteerFullCount, 
                            (long)Fees.MemberEntry * 100, 
                            Configuration["SquareItems:PartyMember"],
                            note: "With 100% discount for 2 volunteer shifts",
                            discountCatalogID: Configuration["SquareItems:Discount_VolunteerFull"]));
                    }
                    if (Discount_VolunteerHalfCount > 0)
                    {
                        var discount = await square.GetOrCreateDiscount(Configuration["SquareItems:Discount_VolunteerHalf"]);
                        if (discount == null)
                        {
                            throw new Exception("No entity defined for Discount_VonunteerHalf");
                        }
                        lineItems.Add(await square.CreateLineItem(
                            EntryMemberText,
                            Discount_VolunteerHalfCount,
                            (long)Fees.MemberEntry * 100,
                            Configuration["SquareItems:PartyMember"],
                            note: "With 50% discount for 1 volunteer shift",
                            discountCatalogID: Configuration["SquareItems:Discount_VolunteerHalf"]));
                    }
                }
                var diff = PartyEntryMemberCount - Math.Max(Discount_VolunteerFullCount, 0) - Math.Max(Discount_VolunteerHalfCount, 0);
                if (diff > 0)
                {
                    lineItems.Add(await square.CreateLineItem(EntryMemberText, diff, (long)Fees.MemberEntry * 100, Configuration["SquareItems:PartyMember"]));
                } 
            }
            if (PartyEntryGuestCount > 0)
            {
                lineItems.Add(await square.CreateLineItem(EntryGuestText, PartyEntryGuestCount, (long)Fees.GuestEntry * 100, Configuration["SquareItems:PartyGuest"]));
            }
            return lineItems;
        }

        private bool DisableSend { get; set; }

        private async Task SubmitInvoce()
        {
            var lineItems = await GenerateLineItems();
            if (!lineItems.Any())
            {
                await js.InvokeVoidAsync("alert", "must submit at least one line item");
                return;
            }
            if (Math.Max(Discount_VolunteerFullCount, 0) + Math.Max(Discount_VolunteerHalfCount, 0) > PartyEntryMemberCount)
            {
                await js.InvokeVoidAsync("alert", "Number of discounts cannot exceed number of member party entries");
                return;
            }
            try
            {
                DisableSend = true;
                StateHasChanged();
                var sending = true;
                while (sending)
                {
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

                        sending = false;
                        await BlazoredModal.CloseAsync(ModalResult.Ok());
                    }
                    catch
                    {
                        if (!await js.InvokeAsync<bool>("confirm", "Failed, try again?"))
                            sending = false;
                    }
                }
            }
            catch (Exception ex) { }
        }
    }
}
