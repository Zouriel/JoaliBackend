using System.ComponentModel.DataAnnotations;

namespace JoaliBackend.DTO.UserDTOs
{
    public class NewCustomerDTO
    {
        
        public string Name { get; set; }
        [EmailAddress(ErrorMessage = "Invalid email address")]
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Password { get; set; }
        public string PasswordConfirm {  get; set; }
        


    }
    public enum UserType
    {
        
        Staff = 1,
        Customer = 2,
    }
}
