using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace WeddingHallAPI.Models
{
    public class Payment
    {
        [Key]
        public int PaymentID { get; set; }

        public int BookingID { get; set; }
        public decimal Amount { get; set; }
        public string? PaymentMethod { get; set; }
        public DateTime PaymentDate { get; set; } = DateTime.Now;
        public string Status { get; set; } = "Pending";
        public string? TransactionRef { get; set; }

        // Navigation
        public Booking? Booking { get; set; }
    }
}