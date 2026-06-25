using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace WeddingHallAPI.Models
{
    public class Package
    {
        [Key]
        public int PackageID { get; set; }

        public int HallID { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public string? Includes { get; set; }

        // Navigation
        public Hall? Hall { get; set; }
        public List<Booking>? Bookings { get; set; }
    }
}