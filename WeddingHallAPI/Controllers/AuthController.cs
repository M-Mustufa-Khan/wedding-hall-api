using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using WeddingHallAPI.Data;
using WeddingHallAPI.Models;
using WeddingHallAPI.DTOs;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Text;

namespace WeddingHallAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _config;

        public AuthController(AppDbContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        // POST: api/auth/register
        [HttpPost("register")]
        public async Task<ActionResult> Register(RegisterDTO dto)
        {
            // Check if email already exists
            var existingUser = await _context.Users.AnyAsync(u => u.Email == dto.Email);
            if (existingUser)
                return BadRequest(new { message = "Email already registered" });

            var hasher = new PasswordHasher<User>();
            var user = new User
            {
                FullName = dto.FullName,
                Email = dto.Email,
                PasswordHash = hasher.HashPassword(new User(), dto.Password),
                Phone = dto.Phone,
                Role = "Customer",
                CreatedAt = DateTime.Now
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Registration successful", userId = user.UserID });
        }

        // POST: api/auth/login
        [HttpPost("login")]
        public async Task<ActionResult> Login(LoginDTO dto)
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email == dto.Email);

            if (user == null)
                return Unauthorized(new { message = "Invalid email or password" });

            var hasher = new PasswordHasher<User>();
            var result = hasher.VerifyHashedPassword(user, user.PasswordHash, dto.Password);

            if (result == PasswordVerificationResult.Failed)
                return Unauthorized(new { message = "Invalid email or password" });

            // Generate JWT Token
            var token = GenerateJwtToken(user);

            return Ok(new
            {
                message = "Login successful",
                token = token,
                user = new
                {
                    user.UserID,
                    user.FullName,
                    user.Email,
                    user.Role
                }
            });
        }

        private string GenerateJwtToken(User user)
        {
            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_config["Jwt:Key"] ?? "MySuperSecretKeyForWeddingHall2025!"));

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserID.ToString()),
                new Claim(ClaimTypes.Name, user.FullName),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role)
            };

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"] ?? "WeddingHallAPI",
                audience: _config["Jwt:Audience"] ?? "WeddingHallClient",
                claims: claims,
                expires: DateTime.Now.AddDays(7),
                signingCredentials: new SigningCredentials(key, SecurityAlgorithms.HmacSha256)
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}