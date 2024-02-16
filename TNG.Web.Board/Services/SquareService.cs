using System;
using Square.Models;
using Square.Exceptions;
using System.Threading.Tasks;
using Azure.Core;
using Square;
using IConfiguration = Microsoft.Extensions.Configuration.IConfiguration;
using Azure.Core.Pipeline;
using System.Data;
using System.Runtime.CompilerServices;
using TNG.Web.Board.Data.RequestModels;
using TNG.Web.Board.Data;

namespace TNG.Web.Board.Services
{
    public class SquareService
    {
        private static ISquareClient? client;

        public SquareService(IConfiguration configuration) 
        {
            if (client is null)
            {
                var env = bool.TryParse(configuration["SquareAPI:ProductionEnabled"], out var enableProd) && enableProd
                ? Square.Environment.Production
                : Square.Environment.Sandbox;

                client ??= new SquareClient.Builder()
               .Environment(env)
               .AccessToken(configuration["SquareAPI:Token"])
               .Build();
            }
        }

        public async Task<Location> GetOrCreateLocation(string locationName)
        {
            var locations = await client.LocationsApi.ListLocationsAsync();
            var existing = locations.Locations.FirstOrDefault(l => l.Name.Equals(locationName, StringComparison.OrdinalIgnoreCase));
            return existing
                ?? (await client.LocationsApi.CreateLocationAsync(new(new(name: locationName)))).Location;
        }

        public async Task<Customer> GetOrCreateCustomer(string customerEmail)
        {
            var customers = await client.CustomersApi.ListCustomersAsync();
            var existing = customers.Customers?.FirstOrDefault(c => c.EmailAddress != null && c.EmailAddress.Equals(customerEmail, StringComparison.OrdinalIgnoreCase));
            return existing
                ?? (await client.CustomersApi.CreateCustomerAsync(new(emailAddress: customerEmail))).Customer;
        }

        public async Task<CatalogDiscount> GetOrCreateDiscount(string discountId, string discountName = "", int? percentOff = null, int? centsOff = null)
        {
            var discount = await client.CatalogApi.RetrieveCatalogObjectAsync(discountId);
            var data = discount?.MObject.DiscountData;
            return data;
        }

        public async Task CreateInvoice(string email, IList<OrderLineItem> lineItems, DateTime paymentDueBy, Guid? invoiceId = null, Guid? raffleEntryId = null)
        {
            var location = await GetOrCreateLocation("The Next Generation - Arizona");
            var newOrder = new Order(
                locationId: location.Id,
                lineItems: lineItems,
                metadata: new Dictionary<string, string>(),
                discounts: lineItems
                    .Where(li => li.AppliedDiscounts != null)
                    .SelectMany(li => li.AppliedDiscounts)
                    .Select(d => new OrderLineItemDiscount(uid: d.DiscountUid, catalogObjectId:d.DiscountUid, scope:"LINE_ITEM"))
                    .ToList()
            );
            if (invoiceId != null)
            {
                newOrder.Metadata.Add(new("invoiceId", invoiceId.Value.ToString()));
            }
            if (raffleEntryId != null)
            {
                newOrder.Metadata.Add(new("raffleEntryId", raffleEntryId.Value.ToString()));
            }
            var order = await client.OrdersApi.CreateOrderAsync(new(newOrder));
            var customer = await GetOrCreateCustomer(email);
            var newInvoice = new Invoice(
                orderId: order.Order.Id,
                primaryRecipient: new(customerId: customer.Id),
                paymentRequests: new List<InvoicePaymentRequest>()
                {
                    new(requestType:"BALANCE", dueDate:paymentDueBy.ToString("yyyy-MM-dd"))
                },
                deliveryMethod: "EMAIL",
                acceptedPaymentMethods: new(card: true));
            var invoice = await client.InvoicesApi.CreateInvoiceAsync(new(newInvoice));
            await client.InvoicesApi.PublishInvoiceAsync(invoice.Invoice.Id, new(invoice.Invoice.Version!.Value));
        }

        public async Task<OrderLineItem> CreateLineItem(string itemName, int itemQuantity, long itemPrice, string? itemId = null, string? note = null, string discountCatalogID = null)
        {
            if (itemId != null)
            {
                var catalogueItem = await client.CatalogApi.RetrieveCatalogObjectAsync(itemId);
                var variation = catalogueItem?.MObject.ItemData.Variations.FirstOrDefault(v => v.ItemVariationData.PriceMoney.Amount == itemPrice);
                if (variation is not null)
                {
                    return new(itemQuantity.ToString(), 
                        catalogObjectId: variation.Id, 
                        appliedDiscounts: !string.IsNullOrEmpty(discountCatalogID)
                            ? new OrderLineItemAppliedDiscount[]
                                {
                                    new OrderLineItemAppliedDiscount(discountCatalogID, uid:discountCatalogID)
                                }
                            : null,
                        note: note
                    );
                }
            }
            return new(quantity: itemQuantity.ToString(), name: itemName, basePriceMoney: new(itemPrice, "USD"));
        }

        public static async Task<IResult> HandleInvoicePaid(IConfiguration configuration, ApplicationDbContext context, InvoicePaidRequest request)
        {
            try
            {
                if (client is null)
                {
                    var env = bool.TryParse(configuration["SquareAPI:ProductionEnabled"], out var enableProd) && enableProd
                    ? Square.Environment.Production
                    : Square.Environment.Sandbox;

                    client ??= new SquareClient.Builder()
                   .Environment(env)
                   .AccessToken(configuration["SquareAPI:Token"])
                   .Build();
                }

                var orderId = request.data._object.invoice.order_id;
                var order = client.OrdersApi.RetrieveOrder(orderId);
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
