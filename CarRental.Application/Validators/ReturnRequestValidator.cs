using FluentValidation;
using CarRental.Application.Requests;

namespace CarRental.Application.Validators
{
    public class ReturnRequestValidator : AbstractValidator<ReturnRequest>
    {
        public ReturnRequestValidator()
        {
            RuleFor(x => x.BookingNumber)
                .NotEmpty().WithMessage("Booking number is required");

            RuleFor(x => x.ReturnDate)
                .NotEmpty().WithMessage("Return date is required")
                .GreaterThanOrEqualTo(DateTime.Today).WithMessage("Return date cannot be in the past");

            RuleFor(x => x.ReturnMeterReading)
                .GreaterThanOrEqualTo(0).WithMessage("Return meter reading must be greater than or equal to 0");
        }
    }
} 