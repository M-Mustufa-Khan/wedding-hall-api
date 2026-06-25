using Microsoft.EntityFrameworkCore;
using WeddingHallAPI.Models;

namespace WeddingHallAPI.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Hall> Halls { get; set; }
        public DbSet<HallImage> HallImages { get; set; }
        public DbSet<Package> Packages { get; set; }
        public DbSet<Booking> Bookings { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<Contact> Contacts { get; set; }
        public DbSet<GalleryImage> GalleryImages { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Prevent pluralization issues
            modelBuilder.Entity<User>().ToTable("Users");
            modelBuilder.Entity<Hall>().ToTable("Halls");
            modelBuilder.Entity<HallImage>().ToTable("HallImages");
            modelBuilder.Entity<Package>().ToTable("Packages");
            modelBuilder.Entity<Booking>().ToTable("Bookings");
            modelBuilder.Entity<Payment>().ToTable("Payments");
            modelBuilder.Entity<Contact>().ToTable("Contacts");
            modelBuilder.Entity<GalleryImage>().ToTable("GalleryImages");
        }
    }
}