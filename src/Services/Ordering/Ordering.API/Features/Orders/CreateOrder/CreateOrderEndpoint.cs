using Carter;
using MediatR;
using Ordering.API.Data;
using Ordering.API.Models;

namespace Ordering.API.Features.Orders.CreateOrder;

public record CreateOrderCommand(
    string CustomerName,
    string CustomerEmail,
    string ShippingAddress,
    List<OrderItemDto> Items
) : IRequest<CreateOrderResult>;

public record OrderItemDto(Guid ProductId, string ProductName, int Quantity, decimal UnitPrice);

public record CreateOrderResult(Guid OrderId);

public class CreateOrderHandler : IRequestHandler<CreateOrderCommand, CreateOrderResult>
{
    private readonly OrderingDbContext _dbContext;

    public CreateOrderHandler(OrderingDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<CreateOrderResult> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        var order = new Order
        {
            Id = Guid.NewGuid(),
            CustomerName = request.CustomerName,
            CustomerEmail = request.CustomerEmail,
            ShippingAddress = request.ShippingAddress,
            OrderDate = DateTime.UtcNow,
            Status = OrderStatus.Pending,
            OrderItems = request.Items.Select(i => new OrderItem
            {
                Id = Guid.NewGuid(),
                ProductId = i.ProductId,
                ProductName = i.ProductName,
                Quantity = i.Quantity,
                UnitPrice = i.UnitPrice
            }).ToList()
        };

        order.TotalAmount = order.OrderItems.Sum(i => i.UnitPrice * i.Quantity);

        _dbContext.Orders.Add(order);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return new CreateOrderResult(order.Id);
    }
}

public class CreateOrderEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/orders", async (CreateOrderCommand command, ISender sender) =>
        {
            var result = await sender.Send(command);
            return Results.Created($"/api/orders/{result.OrderId}", result);
        })
        .WithName("CreateOrder")
        .Produces<CreateOrderResult>(StatusCodes.Status201Created)
        .WithSummary("Create Order")
        .WithDescription("Create a new order");
    }
}