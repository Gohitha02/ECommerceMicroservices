using Basket.API.Data;
using Basket.API.IntegrationEvents.Events;
using EventBus.Abstractions;

namespace Basket.API.IntegrationEvents.EventHandling;

public class ProductPriceChangedIntegrationEventHandler : IIntegrationEventHandler<ProductPriceChangedIntegrationEvent>
{
    private readonly IBasketRepository _repository;
    private readonly ILogger<ProductPriceChangedIntegrationEventHandler> _logger;

    public ProductPriceChangedIntegrationEventHandler(
        IBasketRepository repository,
        ILogger<ProductPriceChangedIntegrationEventHandler> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    public async Task Handle(ProductPriceChangedIntegrationEvent @event)
    {
        _logger.LogInformation("----- Handling integration event: {IntegrationEventId} at {AppName} - ({@IntegrationEvent})",
            @event.Id, "Basket.API", @event);

        _logger.LogInformation("Product price changed: {ProductId} from {OldPrice} to {NewPrice}",
            @event.ProductId, @event.OldPrice, @event.NewPrice);

        await Task.CompletedTask;
    }
}