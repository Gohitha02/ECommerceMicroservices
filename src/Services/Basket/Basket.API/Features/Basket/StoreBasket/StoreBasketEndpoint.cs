using Basket.API.Data;
using Carter;
using MediatR;

namespace Basket.API.Features.Basket.StoreBasket;

public record StoreBasketCommand(Models.ShoppingCart Cart) : IRequest<StoreBasketResult>;

public record StoreBasketResult(string UserName);

public class StoreBasketHandler : IRequestHandler<StoreBasketCommand, StoreBasketResult>
{
    private readonly IBasketRepository _repository;

    public StoreBasketHandler(IBasketRepository repository)
    {
        _repository = repository;
    }

    public async Task<StoreBasketResult> Handle(StoreBasketCommand request, CancellationToken cancellationToken)
    {
        await _repository.StoreBasketAsync(request.Cart, cancellationToken);
        return new StoreBasketResult(request.Cart.UserName);
    }
}

public class StoreBasketEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/basket", async (Models.ShoppingCart cart, ISender sender) =>
        {
            var command = new StoreBasketCommand(cart);
            var result = await sender.Send(command);
            return Results.Created($"/api/basket/{result.UserName}", result);
        })
        .WithName("StoreBasket")
        .Produces<StoreBasketResult>(StatusCodes.Status201Created)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .WithSummary("Store Basket")
        .WithDescription("Store or update shopping cart");
    }
}