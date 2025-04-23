using JoaliBackend.Models;
using System.ComponentModel.DataAnnotations;

namespace JoaliBackend.DTOs
{
    public class OrganizationDto
    {
        [Required]
        [MaxLength(150)]
        public string Name { get; set; }

        [MaxLength(100)]
        public string RegistrationNumber { get; set; }

        [EmailAddress]
        public string Email { get; set; }

        [Phone]
        public string Phone { get; set; }

        [MaxLength(300)]
        public string Address { get; set; }

        [MaxLength(100)]
        public string Country { get; set; }

        [Url]
        public string LogoUrl { get; set; }

        [Url]
        public string Website { get; set; }
        
        public OrgType orgType { get; set; }


    }
}
