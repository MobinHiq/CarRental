using CarRental.Application.Responses;
using MediatR;
using CarRental.Domain;
using CarRental.Domain.Entities;

namespace CarRental.Application.Commands
{
    public class RegisterPickupCommand : IRequest<BaseResponse<RentalPickupResponse>>
    {
        public Rental Rental { get; set; }
    }
}