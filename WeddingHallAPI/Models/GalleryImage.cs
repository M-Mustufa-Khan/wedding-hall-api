using System.ComponentModel.DataAnnotations;

namespace WeddingHallAPI.Models
{
    public class GalleryImage
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Category { get; set; } = string.Empty;

        [Required]
        public string ImageBase64 { get; set; } = string.Empty;

        public string? Caption { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
