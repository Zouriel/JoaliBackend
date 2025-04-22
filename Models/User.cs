using JoaliBackend.DTO.UserDTOs;
using System.ComponentModel.DataAnnotations;

namespace JoaliBackend.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Name { get; set; }
        [EmailAddress(ErrorMessage = "Invalid email address")]
        public string Email { get; set; }
        public string Password_hash { get; set; }
        [Phone(ErrorMessage = "Invalid Phone Number")]
        public string PhoneNumber { get; set; }
        public DateTime CreatedAt { get; set; } 
        public bool IsActive { get; set; }
        public UserType UserType { get; set; }
        public StaffRole? StaffRole { get; set; }
        public string? staffId { get; set; }
        public List<Session>? ActiveSessions { get; set; }
        public string? TemporaryKey { get; set; }
        public DateTime? TemporaryKeyExpiresAt { get; set; }
        public int? OrgId { get; set; }

    }
    public enum StaffRole
    {
        Admin = 0,
        Manager = 1,
        Staff = 2,
    }
    

}
