using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WeddingHallAPI.Data;
using WeddingHallAPI.Models;
using WeddingHallAPI.DTOs;
using WeddingHallAPI.Services;

namespace WeddingHallAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BookingsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly EmailService _email;

        public BookingsController(AppDbContext context, EmailService email)
        {
            _context = context;
            _email   = email;
        }

        // GET: api/bookings
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<IEnumerable<Booking>>> GetBookings()
        {
            var bookings = await _context.Bookings
                .AsNoTracking()
                .Include(b => b.User)
                .Include(b => b.Hall)
                .Include(b => b.Package)
                .Include(b => b.Payments)
                .OrderByDescending(b => b.CreatedAt)
                .ToListAsync();

            return Ok(bookings);
        }

        // GET: api/bookings/check
        [HttpGet("check")]
        public async Task<ActionResult<bool>> CheckAvailability([FromQuery] int hallId, [FromQuery] DateTime date)
        {
            var isBooked = await _context.Bookings
                .AnyAsync(b => b.HallID == hallId
                    && b.EventDate.Date == date.Date
                    && b.Status != "Cancelled");

            return Ok(!isBooked);
        }

        // GET: api/bookings/user/5
        [HttpGet("user/{userId}")]
        public async Task<ActionResult<IEnumerable<Booking>>> GetUserBookings(int userId)
        {
            var bookings = await _context.Bookings
                .AsNoTracking()
                .Include(b => b.Hall)
                .Include(b => b.Package)
                .Include(b => b.Payments)
                .Where(b => b.UserID == userId)
                .OrderByDescending(b => b.CreatedAt)
                .ToListAsync();

            return Ok(bookings);
        }

        // GET: api/bookings/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Booking>> GetBooking(int id)
        {
            var booking = await _context.Bookings
                .AsNoTracking()
                .Include(b => b.User)
                .Include(b => b.Hall)
                .Include(b => b.Package)
                .Include(b => b.Payments)
                .FirstOrDefaultAsync(b => b.BookingID == id);

            if (booking == null)
                return NotFound(new { message = "Booking not found" });

            return Ok(booking);
        }

        // POST: api/bookings
        [HttpPost]
        [Authorize]
        public async Task<ActionResult<Booking>> CreateBooking(CreateBookingDTO dto)
        {
            var existingBooking = await _context.Bookings
                .AnyAsync(b => b.HallID == dto.HallID
                    && b.EventDate.Date == dto.EventDate.Date
                    && b.Status != "Cancelled");

            if (existingBooking)
                return BadRequest(new { message = "Hall is already booked on this date" });

            var booking = new Booking
            {
                UserID = dto.UserID,
                HallID = dto.HallID,
                PackageID = dto.PackageID,
                EventDate = dto.EventDate,
                EventType = dto.EventType,
                GuestCount = dto.GuestCount,
                TotalPrice = dto.TotalPrice,
                SpecialNotes = dto.SpecialNotes,
                CNIC = dto.CNIC,
                CustomerPhone = dto.CustomerPhone,
                CustomerAddress = dto.CustomerAddress,
                PaymentMethod = dto.PaymentMethod,
                TransactionRef = dto.TransactionRef,
                ReceiptImage = dto.ReceiptImage,
                Status = "Pending",
                CreatedAt = DateTime.Now
            };

            _context.Bookings.Add(booking);
            await _context.SaveChangesAsync();

            // Fire-and-forget confirmation email
            var user = await _context.Users.FindAsync(dto.UserID);
            var hall = await _context.Halls.FindAsync(dto.HallID);
            if (user != null && hall != null)
                _ = _email.SendBookingConfirmationAsync(user.Email, user.FullName, hall.Name, booking.EventDate, booking.TotalPrice);

            return CreatedAtAction(nameof(GetBooking), new { id = booking.BookingID }, booking);
        }

        // ✅ FIXED: PUT api/bookings/5/status
        [HttpPut("{id}/status")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateBookingStatus(int id, [FromBody] StatusUpdateDTO dto)
        {
            var booking = await _context.Bookings.FindAsync(id);
            if (booking == null)
                return NotFound();

            booking.Status = dto.Status;
            await _context.SaveChangesAsync();

            // Send status update email
            var fullBooking = await _context.Bookings
                .Include(b => b.User)
                .Include(b => b.Hall)
                .FirstOrDefaultAsync(b => b.BookingID == id);
            if (fullBooking?.User != null && fullBooking?.Hall != null)
                _ = _email.SendBookingStatusUpdateAsync(fullBooking.User.Email, fullBooking.User.FullName, fullBooking.Hall.Name, fullBooking.EventDate, dto.Status);

            return Ok(new { message = "Status updated successfully" });
        }

        // DELETE: api/bookings/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> CancelBooking(int id)
        {
            var booking = await _context.Bookings.FindAsync(id);
            if (booking == null)
                return NotFound();

            booking.Status = "Cancelled";
            await _context.SaveChangesAsync();

            return Ok(new { message = "Booking cancelled" });
        }
    }

    // ✅ ADDED DTO CLASS AT THE BOTTOM
    public class StatusUpdateDTO
    {
        public string Status { get; set; } = string.Empty;
    }
}