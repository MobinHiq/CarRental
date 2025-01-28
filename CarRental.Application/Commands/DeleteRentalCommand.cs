using MediatR;

namespace CarRental.Application.Commands;

public class DeleteRentalCommand : IRequest<Unit>
{
    public string BookingNumber { get; set; } = string.Empty;
} 