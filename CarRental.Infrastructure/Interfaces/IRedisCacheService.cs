using System.Collections.Generic;
using System.Threading.Tasks;
using CarRental.Domain.Entities;

namespace CarRental.Infrastructure.Interfaces
{
    public interface IRedisCacheService
    {
        Task<Rental?> GetCachedRentalAsync(string bookingNumber);
        Task<IEnumerable<Rental>> GetAllCachedRentalsAsync();
        Task CacheRentalAsync(string bookingNumber, Rental rental);
        Task RemoveRentalFromCacheAsync(string bookingNumber);
    }
}