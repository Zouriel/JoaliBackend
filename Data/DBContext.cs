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
                Id = 1001,
                Name = "Admin",
                Email = "admin@Joali.com",
                Password_hash = "$2a$11$u9kvVVyl2jMsAn.NoVmaFOKNyVgkJMKCmd/j1R4OCKMt61xHHqx2m",
                PhoneNumber = "+9601234567",
                CreatedAt = DateTime.Parse("2025-08-01"), // Fixed: Convert string to DateTime
                IsActive = true,
                UserType = UserType.Staff, // Assuming you have this enum value
                StaffRole = StaffRole.Admin, // Or null if optional
                staffId = "ADM-001",
                OrgId = null
            });
        }

        
    }
}
