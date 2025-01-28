using MediatR;
using Microsoft.Extensions.Logging;
using CarRental.Application.Commands;
using CarRental.Domain.Interfaces;
using CarRental.Infrastructure.Interfaces;
using CarRental.Application.Exceptions;

namespace CarRental.Application.Handlers
{
    public class DeleteRentalCommandHandler : IRequestHandler<DeleteRentalCommand, Unit>
    {
        private readonly IRentalRepository _rentalRepository;
        private readonly IRedisCacheService _redisCacheService;
        private readonly ILogger<DeleteRentalCommandHandler> _logger;

        public DeleteRentalCommandHandler(
            IRentalRepository rentalRepository,
            IRedisCacheService redisCacheService,
            ILogger<DeleteRentalCommandHandler> logger)
        {
            _rentalRepository = rentalRepository;
            _redisCacheService = redisCacheService;
            _logger = logger;
        }

        public async Task<Unit> Handle(DeleteRentalCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Deleting rental with booking number: {BookingNumber}", request.BookingNumber);

            var rental = await _rentalRepository.GetByBookingNumberAsync(request.BookingNumber);
            if (rental == null)
            {
                throw new NotFoundException($"Rental with booking number {request.BookingNumber} not found");
            }

            await _rentalRepository.DeleteAsync(request.BookingNumber);
            
            // Remove from cache
            await _redisCacheService.RemoveRentalFromCacheAsync(request.BookingNumber);

            return Unit.Value;
        }
    }
} 