using MediatR;
using CarRental.Domain.Entities;

namespace CarRental.Application.Commands
{
    public class UpdateRentalCommand : IRequest<Rental>
    {
        public Rental Rental { get; set; } = null!;
    }
} 