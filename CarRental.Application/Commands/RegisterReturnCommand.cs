using CarRental.Application.Responses;
using CarRental.Domain.Entities;
using MediatR;

namespace CarRental.Application.Commands
{
    public class RegisterReturnCommand : IRequest<BaseResponse<RentalReturnResponse>>
    {
        public string BookingNumber { get; set; } = string.Empty;
        public DateTime ReturnDate { get; set; }
        public int ReturnMeterReading { get; set; }
        
    }
}