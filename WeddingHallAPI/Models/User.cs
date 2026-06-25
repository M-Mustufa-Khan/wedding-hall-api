using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;  

namespace WeddingHallAPI.Models
{
    public class User
    {
        [Key]
        public int UserID { get; set; }

        [Required]
        public string FullName { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string PasswordHash { get; set; } = string.Empty;

        public string? Phone { get; set; }
        public string Role { get; set; } = "Customer";
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Navigation
        public List<Booking>? Bookings { get; set; }
    }
}