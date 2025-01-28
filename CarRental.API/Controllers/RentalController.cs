using MediatR;
using Microsoft.AspNetCore.Mvc;
using CarRental.Application.Commands;
using CarRental.Application.Requests;
using CarRental.Domain.Entities;
using CarRental.Domain.Interfaces;
using CarRental.Application.Exceptions;
using FluentValidation;
using CarRental.Application.Queries;

namespace CarRental.API.Controllers
{
    [ApiController]
    [Route("api/CarRental")]
    public class RentalController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly IValidator<RentalPickupRequest> _pickupValidator;
        private readonly IValidator<ReturnRequest> _returnValidator;

        public RentalController(
            IMediator mediator,
            IValidator<RentalPickupRequest> pickupValidator,
            IValidator<ReturnRequest> returnValidator)
        {
            _mediator = mediator;
            _pickupValidator = pickupValidator;
            _returnValidator = returnValidator;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllRentals()
        {
            var rentals = await _mediator.Send(new GetAllRentalsQuery());
            return Ok(rentals);
        }

        [HttpGet("{bookingNumber}")]
        public async Task<IActionResult> GetRentalByBookingNumber(string bookingNumber)
        {
            var rental = await _mediator.Send(new GetRentalByBookingNumberQuery { BookingNumber = bookingNumber });
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

            var updatedRental = await _mediator.Send(new UpdateRentalCommand { Rental = rental });
            return Ok(updatedRental);
        }

        [HttpDelete("{bookingNumber}")]
        public async Task<IActionResult> DeleteRental(string bookingNumber)
        {
            await _mediator.Send(new DeleteRentalCommand { BookingNumber = bookingNumber });
            return NoContent();
        }
    }
}