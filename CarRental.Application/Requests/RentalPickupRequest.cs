using CarRental.Domain.Enums;

namespace CarRental.Application.Requests
{
    public class RentalPickupRequest
    {
        public string RegistrationNumber { get; set; } = string.Empty;
        public string CustomerSocialSecurityNumber { get; set; } = string.Empty;
        public CarCategory CarCategory { get; set; }
        public DateTime PickupDate { get; set; }
        public int PickupMeterReading { get; set; }
    }
} 