using Carter;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Ordering.API.Data;
using Ordering.API.Models;

namespace Ordering.API.Features.Orders.GetOrders;

public record GetOrdersQuery(int PageNumber = 1, int PageSize = 10) : IRequest<GetOrdersResult>;

public record GetOrdersResult(List<OrderDto> Orders);

public record OrderDto(
    Guid Id,
    string CustomerName,
    DateTime OrderDate,
    OrderStatus Status,
    decimal TotalAmount,
    int ItemCount
);

public class GetOrdersHandler : IRequestHandler<GetOrdersQuery, GetOrdersResult>
{
    private readonly OrderingDbContext _dbContext;

    public GetOrdersHandler(OrderingDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<GetOrdersResult> Handle(GetOrdersQuery request, CancellationToken cancellationToken)
    {
        var orders = await _dbContext.Orders
            .AsNoTracking()
            .OrderByDescending(o => o.OrderDate)
            .Skip((request.PageNumber - 1) * request.PageSize)
            .Take(request.PageSize)
            .Select(o => new OrderDto(
                o.Id,
                o.CustomerName,
                o.OrderDate,
                o.Status,
                o.TotalAmount,
                o.OrderItems.Count
            ))
            .ToListAsync(cancellationToken);

        return new GetOrdersResult(orders);
    }
}

public class GetOrdersEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/orders", async ([AsParameters] GetOrdersQuery query, ISender sender) =>
        {
            var result = await sender.Send(query);
            return Results.Ok(result);
        })
        .WithName("GetOrders")
        .Produces<GetOrdersResult>(StatusCodes.Status200OK)
        .WithSummary("Get Orders")
        .WithDescription("Get paginated list of orders");
    }
}