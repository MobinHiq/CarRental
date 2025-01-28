using CarRental.Domain.Entities;

namespace CarRental.Infrastructure.Interfaces
{
    public interface IRedisCacheService
    {
        Task CacheRentalAsync(string bookingNumber, Rental rental);
        Task<Rental> GetCachedRentalAsync(string bookingNumber);
    }
}