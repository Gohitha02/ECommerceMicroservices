using EventBus.Events;

namespace Ordering.API.IntegrationEvents.Events;

public record BasketCheckoutIntegrationEvent : IntegrationEvent
{
    public string UserName { get; }
    public string CustomerName { get; }
    public string CustomerEmail { get; }
    public string ShippingAddress { get; }
    public decimal TotalPrice { get; }
    public List<BasketItem> Items { get; }

    public BasketCheckoutIntegrationEvent(
        string userName,
        string customerName,
        string customerEmail,
        string shippingAddress,
        decimal totalPrice,
        List<BasketItem> items)
    {
        UserName = userName;
        CustomerName = customerName;
        CustomerEmail = customerEmail;
        ShippingAddress = shippingAddress;
        TotalPrice = totalPrice;
        Items = items;
    }
}

public record BasketItem(
    Guid ProductId,
    string ProductName,
    int Quantity,
    decimal Price
);
