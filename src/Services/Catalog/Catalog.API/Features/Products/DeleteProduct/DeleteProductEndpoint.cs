using Carter;
using Marten;
using MediatR;

namespace Catalog.API.Features.Products.DeleteProduct;

public record DeleteProductCommand(Guid Id) : IRequest<DeleteProductResult>;

public record DeleteProductResult(bool IsSuccess);

public class DeleteProductHandler : IRequestHandler<DeleteProductCommand, DeleteProductResult>
{
    private readonly IDocumentSession _session;

    public DeleteProductHandler(IDocumentSession session)
    {
        _session = session;
    }

    public async Task<DeleteProductResult> Handle(DeleteProductCommand request, CancellationToken cancellationToken)
    {
        var product = await _session.LoadAsync<Models.Product>(request.Id, cancellationToken);

        if (product == null)
            return new DeleteProductResult(false);

        _session.Delete(product);
        await _session.SaveChangesAsync(cancellationToken);

        return new DeleteProductResult(true);
    }
}

public class DeleteProductEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapDelete("/api/products/{id:guid}", async (Guid id, ISender sender) =>
        {
            var command = new DeleteProductCommand(id);
            var result = await sender.Send(command);

            return result.IsSuccess ? Results.NoContent() : Results.NotFound();
        })
        .WithName("DeleteProduct")
        .Produces(StatusCodes.Status204NoContent)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .WithSummary("Delete Product")
        .WithDescription("Delete a product from the catalog");
    }
}