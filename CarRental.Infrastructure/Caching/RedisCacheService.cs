using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using CarRental.Domain.Entities;
using CarRental.Infrastructure.Interfaces;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace CarRental.Infrastructure.Caching;

public class RedisCacheService : IRedisCacheService
{
    private readonly IConnectionMultiplexer _redis;
    private readonly ILogger<RedisCacheService> _logger;
    private const string KeyPrefix = "rental:";

    public RedisCacheService(IConnectionMultiplexer redis, ILogger<RedisCacheService> logger)
    {
        _redis = redis;
        _logger = logger;
    }

    public async Task<Rental?> GetCachedRentalAsync(string bookingNumber)
    {
        try
        {
            var db = _redis.GetDatabase();
            var value = await db.StringGetAsync($"{KeyPrefix}{bookingNumber}");
            
            return value.HasValue 
                ? JsonSerializer.Deserialize<Rental>(value!) 
                : null;
        }
        catch (RedisTimeoutException ex)
        {
            _logger.LogError(ex, "Redis timeout while getting rental {BookingNumber}", bookingNumber);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting rental from cache {BookingNumber}", bookingNumber);
            return null;
        }
    }

    public async Task<IEnumerable<Rental>> GetAllCachedRentalsAsync()
    {
        try
        {
            var db = _redis.GetDatabase();
            var server = _redis.GetServer(_redis.GetEndPoints().First());
            var rentals = new List<Rental>();

            var keys = server.Keys(pattern: $"{KeyPrefix}*", pageSize: 100);
            foreach (var key in keys)
            {
                try
                {
                    var value = await db.StringGetAsync(key);
                    if (value.HasValue)
                    {
                        var rental = JsonSerializer.Deserialize<Rental>(value!);
                        if (rental != null)
                        {
                            rentals.Add(rental);
                        }
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error deserializing rental from key {Key}", key);
                    continue;
                }
            }

            return rentals;
        }
        catch (RedisTimeoutException ex)
        {
            _logger.LogError(ex, "Redis timeout while getting all rentals");
            return Enumerable.Empty<Rental>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all rentals from cache");
            return Enumerable.Empty<Rental>();
        }
    }

    public async Task CacheRentalAsync(string bookingNumber, Rental rental)
    {
        try
        {
            var db = _redis.GetDatabase();
            var serialized = JsonSerializer.Serialize(rental);
            await db.StringSetAsync($"{KeyPrefix}{bookingNumber}", serialized);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error caching rental {BookingNumber}", bookingNumber);
        }
    }

    public async Task RemoveRentalFromCacheAsync(string bookingNumber)
    {
        try
        {
            var db = _redis.GetDatabase();
            await db.KeyDeleteAsync($"{KeyPrefix}{bookingNumber}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing rental from cache {BookingNumber}", bookingNumber);
        }
    }
}