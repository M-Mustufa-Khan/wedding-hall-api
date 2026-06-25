using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WeddingHallAPI.Data;
using WeddingHallAPI.Models;
using WeddingHallAPI.DTOs;

namespace WeddingHallAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ContactsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ContactsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/contacts
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<IEnumerable<Contact>>> GetContacts()
        {
            return await _context.Contacts.OrderByDescending(c => c.CreatedAt).ToListAsync();
        }

        // POST: api/contacts
        [HttpPost]
        public async Task<ActionResult> CreateContact(ContactDTO dto)
        {
            var contact = new Contact
            {
                Name = dto.Name,
                Email = dto.Email,
                Phone = dto.Phone,
                Subject = dto.Subject,
                Message = dto.Message,
                IsRead = false,
                CreatedAt = DateTime.Now
            };

            _context.Contacts.Add(contact);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Message sent successfully!" });
        }

        // PUT: api/contacts/5/read
        [HttpPut("{id}/read")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> MarkAsRead(int id)
        {
            var contact = await _context.Contacts.FindAsync(id);
            if (contact == null) return NotFound();

            contact.IsRead = true;
            await _context.SaveChangesAsync();

            return Ok(new { message = "Marked as read" });
        }

        // DELETE: api/contacts/5
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> DeleteContact(int id)
        {
            var contact = await _context.Contacts.FindAsync(id);
            if (contact == null) return NotFound();

            _context.Contacts.Remove(contact);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Deleted" });
        }
    }
}