using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace WeddingHallAPI.Models
{
    public class Hall
    {
        [Key]
        public int HallID { get; set; }

        [Required]
        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }

        [Required]
        public int Capacity { get; set; }

        [Required]
        public decimal PricePerDay { get; set; }

        [Required]
        public string Location { get; set; } = string.Empty;

        public string? Address { get; set; }

        public string? ImageURL { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        // Navigation properties
        public List<HallImage>? Images { get; set; }
        public List<Package>? Packages { get; set; }
        public List<Booking>? Bookings { get; set; }
    }
}   