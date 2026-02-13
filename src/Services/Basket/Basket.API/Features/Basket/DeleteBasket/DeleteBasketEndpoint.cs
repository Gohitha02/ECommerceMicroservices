using Basket.API.Data;
using Carter;
using MediatR;

namespace Basket.API.Features.Basket.DeleteBasket;

public record DeleteBasketCommand(string UserName) : IRequest<DeleteBasketResult>;

public record DeleteBasketResult(bool IsSuccess);

public class DeleteBasketHandler : IRequestHandler<DeleteBasketCommand, DeleteBasketResult>
{
    private readonly IBasketRepository _repository;

    public DeleteBasketHandler(IBasketRepository repository)
    {
        _repository = repository;
    }

    public async Task<DeleteBasketResult> Handle(DeleteBasketCommand request, CancellationToken cancellationToken)
    {
        var result = await _repository.DeleteBasketAsync(request.UserName, cancellationToken);
        return new DeleteBasketResult(result);
    }
}

public class DeleteBasketEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapDelete("/api/basket/{userName}", async (string userName, ISender sender) =>
        {
            var command = new DeleteBasketCommand(userName);
            var result = await sender.Send(command);
            return Results.NoContent();
        })
        .WithName("DeleteBasket")
        .Produces(StatusCodes.Status204NoContent)
        .WithSummary("Delete Basket")
        .WithDescription("Delete shopping cart for a user");
    }
}