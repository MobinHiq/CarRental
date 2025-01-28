using CarRental.Domain.Entities;
using CarRental.Infrastructure.Interfaces;
using Newtonsoft.Json;
using StackExchange.Redis;

namespace CarRental.Infrastructure.Caching;

public class RedisCacheService : IRedisCacheService
{
    private readonly IDatabase _redisDatabase;

    public RedisCacheService(IConnectionMultiplexer redisConnection)
    {
        _redisDatabase = redisConnection.GetDatabase();
    }

    public async Task CacheRentalAsync(string bookingNumber, Rental rental)
    {
        var serializedRental = JsonConvert.SerializeObject(rental);
        await _redisDatabase.StringSetAsync(bookingNumber, serializedRental);
    }

    public async Task<Rental> GetCachedRentalAsync(string bookingNumber)
    {
        var serializedRental = await _redisDatabase.StringGetAsync(bookingNumber);
        if (serializedRental.HasValue)
        {
            return JsonConvert.DeserializeObject<Rental>(serializedRental);
        }

        return null;
    }
}