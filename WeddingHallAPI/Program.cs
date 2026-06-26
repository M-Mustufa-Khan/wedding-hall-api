using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using WeddingHallAPI.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using WeddingHallAPI.Models;
using WeddingHallAPI.Services;

// Allow DateTime.Now (local) with PostgreSQL — avoids rewriting every timestamp in the app
AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

var builder = WebApplication.CreateBuilder(args);

// Listen on Railway's dynamic PORT, fall back to 8080 locally
var port = Environment.GetEnvironmentVariable("PORT") ?? "8080";
builder.WebHost.UseUrls($"http://0.0.0.0:{port}");

// ========== ADD SERVICES ==========

// 1. Database Connection — Railway sets DATABASE_URL as a URI; convert it for Npgsql
var rawDb = Environment.GetEnvironmentVariable("DATABASE_URL")
    ?? builder.Configuration.GetConnectionString("DefaultConnection");

string connectionString;
if (rawDb != null && (rawDb.StartsWith("postgresql://") || rawDb.StartsWith("postgres://")))
{
    var uri = new Uri(rawDb);
    var userInfo = uri.UserInfo.Split(':');
    connectionString = $"Host={uri.Host};Port={uri.Port};Database={uri.AbsolutePath.TrimStart('/')};Username={userInfo[0]};Password={userInfo[1]};SSL Mode=Require;Trust Server Certificate=true";
}
else
{
    connectionString = rawDb ?? "";
}

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(connectionString));

// 2. JWT Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"] ?? "WeddingHallAPI",
            ValidAudience = builder.Configuration["Jwt:Audience"] ?? "WeddingHallClient",
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"] ?? "MySuperSecretKeyForWeddingHall2025!AtLeast32Chars"))
        };
    });

// 3. CORS (Allow React frontend to call API)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp",
        policy =>
        {
            var extraOrigins = builder.Configuration["AllowedOrigins"]
                ?.Split(",", StringSplitOptions.RemoveEmptyEntries) ?? Array.Empty<string>();

            var origins = new[] { "http://localhost:3000", "http://localhost:5173" }
                .Concat(extraOrigins).ToArray();

            policy.WithOrigins(origins)
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowCredentials();
        });
});

// 4b. Email service
builder.Services.AddSingleton<EmailService>();

// 4. Controllers
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
    });

// 5. Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Seed admin user and ensure DB created
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();
    if (!db.Users.Any(u => u.Email == "admin@weddinghall.com"))
    {
        var hasher = new PasswordHasher<User>();
        var adminUser = new User
        {
            FullName = "Admin User",
            Email = "admin@weddinghall.com",
            Role = "Admin",
            CreatedAt = DateTime.Now
        };
        adminUser.PasswordHash = hasher.HashPassword(adminUser, "admin123");
        db.Users.Add(adminUser);
        db.SaveChanges();
    }
}

// ========== CONFIGURE PIPELINE ===========
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

if (app.Environment.IsDevelopment())
    app.UseHttpsRedirection();
app.UseCors("AllowReactApp");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();