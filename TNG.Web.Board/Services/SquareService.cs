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

namespace TNG.Web.Board.Services
{
    public class SquareService
    {
        private ISquareClient client;

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
            var existing = customers.Customers.FirstOrDefault(c => c.EmailAddress.Equals(customerEmail, StringComparison.OrdinalIgnoreCase));
            return existing
                ?? (await client.CustomersApi.CreateCustomerAsync(new(emailAddress: customerEmail))).Customer;
        }

        public async Task CreateInvoice(string email, IList<OrderLineItem> lineItems, DateTime paymentDueBy)
        {
            var location = await GetOrCreateLocation("TNG AZ");
            var newOrder = new Order(
                locationId: location.Id,
                lineItems: lineItems);
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
    }
}
