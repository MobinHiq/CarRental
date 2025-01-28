using CarRental.Application.Queries;
using CarRental.Domain.Entities;
using CarRental.Domain.Interfaces;
using MediatR;
using Microsoft.Extensions.Logging;
using CarRental.Infrastructure.Interfaces;
using CarRental.Application.Exceptions;

namespace CarRental.Application.Handlers
{
    public class GetRentalByBookingNumberQueryHandler : IRequestHandler<GetRentalByBookingNumberQuery, Rental>
    {
        private readonly IRentalRepository _rentalRepository;
        private readonly IRedisCacheService _redisCacheService;
        private readonly ILogger<GetRentalByBookingNumberQueryHandler> _logger;

        public GetRentalByBookingNumberQueryHandler(
            IRentalRepository rentalRepository,
            IRedisCacheService redisCacheService,
            ILogger<GetRentalByBookingNumberQueryHandler> logger)
        {
            _rentalRepository = rentalRepository;
            _redisCacheService = redisCacheService;
            _logger = logger;
        }

        public async Task<Rental> Handle(GetRentalByBookingNumberQuery request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Getting rental with booking number: {BookingNumber}", request.BookingNumber);

            // Try to get from cache first
            var rental = await _redisCacheService.GetCachedRentalAsync(request.BookingNumber);
            if (rental != null)
            {
                return rental;
            }

            // If not in cache, get from repository
            rental = await _rentalRepository.GetByBookingNumberAsync(request.BookingNumber);
            if (rental == null)
            {
                throw new NotFoundException($"Rental with booking number {request.BookingNumber} not found");
            }

            // Cache the rental
            await _redisCacheService.CacheRentalAsync(request.BookingNumber, rental);

            return rental;
        }
    }
} 