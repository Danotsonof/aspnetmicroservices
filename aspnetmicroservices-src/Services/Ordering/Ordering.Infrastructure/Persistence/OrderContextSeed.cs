using Microsoft.Extensions.Logging;
using Ordering.Domain.Entities;

namespace Ordering.Infrastructure.Persistence
{
    public class OrderContextSeed
    {
        public static async Task SeedAsync(OrderContext orderContext, ILogger<OrderContextSeed> logger)
        {
            if (!orderContext.Orders.Any())
            {
                orderContext.Orders.AddRange(GetPreconfiguredOrders());
                await orderContext.SaveChangesAsync();
                logger.LogInformation("Seed database associated with context {DbContextName}", typeof(OrderContext).Name);
            }
        }

        private static IEnumerable<Order> GetPreconfiguredOrders()
        {
            return new List<Order>
            {
                new Order() {
                    UserName = "dummy01", 
                    FirstName = "Danot", 
                    LastName = "Sonof", 
                    EmailAddress = "dummy@email.com", 
                    AddressLine = "1, Dummy placeholder address.", 
                    Country = "India", 
                    TotalPrice = 1490 }
            };
        }
    }
}
