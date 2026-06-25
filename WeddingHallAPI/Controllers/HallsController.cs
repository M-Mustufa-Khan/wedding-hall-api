using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WeddingHallAPI.Data;
using WeddingHallAPI.Models;

namespace WeddingHallAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HallsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public HallsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/halls
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Hall>>> GetHalls(
            [FromQuery] string? location,
            [FromQuery] int? minCapacity,
            [FromQuery] int? maxCapacity,
            [FromQuery] decimal? minPrice,
            [FromQuery] decimal? maxPrice)
        {
            // List endpoint: omit Images and Packages — base64 gallery data is ~500KB+ per hall
            var query = _context.Halls
                .Where(h => h.IsActive);

            if (!string.IsNullOrEmpty(location))
                query = query.Where(h => h.Location.Contains(location));

            if (minCapacity.HasValue)
                query = query.Where(h => h.Capacity >= minCapacity);

            if (maxCapacity.HasValue)
                query = query.Where(h => h.Capacity <= maxCapacity);

            if (minPrice.HasValue)
                query = query.Where(h => h.PricePerDay >= minPrice);

            if (maxPrice.HasValue)
                query = query.Where(h => h.PricePerDay <= maxPrice);

            var halls = await query.OrderByDescending(h => h.CreatedAt).ToListAsync();
            return Ok(halls);
        }

        // GET: api/halls/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Hall>> GetHall(int id)
        {
            var hall = await _context.Halls
                .Include(h => h.Images)
                .Include(h => h.Packages)
                .FirstOrDefaultAsync(h => h.HallID == id);

            if (hall == null)
                return NotFound(new { message = "Hall not found" });

            return Ok(hall);
        }

        // POST: api/halls
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<Hall>> CreateHall(Hall hall)
        {
            hall.CreatedAt = DateTime.Now;
            _context.Halls.Add(hall);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetHall), new { id = hall.HallID }, hall);
        }

        // PUT: api/halls/5
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateHall(int id, Hall hall)
        {
            if (id != hall.HallID)
                return BadRequest();

            // Update scalar fields only
            var existing = await _context.Halls.FindAsync(id);
            if (existing == null) return NotFound();

            existing.Name = hall.Name;
            existing.Description = hall.Description;
            existing.Capacity = hall.Capacity;
            existing.PricePerDay = hall.PricePerDay;
            existing.Location = hall.Location;
            existing.Address = hall.Address;
            existing.ImageURL = hall.ImageURL;
            existing.IsActive = hall.IsActive;

            // Replace gallery images (non-primary) if provided
            if (hall.Images != null)
            {
                var oldImages = _context.HallImages
                    .Where(img => img.HallID == id && !img.IsPrimary);
                _context.HallImages.RemoveRange(oldImages);

                foreach (var img in hall.Images.Where(img => !string.IsNullOrEmpty(img.ImageURL)))
                {
                    _context.HallImages.Add(new HallImage
                    {
                        HallID = id,
                        ImageURL = img.ImageURL,
                        IsPrimary = false
                    });
                }
            }

            // Replace packages if provided
            if (hall.Packages != null)
            {
                var oldPackages = _context.Packages.Where(p => p.HallID == id);
                _context.Packages.RemoveRange(oldPackages);

                foreach (var pkg in hall.Packages.Where(p => !string.IsNullOrEmpty(p.Name)))
                {
                    _context.Packages.Add(new Package
                    {
                        HallID = id,
                        Name = pkg.Name,
                        Description = pkg.Description,
                        Price = pkg.Price,
                        Includes = pkg.Includes
                    });
                }
            }

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!HallExists(id))
                    return NotFound();
                throw;
            }

            return NoContent();
        }

        // DELETE: api/halls/5
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteHall(int id)
        {
            var hall = await _context.Halls.FindAsync(id);
            if (hall == null)
                return NotFound();

            hall.IsActive = false; // Soft delete
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool HallExists(int id)
        {
            return _context.Halls.Any(e => e.HallID == id);
        }
    }
}