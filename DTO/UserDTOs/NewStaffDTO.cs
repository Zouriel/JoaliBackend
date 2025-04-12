using System.ComponentModel.DataAnnotations;

namespace JoaliBackend.DTO.UserDTOs
{
    public class NewStaffDTO
    {
        public string Name { get; set; }
        [EmailAddress(ErrorMessage = "Invalid Email Address")]
        public string Email { get; set; }
        [Phone(ErrorMessage = "Invalid Phone Number")]
        public string PhoneNumber { get; set; }

    }
}
