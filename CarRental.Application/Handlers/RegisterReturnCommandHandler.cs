using MediatR;
using Microsoft.Extensions.Logging;
using CarRental.Application.Commands;
using CarRental.Application.Exceptions;
using CarRental.Application.Responses;
using CarRental.Domain.Entities;
using CarRental.Domain.Interfaces;
using CarRental.Infrastructure.Interfaces;
using FluentValidation;

namespace CarRental.Application.Handlers
{
    public class RegisterReturnCommandHandler : IRequestHandler<RegisterReturnCommand, BaseResponse<RentalReturnResponse>>
    {
        private readonly IRentalRepository _rentalRepository;
        private readonly IRedisCacheService _redisCacheService;
        private readonly IPriceCalculationStrategyFactory _priceCalculationStrategyFactory;
        private readonly ILogger<RegisterReturnCommandHandler> _logger;

        public RegisterReturnCommandHandler(
            IRentalRepository rentalRepository,
            IRedisCacheService redisCacheService,
            IPriceCalculationStrategyFactory priceCalculationStrategyFactory,
            ILogger<RegisterReturnCommandHandler> logger)
        {
            _rentalRepository = rentalRepository;
            _redisCacheService = redisCacheService;
            _priceCalculationStrategyFactory = priceCalculationStrategyFactory;
            _logger = logger;
        }

        public async Task<BaseResponse<RentalReturnResponse>> Handle(RegisterReturnCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Registering return for booking number: {BookingNumber}", request.BookingNumber);
            
            var rental = await _redisCacheService.GetCachedRentalAsync(request.BookingNumber);
            
            // If not found in cache, fetch from the database
            if (rental == null)
            {
                rental = await _rentalRepository.GetByBookingNumberAsync(request.BookingNumber);
                if (rental == null)
                {
                    throw new NotFoundException($"Rental not found for booking number {request.BookingNumber}");
                }

                // Cache the rental for future use
                await _redisCacheService.CacheRentalAsync(request.BookingNumber, rental);
            }

            // Validate meter reading
            if (request.ReturnMeterReading < rental.PickupMeterReading)
            {
                throw new InvalidOperationException("Return meter reading cannot be less than pickup meter reading");
            }

            // Validate return date
            if (request.ReturnDate < rental.PickupDate)
            {
                throw new InvalidOperationException("Return date cannot be before pickup date");
            }

            // Save changes to the database
            await _rentalRepository.UpdateAsync(rental);

            // Update the cached rental
            await _redisCacheService.CacheRentalAsync(request.BookingNumber, rental);

            // Calculate the price using the appropriate strategy
            var strategy = _priceCalculationStrategyFactory.GetStrategy(rental.CarCategory);
            var numberOfDays = (int)(request.ReturnDate - rental.PickupDate).TotalDays;
            var numberOfKm = request.ReturnMeterReading - rental.PickupMeterReading;

            // Use fixed base prices for all categories
            const decimal baseDayRental = 100M;
            const decimal baseKmPrice = 10M;

            // Let the strategy handle the multipliers
            var price = strategy.CalculatePrice(baseDayRental, baseKmPrice, numberOfDays, numberOfKm);

            _logger.LogInformation("Calculated price for booking number {BookingNumber}: {Price}", 
                request.BookingNumber, price);
            
            return new BaseResponse<RentalReturnResponse>
            {
                Success = true,
                Data = new RentalReturnResponse
                {
                    BookingNumber = rental.BookingNumber,
                    RegistrationNumber = rental.RegistrationNumber,
                    CustomerSocialSecurityNumber = rental.CustomerSocialSecurityNumber,
                    PickupDate = rental.PickupDate,
                    ReturnDate = request.ReturnDate,
                    ReturnMeterReading = request.ReturnMeterReading,
                    CalculatedPrice = price
                }
            };
        }
    }
}