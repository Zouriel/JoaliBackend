using Microsoft.EntityFrameworkCore;
using Azure.Core;
using Azure;
using System.Data;
using System.Security.Cryptography.Xml;
using System.Text.RegularExpressions;
using JoaliBackend.Models;
using JoaliBackend.DTO.UserDTOs;
using System.Text;
using System.Security.Cryptography;


namespace Joali.Data
{
    public class EFCoreDbContext : DbContext
    {
        public EFCoreDbContext(DbContextOptions<EFCoreDbContext> options) : base(options)
        {
        }
        public DbSet<User> Users { get; set; }
        public DbSet<Session> Sessions { get; set; }
        public DbSet<Organization> Organizations { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<User>().HasData(new User
            {

                Name = "Admin",
                Email = "admin@Joali.com",
                Password_hash = HashPassword("Admin@123"), // Simple SHA256 hash
                PhoneNumber = "+9601234567",
                CreatedAt = DateTime.UtcNow,
                IsActive = true,
                UserType = UserType.Staff, // Assuming you have this enum value
                StaffRole = StaffRole.Admin, // Or null if optional
                staffId = "ADM-001",
                OrgId = null
            });


        }
    
        string HashPassword(string password)
        {
            using var sha256 = SHA256.Create();
            var hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(hash);
        }
    }
}
