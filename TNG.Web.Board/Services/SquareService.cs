using Square;
using System.Data;
using TNG.Web.Board.Data;
using TNG.Web.Board.Data.RequestModels;
using IConfiguration = Microsoft.Extensions.Configuration.IConfiguration;

namespace TNG.Web.Board.Services
{
    public class SquareService
    {
        private static ISquareClient? client;

        public SquareService(IConfiguration configuration) 
        {
            if (client is null)
                LoadClient(configuration);
        }
        private static void LoadClient(IConfiguration config)
        {
            var env = bool.TryParse(config["SquareAPI:ProductionEnabled"], out var enableProd) && enableProd
                ? SquareEnvironment.Production
                : SquareEnvironment.Sandbox;

            client ??= new SquareClient(config["SquareAPI:Token"], new() { BaseUrl = env });
        }
        public async Task<Location> GetOrCreateLocation(string locationName)
        {
            var locations = await client.Locations.ListAsync();
            var existing = locations.Locations.FirstOrDefault(l => l.Name.Equals(locationName, StringComparison.OrdinalIgnoreCase));
            return existing
                ?? (await client.Locations.CreateAsync(new() {  Location = new() {  Name = locationName} })).Location;
        }

        public async Task<Customer> GetOrCreateCustomer(string customerEmail)
        {
            var customers = await client.Customers.ListAsync(default);
            var existing = await customers.FirstOrDefaultAsync(c => c.EmailAddress != null && c.EmailAddress.Equals(customerEmail, StringComparison.OrdinalIgnoreCase));
            return existing
                ?? (await client.Customers.CreateAsync(new() {  EmailAddress = customerEmail})).Customer;
        }

        public async Task<CatalogObjectDiscount> GetOrCreateDiscount(string discountId, string discountName = "", int? percentOff = null, int? centsOff = null)
        {
            var discount = await client.Catalog.BatchGetAsync(new() { ObjectIds = [discountId] });
            var data = discount?.Objects.FirstOrDefault().AsDiscount();
            return data;
        }

        public async Task CreateInvoice(string email, IList<OrderLineItem> lineItems, DateTime paymentDueBy, Guid? invoiceId = null, Guid? raffleEntryId = null)
        {
            var location = await GetOrCreateLocation("The Next Generation - Arizona");
            var newOrder = new Order()
            {
                LocationId = location.Id,
                LineItems = lineItems,
                Discounts = lineItems
                    .Where(li => li.AppliedDiscounts != null)
                    .SelectMany(li => li.AppliedDiscounts)
                    .Select(d => new OrderLineItemDiscount() { CatalogObjectId = d.DiscountUid, Scope = OrderLineItemDiscountScope.LineItem }),
                Metadata = []
            };
            if (invoiceId != null)
            {
                newOrder.Metadata["invoiceId"] = invoiceId.Value.ToString();
            }
            if (raffleEntryId != null)
            {
                newOrder.Metadata["raffleEntryId"] =  raffleEntryId.Value.ToString();
            }
            var order = await client.Orders.CreateAsync(new() { Order = newOrder });
            var customer = await GetOrCreateCustomer(email);
            var newInvoice = new Invoice()
            {
                OrderId = order.Order.Id,
                PrimaryRecipient = new() { CustomerId = customer.Id },
                PaymentRequests = [new() { RequestType = InvoiceRequestType.Balance, DueDate = paymentDueBy.ToString("yyyy-MM-dd") }],
                DeliveryMethod = InvoiceDeliveryMethod.Email,
                AcceptedPaymentMethods = new() { Card = true }
            };
            var invoice = await client.Invoices.CreateAsync(new() { Invoice = newInvoice });
            await client.Invoices.PublishAsync(new() { InvoiceId = invoice.Invoice.Id, Version = invoice.Invoice.Version.Value});
        }

        public async Task<OrderLineItem> CreateLineItem(string itemName, int itemQuantity, long itemPrice, string? itemId = null, string? note = null, string discountCatalogID = null)
        {
            if (itemId != null)
            {
                var catalogueItem = await client.Catalog.BatchGetAsync(new() { ObjectIds = [itemId] });
                var variation = catalogueItem?.Objects.FirstOrDefault()?.AsItem().ItemData.Variations.FirstOrDefault(v => v.AsItemVariation().ItemVariationData.PriceMoney.Amount == itemPrice);
                if (variation is not null)
                {
                    return new()
                    {
                        Quantity = itemQuantity.ToString(),
                        CatalogObjectId = variation.AsItemVariation().Id,
                        AppliedDiscounts = !string.IsNullOrEmpty(discountCatalogID)
                            ? new OrderLineItemAppliedDiscount[]
                                {
                                    new() { DiscountUid = discountCatalogID }
                                }
                            : null,
                        Note = note
                    };
                }
            }
            return new() { Quantity = itemQuantity.ToString(), Name = itemName, BasePriceMoney = new() { Amount = itemPrice, Currency = Currency.Usd } };
        }

        public static async Task<IResult> HandleInvoicePaid(IConfiguration configuration, ApplicationDbContext context, InvoicePaidRequest request)
        {
            try
            {
                if (client is null)
                {
                    LoadClient(configuration);
                }

                var orderId = request.data._object.invoice.order_id;
                var order = await client.Orders.GetAsync(new() { OrderId = orderId });
                var metadata = order.Order.Metadata;

                if (metadata.TryGetValue("invoiceId", out var invoiceId))
                {
                    var invoice = context.EventsInvoices.First(i => i.Id == Guid.Parse(invoiceId));
                    invoice.PaidOnDate = DateTime.Parse(order.Order.ClosedAt);
                    await context.SaveChangesAsync();
                    return Results.Ok();
                }

                if (metadata.TryGetValue("raffleEntryId", out var raffleEntryId))
                {
                    var entry = context.RaffleEntries.First(e => e.RaffleEntryId == Guid.Parse(raffleEntryId));
                    entry.PaidOnDate = DateTime.Parse(order.Order.ClosedAt);
                    await context.SaveChangesAsync();
                    return Results.Ok();
                }
            }
            catch { }
            return Results.Problem();
        }
    }
}
