using CarRental.Domain.Entities;

namespace CarRental.Domain.Interfaces
{
    public interface IRentalRepository
    {
        Task<Rental> GetByBookingNumberAsync(string bookingNumber);
        Task<IEnumerable<Rental>> GetAllAsync();
        Task<Rental> CreateAsync(Rental rental);
        Task<Rental> UpdateAsync(Rental rental);
        Task DeleteAsync(string bookingNumber);
        Task<bool> ExistsAsync(string key);
    }
}