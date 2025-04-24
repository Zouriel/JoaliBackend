using System.ComponentModel.DataAnnotations;

namespace JoaliBackend.DTO.ServiceDTOs
{
    public class NewServiceDTO
    {
        public string Name { get; set; }
        public string? Description { get; set; }

        public int Price { get; set; }
        public int OrgId { get; set; }           // FK to Organization
        public int ServiceTypeId { get; set; }   // FK to ServiceType\
        [Url]
        public string? imageUrl { get; set; }
        public int? Capacity { get; set; }       // Optional
        public int? DurationInMinutes { get; set; } // Optional
    }

}
