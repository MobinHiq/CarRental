using MediatR;
using Microsoft.AspNetCore.Mvc;
using CarRental.Application.Commands;
using CarRental.Application.Requests;
using CarRental.Domain.Entities;
using CarRental.Domain.Interfaces;
using CarRental.Application.Exceptions;
using FluentValidation;

namespace CarRental.API.Controllers
{
    [ApiController]
    [Route("api/CarRental")]
    public class RentalController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly IRentalRepository _rentalRepository;
        private readonly IValidator<RentalPickupRequest> _pickupValidator;
        private readonly IValidator<ReturnRequest> _returnValidator;

        public RentalController(
            IMediator mediator, 
            IRentalRepository rentalRepository,
            IValidator<RentalPickupRequest> pickupValidator,
            IValidator<ReturnRequest> returnValidator)
        {
            _mediator = mediator;
            _rentalRepository = rentalRepository;
            _pickupValidator = pickupValidator;
            _returnValidator = returnValidator;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllRentals()
        {
            var rentals = await _rentalRepository.GetAllAsync();
            return Ok(rentals);
        }

        [HttpGet("{bookingNumber}")]
        public async Task<IActionResult> GetRentalByBookingNumber(string bookingNumber)
        {
            var rental = await _rentalRepository.GetByBookingNumberAsync(bookingNumber);
            if (rental == null)
            {
                throw new NotFoundException($"Rental with booking number {bookingNumber} not found");
            }
            return Ok(rental);
        }

        [HttpPost("pickup")]
        public async Task<IActionResult> RegisterPickup([FromBody] RentalPickupRequest request)
        {
            var validationResult = await _pickupValidator.ValidateAsync(request);
            if (!validationResult.IsValid)
            {
                throw new ValidationException(
                    "Validation failed", 
                    validationResult.Errors);
            }

            var rental = new Rental
            {
                RegistrationNumber = request.RegistrationNumber,
                CustomerSocialSecurityNumber = request.CustomerSocialSecurityNumber,
                CarCategory = request.CarCategory,
                PickupDate = request.PickupDate,
                PickupMeterReading = request.PickupMeterReading
            };

            var response = await _mediator.Send(new RegisterPickupCommand { Rental = rental });
            return Ok(response.Data);
        }

        [HttpPost("return")]
        public async Task<IActionResult> RegisterReturn([FromBody] ReturnRequest request)
        {
            var validationResult = await _returnValidator.ValidateAsync(request);
            if (!validationResult.IsValid)
            {
                throw new ValidationException(
                    "Validation failed", 
                    validationResult.Errors);
            }

            var response = await _mediator.Send(new RegisterReturnCommand
            {
                BookingNumber = request.BookingNumber,
                ReturnDate = request.ReturnDate,
                ReturnMeterReading = request.ReturnMeterReading
            });

            return Ok(response.Data);
        }

        [HttpPut("{bookingNumber}")]
        public async Task<IActionResult> UpdateRental(string bookingNumber, [FromBody] Rental rental)
        {
            if (bookingNumber != rental.BookingNumber)
            {
                throw new BadRequestException("Booking number mismatch");
            }

            var existingRental = await _rentalRepository.GetByBookingNumberAsync(bookingNumber);
            if (existingRental == null)
            {
                throw new NotFoundException($"Rental with booking number {bookingNumber} not found");
            }

            var updatedRental = await _rentalRepository.UpdateAsync(rental);
            return Ok(updatedRental);
        }

        [HttpDelete("{bookingNumber}")]
        public async Task<IActionResult> DeleteRental(string bookingNumber)
        {
            var rental = await _rentalRepository.GetByBookingNumberAsync(bookingNumber);
            if (rental == null)
            {
                throw new NotFoundException($"Rental with booking number {bookingNumber} not found");
            }

            await _rentalRepository.DeleteAsync(bookingNumber);
            return NoContent();
        }
    }
}