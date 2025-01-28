using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;
using CarRental.Domain.Enums;

namespace CarRental.Domain.Entities
{
    public class Rental
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public string BookingNumber { get; set; } = string.Empty;
        public string RegistrationNumber { get; set; } = string.Empty;
        public string CustomerSocialSecurityNumber { get; set; } = string.Empty;
        public CarCategory CarCategory { get; set; }
        public DateTime PickupDate { get; set; }
        public int PickupMeterReading { get; init; }
    }
}