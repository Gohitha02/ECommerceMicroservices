using Carter;
using Marten;
using MediatR;

namespace Catalog.API.Features.Products.GetProductById;

public record GetProductByIdQuery(Guid Id) : IRequest<GetProductByIdResult>;

public record GetProductByIdResult(ProductDetailDto Product);

public record ProductDetailDto(
    Guid Id,
    string Name,
    string Description,
    string Category,
    decimal Price,
    int StockQuantity,
    string? ImageUrl,
    DateTime CreatedAt
);

public class GetProductByIdHandler : IRequestHandler<GetProductByIdQuery, GetProductByIdResult?>
{
    private readonly IDocumentSession _session;

    public GetProductByIdHandler(IDocumentSession session)
    {
        _session = session;
    }

    public async Task<GetProductByIdResult?> Handle(GetProductByIdQuery request, CancellationToken cancellationToken)
    {
        var product = await _session.LoadAsync<Models.Product>(request.Id, cancellationToken);

        if (product == null)
            return null;

        var dto = new ProductDetailDto(
            product.Id,
            product.Name,
            product.Description,
            product.Category,
            product.Price,
            product.StockQuantity,
            product.ImageUrl,
            product.CreatedAt
        );

        return new GetProductByIdResult(dto);
    }
}

public class GetProductByIdEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/products/{id:guid}", async (Guid id, ISender sender) =>
        {
            var query = new GetProductByIdQuery(id);
            var result = await sender.Send(query);

            return result == null ? Results.NotFound() : Results.Ok(result);
        })
        .WithName("GetProductById")
        .Produces<GetProductByIdResult>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .WithSummary("Get Product by Id")
        .WithDescription("Get product details by ID");
    }
}