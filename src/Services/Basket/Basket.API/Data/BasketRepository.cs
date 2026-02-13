using System.Text.Json;
using Basket.API.Models;
using Microsoft.Extensions.Caching.Distributed;

namespace Basket.API.Data;

public class BasketRepository : IBasketRepository
{
    private readonly IDistributedCache _redisCache;

    public BasketRepository(IDistributedCache redisCache)
    {
        _redisCache = redisCache;
    }

    public async Task<ShoppingCart?> GetBasketAsync(string userName, CancellationToken cancellationToken = default)
    {
        var cachedBasket = await _redisCache.GetStringAsync(userName, cancellationToken);

        if (string.IsNullOrEmpty(cachedBasket))
            return null;

        return JsonSerializer.Deserialize<ShoppingCart>(cachedBasket);
    }

    public async Task<ShoppingCart> StoreBasketAsync(ShoppingCart basket, CancellationToken cancellationToken = default)
    {
        var options = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(30),
            SlidingExpiration = TimeSpan.FromDays(7)
        };

        await _redisCache.SetStringAsync(basket.UserName, JsonSerializer.Serialize(basket), options, cancellationToken);

        return await GetBasketAsync(basket.UserName, cancellationToken) ?? basket;
    }

    public async Task<bool> DeleteBasketAsync(string userName, CancellationToken cancellationToken = default)
    {
        await _redisCache.RemoveAsync(userName, cancellationToken);
        return true;
    }
}