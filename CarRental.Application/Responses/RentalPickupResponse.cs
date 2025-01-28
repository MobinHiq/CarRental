namespace CarRental.Application.Responses;

public class RentalPickupResponse
{
    public string BookingNumber { get; init; } = string.Empty;
    
    public string RegistrationNumber { get; init; } = string.Empty;
    
    public string CustomerSocialSecurityNumber { get; init; } = string.Empty;
    
    public DateTime PickupDate { get; init; }
    
    public int PickupMeterReading { get; init; }
}