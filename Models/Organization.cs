namespace JoaliBackend.Models
{
    public class Organization
    {
        public int Id { get; set; }  // Primary Key

        public string Name { get; set; }  // e.g. "Joali Pvt Ltd"

        public string RegistrationNumber { get; set; }  // e.g. government reg number

        public string Email { get; set; }  // main contact email

        public string Phone { get; set; }  // main phone line

        public string Address { get; set; }  // street + city etc

        public string Country { get; set; }  // e.g. "Maldives"
        public string Website { get; set; }  // optional for branding, store path/URL

        public string LogoUrl { get; set; }  // optional for branding, store path/URL

        public bool IsActive { get; set; } = true;  // soft-delete friendly

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }
        public int? ParentOrganizationId { get; set; } // for org hierarchy
        public Organization? ParentOrganization { get; set; }
    }

}
