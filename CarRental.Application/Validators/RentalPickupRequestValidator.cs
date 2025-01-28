using FluentValidation;
using CarRental.Application.Requests;

namespace CarRental.Application.Validators
{
    public class RentalPickupRequestValidator : AbstractValidator<RentalPickupRequest>
    {
        public RentalPickupRequestValidator()
        {
            RuleFor(x => x.RegistrationNumber)
                .NotEmpty().WithMessage("Registration number is required");

            RuleFor(x => x.CustomerSocialSecurityNumber)
                .NotEmpty().WithMessage("Customer social security number is required");

            RuleFor(x => x.PickupDate)
                .NotEmpty().WithMessage("Pickup date is required")
                .GreaterThanOrEqualTo(DateTime.Today).WithMessage("Pickup date cannot be in the past");

            RuleFor(x => x.PickupMeterReading)
                .GreaterThanOrEqualTo(0).WithMessage("Pickup meter reading must be greater than or equal to 0");
        }
    }
} 