using Basket.API.Data;
using Carter;
using MediatR;

namespace Basket.API.Features.Basket.GetBasket;

public record GetBasketQuery(string UserName) : IRequest<GetBasketResult>;

public record GetBasketResult(Models.ShoppingCart Cart);

public class GetBasketHandler : IRequestHandler<GetBasketQuery, GetBasketResult?>
{
    private readonly IBasketRepository _repository;

    public GetBasketHandler(IBasketRepository repository)
    {
        _repository = repository;
    }

    public async Task<GetBasketResult?> Handle(GetBasketQuery request, CancellationToken cancellationToken)
    {
        var basket = await _repository.GetBasketAsync(request.UserName, cancellationToken);

        return basket == null ? null : new GetBasketResult(basket);
    }
}

public class GetBasketEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/basket/{userName}", async (string userName, ISender sender) =>
        {
            var query = new GetBasketQuery(userName);
            var result = await sender.Send(query);

            return result == null ? Results.NotFound() : Results.Ok(result);
        })
        .WithName("GetBasket")
        .Produces<GetBasketResult>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .WithSummary("Get Basket")
        .WithDescription("Get shopping cart for a user");
    }
}