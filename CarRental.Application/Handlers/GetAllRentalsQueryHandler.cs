using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MediatR;
using CarRental.Application.Queries;
using CarRental.Domain.Entities;
using CarRental.Domain.Interfaces;
using CarRental.Infrastructure.Interfaces;
namespace CarRental.Application.Handlers
{
    public class GetAllRentalsQueryHandler : IRequestHandler<GetAllRentalsQuery, IEnumerable<Rental>>
    {
        private readonly IRentalRepository _rentalRepository;
        private readonly IRedisCacheService _redisCacheService;
        private readonly ILogger<GetAllRentalsQueryHandler> _logger;

        public GetAllRentalsQueryHandler(
            IRentalRepository rentalRepository,
            IRedisCacheService redisCacheService,
            ILogger<GetAllRentalsQueryHandler> logger)
        {
            _rentalRepository = rentalRepository;
            _redisCacheService = redisCacheService;
            _logger = logger;
        }

        public async Task<IEnumerable<Rental>> Handle(GetAllRentalsQuery request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Getting all rentals");
            
            // Try to get from cache first
            var rentals = await _redisCacheService.GetAllCachedRentalsAsync();
            if (rentals != null && rentals.Any())
            {
                return rentals;
            }

            // If not in cache, get from repository
            rentals = await _rentalRepository.GetAllAsync();

            // Cache all rentals
            foreach (var rental in rentals)
            {
                await _redisCacheService.CacheRentalAsync(rental.BookingNumber, rental);
            }

            return rentals;
        }
    }
} 