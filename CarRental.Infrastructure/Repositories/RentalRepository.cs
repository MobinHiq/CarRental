using CarRental.Domain.Entities;
using CarRental.Domain.Interfaces;
using CarRental.Infrastructure.DbContext;
using Microsoft.EntityFrameworkCore;

namespace CarRental.Infrastructure.Repositories
{
    public class RentalRepository : IRentalRepository
    {
        private readonly CarRentalDbContext _dbContext;

        public RentalRepository(CarRentalDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Rental> CreateAsync(Rental rental)
        {
            try
            {
                rental.BookingNumber = Guid.NewGuid().ToString();
                _dbContext.Rentals.Add(rental);
                await _dbContext.SaveChangesAsync();

                return rental;
            }
            catch (DbUpdateException ex)
            {
                throw new InvalidOperationException("Failed to create a car rental request!", ex);
            }
        }
        
        public async Task<Rental> UpdateAsync(Rental rental)
        {
            var updatedRental = await _dbContext.Rentals
                .FirstOrDefaultAsync(c => c.BookingNumber == rental.BookingNumber);
            if (updatedRental == null)
            {
                throw new KeyNotFoundException($"Booking number {rental.BookingNumber} not found.");
            }

            // Update the entity properties
            updatedRental.BookingNumber = rental.BookingNumber;
            updatedRental.RegistrationNumber = rental.RegistrationNumber;
            updatedRental.CarCategory = rental.CarCategory;
            updatedRental.PickupDate = rental.PickupDate;
            updatedRental.CustomerSocialSecurityNumber = updatedRental.CustomerSocialSecurityNumber;

            await _dbContext.SaveChangesAsync();
        
            // Verify the timestamp was updated
            await _dbContext.Entry(updatedRental).ReloadAsync();

            return updatedRental;
        }

        public async Task DeleteAsync(string bookingNumber)
        {
            var entity = await _dbContext.Rentals
                .FirstOrDefaultAsync(c => c.BookingNumber == bookingNumber);
            if (entity == null)
            {
                throw new KeyNotFoundException($"Booking number with ID {bookingNumber} not found.");
            }
            _dbContext.Rentals.Remove(entity);
            await _dbContext.SaveChangesAsync();
        }

        public async Task<bool> ExistsAsync(string key)
        {
            return await _dbContext.Rentals
                .AnyAsync(c => c.BookingNumber == key);
        }

        public async Task<IEnumerable<Rental>> GetAllAsync()
        {
            var query = _dbContext.Rentals.AsNoTracking();
            var rentals = await query.ToListAsync();

            return rentals;
        }

        public async Task<Rental> GetByBookingNumberAsync(string bookingNumber)
        {
            var rental = await _dbContext.Rentals
                .FirstOrDefaultAsync(c => c.BookingNumber == bookingNumber);

            return rental;
        }
    }
}