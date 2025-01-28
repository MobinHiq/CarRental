using CarRental.Application.Commands;
using CarRental.Application.Responses;
using CarRental.Domain.Entities;
using MediatR;
using CarRental.Domain.Interfaces;
using CarRental.Infrastructure.Interfaces;
using Microsoft.Extensions.Logging;
using CarRental.Application.Exceptions;
using FluentValidation;

namespace CarRental.Application.Handlers
{
    public class RegisterPickupCommandHandler : 
        IRequestHandler<RegisterPickupCommand, BaseResponse<RentalPickupResponse>>
    {
        private readonly IRentalRepository _rentalRepository;
        private readonly IRedisCacheService _redisCacheService;
        private readonly ILogger<RegisterPickupCommandHandler> _logger;

        public RegisterPickupCommandHandler(
            IRentalRepository rentalRepository,
            IRedisCacheService redisCacheService,
            ILogger<RegisterPickupCommandHandler> logger)
        {
            _rentalRepository = rentalRepository;
            _redisCacheService = redisCacheService;
            _logger = logger;
        }

        public async Task<BaseResponse<RentalPickupResponse>> Handle(RegisterPickupCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Registering new pickup");
            
            if (request.Rental == null)
            {
                throw new ValidationException("Rental cannot be null");
            }

            var rental = await _rentalRepository.CreateAsync(request.Rental);
            if (rental == null)
            {
                throw new InvalidOperationException("Failed to create rental");
            }

            await _redisCacheService.CacheRentalAsync(rental.BookingNumber, rental);

            return new BaseResponse<RentalPickupResponse>
            {
                Success = true,
                Data = new RentalPickupResponse
                {
                    BookingNumber = rental.BookingNumber,
                    CustomerSocialSecurityNumber = rental.CustomerSocialSecurityNumber,
                    PickupDate = rental.PickupDate,
                    PickupMeterReading = rental.PickupMeterReading,
                    RegistrationNumber = rental.RegistrationNumber
                }
            };
        }
    }
}