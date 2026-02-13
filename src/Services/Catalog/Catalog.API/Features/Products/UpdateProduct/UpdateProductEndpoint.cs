using Carter;
using Catalog.API.Models.Requests;
using Mapster;
using Marten;
using MediatR;

namespace Catalog.API.Features.Products.UpdateProduct;

public record UpdateProductCommand(
    Guid Id,
    string Name,
    string Description,
    string Category,
    decimal Price,
    int StockQuantity,
    string? ImageUrl
) : IRequest<UpdateProductResult>;

public record UpdateProductResult(bool IsSuccess);

public class UpdateProductHandler : IRequestHandler<UpdateProductCommand, UpdateProductResult>
{
    private readonly IDocumentSession _session;

    public UpdateProductHandler(IDocumentSession session)
    {
        _session = session;
    }

    public async Task<UpdateProductResult> Handle(UpdateProductCommand request, CancellationToken cancellationToken)
    {
        var product = await _session.LoadAsync<Models.Product>(request.Id, cancellationToken);

        if (product == null)
            return new UpdateProductResult(false);

        product.Name = request.Name;
        product.Description = request.Description;
        product.Category = request.Category;
        product.Price = request.Price;
        product.StockQuantity = request.StockQuantity;
        product.ImageUrl = request.ImageUrl;
        product.UpdatedAt = DateTime.UtcNow;

        _session.Store(product);
        await _session.SaveChangesAsync(cancellationToken);

        return new UpdateProductResult(true);
    }
}

public class UpdateProductEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPut("/api/products/{id:guid}", async (Guid id, UpdateProductRequest request, ISender sender) =>
        {
            var command = request with { Id = id };
            var result = await sender.Send(command.Adapt<UpdateProductCommand>());

            return result.IsSuccess ? Results.NoContent() : Results.NotFound();
        })
        .WithName("UpdateProduct")
        .Produces(StatusCodes.Status204NoContent)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .WithSummary("Update Product")
        .WithDescription("Update an existing product");
    }
}