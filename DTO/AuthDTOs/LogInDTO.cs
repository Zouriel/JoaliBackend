using System.ComponentModel.DataAnnotations;

namespace JoaliBackend.DTO.AuthDTOs
{
    public class LogInDTO
    {
        [EmailAddress(ErrorMessage = "Invalid email address")]
        [Required]
        public string Email { get; set; }

        [Required]
        public string Password { get; set; }
        [Required]
        public string APIKEY { get; set; }
    }
}
