using MediatR;
using CarRental.Domain.Entities;

namespace CarRental.Application.Queries;

public class GetRentalByBookingNumberQuery : IRequest<Rental>
{
    public string BookingNumber { get; set; } = string.Empty;
} 