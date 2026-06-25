using System.ComponentModel.DataAnnotations;

namespace WeddingHallAPI.Models
{
    public class Contact
    {
        [Key]
        public int ContactID { get; set; }

        [Required]
        public string Name { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        public string? Phone { get; set; }
        public string? Subject { get; set; }

        [Required]
        public string Message { get; set; } = string.Empty;

        public bool IsRead { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}