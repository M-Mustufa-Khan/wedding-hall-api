using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization; 

namespace WeddingHallAPI.Models
{
    public class HallImage
    {
        [Key]
        public int ImageID { get; set; }

        public int HallID { get; set; }
        public string ImageURL { get; set; } = string.Empty;
        public bool IsPrimary { get; set; }

        // Navigation
        public Hall? Hall { get; set; }
    }
}