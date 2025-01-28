using CarRental.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CarRental.Infrastructure.DbContext
{
    public class CarRentalDbContext : Microsoft.EntityFrameworkCore.DbContext
    {
        public DbSet<Rental> Rentals { get; set; }

        public CarRentalDbContext(DbContextOptions<CarRentalDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Rental>()
                .HasKey(r => r.BookingNumber);

            modelBuilder.Entity<Rental>()
                .Property(r => r.CarCategory)
                .HasConversion<string>();

            base.OnModelCreating(modelBuilder);
        }
    }
}