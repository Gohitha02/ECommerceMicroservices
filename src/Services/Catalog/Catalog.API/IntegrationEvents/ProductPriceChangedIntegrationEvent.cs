using EventBus.Events;

namespace Catalog.API.IntegrationEvents;

public record ProductPriceChangedIntegrationEvent : IntegrationEvent
{
    public Guid ProductId { get; }
    public string ProductName { get; }
    public decimal OldPrice { get; }
    public decimal NewPrice { get; }

    public ProductPriceChangedIntegrationEvent(Guid productId, string productName, decimal oldPrice, decimal newPrice)
    {
        ProductId = productId;
        ProductName = productName;
        OldPrice = oldPrice;
        NewPrice = newPrice;
    }
}