using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.Extensions.Logging;
using CarRental.Application.Commands;
using CarRental.Domain.Entities;
using CarRental.Domain.Interfaces;
using CarRental.Infrastructure.Interfaces;
using CarRental.Application.Exceptions;

namespace CarRental.Application.Handlers
{
    public class UpdateRentalCommandHandler : IRequestHandler<UpdateRentalCommand, Rental>
    {
        private readonly IRentalRepository _rentalRepository;
        private readonly IRedisCacheService _redisCacheService;
        private readonly ILogger<UpdateRentalCommandHandler> _logger;

        public UpdateRentalCommandHandler(
            IRentalRepository rentalRepository,
            IRedisCacheService redisCacheService,
            ILogger<UpdateRentalCommandHandler> logger)
        {
            _rentalRepository = rentalRepository;
            _redisCacheService = redisCacheService;
            _logger = logger;
        }

        public async Task<Rental> Handle(UpdateRentalCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Updating rental with booking number: {BookingNumber}", request.Rental.BookingNumber);

            var existingRental = await _rentalRepository.GetByBookingNumberAsync(request.Rental.BookingNumber);
            if (existingRental == null)
            {
                throw new NotFoundException($"Rental with booking number {request.Rental.BookingNumber} not found");
            }

            var updatedRental = await _rentalRepository.UpdateAsync(request.Rental);
            
            // Update cache
            await _redisCacheService.CacheRentalAsync(updatedRental.BookingNumber, updatedRental);

            return updatedRental;
        }
    }
} 