namespace CarRental.Application.Responses;

public class RentalReturnResponse
{
    public string BookingNumber { get; init; } = string.Empty;
    
    public string RegistrationNumber { get; init; } = string.Empty;
    
    public string CustomerSocialSecurityNumber { get; init; } = string.Empty;
    
    public DateTime PickupDate { get; init; }
    
    public DateTime ReturnDate { get; init; }
    
    public int ReturnMeterReading { get; set; }
    
    public decimal CalculatedPrice { get; set; }
}