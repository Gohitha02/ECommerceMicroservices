using Ordering.API.Data;
using Ordering.API.IntegrationEvents.Events;
using Ordering.API.Models;
using EventBus.Abstractions;

namespace Ordering.API.IntegrationEvents.EventHandling;

public class BasketCheckoutIntegrationEventHandler : IIntegrationEventHandler<BasketCheckoutIntegrationEvent>
{
    private readonly OrderingDbContext _dbContext;
    private readonly ILogger<BasketCheckoutIntegrationEventHandler> _logger;

    public BasketCheckoutIntegrationEventHandler(
        OrderingDbContext dbContext,
        ILogger<BasketCheckoutIntegrationEventHandler> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task Handle(BasketCheckoutIntegrationEvent @event)
    {
        _logger.LogInformation("----- Handling integration event: {IntegrationEventId} at {AppName} - ({@IntegrationEvent})",
            @event.Id, "Ordering.API", @event);

        var order = new Order
        {
            Id = Guid.NewGuid(),
            CustomerName = @event.CustomerName,
            CustomerEmail = @event.CustomerEmail,
            ShippingAddress = @event.ShippingAddress,
            OrderDate = DateTime.UtcNow,
            Status = OrderStatus.Pending,
            TotalAmount = @event.TotalPrice,
            OrderItems = @event.Items.Select(i => new OrderItem
            {
                Id = Guid.NewGuid(),
                ProductId = i.ProductId,
                ProductName = i.ProductName,
                Quantity = i.Quantity,
                UnitPrice = i.Price
            }).ToList()
        };

        _dbContext.Orders.Add(order);
        await _dbContext.SaveChangesAsync();

        _logger.LogInformation("----- Order {OrderId} created for {CustomerName}", order.Id, order.CustomerName);
    }
}