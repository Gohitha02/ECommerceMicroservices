namespace Catalog.API.Models.Requests;

public record CreateProductRequest(
    string Name,
    string Description,
    string Category,
    decimal Price,
    int StockQuantity,
    string? ImageUrl = null
);