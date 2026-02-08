using Blazored.Modal;
using Microsoft.AspNetCore.Components;
using Microsoft.EntityFrameworkCore.Infrastructure.Internal;
using Square;
using TNG.Web.Board.Data.DTOs;
using TNG.Web.Board.Services;
using TNG.Web.Board.Utilities;

namespace TNG.Web.Board.Pages.Shared
{
    public partial class Invoice
    {
#nullable disable
        [Parameter]
        public Member InvoiceMember { get; set; }
        [Parameter]
        public Google.Apis.Calendar.v3.Data.Event CalendarEvent { get; set; }
        [Inject]
        private SquareService square { get; set; }
        [CascadingParameter]
        BlazoredModalInstance BlazoredModal { get; set; }
#nullable enable
        private class InvoiceItem
        {
            public int Quantity { get; set; } = 0;
            public string Name { get; set; } = string.Empty;
            public long PricePerItem { get; set; } = 0;
        }

        private List<InvoiceItem> InvoiceItems { get; set; } = new();

        private DateTime DueDate { get; set; } = DateTime.Now.ToAZTime().AddDays(1);

        private void AddInvoiceItem()
        {
            InvoiceItems.Add(new());
        }
        private void RemoveInvoiceItem(InvoiceItem item)
        {
            InvoiceItems.Remove(item);
        }

        private async Task SubmitInvoce()
        {
            if (!InvoiceItems.Any() || InvoiceItems.Any(i => i.Quantity <= 0 || i.PricePerItem <=0 || string.IsNullOrEmpty(i.Name)))
            {
                return;
            }
            try
            {
                await square.CreateInvoice(
                InvoiceMember.EmailAddress,
                InvoiceItems.Select(i => new OrderLineItem() { Quantity = i.Quantity.ToString(), Name = i.Name, BasePriceMoney = new() { Amount = i.PricePerItem * 100, Currency = Currency.Usd } }).ToList(),
                DueDate);

                await BlazoredModal.CloseAsync();
            }
            catch (Exception ex) { }
        }
    }
}
