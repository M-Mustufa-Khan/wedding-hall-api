using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WeddingHallAPI.Data;
using WeddingHallAPI.Models;

namespace WeddingHallAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class GalleryController : ControllerBase
    {
        private readonly AppDbContext _db;

        public GalleryController(AppDbContext db)
        {
            _db = db;
        }

        // GET api/Gallery — public, returns all images ordered newest first
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var images = await _db.GalleryImages
                .OrderByDescending(g => g.CreatedAt)
                .Select(g => new {
                    g.Id,
                    g.Category,
                    g.ImageBase64,
                    g.Caption,
                    g.CreatedAt
                })
                .ToListAsync();

            return Ok(images);
        }

        // POST api/Gallery — admin only
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([FromBody] GalleryImageDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Category) || string.IsNullOrWhiteSpace(dto.ImageBase64))
                return BadRequest("Category and image are required.");

            var image = new GalleryImage
            {
                Category  = dto.Category.Trim(),
                ImageBase64 = dto.ImageBase64,
                Caption   = dto.Caption?.Trim(),
                CreatedAt = DateTime.Now
            };

            _db.GalleryImages.Add(image);
            await _db.SaveChangesAsync();

            return Ok(new { image.Id, image.Category, image.ImageBase64, image.Caption, image.CreatedAt });
        }

        // DELETE api/Gallery/{id} — admin only
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var image = await _db.GalleryImages.FindAsync(id);
            if (image == null) return NotFound();

            _db.GalleryImages.Remove(image);
            await _db.SaveChangesAsync();

            return NoContent();
        }
    }

    public class GalleryImageDto
    {
        public string Category { get; set; } = string.Empty;
        public string ImageBase64 { get; set; } = string.Empty;
        public string? Caption { get; set; }
    }
}
