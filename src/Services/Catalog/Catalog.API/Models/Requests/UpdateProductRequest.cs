namespace Catalog.API.Models.Requests;

public record UpdateProductRequest(
    Guid Id,
    string Name,
    string Description,
    string Category,
    decimal Price,
    int StockQuantity,
    string? ImageUrl = null
);