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
        private static ISquareClient client;

        public SquareService(IConfiguration configuration) 
        {
            var env = bool.TryParse(configuration["SquareAPI:ProductionEnabled"], out var enableProd) && enableProd
                ? Square.Environment.Production
                : Square.Environment.Sandbox;

             client = new SquareClient.Builder()
            .Environment(env)
            .AccessToken(configuration["SquareAPI:Token"])
            .Build();
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
            var existing = customers.Customers?.FirstOrDefault(c => c.EmailAddress.Equals(customerEmail, StringComparison.OrdinalIgnoreCase));
            return existing
                ?? (await client.CustomersApi.CreateCustomerAsync(new(emailAddress: customerEmail))).Customer;
        }

        public async Task CreateInvoice(string email, IList<OrderLineItem> lineItems, DateTime paymentDueBy, Guid? invoiceId = null)
        {
            var location = await GetOrCreateLocation("The Next Generation - Arizona");
            var newOrder = new Order(
                locationId: location.Id,
                lineItems: lineItems,
                metadata: invoiceId.HasValue
                    ? new Dictionary<string,string>() { { "invoiceId", invoiceId!.ToString()} }
                    : null
                );
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

        public async Task<OrderLineItem> CreateLineItem(string itemName, int itemQuantity, long itemPrice, string? itemId = null)
        {
            if (itemId != null)
            {
                var catalogueItem = await client.CatalogApi.RetrieveCatalogObjectAsync(itemId);
                var variation = catalogueItem?.MObject.ItemData.Variations.FirstOrDefault();
                if (variation?.ItemVariationData.PriceMoney.Amount == itemPrice)
                {
                    return new(itemQuantity.ToString(), catalogObjectId: variation.Id);
                }
            }
            return new(quantity: itemQuantity.ToString(), name: itemName, basePriceMoney: new(itemPrice, "USD"));
        }

        public static async Task<IResult> HandleInvoicePaid(IConfiguration configuration, ApplicationDbContext context, InvoicePaidRequest request)
        {
            try
            {
                var env = bool.TryParse(configuration["SquareAPI:ProductionEnabled"], out var enableProd) && enableProd
                ? Square.Environment.Production
                : Square.Environment.Sandbox;

                var client = new SquareClient.Builder()
               .Environment(env)
               .AccessToken(configuration["SquareAPI:Token"])
               .Build();

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
            }
            catch { }
            return Results.Problem();
        }
    }
}
