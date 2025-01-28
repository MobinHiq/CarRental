namespace CarRental.Application.Requests
{
    public class ReturnRequest
    {
        public string BookingNumber { get; set; } = string.Empty;
        public DateTime ReturnDate { get; set; }
        public int ReturnMeterReading { get; set; }
    }
} 