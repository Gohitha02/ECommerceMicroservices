using Carter;
using Marten;
using MediatR;

namespace Catalog.API.Features.Products.GetProducts;

public record GetProductsQuery(int? PageNumber = 1, int? PageSize = 10) : IRequest<GetProductsResult>;

public record GetProductsResult(IEnumerable<ProductDto> Products);

public record ProductDto(
    Guid Id,
    string Name,
    string Category,
    decimal Price,
    int StockQuantity
);

public class GetProductsHandler : IRequestHandler<GetProductsQuery, GetProductsResult>
{
    private readonly IDocumentSession _session;

    public GetProductsHandler(IDocumentSession session)
    {
        _session = session;
    }

    public async Task<GetProductsResult> Handle(GetProductsQuery request, CancellationToken cancellationToken)
    {
        var products = await _session.Query<Models.Product>()
            .OrderBy(p => p.Name)
            .Skip((request.PageNumber - 1) * request.PageSize ?? 0)
            .Take(request.PageSize ?? 10)
            .Select(p => new ProductDto(p.Id, p.Name, p.Category, p.Price, p.StockQuantity))
            .ToListAsync(cancellationToken);

        return new GetProductsResult(products);
    }
}

public class GetProductsEndpoint : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/products", async ([AsParameters] GetProductsQuery query, ISender sender) =>
        {
            var result = await sender.Send(query);
            return Results.Ok(result);
        })
        .WithName("GetProducts")
        .Produces<GetProductsResult>(StatusCodes.Status200OK)
        .WithSummary("Get Products")
        .WithDescription("Get paginated list of products");
    }
}