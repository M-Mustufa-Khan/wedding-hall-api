using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace WeddingHallAPI.Models
{
    public class Booking
    {
        [Key]
        public int BookingID { get; set; }

        public int UserID { get; set; }
        public int HallID { get; set; }
        public int? PackageID { get; set; }

        [Required]
        public DateTime EventDate { get; set; }

        public string? EventType { get; set; }
        public int? GuestCount { get; set; }
        public decimal TotalPrice { get; set; }
        public string Status { get; set; } = "Pending";
        public string? SpecialNotes { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public string? CNIC { get; set; }
        public string? CustomerPhone { get; set; }
        public string? CustomerAddress { get; set; }
        public string? PaymentMethod { get; set; }
        public string? TransactionRef { get; set; }
        public string? ReceiptImage { get; set; }

        // Navigation
        public User? User { get; set; }
        public Hall? Hall { get; set; }
        public Package? Package { get; set; }
        public List<Payment>? Payments { get; set; }
    }
}