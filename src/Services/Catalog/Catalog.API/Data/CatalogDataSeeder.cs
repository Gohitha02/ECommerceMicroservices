using Catalog.API.Models;
using Marten;

namespace Catalog.API.Data;

public static class CatalogDataSeeder
{
    public static async Task SeedAsync(IDocumentStore store)
    {
        using var session = store.LightweightSession();

        if (session.Query<Product>().Any())
            return;

        var products = new List<Product>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Name = "Wireless Bluetooth Headphones",
                Description = "Premium noise-cancelling headphones with 30-hour battery life",
                Category = "Electronics",
                Price = 299.99m,
                StockQuantity = 50,
                ImageUrl = "https://example.com/images/headphones.jpg",
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = Guid.NewGuid(),
                Name = "Ergonomic Office Chair",
                Description = "Adjustable lumbar support and breathable mesh back",
                Category = "Furniture",
                Price = 449.99m,
                StockQuantity = 25,
                ImageUrl = "https://example.com/images/chair.jpg",
                CreatedAt = DateTime.UtcNow
            },
            new()
            {
                Id = Guid.NewGuid(),
                Name = "Mechanical Gaming Keyboard",
                Description = "RGB backlit with Cherry MX switches",
                Category = "Electronics",
                Price = 159.99m,
                StockQuantity = 100,
                ImageUrl = "https://example.com/images/keyboard.jpg",
                CreatedAt = DateTime.UtcNow
            }
        };

        session.Store(products);
        await session.SaveChangesAsync();
    }
}